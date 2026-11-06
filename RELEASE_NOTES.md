# PredatorRGB v1.0.0

**PredatorRGB v1.0.0** is the initial public release of the lightweight, high-performance 4-zone RGB keyboard control suite built specifically for Acer Predator laptops (tested on Acer Predator Helios Neo 16 / PHN16-72).

---

## Highlights & Features

- **Direct Acer WMI Integration**: High-speed, native BIOS communication via `root\wmi` (`AcerGamingFunction`) without the heavy memory and CPU footprint of OEM software.
- **Independent 4-Zone Control**: Real-time per-zone color customization for Zone 1, Zone 2, Zone 3, and Zone 4.
- **Hardware & Software Lighting Modes**:
  - Embedded Controller (EC) hardware effects: *Wave*, *Breathing*, *Neon*, and *Static* with adjustable speed and brightness.
  - Over 1,000 curated software lighting animations across 9 distinct categories (*Wave & Motion*, *Gaming & Esports*, *Cyberpunk & Sci-Fi*, *Nature & Elements*, *Ambient & Flow*, *Anime & Fantasy*, *Reactive Touch*, *Signature Predator*, and *Standard*).
- **Audio & Music Synchronization**: Real-time WASAPI loopback audio capture with FFT frequency analysis, synchronizing keyboard illumination to system music and game sounds.
- **Screen Ambilight**: Edge-sampling display capture mapping real-time screen colors directly to the 4 keyboard zones.
- **Reactive Keystroke Lighting**: Low-latency keyboard hooks creating ripple, wave, and trail lighting upon typing.
- **AcerLightingService Manager**: Built-in service controller to detect and stop conflicting OEM background services that block hardware control.
- **Modern Flutter Desktop UI**: Dark cyberpunk aesthetics, live 4-zone keyboard preview, and system tray integration.

---

## Distribution Assets

| Asset Name | Description | Size |
| :--- | :--- | :--- |
| **`PredatorRGB_Setup_v1.0.0.exe`** | Windows Inno Setup installer with desktop shortcut and optional auto-start | ~30 MB |
| **`PredatorRGB_Bundle_v1.0.0.exe`** | Portable single-file executable (self-contained, no installation needed) | ~18.5 MB |
| **`PredatorRGB_Standalone_v1.0.0.zip`** | Portable zipped application directory containing the full suite | ~30 MB |
| **`checksums.txt`** | SHA-256 verification hashes for all release artifacts | < 1 KB |

---

## Installation & Usage

1. **Installer**: Run `PredatorRGB_Setup_v1.0.0.exe` and follow the setup wizard.
2. **Portable**: Run `PredatorRGB_Bundle_v1.0.0.exe` directly from any folder.
3. **Privileges**: Launch with **Administrator privileges** (UAC prompt) to enable Acer WMI BIOS access.
4. **First Run**: If keyboard lighting does not update immediately, navigate to **Settings & Services** and click **Stop Acer Service** to release hardware locks held by `AcerLightingService`.

---

## Checksums (SHA-256)

```
47094341B17F1BE65C5389BB7B82C7E00B19015DA222F810E60C27544F7D804D  PredatorRGB_Bundle_v1.0.0.exe
592321B1ECCDBE9010DE0F51B13AAD51A087A5DFE8FB47412FB98B86D5DCA125  PredatorRGB_Setup_v1.0.0.exe
597898227DE575979E3A1E8F355BC1B0F133D4A5873AADEA83FA5A8B4B29FEBB  PredatorRGB_Standalone_v1.0.0.zip
```

---

## Requirements & Known Issues

- **OS**: Windows 11 / 10 64-bit (x64).
- **Admin Elevation**: Required for WMI namespace `root\wmi` hardware access.
- **OEM Service Lock**: Acer's default background services (`AcerLightingService`) can overwrite EC registers; use the built-in service manager in Settings to stop it.

