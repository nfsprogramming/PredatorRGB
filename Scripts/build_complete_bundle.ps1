# build_complete_bundle.ps1
$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Building Complete PredatorRGB Bundle" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$root = if ($PSScriptRoot) { Split-Path -Parent $PSScriptRoot } else { (Get-Location).Path }
$staging = Join-Path $root "build_staging"
$uiRelease = Join-Path $root "forgesense_ui\build\windows\x64\runner\Release"
$corePublish = Join-Path $root "ForgeSense.Core\publish"
$launcherDir = Join-Path $root "ForgeSenseLauncher"
$zipPath = Join-Path $launcherDir "ForgeSense_Windows_v1.zip"
$finalOutputDir = Join-Path $root "PredatorRGB_App"

# 1. Build and Publish ForgeSense.Core if needed
Write-Host "`n[1/5] Publishing ForgeSense.Core backend..." -ForegroundColor Yellow
Set-Location (Join-Path $root "ForgeSense.Core")
dotnet publish -c Release -o $corePublish --nologo

# 2. Build forgesense_ui
Write-Host "`n[2/5] Building Flutter Desktop UI (Release)..." -ForegroundColor Yellow
Set-Location (Join-Path $root "forgesense_ui")
flutter build windows --release

# Gracefully stop running app processes if any
try {
    $tcp = New-Object System.Net.Sockets.TcpClient
    $iar = $tcp.BeginConnect("127.0.0.1", 4050, $null, $null)
    if ($iar.AsyncWaitHandle.WaitOne(800)) {
        $tcp.EndConnect($iar)
        $sw = New-Object System.IO.StreamWriter($tcp.GetStream())
        $sw.AutoFlush = $true
        $sw.WriteLine("EXIT")
        Start-Sleep -Milliseconds 400
    }
    $tcp.Dispose()
} catch {}
Get-Process -Name 'ForgeSense.Core', 'PredatorRGB', 'forgesense_ui', 'ForgeSenseLauncher' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 3. Prepare Staging Directory
Write-Host "`n[3/5] Staging all frontend and backend components..." -ForegroundColor Yellow
if (-not (Test-Path $staging)) { New-Item -ItemType Directory -Path $staging | Out-Null }
Copy-Item -Recurse -Force "$uiRelease\*" $staging
Copy-Item -Recurse -Force "$corePublish\*" $staging
# Copy PredatorRGB.exe to forgesense_ui.exe for dual compatibility
Copy-Item -Force (Join-Path $staging "PredatorRGB.exe") (Join-Path $staging "forgesense_ui.exe")

# Also save an uncompressed standalone version in PredatorRGB_App
if (-not (Test-Path $finalOutputDir)) { New-Item -ItemType Directory -Path $finalOutputDir | Out-Null }
Copy-Item -Recurse -Force "$staging\*" $finalOutputDir
Write-Host "Standalone folder ready at: $finalOutputDir" -ForegroundColor Green

# 4. Create ForgeSense_Windows_v1.zip for the single-exe launcher
Write-Host "`n[4/5] Compressing bundle into embedded zip..." -ForegroundColor Yellow
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($staging, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host "Zip created: $([math]::round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green

# 5. Build ForgeSenseLauncher.exe
Write-Host "`n[5/5] Building Single-Executable Bundle (ForgeSenseLauncher)..." -ForegroundColor Yellow
Set-Location $launcherDir
$bundlePublishDir = Join-Path $root "bundle_publish"
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o $bundlePublishDir --nologo

$launcherExe = Join-Path $bundlePublishDir "ForgeSenseLauncher.exe"
$finalBundleExe = Join-Path $root "PredatorRGB_Bundle.exe"
$completeBundleExe = Join-Path $root "ForgeSense_Complete_Bundle.exe"
if (Test-Path $launcherExe) {
    Copy-Item $launcherExe $completeBundleExe -Force
    try {
        Copy-Item $launcherExe $finalBundleExe -Force
        Write-Host " Copied to $finalBundleExe" -ForegroundColor Green
    } catch {
        Write-Host " Warning: $finalBundleExe is currently open in Windows. Also copied to PredatorRGB_Bundle_v2.exe." -ForegroundColor Yellow
        Copy-Item $launcherExe (Join-Path $root "PredatorRGB_Bundle_v2.exe") -Force
    }

    # Also put launcher in standalone folder
    Copy-Item $launcherExe (Join-Path $finalOutputDir "PredatorRGB_Launcher.exe") -Force

    # 6. Build Windows Installable Setup EXE using Inno Setup
    Write-Host "`n[6/6] Building Windows Installable Setup EXE (Inno Setup)..." -ForegroundColor Yellow
    $isccCandidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    $iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

    $installerExe = Join-Path $root "PredatorRGB_Setup.exe"
    if ($iscc) {
        $issFile = Join-Path $root "PredatorRGB_Setup.iss"
        Write-Host "Compiling installer with: $iscc" -ForegroundColor Gray
        & $iscc $issFile
        if (Test-Path $installerExe) {
            Copy-Item $installerExe (Join-Path $root "PredatorRGB_Installer.exe") -Force
            Write-Host " Installer created: $([math]::round((Get-Item $installerExe).Length / 1MB, 2)) MB" -ForegroundColor Green
        }
    } else {
        Write-Host " Warning: Inno Setup (ISCC.exe) not found. Skipped installer compilation." -ForegroundColor Yellow
    }

    Write-Host "`n==========================================" -ForegroundColor Green
    Write-Host " SUCCESS: Complete build generated!" -ForegroundColor Green
    Write-Host " 1. Installable Setup EXE: $installerExe" -ForegroundColor Green
    Write-Host " 2. Single-file Launcher:  $finalBundleExe" -ForegroundColor Green
    Write-Host " 3. Standalone Folder:     $finalOutputDir" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
} else {
    Write-Error "Launcher executable not found at $launcherExe"
}
