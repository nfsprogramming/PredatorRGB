import 'dart:convert';
import 'dart:io';
import 'package:flutter/foundation.dart';
import '../models/audio_features.dart';

class TelemetryService extends ChangeNotifier {
  RawDatagramSocket? _socket;
  AudioFeatures? latestFeatures;

  Future<void> startListening() async {
    try {
      _socket = await RawDatagramSocket.bind(InternetAddress.loopbackIPv4, 4051);
      _socket!.listen((RawSocketEvent event) {
        if (event == RawSocketEvent.read) {
          Datagram? datagram = _socket!.receive();
          if (datagram != null) {
            String jsonStr = utf8.decode(datagram.data);
            try {
              final decoded = json.decode(jsonStr);
              latestFeatures = AudioFeatures.fromJson(decoded);
              notifyListeners();
            } catch (e) {
              print('Failed to parse telemetry: $e');
            }
          }
        }
      });
    } catch (e) {
      print('UDP Bind Error: $e');
    }
  }

  @override
  void dispose() {
    _socket?.close();
    super.dispose();
  }
}
