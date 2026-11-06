import json

effects = {
    'Standard': ['Static', 'DualColor', 'TripleColor', 'QuadColor', 'Gradient', 'ColorCycle', 'RandomPalette', 'Breathing', 'SmoothFade', 'Pulse'],
    'Advanced': ['SpectrumFlow', 'ColorWave', 'Plasma', 'Aurora', 'Nebula', 'Liquid', 'EnergyFlow', 'Prism', 'Vortex', 'ColorDrift'],
    'Wave': ['RainbowWave', 'ReverseRainbow', 'ColorMorph', 'DualWave', 'OppositeWave', 'CenterOut', 'OutsideIn', 'ZoneChase', 'ZoneScanner', 'RgbRadar'],
    'Nature': ['EnergyRipple', 'Fire', 'Lava', 'Water', 'Ocean', 'LightningStorm', 'AuroraSky', 'Sunset', 'MeteorShower', 'Galaxy'],
    'Cinematic': ['Ice', 'Thunder', 'ElectricStorm', 'NeonFlux', 'Synthwave', 'Matrix', 'DigitalGlitch', 'EnergyCore', 'PowerSurge', 'CyberGrid'],
    'Gaming': ['Rage', 'CriticalHit', 'DamageFlash', 'HealthPulse', 'Combo', 'LevelUp', 'Victory', 'Defeat', 'Respawn', 'GameOver'],
    'Sci-Fi': ['Hologram', 'CyberPulse', 'DigitalRain', 'QuantumFlow', 'Singularity', 'BlackHole', 'Wormhole', 'TimeWarp', 'HolographicSpectrum', 'CyberPulseWave'],
    'Reactive': ['KeyReactive', 'ZoneReactive', 'RippleReactive', 'WaveReactive', 'KeyTrail', 'ReactiveBurst', 'ReactiveSpectrum', 'ReactivePulse', 'ReactiveShockwave', 'ReactivePlasma'],
    'Audio': ['AudioPulse', 'BeatFlash', 'BassReactor', 'SpectrumWave', 'AudioVortex', 'BassTunnel', 'BeatRipple', 'FrequencyFlow', 'MusicAurora', 'AudioPlasma'],
    'Signature': ['Firestorm', 'OceanPulse', 'AuroraBorealis', 'CosmicDust', 'SolarFlare', 'VolcanicCore', 'GhostMode', 'CyberSamurai', 'PredatorVision', 'UltimateFusion']
}

icons = {
    'Standard': 'Icons.lightbulb_outline',
    'Advanced': 'Icons.auto_awesome',
    'Wave': 'Icons.waves',
    'Nature': 'Icons.nature',
    'Cinematic': 'Icons.movie',
    'Gaming': 'Icons.sports_esports',
    'Sci-Fi': 'Icons.science',
    'Reactive': 'Icons.touch_app',
    'Audio': 'Icons.music_note',
    'Signature': 'Icons.stars'
}

dart_code = '''import 'package:flutter/material.dart';
import '../services/tcp_service.dart';
import '../theme/app_theme.dart';

class EffectsScreen extends StatelessWidget {
  final TcpService tcpService;

  const EffectsScreen({super.key, required this.tcpService});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(32.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Lighting Effects', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontSize: 32)),
          const SizedBox(height: 8),
          Text('Choose an animation for your keyboard.', style: Theme.of(context).textTheme.headlineSmall?.copyWith(color: AppTheme.onSurfaceVariant)),
          const SizedBox(height: 32),
          Expanded(
            child: ListView(
              children: [
'''

for category, items in effects.items():
    dart_code += f'''                Text('{category}', style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold, color: AppTheme.primary)),
                const SizedBox(height: 16),
                GridView.count(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisCount: 5,
                  crossAxisSpacing: 12,
                  mainAxisSpacing: 12,
                  childAspectRatio: 1.5,
                  children: [
'''
    icon = icons[category]
    for item in items:
        # Convert CamelCase to Spaced String
        import re
        name = re.sub(r'(?<!^)(?=[A-Z])', ' ', item)
        mode = 'MODE:' + item.upper()
        dart_code += f"                    _buildEffectCard(context, '{name}', {icon}, '{mode}'),\n"
    dart_code += '''                  ],
                ),
                const SizedBox(height: 32),
'''

dart_code += '''              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildEffectCard(BuildContext context, String name, IconData icon, String command) {
    return InkWell(
      onTap: () => tcpService.sendCommand(command),
      borderRadius: BorderRadius.circular(12),
      child: Container(
        decoration: BoxDecoration(
          color: AppTheme.surfaceContainerHigh.withOpacity(0.8),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: Colors.white.withOpacity(0.1)),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 32, color: AppTheme.primary),
            const SizedBox(height: 8),
            Text(name, style: Theme.of(context).textTheme.bodyMedium, textAlign: TextAlign.center),
          ],
        ),
      ),
    );
  }
}
'''
with open('e:/PredatorRGB/forgesense_ui/lib/screens/effects_screen.dart', 'w') as f:
    f.write(dart_code)
print('Done!')
