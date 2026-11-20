#!/usr/bin/env bash
# Build EE2 Exporter.exe under the repo-local Wine prefix (no Windows Docker /
# Visual Studio required on the host).
#
# Why not a Windows SDK Docker image? Those are Windows containers and do not
# run on Linux Docker. On Linux we compile with Wine + MSBuild 4.8 + MIT-licensed
# Microsoft.NETFramework.ReferenceAssemblies.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PREFIX="${ROOT}/wine"
MARKER="${PREFIX}/.eudeditor-wine-ready"
PKG_DIR="${ROOT}/packages"
PROJECT="${ROOT}/EUD Editor/EUD Editor.vbproj"
CONFIGURATION="${CONFIGURATION:-Debug}"
PLATFORM="${PLATFORM:-x86}"
LOG="${ROOT}/wine-build.log"

export WINEPREFIX="${PREFIX}"
export WINEARCH=win64
unset WINEDLLOVERRIDES || true

pick_wine() {
  if command -v wine >/dev/null 2>&1; then
    echo wine
  elif command -v wine64 >/dev/null 2>&1; then
    echo wine64
  else
    echo "error: wine/wine64 not found." >&2
    exit 1
  fi
}

WINE_BIN="$(pick_wine)"

if [[ ! -f "${MARKER}" ]]; then
  echo "Wine prefix not ready. Run ./setup-wine.sh first." >&2
  exit 1
fi

MSBUILD="${PREFIX}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe"
if [[ ! -f "${MSBUILD}" ]]; then
  echo "error: MSBuild.exe not found in the Wine prefix. Re-run ./setup-wine.sh" >&2
  exit 1
fi

download_nupkg() {
  local id="$1"
  local version="$2"
  local dest="$3"
  local url="https://api.nuget.org/v3-flatcontainer/${id,,}/${version}/${id,,}.${version}.nupkg"
  local tmp
  tmp="$(mktemp --suffix=.nupkg)"
  echo "==> Fetching ${id} ${version}"
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "${url}" -o "${tmp}"
  else
    wget -q -O "${tmp}" "${url}"
  fi
  rm -rf "${dest}"
  mkdir -p "${dest}"
  unzip -q -o "${tmp}" -d "${dest}"
  rm -f "${tmp}"
}

mkdir -p "${PKG_DIR}"

# HintPath expects FCTB.2.16.21.0; NuGet publishes "2.16.21". Prefer repo dll/, else NuGet.
if [[ ! -f "${PKG_DIR}/FCTB.2.16.21.0/lib/FastColoredTextBox.dll" ]]; then
  if [[ -f "${ROOT}/dll/FastColoredTextBox.dll" ]]; then
    echo "==> Staging FastColoredTextBox from dll/"
    mkdir -p "${PKG_DIR}/FCTB.2.16.21.0/lib"
    cp -f "${ROOT}/dll/FastColoredTextBox.dll" "${PKG_DIR}/FCTB.2.16.21.0/lib/"
  else
    download_nupkg FCTB 2.16.21 "${PKG_DIR}/FCTB.2.16.21"
    mkdir -p "${PKG_DIR}/FCTB.2.16.21.0/lib"
    cp -f "${PKG_DIR}/FCTB.2.16.21/lib/FastColoredTextBox.dll" "${PKG_DIR}/FCTB.2.16.21.0/lib/"
  fi
fi
if [[ ! -f "${PKG_DIR}/Newtonsoft.Json.10.0.3/lib/net45/Newtonsoft.Json.dll" ]]; then
  if [[ -f "${ROOT}/dll/Newtonsoft.Json.dll" ]]; then
    echo "==> Staging Newtonsoft.Json from dll/"
    mkdir -p "${PKG_DIR}/Newtonsoft.Json.10.0.3/lib/net45"
    cp -f "${ROOT}/dll/Newtonsoft.Json.dll" "${PKG_DIR}/Newtonsoft.Json.10.0.3/lib/net45/"
  else
    download_nupkg Newtonsoft.Json 10.0.3 "${PKG_DIR}/Newtonsoft.Json.10.0.3"
  fi
fi
if [[ ! -d "${PKG_DIR}/Microsoft.NETFramework.ReferenceAssemblies.net48.1.0.3/build/.NETFramework/v4.8" ]]; then
  download_nupkg Microsoft.NETFramework.ReferenceAssemblies.net48 1.0.3 \
    "${PKG_DIR}/Microsoft.NETFramework.ReferenceAssemblies.net48.1.0.3"
fi

REF_ASSEMBLIES="${PKG_DIR}/Microsoft.NETFramework.ReferenceAssemblies.net48.1.0.3/build/.NETFramework/v4.8"
if [[ ! -f "${REF_ASSEMBLIES}/mscorlib.dll" ]]; then
  echo "error: reference assemblies missing at ${REF_ASSEMBLIES}" >&2
  exit 1
fi

# Wine path for FrameworkPathOverride (Z: maps to / on Wine)
WIN_REF="Z:${REF_ASSEMBLIES//\//\\}"
WIN_PROJECT="Z:${PROJECT//\//\\}"
WIN_MSBUILD="C:\\windows\\Microsoft.NET\\Framework\\v4.0.30319\\MSBuild.exe"

echo "==> WINEPREFIX=${WINEPREFIX}"
echo "==> Configuration=${CONFIGURATION} Platform=${PLATFORM}"
echo "==> FrameworkPathOverride=${REF_ASSEMBLIES}"
echo "==> Log: ${LOG}"

set +e
"${WINE_BIN}" "${WIN_MSBUILD}" "${WIN_PROJECT}" \
  "/p:Configuration=${CONFIGURATION}" \
  "/p:Platform=${PLATFORM}" \
  "/p:FrameworkPathOverride=${WIN_REF}" \
  "/p:TargetFrameworkRootPath=${WIN_REF}\\.." \
  "/p:PreBuildEvent=" \
  "/p:CodePage=65001" \
  "/v:m" \
  2>&1 | tee "${LOG}"
STATUS=${PIPESTATUS[0]}
set -e

if [[ "${STATUS}" -ne 0 ]]; then
  echo "error: MSBuild failed (${STATUS}). See ${LOG}" >&2
  exit "${STATUS}"
fi

OUT="${ROOT}/EUD Editor/bin/${PLATFORM}/${CONFIGURATION}/EE2 Exporter.exe"
if [[ ! -f "${OUT}" ]]; then
  # AnyCPU fallback paths
  OUT_ALT="${ROOT}/EUD Editor/bin/${CONFIGURATION}/EE2 Exporter.exe"
  if [[ -f "${OUT_ALT}" ]]; then
    OUT="${OUT_ALT}"
  else
    echo "error: build reported success but exe not found under EUD Editor/bin/" >&2
    exit 1
  fi
fi

echo "==> Built: ${OUT}"
echo "    Run with: ./run-eud-editor.sh"
