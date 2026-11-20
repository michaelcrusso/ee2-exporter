#!/usr/bin/env bash
# Create a repo-local Wine prefix with .NET Framework 4.8 (+ deps)
# for running EE2 Exporter under Linux. Does not use ~/.wine or other leftovers.
#
# Wine 10+ on Ubuntu is wow64-only: pure WINEARCH=win32 prefixes fail with
#   "WINEARCH is set to 'win32' but this is not supported in wow64 mode."
# So we use win64 and run the x86 editor under WoW64.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PREFIX="${ROOT}/wine"
MARKER="${PREFIX}/.eudeditor-wine-ready"
LOG="${ROOT}/wine-setup.log"
VERBS=(corefonts gdiplus vcrun2015 vcrun2019 xna40 dotnet48)

export WINEPREFIX="${PREFIX}"
export WINEARCH=win64
export DISPLAY="${DISPLAY:-}"

pick_wine() {
  # Prefer the multi-arch dispatcher; wine32 alone cannot host a win64 prefix.
  if command -v wine >/dev/null 2>&1; then
    echo wine
  elif command -v wine64 >/dev/null 2>&1; then
    echo wine64
  else
    echo "error: wine/wine64 not found. Install Wine (with 32-bit WoW64 support)." >&2
    exit 1
  fi
}

WINE_BIN="$(pick_wine)"

if ! command -v winetricks >/dev/null 2>&1; then
  echo "error: winetricks not found. Install winetricks, then re-run." >&2
  exit 1
fi

echo "==> Wine binary : ${WINE_BIN} ($("${WINE_BIN}" --version 2>/dev/null || true))"
echo "==> WINEPREFIX  : ${WINEPREFIX}"
echo "==> WINEARCH    : ${WINEARCH}"
echo "==> Log         : ${LOG}"

# Refuse a leftover broken win32 prefix from earlier attempts
if [[ -f "${PREFIX}/system.reg" ]] && grep -q '^#arch=win32' "${PREFIX}/system.reg" 2>/dev/null; then
  echo "error: ${PREFIX} is a win32 prefix; Wine wow64 cannot use it." >&2
  echo "       Remove it and re-run:  rm -rf \"${PREFIX}\"" >&2
  exit 1
fi

mkdir -p "${PREFIX}"

if [[ ! -d "${PREFIX}/drive_c/windows" ]]; then
  echo "==> Initializing new win64 prefix (wineboot)…"
  "${WINE_BIN}" wineboot --init
fi

if [[ -f "${MARKER}" ]]; then
  echo "==> Prefix already marked ready (${MARKER})."
  echo "    Delete that file and re-run to force reinstall of winetricks verbs."
  echo "==> Done."
  exit 0
fi

echo "==> Installing winetricks verbs (this can take a long time):"
printf '    %s\n' "${VERBS[@]}"
echo "    (Also logging to ${LOG})"

# During winetricks .NET installs, disable Mono's mscoree so the real Framework can install.
# run-eud-editor.sh does not set this, so the app can use native .NET.
export WINEDLLOVERRIDES="mscoree,mshtml="

set +e
winetricks -q --force "${VERBS[@]}" 2>&1 | tee -a "${LOG}"
WT_STATUS=${PIPESTATUS[0]}
set -e
unset WINEDLLOVERRIDES || true

if [[ "${WT_STATUS}" -ne 0 ]]; then
  echo "error: winetricks failed with exit code ${WT_STATUS}. See ${LOG}" >&2
  exit "${WT_STATUS}"
fi

# Sanity: Framework should exist (32-bit under Framework, 64 under Framework64)
DOTNET32="${PREFIX}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319"
DOTNET64="${PREFIX}/drive_c/windows/Microsoft.NET/Framework64/v4.0.30319"
if [[ ! -d "${DOTNET32}" && ! -d "${DOTNET64}" ]]; then
  echo "error: .NET Framework v4.0.30319 not found under the prefix." >&2
  echo "       winetricks dotnet48 may have failed; inspect ${LOG}." >&2
  exit 1
fi

date -u +"%Y-%m-%dT%H:%M:%SZ" >"${MARKER}"
{
  echo "arch=win64"
  echo "verbs=${VERBS[*]}"
  echo "wine=$(command -v "${WINE_BIN}")"
  echo "wine_version=$("${WINE_BIN}" --version 2>/dev/null || true)"
  echo "dotnet32=$([[ -d ${DOTNET32} ]] && echo yes || echo no)"
  echo "dotnet64=$([[ -d ${DOTNET64} ]] && echo yes || echo no)"
} >>"${MARKER}"

echo "==> Wine prefix ready."
echo "    Run the editor with: ./run-eud-editor.sh"
echo "    (Build EE2 Exporter.exe first on Windows/VS, or place a build under EUD Editor/bin/.)"
