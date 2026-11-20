# EE2 Exporter — goals & agent handoff

**Read this first** if you are an agent continuing work in this repository.

## What this repo is for

This tree started as a fork of [armoha/EUDEditor](https://github.com/armoha/EUDEditor) (**EUD Editor 2** / EE2). It is **not** an EE2 product iteration and **not** a map build system.

It is a **companion GUI** for custom TrigScript / epScript (and later classic TrigEdit) build chains:

1. Edit vanilla DAT tables (units, weapons, graphics, …) and Firegraft-style buttons/requirements against StarCraft MPQ data.
2. Export those edits as **importable source** — primarily **TrigScript** (`setMemory`), optionally **epScript** (`SetMemory` in `onPluginStart`).

**Display name:** EE2 Exporter (full: *EUD Editor 2 Exporter*). **Repo slug:** `ee2-exporter`.

**Linux-first:** The point of the fork was approximate Linux-native usability without rewriting EE2. The WinForms app runs under **Wine** so Linux mapmakers can use it beside scmJS / TrigScript development; the same binary runs on Windows.

```text
  [DAT / Firegraft GUI]  --export-->  eud-patches.trig  (or .eps)
                                              |
                                              v
                                    custom TrigScript / scmJS or epScript build
```

Pixel-perfect upstream EE2 UI is **not** a goal. Behavioural usefulness for DAT/Firegraft editing + clean export **is**.

**Stripped from EE2:** map Input/Output / inject / euddraft compile, EDD insert, TileSet, and **Plugins** (disabled while TrigScript is primary; may return later for epScript, where plugins fit more naturally).

### Naming & attribution

- Homage name acknowledging upstream EUD Editor 2; not claiming to be the official EE2 product.
- License: **MIT** (same as upstream). Keep `LICENSE` copyright for LeeJungHun; credit [armoha/EUDEditor](https://github.com/armoha/EUDEditor) in the README.
- Local project folder remains `EUD Editor/` (VB project path churn avoided); shipped exe / product title is **EE2 Exporter**.

---

## Sister project — paths on this machine

| Role | Absolute path |
| --- | --- |
| **This repo** | `/home/michael/maps/ee2-exporter/` |
| **CK map project** | `/home/michael/maps/SCR_CrusaderKings/` |
| **TrigScript / scmJS docs** | `/home/michael/maps/SCR_CrusaderKings/docs/trigscript-reference/` |
| **epScript docs (legacy / dual export)** | `/home/michael/maps/SCR_CrusaderKings/docs/epscript-reference/` |
| **Export convention (CK)** | `/home/michael/maps/SCR_CrusaderKings/docs/eud-editor-export.md` |
| **Typical patch drop** | `/home/michael/maps/SCR_CrusaderKings/trigscript/eud-patches/` |

Online TrigScript docs: https://docs.scmjs.dev/

---

## How TrigScript expects table patches

Prefer this order in **hand-written or future codegen** map logic (from TrigScript docs):

1. **`stats(...)`** — named unit / weapon / upgrade / tech / player fields validated for Remastered (e.g. `stats(units.TerranMarine).minerals = 25`).
2. **`memory.*`** — named non-table game values when listed.
3. **`setMemory(address, modifier, value)`** — full EUD escape hatch for everything else.

**What this tool emits today:** (3) only — a list of `setMemory` / epScript `SetMemory` ops with comments carrying table/field/objectId. That matches EE2’s native “delta at absolute address” model.

**“Lift some DatEdit fields to `stats()`”** (optional later, *not* required for Phase 1): for fields TrigScript already exposes under `stats(units.X).hp` (etc.), emit that high-level assignment instead of a raw `setMemory` to the Hit Points dword. Safer for Remastered, clearer in map code. Deep Firegraft/button/require work stays on `setMemory`. Do **not** block shipping on this lift.

---

## Current status (done)

- Wine prefix + `./setup-wine.sh` / `./build-eud-editor.sh` / `./run-eud-editor.sh` (Linux companion workflow)
- Map Input/Output **disabled** (not “optional”); euddraft build path disabled
- Export: **Tools** format checkables (TrigScript / epScript; Classic TrigEdit stubbed) + **Export** / AutoExport
- Output: `eud-patches.trig` or `eud-patches.eps` (not JSON)
- Firegraft require/button dumps only when those tables were actually edited
- TileSet + Plugin UI disabled; Help → https://github.com/michaelcrusso/ee2-exporter
- Renamed product/repo to **EE2 Exporter** / `ee2-exporter`

### Export quick use

1. Open/create a project (no map load/save).
2. Edit DatEdit / Firegraft as needed.
3. **Tool → TrigScript Export** or **epScript Export**, then **Export Now…** (or toolbar **Export**).
4. Import the generated file into the TrigScript or epScript project.

---

## Runtime: Wine *and* Windows

The app is **.NET Framework 4.8 WinForms (x86)**. Wine is the intentional Linux path, not a second product.

| Host | How |
| --- | --- |
| **Linux** | Repo Wine prefix; scripts above |
| **Windows** | Open `EUD Editor.sln` in Visual Studio (or MSBuild), Platform **x86**, build Debug/Release. Same `EE2 Exporter.exe`. Point settings at real `StarCraft.exe` and MPQs (or this repo’s `mpqs/`). |

PreBuild `taskkill` is Windows-native-friendly. Linux-only pieces are the shell scripts and `wine/` prefix — not required on Windows.

---

## Out of scope / non-goals

- euddraft as a required dependency for DAT patches
- Classic TrigEdit text export (needs careful EPD encoding — deferred)
- Restoring EE2 SE “integrated build” features (map inject, EDD, plugins, TileSet, …)
- Full Linux-native UI (possible later Phase 2; not blocking)
- Expanding epScript *inside CK* as the primary toolchain (epScript *export* exists for other users)

---

## Suggested next work (only if needed)

1. Refresh CK `docs/eud-editor-export.md` to `.trig` / `.eps` (drop JSON wording).
2. Optional: `stats()` lift for common DatEdit fields.
3. Optional: Classic TrigEdit emitter with golden Remastered samples.
4. Further UI thinning (hide BGM/GRP/BinEditor/Debug if unused).

---

## Quick orientation in this codebase

| Area | Where |
| --- | --- |
| Solution | `EUD Editor.sln`, `EUD Editor/EUD Editor.vbproj` |
| DAT GUI | `EUD Editor/Form/DatEditForm.*` |
| Export | `EUD Editor/Module/TrigScriptExportModule.vb` |
| Legacy eudplib emit (dormant) | `EUD Editor/Module/eudplibModule.vb` |
| Native MPQ | `dll/`, `SFmpq.vb`, `StormLib.vb`, `CascLib.vb` |
