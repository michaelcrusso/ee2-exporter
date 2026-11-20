# EE2 Exporter

**EE2 Exporter** (full name: *EUD Editor 2 Exporter*) is a pared-down StarCraft DAT / Firegraft GUI, derived from [EUD Editor 2](https://github.com/armoha/EUDEditor), that **exports memory patches** for map scripts instead of driving an euddraft build.

It is **not** a drop-in replacement for EUD Editor 2 SE as an integrated map compiler. Use it to edit unit tables (and related Firegraft data) against game MPQs, then export:

| Format | File | Use |
| --- | --- | --- |
| **TrigScript** (default) | `eud-patches.trig` | `program(() => { setMemory(...); });` for scmJS / TrigScript maps |
| **epScript** | `eud-patches.eps` | `onPluginStart` + `SetMemory(...)` for euddraft users |
| Classic TrigEdit | — | Not implemented yet |

Primary motivation: Remastered UMS maps (e.g. Crusader Kings) that need map-local EUD without euddraft for DAT patches. See [`GOALS.md`](GOALS.md).

**Repo:** https://github.com/michaelcrusso/ee2-exporter

---

## License & attribution

This project is based on [armoha/EUDEditor](https://github.com/armoha/EUDEditor) (**EUD Editor 2**), originally by **LeeJungHun** (`sksljh2091`), and is distributed under the same **MIT License** (see [`LICENSE`](LICENSE)). The MIT copyright notice for LeeJungHun must be preserved in copies and substantial portions of the Software.

EE2 Exporter keeps the DAT / Firegraft editing foundations and replaces the integrated build/inject path with TrigScript / epScript exporters.

---

## What you need

- StarCraft (Remastered or 1.16.1) — `StarCraft.exe` for Casc/path roots
- The four classic MPQs (`Patch_rt` / `BrooDat` / `BroodWar` / `StarDat`) — this tree’s `mpqs/` folder is used automatically when present (gitignored; copy your own)
- **Not** required: euddraft, Input/Output map paths (optional)

---

## Windows

This is a normal **.NET Framework 4.8** WinForms **x86** app. On Windows:

1. Open `EUD Editor.sln` in Visual Studio (or build with MSBuild).
2. Platform target **x86**, Configuration Debug or Release.
3. Run `EE2 Exporter.exe` from `EUD Editor\bin\x86\…`.
4. Set `StarCraft.exe` once; MPQs resolve from settings, `mpqs\`, or next to StarCraft.

Wine is only needed on Linux. The same binary shape runs under native Windows.

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

1. **File → New** (or Open a `.e2s` / `.e2p`). Maps are optional.
2. Edit **DatEdit** / **Firegraft** as needed. (TileSet and Plugin are disabled.)
3. **Tool →** choose **TrigScript Export** or **epScript Export**, then **Export Now…** (or the toolbar **Export** button). **AutoExport** re-exports to the last folder on save.
4. Import the generated `.trig` / `.eps` into your map project.

Last export/project folders are remembered in settings and in the project file (`trigScriptExportDir`, `trigScriptExportFormat`).

---

## Lineage

Upstream lineage: [armoha/EUDEditor](https://github.com/armoha/EUDEditor) (MIT, Copyright © 2018 LeeJungHun). Earlier “SE” UX work lived in community forks; this repo deliberately **strips** the build/inject/EDD/plugin path and keeps the DAT GUI + exporters.

Agent-oriented purpose and status: **[`GOALS.md`](GOALS.md)**.
