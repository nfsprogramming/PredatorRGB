import 'package:flutter/material.dart';
import '../services/tcp_service.dart';
import '../theme/app_theme.dart';

class SettingsScreen extends StatefulWidget {
  final TcpService tcpService;
  final VoidCallback onStartBackend;

  const SettingsScreen({
    super.key,
    required this.tcpService,
    required this.onStartBackend,
  });

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  bool _startWithWindows = false;
  bool _minimizeToTray = true;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: widget.tcpService,
      builder: (context, _) {
        final acerStatus = widget.tcpService.acerServiceStatus;
        final isAcerRunning = acerStatus == 'RUNNING';
        final isBackendRunning = widget.tcpService.isConnected;
        final isProcessing = widget.tcpService.isProcessingService;

        return SingleChildScrollView(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header
              Text(
                'Settings & Services',
                style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontSize: 32,
                      fontWeight: FontWeight.bold,
                    ),
              ),
              const SizedBox(height: 8),
              Text(
                'Manage keyboard lighting services, hardware drivers, and background processes.',
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      color: AppTheme.onSurfaceVariant,
                      fontSize: 16,
                    ),
              ),
              const SizedBox(height: 32),

              // ==========================================
              // 1. Acer Lighting Services Control Card
              // ==========================================
              _buildSectionTitle(context, 'LIGHTING SERVICE CONTROLS', Icons.tune),
              const SizedBox(height: 12),
              _buildAcerServiceCard(context, isAcerRunning, acerStatus, isProcessing),
              const SizedBox(height: 24),

              // ==========================================
              // 2. ForgeSense Backend Engine Card
              // ==========================================
              _buildSectionTitle(context, 'BACKEND ENGINE STATUS', Icons.memory),
              const SizedBox(height: 12),
              _buildBackendCard(context, isBackendRunning),
              const SizedBox(height: 24),

              // ==========================================
              // 3. Hardware Diagnostics Card
              // ==========================================
              _buildSectionTitle(context, 'HARDWARE DIAGNOSTICS', Icons.developer_board),
              const SizedBox(height: 12),
              _buildHardwareCard(context),
              const SizedBox(height: 24),

              // ==========================================
              // 4. General App Preferences
              // ==========================================
              _buildSectionTitle(context, 'PREFERENCES', Icons.settings),
              const SizedBox(height: 12),
              _buildPreferencesCard(context),
            ],
          ),
        );
      },
    );
  }

  Widget _buildSectionTitle(BuildContext context, String title, IconData icon) {
    return Row(
      children: [
        Icon(icon, size: 18, color: AppTheme.primary),
        const SizedBox(width: 8),
        Text(
          title,
          style: Theme.of(context).textTheme.labelSmall?.copyWith(
                color: AppTheme.primary,
                letterSpacing: 1.5,
                fontWeight: FontWeight.bold,
              ),
        ),
      ],
    );
  }

  Widget _buildAcerServiceCard(
    BuildContext context,
    bool isRunning,
    String status,
    bool isProcessing,
  ) {
    Color statusColor = isRunning ? const Color(0xFFFFA726) : const Color(0xFF00E676);
    String statusBadgeText = isRunning ? 'RUNNING (CONFLICTING)' : 'STOPPED (EXCLUSIVE CONTROL)';
    if (status == 'NOT_INSTALLED') {
      statusColor = AppTheme.onSurfaceVariant;
      statusBadgeText = 'NOT INSTALLED';
    }

    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppTheme.surfaceContainerHigh.withOpacity(0.9),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isRunning ? const Color(0xFFFFA726).withOpacity(0.3) : Colors.white.withOpacity(0.1),
          width: isRunning ? 1.5 : 1.0,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: isRunning
                          ? const Color(0xFFFFA726).withOpacity(0.15)
                          : const Color(0xFF00E676).withOpacity(0.15),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(
                      Icons.settings_system_daydream,
                      color: statusColor,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Acer Lighting Service (AcerLightingService)',
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'OEM Windows Background Service for PredatorSense',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                              color: AppTheme.onSurfaceVariant,
                            ),
                      ),
                    ],
                  ),
                ],
              ),
              // Status Badge
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: statusColor.withOpacity(0.15),
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: statusColor.withOpacity(0.5)),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: BoxDecoration(
                        color: statusColor,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      statusBadgeText,
                      style: Theme.of(context).textTheme.labelSmall?.copyWith(
                            color: statusColor,
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          const Divider(color: Colors.white10),
          const SizedBox(height: 12),
          Text(
            isRunning
                ? '⚠️ Acer Lighting Service is currently running. This service constantly resets your keyboard back to PredatorSense defaults and prevents custom RGB effects. Stop this service to allow PredatorRGB uninterrupted control.'
                : '✓ Acer Lighting Service is stopped. PredatorRGB has full, direct control over the 4-zone lighting without interference.',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: isRunning ? const Color(0xFFFFCC80) : const Color(0xFFB9F6CA),
                  height: 1.4,
                ),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              // Stop Service Button
              ElevatedButton.icon(
                onPressed: isProcessing || !isRunning
                    ? null
                    : () async {
                        final ok = await widget.tcpService.stopAcerLightingService();
                        if (context.mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(ok
                                  ? 'Acer Lighting Service stopped successfully!'
                                  : 'Failed to stop Acer Lighting Service. Ensure you are running as Administrator.'),
                              backgroundColor: ok ? AppTheme.primary : AppTheme.errorContainer,
                            ),
                          );
                        }
                      },
                icon: isProcessing
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                      )
                    : const Icon(Icons.stop_circle_outlined),
                label: const Text('Stop Service (Recommended)'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFFD32F2F),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
              ),
              const SizedBox(width: 12),
              // Start Service Button
              OutlinedButton.icon(
                onPressed: isProcessing || isRunning
                    ? null
                    : () async {
                        final ok = await widget.tcpService.startAcerLightingService();
                        if (context.mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(ok
                                  ? 'Acer Lighting Service started.'
                                  : 'Failed to start Acer Lighting Service.'),
                            ),
                          );
                        }
                      },
                icon: const Icon(Icons.play_circle_outline),
                label: const Text('Restore / Start Service'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppTheme.onSurfaceVariant,
                  side: BorderSide(color: Colors.white.withOpacity(0.2)),
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
              ),
              const SizedBox(width: 12),
              // Refresh Button
              IconButton(
                onPressed: isProcessing ? null : () => widget.tcpService.refreshAllStatuses(),
                tooltip: 'Check Status Now',
                icon: const Icon(Icons.refresh, color: AppTheme.primary),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildBackendCard(BuildContext context, bool isRunning) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppTheme.surfaceContainerHigh.withOpacity(0.8),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.white.withOpacity(0.1)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: isRunning
                          ? AppTheme.primary.withOpacity(0.15)
                          : AppTheme.errorContainer.withOpacity(0.15),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(
                      Icons.terminal,
                      color: isRunning ? AppTheme.primary : AppTheme.errorContainer,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'ForgeSense Core Engine (ForgeSense.Core.exe)',
                        style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Local IPC Hardware Lighting Daemon (TCP Port 4050)',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                              color: AppTheme.onSurfaceVariant,
                            ),
                      ),
                    ],
                  ),
                ],
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: (isRunning ? AppTheme.primary : AppTheme.errorContainer).withOpacity(0.15),
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(
                    color: (isRunning ? AppTheme.primary : AppTheme.errorContainer).withOpacity(0.5),
                  ),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: BoxDecoration(
                        color: isRunning ? AppTheme.primary : AppTheme.errorContainer,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      isRunning ? 'RUNNING & CONNECTED' : 'OFFLINE',
                      style: Theme.of(context).textTheme.labelSmall?.copyWith(
                            color: isRunning ? AppTheme.primary : AppTheme.errorContainer,
                            fontWeight: FontWeight.bold,
                          ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          const Divider(color: Colors.white10),
          const SizedBox(height: 12),
          Text(
            'The core engine generates RGB effects at 60 FPS and communicates with the hardware keyboard controller. It runs automatically in the background when PredatorRGB is launched.',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: AppTheme.onSurfaceVariant,
                ),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              if (!isRunning)
                ElevatedButton.icon(
                  onPressed: () {
                    widget.onStartBackend();
                    widget.tcpService.refreshAllStatuses();
                  },
                  icon: const Icon(Icons.rocket_launch),
                  label: const Text('Start Backend Engine'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primary,
                    foregroundColor: AppTheme.background,
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  ),
                ),
              if (isRunning) ...[
                OutlinedButton.icon(
                  onPressed: () async {
                    await widget.tcpService.stopBackendEngine();
                    await Future.delayed(const Duration(milliseconds: 500));
                    widget.onStartBackend();
                    await Future.delayed(const Duration(seconds: 1));
                    widget.tcpService.refreshAllStatuses();
                  },
                  icon: const Icon(Icons.replay),
                  label: const Text('Restart Backend'),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppTheme.primary,
                    side: BorderSide(color: AppTheme.primary.withOpacity(0.5)),
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  ),
                ),
                const SizedBox(width: 12),
                OutlinedButton.icon(
                  onPressed: () => widget.tcpService.stopBackendEngine(),
                  icon: const Icon(Icons.stop),
                  label: const Text('Stop Backend'),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppTheme.errorContainer,
                    side: BorderSide(color: AppTheme.errorContainer.withOpacity(0.5)),
                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  ),
                ),
              ],
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildHardwareCard(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppTheme.surfaceContainerHigh.withOpacity(0.8),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.white.withOpacity(0.1)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildInfoRow('Device Target', 'Acer Predator Helios Neo 16 (PHN16-72)'),
          const Divider(color: Colors.white10, height: 24),
          _buildInfoRow('Active Controller', widget.tcpService.hardwareStatus),
          const Divider(color: Colors.white10, height: 24),
          _buildInfoRow('WMI Interface', 'root\\wmi -> AcerGamingFunction (4-Zone RGB Backlight)'),
          const Divider(color: Colors.white10, height: 24),
          _buildInfoRow('Communication Bus', 'Direct Windows Management Instrumentation (WMI) / EC'),
          const SizedBox(height: 16),
          ElevatedButton.icon(
            onPressed: () {
              // Test flash: Send White color command to verify hardware responsiveness
              widget.tcpService.sendCommand('COLOR:255,255,255:999');
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Sent hardware test pulse (All Zones: White).'),
                  duration: Duration(seconds: 2),
                ),
              );
            },
            icon: const Icon(Icons.flash_on),
            label: const Text('Test Hardware Pulse (White Flash)'),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.surfaceContainer,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPreferencesCard(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppTheme.surfaceContainerHigh.withOpacity(0.8),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.white.withOpacity(0.1)),
      ),
      child: Column(
        children: [
          SwitchListTile(
            title: const Text('Minimize to System Tray on Close'),
            subtitle: const Text('Keeps RGB lighting effects active in background when window is closed'),
            value: _minimizeToTray,
            activeColor: AppTheme.primary,
            onChanged: (val) {
              setState(() => _minimizeToTray = val);
            },
          ),
          const Divider(color: Colors.white10),
          SwitchListTile(
            title: const Text('Start with Windows'),
            subtitle: const Text('Automatically launch PredatorRGB when you log in'),
            value: _startWithWindows,
            activeColor: AppTheme.primary,
            onChanged: (val) {
              setState(() => _startWithWindows = val);
            },
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: const TextStyle(color: AppTheme.onSurfaceVariant, fontSize: 14),
        ),
        Text(
          value,
          style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 14),
        ),
      ],
    );
  }
}
