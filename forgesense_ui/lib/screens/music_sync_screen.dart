import 'package:flutter/material.dart';
import '../services/tcp_service.dart';
import '../services/telemetry_service.dart';
import '../models/audio_features.dart';
import '../theme/app_theme.dart';

class MusicSyncScreen extends StatefulWidget {
  final TcpService tcpService;
  final TelemetryService telemetryService;

  const MusicSyncScreen({
    super.key,
    required this.tcpService,
    required this.telemetryService,
  });

  @override
  State<MusicSyncScreen> createState() => _MusicSyncScreenState();
}

class _MusicSyncScreenState extends State<MusicSyncScreen> {
  String _selectedMode = 'musicsync_beatpulse';

  void _setMode(String mode) {
    setState(() {
      _selectedMode = mode;
    });
    widget.tcpService.sendCommand('MODE:$mode');
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(32.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Music Sync', style: Theme.of(context).textTheme.headlineMedium),
          const SizedBox(height: 32),
          Expanded(
            child: Row(
              children: [
                // Left Panel: Visualizer & Controls
                Expanded(
                  flex: 2,
                  child: Column(
                    children: [
                      // Visualizer Box
                      Expanded(
                        child: Container(
                          decoration: BoxDecoration(
                            color: AppTheme.surfaceContainer,
                            borderRadius: BorderRadius.circular(16),
                            border: Border.all(color: Colors.white10),
                          ),
                          child: ListenableBuilder(
                            listenable: widget.telemetryService,
                            builder: (context, child) {
                              final features = widget.telemetryService.latestFeatures;
                              if (features == null) {
                                return const Center(child: Text('No Audio Data'));
                              }
                              return Padding(
                                padding: const EdgeInsets.all(24.0),
                                child: Column(
                                  children: [
                                    _buildBeatIndicator(features),
                                    const SizedBox(height: 24),
                                    Expanded(child: _buildSpectrum(features)),
                                    const SizedBox(height: 24),
                                    _buildEnergyBars(features),
                                  ],
                                ),
                              );
                            },
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                      // Controls
                      Container(
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: AppTheme.surfaceContainer,
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: Colors.white10),
                        ),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                          children: [
                            _buildModeButton('Beat Pulse', 'musicsync_beatpulse'),
                            _buildModeButton('Spectrum', 'musicsync_spectrum'),
                            _buildModeButton('Dynamic', 'musicsync_dynamic'),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 32),
                // Right Panel: 4-Zone View
                Expanded(
                  flex: 1,
                  child: Container(
                    decoration: BoxDecoration(
                      color: AppTheme.surfaceContainer,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: Colors.white10),
                    ),
                    child: Center(
                      child: Text('4-Zone Mapper (Coming Soon)'),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBeatIndicator(AudioFeatures f) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text('BEAT', style: TextStyle(
          color: f.isBeat ? Colors.redAccent : Colors.white24,
          fontWeight: FontWeight.bold,
          fontSize: 24,
        )),
        Text('Volume: ${(f.volume * 100).toInt()}%', style: const TextStyle(color: Colors.white70)),
      ],
    );
  }

  Widget _buildSpectrum(AudioFeatures f) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: f.spectrum.map((val) {
        return Expanded(
          child: Container(
            margin: const EdgeInsets.symmetric(horizontal: 1),
            height: val * 200,
            decoration: BoxDecoration(
              color: Color.lerp(Colors.blue, Colors.red, val),
              borderRadius: const BorderRadius.vertical(top: Radius.circular(4)),
            ),
          ),
        );
      }).toList(),
    );
  }

  Widget _buildEnergyBars(AudioFeatures f) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: [
        _buildBar('Bass', f.bass),
        _buildBar('Mid', f.mid),
        _buildBar('Treble', f.treble),
      ],
    );
  }

  Widget _buildBar(String label, double val) {
    return Column(
      children: [
        Container(
          width: 40,
          height: 100,
          decoration: BoxDecoration(
            color: Colors.black26,
            borderRadius: BorderRadius.circular(8),
          ),
          alignment: Alignment.bottomCenter,
          child: Container(
            width: 40,
            height: val * 100,
            decoration: BoxDecoration(
              color: Colors.greenAccent,
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        ),
        const SizedBox(height: 8),
        Text(label, style: const TextStyle(color: Colors.white70, fontSize: 12)),
      ],
    );
  }

  Widget _buildModeButton(String label, String mode) {
    final isSelected = _selectedMode == mode;
    return ElevatedButton(
      style: ElevatedButton.styleFrom(
        backgroundColor: isSelected ? AppTheme.primary : AppTheme.surfaceContainerHigh,
        foregroundColor: isSelected ? Colors.white : Colors.white70,
      ),
      onPressed: () => _setMode(mode),
      child: Text(label),
    );
  }
}
