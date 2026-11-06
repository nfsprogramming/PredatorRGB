using System;

namespace ForgeSense.Core.DSP
{
    public class AudioFeatureState
    {
        // Global
        public float Volume { get; set; }
        public float Peak { get; set; }
        public float RMS { get; set; }

        // Frequency Bands (0.0 to 1.0)
        public float SubBass { get; set; }    // 20-60 Hz
        public float Bass { get; set; }       // 60-250 Hz
        public float LowMid { get; set; }     // 250-500 Hz
        public float Mid { get; set; }        // 500-2000 Hz
        public float HighMid { get; set; }    // 2000-4000 Hz
        public float Treble { get; set; }     // 4000-20000 Hz

        // Beat Detection
        public bool IsBeat { get; set; }
        public float BeatConfidence { get; set; }
        public float BPM { get; set; }

        // Raw FFT Data (downsampled for UI)
        public float[] Spectrum { get; set; } = new float[64];

        public void CopyFrom(AudioFeatureState other)
        {
            Volume = other.Volume;
            Peak = other.Peak;
            RMS = other.RMS;
            SubBass = other.SubBass;
            Bass = other.Bass;
            LowMid = other.LowMid;
            Mid = other.Mid;
            HighMid = other.HighMid;
            Treble = other.Treble;
            IsBeat = other.IsBeat;
            BeatConfidence = other.BeatConfidence;
            BPM = other.BPM;
            if (other.Spectrum != null && Spectrum.Length == other.Spectrum.Length)
            {
                Array.Copy(other.Spectrum, Spectrum, Spectrum.Length);
            }
        }
    }
}
