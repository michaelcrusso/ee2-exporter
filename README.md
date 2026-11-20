# EE2 Exporter

**EE2 Exporter** is a pared-down StarCraft DAT / Firegraft GUI, derived from [EUD Editor 2](https://github.com/armoha/EUDEditor). It is a **companion GUI** for editing unit stats, graphics, buttons, and related tables, then **exporting a file** you import into a custom **TrigScript**, **epScript**, or (later) classic TrigEdit build chain — not an integrated map compiler.

Full disclosure: this is almost entirely vibe-coded, as I just wanted a tool like this for Linux work with an scmJS/TrigEdit build chain, and no DatEdit style editor is yet available. 

| Format | File | Use |
| --- | --- | --- |
| **TrigScript** (default) | `eud-patches.trig` | `program(() => { setMemory(...); });` for scmJS / TrigScript maps |
| **epScript** | `eud-patches.eps` | `onPluginStart` + `SetMemory(...)` for epScript / euddraft workflows |
| Classic TrigEdit | — | Not implemented yet |

---

## Why this exists (Linux-first)

The initial goal was something **approximately Linux-native** without reinventing EE2’s DAT/Firegraft UI from scratch. The WinForms app runs cleanly under **Wine**, so a Linux mapmaker can use it as a comfortable companion beside **scmJS / TrigScript** (or epScript) development. The same binary builds and runs on native Windows.

---

## What it does / what EE2 features are stripped

**Kept:** DatEdit, Firegraft (buttons / requirements), MPQ / StarCraft path setup, project save (`.e2s` / `.e2p`), and **Export** to TrigScript or epScript.

**Disabled / removed from the workflow** (this is not full EUD Editor 2 SE):

| EE2 capability | Status here |
| --- | --- |
| Map Input / Output paths, open-map / inject / “compile into map” | **Disabled** — no map I/O; export files instead |
| euddraft-driven build, EDD insert, Update | **Disabled** |
| **Plugins** panel (euddraft plugin bundle UI) | **Disabled** for now — especially irrelevant when the target is TrigScript / scmJS |
| TileSet editor | **Disabled** |
| Classic TrigEdit text export | **Not implemented yet** |

**Plugins note:** Plugins stay off while the primary path is TrigScript. They may return later for **epScript** workflows, where plugins are a more natural / native fit than under TrigScript.

---

## License & attribution

Based on [armoha/EUDEditor](https://github.com/armoha/EUDEditor) (**EUD Editor 2**), originally by **LeeJungHun** (`sksljh2091`), under the same **MIT License** (see [`LICENSE`](LICENSE)). Keep the LeeJungHun copyright notice in copies and substantial portions.

EE2 Exporter keeps the DAT / Firegraft foundations and replaces the integrated build/inject path with exporters.

---

## What you need

- StarCraft (Remastered or 1.16.1) — `StarCraft.exe` for Casc/path roots
- The four classic MPQs (`Patch_rt` / `BrooDat` / `BroodWar` / `StarDat`) — place copies in this tree’s `mpqs/` when convenient (gitignored)
- **Not** required: euddraft, or any map file

---

## Windows

Normal **.NET Framework 4.8** WinForms **x86** app:

1. Open `EUD Editor.sln` in Visual Studio (or MSBuild).
2. Platform **x86**, Debug or Release.
3. Run `EE2 Exporter.exe` from `EUD Editor\bin\x86\…`.
4. Set `StarCraft.exe` once; MPQs resolve from settings, `mpqs\`, or next to StarCraft.

---

## Linux (Wine)

Repo-local prefix: `wine/` (gitignored). Wine 10+ wow64 → prefix is **`win64`**.

```bash
chmod +x setup-wine.sh build-eud-editor.sh run-eud-editor.sh
./setup-wine.sh          # once; long first run → wine-setup.log
./build-eud-editor.sh    # → wine-build.log
./run-eud-editor.sh
```

Prerequisites: `wine` (32-bit WoW64), `winetricks`, `curl`/`wget`, `unzip`.

Official .NET Framework SDK Docker images are **Windows containers** and do not run on Linux Docker — hence Wine + reference assemblies for builds here.

---

## Workflow

1. **File → New** (or Open a `.e2s` / `.e2p`). No map load/save.
2. Edit **DatEdit** / **Firegraft** as needed.
3. **Tool →** choose **TrigScript Export** or **epScript Export**, then **Export Now…** (or the toolbar **Export** button). **AutoExport** re-exports to the last folder on save.
4. Import the generated `.trig` / `.eps` into your TrigScript or epScript project.

Last export/project folders are remembered in settings and in the project file (`trigScriptExportDir`, `trigScriptExportFormat`).

---

## Lineage

Upstream: [armoha/EUDEditor](https://github.com/armoha/EUDEditor) (MIT, Copyright © 2018 LeeJungHun). Earlier “SE” UX work lived in community forks; this repo deliberately strips the build/inject/EDD/plugin path and keeps the DAT GUI + exporters.
