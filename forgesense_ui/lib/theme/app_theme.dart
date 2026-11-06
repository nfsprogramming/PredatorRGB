import 'package:flutter/material.dart';

class AppTheme {
  static const Color primary = Color(0xFFa3c9ff);
  static const Color primaryFixed = Color(0xFFd3e3ff);
  static const Color background = Color(0xFF131313);
  static const Color surface = Color(0xFF131313);
  static const Color surfaceContainer = Color(0xFF201f1f);
  static const Color surfaceContainerHigh = Color(0xFF2a2a2a);
  static const Color onSurface = Color(0xFFe5e2e1);
  static const Color onSurfaceVariant = Color(0xFFc0c7d4);
  static const Color primaryContainer = Color(0xFF0078d4);
  static const Color secondaryContainer = Color(0xFF00f1fe);
  static const Color tertiaryContainer = Color(0xFFd600c0);
  static const Color errorContainer = Color(0xFF93000a);
  static const Color tertiary = Color(0xFFfface8);
  static const Color tertiaryFixed = Color(0xFFffd7f0);
  static const Color secondaryFixed = Color(0xFF74f5ff);

  static final ThemeData darkTheme = ThemeData(
    brightness: Brightness.dark,
    scaffoldBackgroundColor: background,
    colorScheme: const ColorScheme.dark(
      primary: primary,
      surface: surface,
      onSurface: onSurface,
      onSurfaceVariant: onSurfaceVariant,
      primaryContainer: primaryContainer,
      secondaryContainer: secondaryContainer,
      tertiaryContainer: tertiaryContainer,
      errorContainer: errorContainer,
    ),
    useMaterial3: true,
    fontFamily: 'Segoe UI',
    textTheme: const TextTheme(
      headlineMedium: TextStyle(fontSize: 24, fontWeight: FontWeight.w600, color: onSurface),
      headlineSmall: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: onSurface),
      bodyLarge: TextStyle(fontSize: 16, fontWeight: FontWeight.w400, color: onSurface),
      bodyMedium: TextStyle(fontSize: 14, fontWeight: FontWeight.w400, color: onSurface),
      labelSmall: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, letterSpacing: 0.5, color: onSurfaceVariant),
    ),
  );
}
