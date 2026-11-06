class AudioFeatures {
  final double volume;
  final double peak;
  final double rms;
  final double subBass;
  final double bass;
  final double lowMid;
  final double mid;
  final double highMid;
  final double treble;
  final bool isBeat;
  final double beatConfidence;
  final List<double> spectrum;

  AudioFeatures({
    required this.volume,
    required this.peak,
    required this.rms,
    required this.subBass,
    required this.bass,
    required this.lowMid,
    required this.mid,
    required this.highMid,
    required this.treble,
    required this.isBeat,
    required this.beatConfidence,
    required this.spectrum,
  });

  factory AudioFeatures.fromJson(Map<String, dynamic> json) {
    return AudioFeatures(
      volume: (json['Volume'] as num?)?.toDouble() ?? 0,
      peak: (json['Peak'] as num?)?.toDouble() ?? 0,
      rms: (json['RMS'] as num?)?.toDouble() ?? 0,
      subBass: (json['SubBass'] as num?)?.toDouble() ?? 0,
      bass: (json['Bass'] as num?)?.toDouble() ?? 0,
      lowMid: (json['LowMid'] as num?)?.toDouble() ?? 0,
      mid: (json['Mid'] as num?)?.toDouble() ?? 0,
      highMid: (json['HighMid'] as num?)?.toDouble() ?? 0,
      treble: (json['Treble'] as num?)?.toDouble() ?? 0,
      isBeat: json['IsBeat'] as bool? ?? false,
      beatConfidence: (json['BeatConfidence'] as num?)?.toDouble() ?? 0,
      spectrum: (json['Spectrum'] as List<dynamic>?)?.map((e) => (e as num).toDouble()).toList() ?? List.filled(64, 0.0),
    );
  }
}
