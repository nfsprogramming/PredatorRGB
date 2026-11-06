import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';

class TcpService extends ChangeNotifier {
  String _status = 'Disconnected';
  bool _isConnected = false;
  String _acerServiceStatus = 'UNKNOWN'; // RUNNING, STOPPED, NOT_INSTALLED, UNKNOWN
  String _hardwareStatus = 'Checking...';
  bool _isProcessingService = false;
  bool _autoStopAttempted = false;

  Socket? _streamSocket;
  bool _isConnectingStream = false;

  String get status => _status;
  bool get isConnected => _isConnected;
  String get acerServiceStatus => _acerServiceStatus;
  String get hardwareStatus => _hardwareStatus;
  bool get isProcessingService => _isProcessingService;

  Timer? _statusTimer;

  TcpService() {
    // Initial status check and periodic monitor
    refreshAllStatuses();
    _statusTimer = Timer.periodic(const Duration(seconds: 4), (_) {
      refreshAllStatuses();
    });
  }

  @override
  void dispose() {
    _statusTimer?.cancel();
    _streamSocket?.destroy();
    _streamSocket = null;
    super.dispose();
  }

  int _lastSentPacked0 = -1;
  int _lastSentPacked1 = -1;
  int _lastSentPacked2 = -1;
  int _lastSentPacked3 = -1;

  /// High-speed persistent TCP zone streaming (~30-35 FPS) directly to physical keyboard hardware
  void streamZoneColors(Color z0, Color z1, Color z2, Color z3) {
    if (_streamSocket == null) {
      if (!_isConnectingStream) {
        _isConnectingStream = true;
        Socket.connect('127.0.0.1', 4050, timeout: const Duration(milliseconds: 600)).then((sock) {
          _streamSocket = sock;
          try {
            _streamSocket?.setOption(SocketOption.tcpNoDelay, true);
          } catch (_) {}
          _isConnectingStream = false;
          _writeZonePacket(z0, z1, z2, z3);
        }).catchError((_) {
          _isConnectingStream = false;
        });
      }
      return;
    }
    _writeZonePacket(z0, z1, z2, z3);
  }

  void _writeZonePacket(Color z0, Color z1, Color z2, Color z3) {
    // Differential filter: skip redundant socket writes if colors haven't changed
    final p0 = (z0.red << 16) | (z0.green << 8) | z0.blue;
    final p1 = (z1.red << 16) | (z1.green << 8) | z1.blue;
    final p2 = (z2.red << 16) | (z2.green << 8) | z2.blue;
    final p3 = (z3.red << 16) | (z3.green << 8) | z3.blue;

    if (p0 == _lastSentPacked0 && p1 == _lastSentPacked1 && p2 == _lastSentPacked2 && p3 == _lastSentPacked3) {
      return;
    }

    _lastSentPacked0 = p0;
    _lastSentPacked1 = p1;
    _lastSentPacked2 = p2;
    _lastSentPacked3 = p3;

    try {
      final msg = 'ZONES:${z0.red},${z0.green},${z0.blue}:${z1.red},${z1.green},${z1.blue}:${z2.red},${z2.green},${z2.blue}:${z3.red},${z3.green},${z3.blue}\n';
      _streamSocket?.write(msg);
    } catch (e) {
      _streamSocket?.destroy();
      _streamSocket = null;
      _lastSentPacked0 = -1;
    }
  }

  /// Sends a command to the ForgeSense Core TCP server and returns response string
  Future<String?> sendCommand(String command, {Duration timeout = const Duration(milliseconds: 1500)}) async {
    Socket? socket;
    try {
      socket = await Socket.connect('127.0.0.1', 4050, timeout: timeout);
      socket.write('$command\n');
      await socket.flush();

      final completer = Completer<String?>();
      late StreamSubscription sub;
      sub = socket.listen(
        (data) {
          final res = utf8.decode(data).trim();
          if (!completer.isCompleted) {
            completer.complete(res);
          }
        },
        onError: (err) {
          if (!completer.isCompleted) completer.complete(null);
        },
        onDone: () {
          if (!completer.isCompleted) completer.complete(null);
        },
        cancelOnError: true,
      );

      final response = await completer.future.timeout(
        timeout,
        onTimeout: () => null,
      );

      await sub.cancel();
      socket.destroy();

      _status = 'Connected';
      _isConnected = true;
      notifyListeners();
      return response;
    } catch (e) {
      socket?.destroy();
      _status = 'Disconnected';
      _isConnected = false;
      notifyListeners();
      debugPrint('TCP command error ($command): $e');
      return null;
    }
  }

  /// Refreshes both Backend connection, Acer Service status, and Hardware provider status
  Future<void> refreshAllStatuses() async {
    // 1. Check Backend connection via PING
    final pingRes = await sendCommand('PING', timeout: const Duration(milliseconds: 800));
    if (pingRes != null && pingRes.contains('PONG')) {
      _isConnected = true;
      _status = 'Connected';

      // 2. Query Hardware Provider
      final hwRes = await sendCommand('HARDWARE:STATUS');
      if (hwRes != null && hwRes.contains('ACER_WMI_4ZONE')) {
        _hardwareStatus = 'Acer WMI 4-Zone Hardware (Active)';
      } else {
        _hardwareStatus = 'Simulation Mode';
      }

      // 3. Query Acer Lighting Service status from backend
      final svcRes = await sendCommand('SERVICE:ACER:STATUS');
      if (svcRes != null && svcRes.startsWith('STATUS:ACER:')) {
        _acerServiceStatus = svcRes.replaceFirst('STATUS:ACER:', '').trim();
      } else {
        await _checkAcerServiceDirectly();
      }

      if (_acerServiceStatus == 'RUNNING' && !_autoStopAttempted) {
        _autoStopAttempted = true;
        stopAcerLightingService();
      }
    } else {
      _isConnected = false;
      _status = 'Offline';
      _hardwareStatus = 'Backend Offline';
      await _checkAcerServiceDirectly();
      if (_acerServiceStatus == 'RUNNING' && !_autoStopAttempted) {
        _autoStopAttempted = true;
        stopAcerLightingService();
      }
    }
    notifyListeners();
  }

  /// Fallback to direct OS check using 'sc query AcerLightingService'
  Future<void> _checkAcerServiceDirectly() async {
    try {
      final res = await Process.run('cmd', ['/c', 'sc', 'query', 'AcerLightingService']);
      final out = res.stdout.toString().toUpperCase();
      if (out.contains('RUNNING')) {
        _acerServiceStatus = 'RUNNING';
      } else if (out.contains('STOPPED')) {
        _acerServiceStatus = 'STOPPED';
      } else if (out.contains('1060') || out.contains('DOES NOT EXIST')) {
        _acerServiceStatus = 'NOT_INSTALLED';
      } else {
        _acerServiceStatus = 'UNKNOWN';
      }
    } catch (_) {
      _acerServiceStatus = 'UNKNOWN';
    }
  }

  /// Stop Acer Lighting Service
  Future<bool> stopAcerLightingService() async {
    _isProcessingService = true;
    notifyListeners();
    bool success = false;

    try {
      // Try via backend RPC first
      final res = await sendCommand('SERVICE:ACER:STOP', timeout: const Duration(seconds: 4));
      if (res != null && (res.startsWith('OK') || res.contains('STOPPED'))) {
        success = true;
      } else {
        // Fallback: run elevated PowerShell command
        final proc = await Process.run(
          'powershell',
          ['-Command', 'Start-Process sc -ArgumentList "stop AcerLightingService" -Verb RunAs -Wait'],
        );
        success = proc.exitCode == 0;
      }
    } catch (e) {
      debugPrint('Error stopping Acer service: $e');
    } finally {
      _isProcessingService = false;
      await refreshAllStatuses();
    }

    return success;
  }

  /// Start Acer Lighting Service
  Future<bool> startAcerLightingService() async {
    _isProcessingService = true;
    notifyListeners();
    bool success = false;

    try {
      final res = await sendCommand('SERVICE:ACER:START', timeout: const Duration(seconds: 4));
      if (res != null && (res.startsWith('OK') || res.contains('RUNNING'))) {
        success = true;
      } else {
        final proc = await Process.run(
          'powershell',
          ['-Command', 'Start-Process sc -ArgumentList "start AcerLightingService" -Verb RunAs -Wait'],
        );
        success = proc.exitCode == 0;
      }
    } catch (e) {
      debugPrint('Error starting Acer service: $e');
    } finally {
      _isProcessingService = false;
      await refreshAllStatuses();
    }

    return success;
  }

  /// Stop ForgeSense Core backend process cleanly
  Future<void> stopBackendEngine() async {
    await sendCommand('EXIT');
    _isConnected = false;
    _status = 'Offline';
    _hardwareStatus = 'Backend Stopped';
    notifyListeners();
  }
}
