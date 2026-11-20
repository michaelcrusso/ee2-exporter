#!/usr/bin/env bash
# Launch EE2 Exporter using the repo-local Wine prefix created by setup-wine.sh.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PREFIX="${ROOT}/wine"
MARKER="${PREFIX}/.eudeditor-wine-ready"

export WINEPREFIX="${PREFIX}"
export WINEARCH=win64
# Ensure native .NET (mscoree) is used — do not disable it here.
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
  echo "Wine prefix not ready. Run: ${ROOT}/setup-wine.sh" >&2
  exit 1
fi

if [[ -f "${PREFIX}/system.reg" ]] && grep -q '^#arch=win32' "${PREFIX}/system.reg" 2>/dev/null; then
  echo "error: ${PREFIX} is a broken win32 prefix. Remove it and re-run setup-wine.sh:" >&2
  echo "       rm -rf \"${PREFIX}\" && ./setup-wine.sh" >&2
  exit 1
fi

CANDIDATES=(
  "${ROOT}/EUD Editor/bin/x86/Release/EE2 Exporter.exe"
  "${ROOT}/EUD Editor/bin/x86/Debug/EE2 Exporter.exe"
  "${ROOT}/EUD Editor/bin/Release/EE2 Exporter.exe"
  "${ROOT}/EUD Editor/bin/Debug/EE2 Exporter.exe"
  "${ROOT}/dist/EE2 Exporter.exe"
)

EXE=""
for candidate in "${CANDIDATES[@]}"; do
  if [[ -f "${candidate}" ]]; then
    EXE="${candidate}"
    break
  fi
done

if [[ -z "${EXE}" ]]; then
  echo "error: no EE2 Exporter.exe found under EUD Editor/bin/ (or dist/)." >&2
  echo "Build the WinForms project (Visual Studio / MSBuild, x86), then re-run." >&2
  exit 1
fi

echo "==> WINEPREFIX=${WINEPREFIX}"
echo "==> Starting: ${EXE}"
cd "$(dirname "${EXE}")"
exec "${WINE_BIN}" "$(basename "${EXE}")" "$@"
