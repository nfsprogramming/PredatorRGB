import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'dart:math' as math;

enum LightEffect {
  static, wave, breathing, heartbeat, fire, aura, starlight, matrix, meteor, ripple, reactive, reactiveVariant
}

typedef ZoneColorsCallback = void Function(Color z0, Color z1, Color z2, Color z3);

class KeyDef {
  final String label;
  final double w;
  KeyDef(this.label, [this.w = 1.0]);
}

Color getRainbowColor(double t) {
  const double p1 = 2.0943951; // 2 * pi / 3 (120 deg)
  const double p2 = 4.1887902; // 4 * pi / 3 (240 deg)
  int r = ((math.sin(t) + 1.0) * 127.5).round().clamp(0, 255);
  int g = ((math.sin(t + p1) + 1.0) * 127.5).round().clamp(0, 255);
  int b = ((math.sin(t + p2) + 1.0) * 127.5).round().clamp(0, 255);
  return Color.fromARGB(255, r, g, b);
}

Color calculateZoneLightById(double zone, double t, String effectId, Color baseColor) {
  final normId = effectId.toLowerCase().replaceAll('-', '_').replaceAll(' ', '');

  // 1. WAVE & MOTION
  if (normId.contains('reverse_rainbow')) {
    return getRainbowColor(t * 2.4 - zone * 1.5707963);
  } else if (normId.contains('wave') || normId.contains('radar') || normId.contains('scanner') || normId.contains('chase') || normId.contains('prism') || normId.contains('vortex')) {
    double speed = normId.contains('scanner') || normId.contains('chase') ? 3.5 : 2.4;
    return getRainbowColor(t * speed + zone * 1.5707963);
  }

  // 2. BREATHING & AMBIENT
  if (normId.contains('breath') || normId.contains('fade') || normId.contains('aurora') || normId.contains('nebula') || normId.contains('liquid') || normId.contains('drift') || normId.contains('aura')) {
    final double breath = (math.exp(math.sin(t * 1.8 + zone * 0.3)) - 0.3678794) * 0.425459;
    final double opacity = 0.08 + 0.92 * breath.clamp(0.0, 1.0);
    if (normId.contains('nebula')) {
      return Color.lerp(const Color(0xFF9C27B0), const Color(0xFF00E5FF), breath.clamp(0.0, 1.0))!.withValues(alpha: opacity);
    } else if (normId.contains('aurora')) {
      double hue = ((math.sin(t * 0.6 + zone * 0.4) * 0.5 + 0.5) * 140.0 + 130.0) % 360.0;
      return HSVColor.fromAHSV(opacity, hue, 0.9, 1.0).toColor();
    }
    return baseColor.withValues(alpha: opacity);
  }

  // 3. FIRE & LAVA & SOLAR
  if (normId.contains('fire') || normId.contains('lava') || normId.contains('volcanic') || normId.contains('solar') || normId.contains('rage')) {
    double f1 = math.sin(t * 3.6 + zone * 1.7);
    double f2 = math.sin(t * 7.2 - zone * 2.3);
    double f3 = math.cos(t * 11.4 + zone * 3.1);
    double flame = (((f1 + 0.6 * f2 + 0.35 * f3) / 1.95 + 1.0) / 2.0).clamp(0.0, 1.0);
    double hue = 8.0 + flame * 34.0;
    double val = 0.42 + flame * 0.58;
    return HSVColor.fromAHSV(1.0, hue, 1.0, val.clamp(0.0, 1.0)).toColor();
  }

  // 4. WATER & OCEAN & CRYO
  if (normId.contains('water') || normId.contains('ocean') || normId.contains('ice') || normId.contains('ripple')) {
    double centerDist = (zone - 1.5).abs();
    double wave = math.sin(t * 4.0 - centerDist * 2.2);
    double ripple = (wave * 0.5 + 0.5);
    int r = (30 * ripple).toInt().clamp(0, 255);
    int g = (140 + 100 * ripple).toInt().clamp(0, 255);
    int b = 255;
    return Color.fromARGB(255, r, g, b);
  }

  // 5. MATRIX & CYBER & NEON
  if (normId.contains('matrix') || normId.contains('digital_rain')) {
    double head = (t * 1.8 - zone * 0.25) % 1.0;
    if (head < 0) head += 1.0;
    double trail = math.pow(1.0 - head, 2.2).toDouble();
    double intensity = (0.05 + trail * 0.95).clamp(0.0, 1.0);
    return Color.fromARGB(255, (25 * intensity).toInt().clamp(0, 255), (255 * intensity).toInt().clamp(0, 255), (45 * intensity).toInt().clamp(0, 255));
  } else if (normId.contains('synthwave')) {
    double phase = (t * 1.5 + zone * 0.5) % 1.0;
    return Color.lerp(const Color(0xFFFF007F), const Color(0xFF00F0FF), phase)!;
  } else if (normId.contains('neon')) {
    double phase = (t * 2.0 + zone * 0.7) % 1.0;
    return Color.lerp(const Color(0xFFFF00E5), const Color(0xFF00E5FF), phase)!;
  }

  // 6. METEOR & COSMIC & GALAXY
  if (normId.contains('meteor')) {
    double mPos = (t * 2.0 - zone * 0.3) % 1.0;
    if (mPos < 0) mPos += 1.0;
    double tail = math.pow(1.0 - mPos, 2.8).toDouble();
    int r = (80 * tail).toInt().clamp(0, 255);
    int g = (210 * tail).toInt().clamp(0, 255);
    int b = (255 * (0.08 + 0.92 * tail)).toInt().clamp(0, 255);
    return Color.fromARGB(255, r, g, b);
  } else if (normId.contains('star') || normId.contains('galaxy') || normId.contains('cosmic')) {
    double starPhase = t * 1.8 + zone * 2.1;
    double twinkle = math.pow((math.sin(starPhase) * 0.5 + 0.5), 2.5).toDouble();
    double intensity = 0.15 + 0.85 * twinkle;
    return Color.fromARGB(
      255,
      (baseColor.r * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.g * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.b * 255.0 * intensity).round().clamp(0, 255),
    );
  }

  // 7. HEARTBEAT & PULSE
  if (normId.contains('pulse') || normId.contains('heartbeat')) {
    double cycle = (t * 1.1) % 1.0;
    if (cycle < 0) cycle += 1.0;
    double pulse1 = math.exp(-cycle * 16.0) * 0.95;
    double pulse2 = (cycle > 0.22) ? math.exp(-(cycle - 0.22) * 14.0) * 0.75 : 0.0;
    double intensity = (0.10 + pulse1 + pulse2).clamp(0.0, 1.0);
    return Color.fromARGB(
      255,
      (baseColor.r * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.g * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.b * 255.0 * intensity).round().clamp(0, 255),
    );
  }

  // 8. GAMING & COMBAT
  if (normId.contains('critical') || normId.contains('victory') || normId.contains('level_up')) {
    double vPulse = (math.sin(t * 3.0 + zone * 0.5) * 0.5 + 0.5);
    return Color.lerp(const Color(0xFFFFD700), const Color(0xFFFF6D00), vPulse)!;
  } else if (normId.contains('damage')) {
    double dStrobe = ((math.sin(t * 8.0) * 0.5 + 0.5) > 0.5) ? 1.0 : 0.1;
    return Color.fromARGB(255, (255 * dStrobe).toInt(), 0, 0);
  }

  // 9. AUDIO & SPECTRUM
  if (normId.contains('audio') || normId.contains('beat') || normId.contains('bass')) {
    double beat = (math.sin(t * 4.0 + zone * 1.2) * 0.5 + 0.5);
    return Color.lerp(const Color(0xFF00E5FF), const Color(0xFFFF007F), beat)!;
  }

  // 10. REACTIVE
  if (normId.contains('reactive')) {
    double rPhase = (t * 1.4 + zone * 0.7) % 1.0;
    if (rPhase < 0) rPhase += 1.0;
    double decay = math.exp(-rPhase * 4.5);
    double intensity = (0.08 + 0.92 * decay).clamp(0.0, 1.0);
    return Color.fromARGB(
      255,
      (baseColor.r * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.g * 255.0 * intensity).round().clamp(0, 255),
      (baseColor.b * 255.0 * intensity).round().clamp(0, 255),
    );
  }

  // Default: smooth rainbow wave
  return getRainbowColor(t * 2.2 + zone * 1.5707963);
}

Color calculateZoneLight(double zone, double t, LightEffect effect, Color baseColor) {
  switch (effect) {
    case LightEffect.static:
      return baseColor;
    case LightEffect.wave:
      return calculateZoneLightById(zone, t, 'rainbow_wave', baseColor);
    case LightEffect.breathing:
      return calculateZoneLightById(zone, t, 'breathing', baseColor);
    case LightEffect.fire:
      return calculateZoneLightById(zone, t, 'fire', baseColor);
    case LightEffect.heartbeat:
      return calculateZoneLightById(zone, t, 'heartbeat', baseColor);
    case LightEffect.aura:
      return calculateZoneLightById(zone, t, 'aurora', baseColor);
    case LightEffect.starlight:
      return calculateZoneLightById(zone, t, 'starlight', baseColor);
    case LightEffect.matrix:
      return calculateZoneLightById(zone, t, 'matrix', baseColor);
    case LightEffect.meteor:
      return calculateZoneLightById(zone, t, 'meteor_shower', baseColor);
    case LightEffect.ripple:
      return calculateZoneLightById(zone, t, 'energy_ripple', baseColor);
    case LightEffect.reactive:
    case LightEffect.reactiveVariant:
      return calculateZoneLightById(zone, t, 'reactive', baseColor);
  }
}

class KeyboardPreview extends StatefulWidget {
  final LightEffect effect;
  final String? effectId;
  final Color baseColor;
  final double brightness;
  final double speed;
  final ZoneColorsCallback? onZoneColorsChanged;

  const KeyboardPreview({
    super.key,
    this.effect = LightEffect.wave,
    this.effectId,
    this.baseColor = const Color(0xFF0078d4),
    this.brightness = 1.0,
    this.speed = 1.0,
    this.onZoneColorsChanged,
  });

  @override
  State<KeyboardPreview> createState() => _KeyboardPreviewState();
}

class _KeyboardPreviewState extends State<KeyboardPreview> with SingleTickerProviderStateMixin {
  late Ticker _ticker;
  final ValueNotifier<double> _timeNotifier = ValueNotifier<double>(0.0);
  double _time = 0.0;
  int _lastElapsed = 0;
  int _lastStreamMs = 0;

  @override
  void initState() {
    super.initState();
    _ticker = createTicker((elapsed) {
      if (_lastElapsed == 0) _lastElapsed = elapsed.inMilliseconds;
      final double delta = ((elapsed.inMilliseconds - _lastElapsed) / 1000.0).clamp(0.0, 0.05);
      _lastElapsed = elapsed.inMilliseconds;

      _time += delta * widget.speed;
      _timeNotifier.value = _time;

      // Stream to physical hardware at 55ms (~18 FPS)
      // This is the optimal frequency for ACPI Embedded Controllers to execute with zero lag or bus contention
      if (elapsed.inMilliseconds - _lastStreamMs >= 55) {
        _lastStreamMs = elapsed.inMilliseconds;
        _dispatchZoneColors();
      }
    });

    if (widget.effect != LightEffect.static) {
      _ticker.start();
    }

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _dispatchZoneColors();
    });
  }

  Color _applyBrightness(Color c) {
    final double b = (widget.brightness * c.a).clamp(0.0, 1.0);
    return Color.fromARGB(
      255,
      (c.r * 255.0 * b).round().clamp(0, 255),
      (c.g * 255.0 * b).round().clamp(0, 255),
      (c.b * 255.0 * b).round().clamp(0, 255),
    );
  }

  void _dispatchZoneColors() {
    if (widget.onZoneColorsChanged == null) return;
    final z0 = _applyBrightness(_computeZone(0.0));
    final z1 = _applyBrightness(_computeZone(1.0));
    final z2 = _applyBrightness(_computeZone(2.0));
    final z3 = _applyBrightness(_computeZone(3.0));
    widget.onZoneColorsChanged!(z0, z1, z2, z3);
  }

  Color _computeZone(double zone) {
    if (widget.effectId != null && widget.effectId!.isNotEmpty) {
      return calculateZoneLightById(zone, _time, widget.effectId!, widget.baseColor);
    }
    return calculateZoneLight(zone, _time, widget.effect, widget.baseColor);
  }

  @override
  void didUpdateWidget(KeyboardPreview oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.effect != oldWidget.effect || widget.effectId != oldWidget.effectId) {
      if (widget.effect == LightEffect.static && (widget.effectId == null || widget.effectId == 'static')) {
        _ticker.stop();
      } else {
        if (!_ticker.isTicking) {
          _lastElapsed = 0;
          _ticker.start();
        }
      }
    }
    _dispatchZoneColors();
  }

  @override
  void dispose() {
    _ticker.dispose();
    _timeNotifier.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
      size: const Size(double.infinity, 260),
      painter: _KeyboardPainter(
        timeNotifier: _timeNotifier,
        effect: widget.effect,
        effectId: widget.effectId,
        baseColor: widget.baseColor,
        brightness: widget.brightness,
      ),
    );
  }
}

class _PrecomputedKey {
  final RRect keyRRect;
  final RRect innerRRect;
  final int zoneIndex;
  final TextPainter? textPainter;
  final Offset textOffset;

  _PrecomputedKey({
    required this.keyRRect,
    required this.innerRRect,
    required this.zoneIndex,
    this.textPainter,
    required this.textOffset,
  });
}

class _KeyboardPainter extends CustomPainter {
  final ValueNotifier<double> timeNotifier;
  final LightEffect effect;
  final String? effectId;
  final Color baseColor;
  final double brightness;

  static Size? _cachedSize;
  static List<_PrecomputedKey>? _cachedKeys;
  static RRect? _cachedBaseRRect;
  static RRect? _cachedWellRRect;

  static final Paint _basePaint = Paint()..color = const Color(0xFF131313);
  static final Paint _wellPaint = Paint()..color = const Color(0xFF080808);
  static final Paint _keyBasePaint = Paint()
    ..color = const Color(0xFF202020)
    ..style = PaintingStyle.fill;
  static final Paint _glowPaint = Paint()
    ..style = PaintingStyle.stroke
    ..strokeWidth = 1.5;
  static final Paint _innerGlowPaint = Paint()
    ..style = PaintingStyle.fill;

  static final List<List<KeyDef>> layout = [
    [KeyDef('Esc'), KeyDef('F1'), KeyDef('F2'), KeyDef('F3'), KeyDef('F4'), KeyDef('F5'), KeyDef('F6'), KeyDef('F7'), KeyDef('F8'), KeyDef('F9'), KeyDef('F10'), KeyDef('F11'), KeyDef('F12'), KeyDef('PrtSc'), KeyDef('Ins'), KeyDef('Del'), KeyDef('P'), KeyDef('N'), KeyDef('/'), KeyDef('*'), KeyDef('-')],
    [KeyDef('~'), KeyDef('1'), KeyDef('2'), KeyDef('3'), KeyDef('4'), KeyDef('5'), KeyDef('6'), KeyDef('7'), KeyDef('8'), KeyDef('9'), KeyDef('0'), KeyDef('-'), KeyDef('='), KeyDef('Bksp', 3.0), KeyDef('Num'), KeyDef('7'), KeyDef('8'), KeyDef('9'), KeyDef('+')],
    [KeyDef('Tab', 2.0), KeyDef('Q'), KeyDef('W'), KeyDef('E'), KeyDef('R'), KeyDef('T'), KeyDef('Y'), KeyDef('U'), KeyDef('I'), KeyDef('O'), KeyDef('P'), KeyDef('['), KeyDef(']'), KeyDef('\\', 2.0), KeyDef('4'), KeyDef('5'), KeyDef('6'), KeyDef('')],
    [KeyDef('Caps', 2.5), KeyDef('A'), KeyDef('S'), KeyDef('D'), KeyDef('F'), KeyDef('G'), KeyDef('H'), KeyDef('J'), KeyDef('K'), KeyDef('L'), KeyDef(';'), KeyDef('\''), KeyDef('Enter', 3.5), KeyDef('1'), KeyDef('2'), KeyDef('3'), KeyDef('Ent')],
    [KeyDef('Shift', 3.0), KeyDef('Z'), KeyDef('X'), KeyDef('C'), KeyDef('V'), KeyDef('B'), KeyDef('N'), KeyDef('M'), KeyDef(','), KeyDef('.'), KeyDef('/'), KeyDef('Shift', 3.0), KeyDef('0', 2.0), KeyDef('.'), KeyDef('')],
    [KeyDef('Ctrl', 1.5), KeyDef('Win', 1.5), KeyDef('Alt', 1.5), KeyDef('Space', 7.5), KeyDef('Alt', 1.5), KeyDef('Fn', 1.5), KeyDef('Ctrl', 1.5), KeyDef('Left', 1.0), KeyDef('Up', 1.0), KeyDef('Right', 1.0), KeyDef('', 1.5)]
  ];

  _KeyboardPainter({
    required this.timeNotifier,
    required this.effect,
    this.effectId,
    required this.baseColor,
    required this.brightness,
  }) : super(repaint: timeNotifier);

  void _buildGeometryCache(Size size) {
    final rect = Offset.zero & size;
    _cachedBaseRRect = RRect.fromRectAndRadius(rect, const Radius.circular(8));

    final wellRect = Rect.fromLTWH(16, 16, size.width - 32, size.height * 0.85);
    _cachedWellRRect = RRect.fromRectAndRadius(wellRect, const Radius.circular(8));

    final List<_PrecomputedKey> keys = [];
    double currentY = wellRect.top + 4;
    double keyHeight = (wellRect.height - 8 - (layout.length - 1) * 4) / layout.length;

    for (int r = 0; r < layout.length; r++) {
      final row = layout[r];
      double totalUnits = row.fold(0.0, (sum, key) => sum + key.w);
      double currentX = wellRect.left + 4;

      for (int c = 0; c < row.length; c++) {
        final keyDef = row[c];
        double keyWidth = (wellRect.width - 8 - (row.length - 1) * 4) * (keyDef.w / totalUnits);

        final keyRect = Rect.fromLTWH(currentX, currentY, keyWidth, keyHeight);
        final int zoneInt = ((currentX + keyWidth / 2 - wellRect.left) / wellRect.width * 4).floor().clamp(0, 3);

        if (keyDef.w > 0) {
          final kRRect = RRect.fromRectAndRadius(keyRect, const Radius.circular(4));
          final inRRect = RRect.fromRectAndRadius(keyRect.deflate(1), const Radius.circular(3));

          TextPainter? tp;
          Offset tOff = Offset.zero;

          if (keyDef.label.isNotEmpty) {
            final textSpan = TextSpan(
              text: keyDef.label,
              style: const TextStyle(
                color: Color(0xDDFFFFFF),
                fontSize: 10,
                fontWeight: FontWeight.w700,
                fontFamily: 'Segoe UI',
              ),
            );
            tp = TextPainter(text: textSpan, textDirection: TextDirection.ltr);
            tp.layout();
            tOff = Offset(
              currentX + (keyWidth - tp.width) / 2,
              currentY + (keyHeight - tp.height) / 2,
            );
          }

          keys.add(_PrecomputedKey(
            keyRRect: kRRect,
            innerRRect: inRRect,
            zoneIndex: zoneInt,
            textPainter: tp,
            textOffset: tOff,
          ));
        }

        currentX += keyWidth + 4;
      }
      currentY += keyHeight + 4;
    }

    _cachedKeys = keys;
    _cachedSize = size;
  }

  Color _calcColor(double zone, double time) {
    if (effectId != null && effectId!.isNotEmpty) {
      return calculateZoneLightById(zone, time, effectId!, baseColor);
    }
    return calculateZoneLight(zone, time, effect, baseColor);
  }

  Color _applyBrightness(Color c) {
    final double b = (brightness * c.a).clamp(0.0, 1.0);
    return Color.fromARGB(
      255,
      (c.r * 255.0 * b).round().clamp(0, 255),
      (c.g * 255.0 * b).round().clamp(0, 255),
      (c.b * 255.0 * b).round().clamp(0, 255),
    );
  }

  @override
  void paint(Canvas canvas, Size size) {
    if (_cachedSize != size || _cachedKeys == null) {
      _buildGeometryCache(size);
    }

    if (_cachedBaseRRect != null) canvas.drawRRect(_cachedBaseRRect!, _basePaint);
    if (_cachedWellRRect != null) canvas.drawRRect(_cachedWellRRect!, _wellPaint);

    final double time = timeNotifier.value;

    final z0 = _applyBrightness(_calcColor(0.0, time));
    final z1 = _applyBrightness(_calcColor(1.0, time));
    final z2 = _applyBrightness(_calcColor(2.0, time));
    final z3 = _applyBrightness(_calcColor(3.0, time));
    final zoneColors = [z0, z1, z2, z3];

    final keys = _cachedKeys!;
    for (int i = 0; i < keys.length; i++) {
      final key = keys[i];
      final keyColor = zoneColors[key.zoneIndex];

      // Draw Keycap Base
      canvas.drawRRect(key.keyRRect, _keyBasePaint);

      // Draw RGB Edge Glow
      _glowPaint.color = keyColor.withValues(alpha: 0.85);
      canvas.drawRRect(key.keyRRect, _glowPaint);

      // Draw Inner Backlight Bleed
      _innerGlowPaint.color = keyColor.withValues(alpha: 0.24);
      canvas.drawRRect(key.innerRRect, _innerGlowPaint);

      // Paint precomputed laser-etched legend (0 layout overhead)
      key.textPainter?.paint(canvas, key.textOffset);
    }
  }

  @override
  bool shouldRepaint(covariant _KeyboardPainter oldDelegate) {
    return oldDelegate.effect != effect ||
        oldDelegate.effectId != effectId ||
        oldDelegate.brightness != brightness ||
        oldDelegate.baseColor != baseColor;
  }
}
