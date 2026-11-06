import '../widgets/keyboard_preview.dart';

class LightingProfile {
  final String id;
  final String name;
  final String effectName;
  final LightEffect effectType;
  final String tcpCommand;

  const LightingProfile({
    required this.id,
    required this.name,
    required this.effectName,
    required this.effectType,
    required this.tcpCommand,
  });
}
