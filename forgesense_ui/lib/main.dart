import 'dart:io';
import 'package:flutter/material.dart';
import 'theme/app_theme.dart';
import 'services/tcp_service.dart';
import 'screens/dashboard_screen.dart';
import 'screens/effects_screen.dart';
import 'screens/music_sync_screen.dart';
import 'screens/profiles_screen.dart';
import 'screens/settings_screen.dart';
import 'package:window_manager/window_manager.dart';
import 'package:system_tray/system_tray.dart';
import 'services/telemetry_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await windowManager.ensureInitialized();

  WindowOptions windowOptions = const WindowOptions(
    size: Size(1280, 800),
    center: true,
    backgroundColor: Colors.transparent,
    skipTaskbar: false,
    titleBarStyle: TitleBarStyle.normal,
    title: 'PredatorRGB',
  );
  
  await windowManager.waitUntilReadyToShow(windowOptions, () async {
    await windowManager.show();
    await windowManager.focus();
  });

  runApp(const PredatorRgbApp());
}

class PredatorRgbApp extends StatelessWidget {
  const PredatorRgbApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PredatorRGB Dashboard',
      theme: AppTheme.darkTheme,
      debugShowCheckedModeBanner: false,
      home: const MainLayout(),
    );
  }
}

class MainLayout extends StatefulWidget {
  const MainLayout({super.key});

  @override
  State<MainLayout> createState() => _MainLayoutState();
}

class _MainLayoutState extends State<MainLayout> with WindowListener {
  final TcpService _tcpService = TcpService();
  final TelemetryService _telemetryService = TelemetryService();
  final SystemTray _systemTray = SystemTray();
  final AppWindow _appWindow = AppWindow();
  int _selectedIndex = 0;
  Process? _backendProcess;

  @override
  void initState() {
    super.initState();
    windowManager.addListener(this);
    windowManager.setPreventClose(true); // Intercept close
    _initSystemTray();
    _startBackend();
    _telemetryService.startListening();
  }

  Future<void> _initSystemTray() async {
    await _systemTray.initSystemTray(
      title: "PredatorRGB",
      iconPath: Platform.isWindows ? 'assets/images/logo.ico' : 'assets/images/logo.png',
    );

    final Menu menu = Menu();
    await menu.buildFrom([
      MenuItemLabel(label: 'Open PredatorRGB', onClicked: (menuItem) => _appWindow.show()),
      MenuItemLabel(label: 'RGB Studio', onClicked: (menuItem) {
        _onItemTapped(1);
        _appWindow.show();
      }),
      MenuSeparator(),
      MenuItemLabel(label: 'Exit PredatorRGB', onClicked: (menuItem) {
        windowManager.destroy(); // Actually exit
      }),
    ]);

    await _systemTray.setContextMenu(menu);

    _systemTray.registerSystemTrayEventHandler((eventName) {
      if (eventName == kSystemTrayEventClick) {
        _appWindow.show();
      }
    });
  }

  @override
  void onWindowClose() {
    _appWindow.hide();
  }

  Future<void> _startBackend() async {
    try {
      final exeDir = File(Platform.resolvedExecutable).parent.path;
      final candidates = [
        '$exeDir\\ForgeSense.Core.exe',
        '$exeDir\\..\\ForgeSense.Core\\bin\\Release\\net10.0-windows\\ForgeSense.Core.exe',
        'e:\\PredatorRGB\\ForgeSense.Core\\bin\\Release\\net10.0-windows\\ForgeSense.Core.exe',
        'ForgeSense.Core.exe',
      ];

      String? targetPath;
      for (final p in candidates) {
        if (File(p).existsSync()) {
          targetPath = p;
          break;
        }
      }

      if (targetPath != null) {
        _backendProcess = await Process.start(
          targetPath,
          [],
          workingDirectory: File(targetPath).parent.path,
          runInShell: false,
          mode: ProcessStartMode.detached,
        );
        debugPrint('Launched backend process from $targetPath');
      } else {
        _backendProcess = await Process.start('ForgeSense.Core.exe', [], runInShell: false);
      }
    } catch (e) {
      debugPrint('Could not start backend executable. It might already be running or missing: $e');
    }
  }

  @override
  void dispose() {
    windowManager.removeListener(this);
    _tcpService.sendCommand('EXIT');
    _backendProcess?.kill();
    _tcpService.dispose();
    _telemetryService.dispose();
    super.dispose();
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    final List<Widget> pages = [
      DashboardScreen(
        tcpService: _tcpService,
        onOpenRgbStudio: () => _onItemTapped(1),
      ),
      EffectsScreen(tcpService: _tcpService),
      ProfilesScreen(tcpService: _tcpService),
      MusicSyncScreen(tcpService: _tcpService, telemetryService: _telemetryService),
      SettingsScreen(
        tcpService: _tcpService,
        onStartBackend: _startBackend,
      ),
    ];

    return Scaffold(
      body: Row(
        children: [
          // Sidebar
          Container(
            width: 280,
            decoration: BoxDecoration(
              color: AppTheme.surfaceContainer.withOpacity(0.8),
              border: Border(
                right: BorderSide(color: Colors.white.withOpacity(0.1)),
              ),
            ),
            child: Column(
              children: [
                // Brand Header
                Padding(
                  padding: const EdgeInsets.all(32.0),
                  child: Row(
                    children: [
                      Container(
                        width: 40,
                        height: 40,
                        decoration: BoxDecoration(
                          color: Colors.transparent,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Image.asset('assets/images/logo.png', fit: BoxFit.contain),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('PredatorRGB', style: Theme.of(context).textTheme.headlineSmall?.copyWith(color: AppTheme.primary)),
                          Text('4-Zone RGB Controller', style: Theme.of(context).textTheme.labelSmall),
                        ],
                      ),
                    ],
                  ),
                ),
                // Navigation Links
                Expanded(
                  child: ListView(
                    padding: const EdgeInsets.symmetric(horizontal: 8.0),
                    children: [
                      _buildNavItem(0, Icons.dashboard, 'Dashboard'),
                      _buildNavItem(1, Icons.palette, 'RGB Studio'),
                      _buildNavItem(2, Icons.account_circle, 'Profiles'),
                      _buildNavItem(3, Icons.music_note, 'Music Sync'),
                      _buildNavItem(4, Icons.settings, 'Settings'),
                    ],
                  ),
                ),
                // Status Footer
                Padding(
                  padding: const EdgeInsets.all(32.0),
                  child: ListenableBuilder(
                    listenable: _tcpService,
                    builder: (context, child) {
                      final isAcerRunning = _tcpService.acerServiceStatus == 'RUNNING';
                      return Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          if (isAcerRunning) ...[
                            InkWell(
                              onTap: () => _onItemTapped(4),
                              borderRadius: BorderRadius.circular(6),
                              child: Container(
                                margin: const EdgeInsets.only(bottom: 8),
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                                decoration: BoxDecoration(
                                  color: const Color(0xFFFFA726).withOpacity(0.15),
                                  borderRadius: BorderRadius.circular(6),
                                  border: Border.all(color: const Color(0xFFFFA726).withOpacity(0.5)),
                                ),
                                child: const Row(
                                  children: [
                                    Icon(Icons.warning_amber_rounded, color: Color(0xFFFFA726), size: 14),
                                    SizedBox(width: 6),
                                    Expanded(
                                      child: Text(
                                        'Acer Service Active',
                                        style: TextStyle(color: Color(0xFFFFCC80), fontSize: 11, fontWeight: FontWeight.bold),
                                      ),
                                    ),
                                    Icon(Icons.chevron_right, color: Color(0xFFFFA726), size: 14),
                                  ],
                                ),
                              ),
                            ),
                          ],
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: AppTheme.surfaceContainerHigh,
                              borderRadius: BorderRadius.circular(8),
                              border: Border.all(color: Colors.white.withOpacity(0.05)),
                            ),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Row(
                                  children: [
                                    const Icon(Icons.sensors, color: AppTheme.secondaryFixed, size: 16),
                                    const SizedBox(width: 8),
                                    Text('CORE ENGINE', style: Theme.of(context).textTheme.labelSmall),
                                  ],
                                ),
                                Row(
                                  children: [
                                    Container(
                                      width: 8,
                                      height: 8,
                                      decoration: BoxDecoration(
                                        color: _tcpService.isConnected ? AppTheme.secondaryFixed : AppTheme.errorContainer,
                                        shape: BoxShape.circle,
                                        boxShadow: [
                                          BoxShadow(
                                            color: _tcpService.isConnected ? AppTheme.secondaryFixed.withOpacity(0.8) : AppTheme.errorContainer.withOpacity(0.8),
                                            blurRadius: 8,
                                          ),
                                        ],
                                      ),
                                    ),
                                    const SizedBox(width: 8),
                                    Text(
                                      _tcpService.isConnected ? 'Connected' : 'Offline',
                                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                        color: _tcpService.isConnected ? AppTheme.secondaryFixed : AppTheme.errorContainer,
                                      ),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                        ],
                      );
                    }
                  ),
                ),
              ],
            ),
          ),
          // Main Content Canvas
          Expanded(
            child: Container(
              color: AppTheme.background,
              child: pages[_selectedIndex],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildNavItem(int index, IconData icon, String title) {
    final isSelected = _selectedIndex == index;
    return Padding(
      padding: const EdgeInsets.only(bottom: 4.0),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () => _onItemTapped(index),
          borderRadius: BorderRadius.circular(4),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: isSelected ? AppTheme.primaryContainer.withOpacity(0.2) : Colors.transparent,
              borderRadius: BorderRadius.circular(4),
              border: Border(
                left: BorderSide(
                  color: isSelected ? AppTheme.primary : Colors.transparent,
                  width: 4,
                ),
              ),
            ),
            child: Row(
              children: [
                Icon(
                  icon,
                  color: isSelected ? Colors.white : AppTheme.onSurfaceVariant,
                  size: 20,
                ),
                const SizedBox(width: 12),
                Text(
                  title,
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: isSelected ? Colors.white : AppTheme.onSurfaceVariant,
                    fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
