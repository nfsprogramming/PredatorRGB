<div align="center">

<img src="Assets/logo.png" alt="PredatorRGB Logo" width="128" height="128">

# PredatorRGB

**Lightweight, High-Performance 4-Zone RGB Lighting Suite for Acer Predator Laptops**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010%20(x64)-0078D6.svg)](https://microsoft.com/windows)
[![Frontend](https://img.shields.io/badge/Frontend-Flutter%20Desktop-02569B.svg)](https://flutter.dev)
[![Backend](https://img.shields.io/badge/Backend-.NET%2010%20C%23-512BD4.svg)](https://dotnet.microsoft.com)

</div>

---

## Overview

**PredatorRGB** is a dedicated, resource-efficient RGB keyboard control suite and desktop application built specifically for Acer Predator laptops with 4-zone RGB keyboards (including the **Acer Predator Helios Neo 16 / PHN16-72** series).

Unlike heavy OEM management software, PredatorRGB focuses solely on responsive lighting control with minimal CPU and RAM usage. It pairs a modern Flutter desktop interface with a high-speed C# .NET 10 hardware daemon (`ForgeSense.Core`) communicating over local TCP socket IPC, interfacing directly with laptop firmware via Acer WMI (`root\wmi` namespace) and HID drivers.

---

## Features

- **4-Zone Independent RGB Control**: Customize colors individually for Zone 1, Zone 2, Zone 3, and Zone 4 with instant live preview.
- **Hardware & Software Effects**:
  - **Firmware EC Effects**: Native hardware modes (Wave, Breathing, Neon, Static) running directly on the embedded controller with adjustable speed and brightness.
  - **Extensive Software Catalog**: Over 1,000 curated lighting effects spanning *Wave & Motion*, *Gaming & Esports*, *Cyberpunk & Sci-Fi*, *Nature & Elements*, *Ambient & Flow*, *Anime & Fantasy*, *Reactive Touch*, and *Signature Predator* styles.
- **Audio & Music Visualizer**: Real-time WASAPI loopback capture powered by NAudio with FFT frequency analysis, synchronizing keyboard lighting pulses to music and game audio.
- **Screen Sync (Ambilight)**: Edge-sampling screen synchronizer that captures real-time display colors and projects them onto the keyboard zones.
- **Reactive Typing Engine**: Dynamic lighting feedback that triggers ripples, waves, and trails responding to live keystrokes.
- **Lighting Service Conflict Manager**: Built-in service supervisor that detects and gracefully stops background OEM services (`AcerLightingService`) to prevent hardware command collisions.
- **System Tray & Window Management**: Background execution, minimize-to-tray capability, and lightweight resource footprint.

---

## Architecture

PredatorRGB utilizes a decoupled client-server architecture:

```
┌───────────────────────────────────────────┐
│        PredatorRGB GUI (Flutter)         │
│  • Dashboard & 4-Zone Color Palette       │
│  • 1,000+ Effect Library & Music Visualizer│
│  • Service & Hardware Configuration       │
└─────────────────────┬─────────────────────┘
                      │ Local TCP Socket (127.0.0.1:4050)
┌─────────────────────▼─────────────────────┐
│       ForgeSense.Core (.NET 10 C#)        │
│  • Acer WMI Driver (root\wmi / EC Calls)  │
│  • Lighting Engine & Dynamic Render Loop  │
│  • WASAPI Audio Analyzer & Screen Sync    │
│  • AcerLightingService Manager            │
└───────────────────────────────────────────┘
```

---

## Project Structure

```
PredatorRGB/
├── Assets/                        # Application icons and branding assets
│   ├── logo.ico
│   └── logo.png
├── ForgeSense.Core/               # .NET 10 C# backend hardware control daemon
│   ├── AcerWmiDriver.cs           # Low-level Acer WMI BIOS driver (root\wmi)
│   ├── DSP/                       # Digital Signal Processing (WASAPI audio capture, FFT)
│   ├── Effects/                   # Effect algorithms (Built-in, Wave, Cyber, Audio, Nature)
│   ├── Input/                     # Keyboard hook and reactive typing engine
│   ├── LightingEngine.cs          # Master lighting pipeline and hardware queue
│   ├── Program.cs                 # Daemon entry point & TCP IPC server (port 4050)
│   ├── ScreenSyncEngine.cs        # Ambilight screen sampling engine
│   ├── TelemetryEngine.cs         # System telemetry & hardware monitors
│   └── WindowsServiceManager.cs   # Windows service controller for Acer services
├── ForgeSenseLauncher/            # Lightweight single-executable launcher
│   └── Program.cs                 # Extracts and orchestrates backend & UI processes
├── forgesense_ui/                 # Flutter desktop frontend
│   ├── lib/
│   │   ├── main.dart              # UI entry point & window management
│   │   ├── models/                # Effect catalog and lighting profile models
│   │   ├── screens/               # Dashboard, Effects, Music Sync, Profiles, Settings
│   │   ├── services/              # TCP client service (127.0.0.1:4050)
│   │   ├── theme/                 # Dark cyberpunk styling and design tokens
│   │   └── widgets/               # Interactive 4-zone keyboard preview widget
│   └── test/                      # Widget and unit tests
├── WmiTester/                     # Standalone Acer WMI testing utility
├── Scripts/
│   ├── build_complete_bundle.ps1  # Automated end-to-end build and packaging script
│   ├── generate_ui.py             # Effect generator utility
│   └── test_input.cs              # Input hook verification test
├── PredatorRGB_Setup.iss          # Inno Setup 6 installer script
├── LICENSE                        # MIT License
└── README.md
```

---

## Prerequisites

- **Operating System**: Windows 11 or Windows 10 (64-bit x64).
- **Permissions**: Administrator rights (required for Acer WMI BIOS communication and service management).
- **Supported Hardware**: Acer Predator laptops with 4-zone RGB keyboards (tested on Helios Neo 16 / PHN16-72).

### Build Tools (Developers Only)

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or .NET 8+)
- [Flutter SDK](https://docs.flutter.dev/get-started/install/windows) (3.27+ with desktop support enabled)
- [Inno Setup 6](https://jrsoftware.org/isdl.php) (optional, required for compiling `PredatorRGB_Setup.exe`)

---

## Building from Source

### Quick Build (All-in-One Automated Script)

To compile the entire suite (backend, Flutter UI, single-file bundle, and Inno Setup installer) in a single command, run:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts\build_complete_bundle.ps1
```

### Manual Build Steps

#### 1. Compile the Backend Daemon

```powershell
cd ForgeSense.Core
dotnet publish -c Release -r win-x64 --self-contained false -o publish
cd ..
```

#### 2. Compile the Flutter Desktop GUI

```powershell
cd forgesense_ui
flutter pub get
flutter build windows --release
cd ..
```

#### 3. Compile the Launcher Bundle

```powershell
cd ForgeSenseLauncher
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o ..\bundle_publish
cd ..
```

#### 4. Compile the Windows Installer (Optional)

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" PredatorRGB_Setup.iss
```

---

## Usage

1. **Launch PredatorRGB**: Run `PredatorRGB_Setup.exe` to install, or launch `PredatorRGB_Bundle.exe` directly as a standalone portable app.
2. **Grant Administrator Privileges**: The app will request UAC elevation to communicate with Acer WMI BIOS methods.
3. **Resolve Service Conflicts**: If prompted or if lighting fails to update, open **Settings & Services** in the UI and click **Stop Acer Service** to release hardware locks held by `AcerLightingService`.
4. **Choose Lighting Mode**:
   - **Static / 4-Zone**: Select custom colors for any of the 4 zones from the Dashboard.
   - **Effects**: Browse over 1,000 animations categorized by theme.
   - **Music Sync**: Enable audio visualization to pulsate to system audio.
   - **Screen Sync**: Project display colors directly onto the keyboard.

---

## Testing & Quality Assurance

- **Flutter Analyzer**:
  ```powershell
  cd forgesense_ui
  flutter analyze
  ```
- **Flutter Unit & Widget Tests**:
  ```powershell
  cd forgesense_ui
  flutter test
  ```
- **.NET Compilation Validation**:
  ```powershell
  dotnet build ForgeSense.Core\ForgeSense.Core.csproj --nologo
  dotnet build ForgeSenseLauncher\ForgeSenseLauncher.csproj --nologo
  dotnet build WmiTester\WmiTester.csproj --nologo
  ```
- **WMI Diagnostics**:
  ```powershell
  dotnet run --project WmiTester\WmiTester.csproj
  ```

---

## Known Limitations

- **Administrative Privileges**: WMI access to `AcerGamingFunction` in `root\wmi` requires Administrator permissions.
- **OEM Service Lock**: Acer's default background services can overwrite EC registers; use the built-in service manager in Settings to stop `AcerLightingService`.
- **Keyboard Layout**: Designed for 4-zone RGB keyboards; laptops with per-key RGB or single-zone backlights will only reflect 4-zone mapping blocks.

---

## Contributing

Contributions, bug reports, and hardware compatibility reports are welcome! Please open an issue or submit a pull request with details on your Acer laptop model and test results.

---

## License

This project is licensed under the [MIT License](LICENSE).
