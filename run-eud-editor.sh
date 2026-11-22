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

# Prefer the renamed assembly; fall back to pre-rename builds still on disk.
BIN_DIRS=(
  "${ROOT}/EUD Editor/bin/x86/Release"
  "${ROOT}/EUD Editor/bin/x86/Debug"
  "${ROOT}/EUD Editor/bin/Release"
  "${ROOT}/EUD Editor/bin/Debug"
  "${ROOT}/dist"
)
EXE_NAMES=(
  "EE2 Exporter.exe"
  "EUD Editor SE.exe"
)

EXE=""
for bin_dir in "${BIN_DIRS[@]}"; do
  for exe_name in "${EXE_NAMES[@]}"; do
    candidate="${bin_dir}/${exe_name}"
    if [[ -f "${candidate}" ]]; then
      EXE="${candidate}"
      break 2
    fi
  done
done

if [[ -z "${EXE}" ]]; then
  echo "error: no EE2 Exporter.exe (or legacy EUD Editor SE.exe) found under EUD Editor/bin/ (or dist/)." >&2
  echo "Build with: ./build-eud-editor.sh" >&2
  exit 1
fi

echo "==> WINEPREFIX=${WINEPREFIX}"
echo "==> Starting: ${EXE}"
cd "$(dirname "${EXE}")"
exec "${WINE_BIN}" "$(basename "${EXE}")" "$@"
