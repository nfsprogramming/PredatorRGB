using System;
using ForgeSense.Core.DSP;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public class BeatPulseEffect : BaseEffect
    {
        public override string Id => "musicsync_beatpulse";
        public override string DisplayName => "Beat Pulse";
        private AudioAnalyzer _audio;

        public BeatPulseEffect(AudioAnalyzer audio) { _audio = audio; }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float intensity = _audio.Features.IsBeat ? 1.0f : _audio.Features.BeatConfidence;
            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color((byte)(State.CurrentColor.R * intensity), (byte)(State.CurrentColor.G * intensity), (byte)(State.CurrentColor.B * intensity));
            }
        }
    }

    public class SpectrumZonesEffect : BaseEffect
    {
        public override string Id => "musicsync_spectrum";
        public override string DisplayName => "Spectrum Zones";
        private AudioAnalyzer _audio;

        public SpectrumZonesEffect(AudioAnalyzer audio) { _audio = audio; }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float[] zoneEnergies = new float[4];
            zoneEnergies[0] = _audio.Features.SubBass + _audio.Features.Bass;
            zoneEnergies[1] = _audio.Features.LowMid;
            zoneEnergies[2] = _audio.Features.HighMid;
            zoneEnergies[3] = _audio.Features.Treble;

            for (int i = 0; i < LedCount; i++)
            {
                int virtualZone = Math.Clamp((int)(i / (LedCount / 4.0f)), 0, 3);
                float energy = Math.Clamp(zoneEnergies[virtualZone] * 1.5f, 0f, 1f);
                
                colors[i] = new Color((byte)(State.CurrentColor.R * energy), (byte)(State.CurrentColor.G * energy), (byte)(State.CurrentColor.B * energy));
            }
        }
    }

    public class DynamicColorEffect : BaseEffect
    {
        public override string Id => "musicsync_dynamic";
        public override string DisplayName => "Dynamic Color";
        private AudioAnalyzer _audio;

        public DynamicColorEffect(AudioAnalyzer audio) { _audio = audio; }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float bass = _audio.Features.Bass;
            float mid = _audio.Features.Mid;
            float treble = _audio.Features.Treble;

            for (int i = 0; i < LedCount; i++)
            {
                byte r = (byte)Math.Clamp(bass * 255, 0, 255);
                byte g = (byte)Math.Clamp(mid * 255, 0, 255);
                byte b = (byte)Math.Clamp(treble * 255, 0, 255);
                colors[i] = new Color(r, g, b);
            }
        }
    }
}
