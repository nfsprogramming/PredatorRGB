using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;
using ForgeSense.Core.DSP;

namespace ForgeSense.Core.Effects
{
    // 81. Audio Pulse
    public class AudioPulseEffect : BaseAudioEffect
    {
        public override string Id => "audio_pulse";
        public override string DisplayName => "Audio Pulse";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float intensity = AudioData.Volume * State.Intensity;

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 82. Beat Flash
    public class BeatFlashEffect : BaseAudioEffect
    {
        public override string Id => "beat_flash";
        public override string DisplayName => "Beat Flash";

        private Color _base = new Color(0, 0, 50); // Idle blue
        private Color _flash = new Color(0, 255, 255); // Cyan

        private float _beatDecay = 0f;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            if (AudioData.IsBeat)
            {
                _beatDecay = 1.0f;
            }
            else
            {
                _beatDecay -= deltaTime * State.Speed * 5f;
                _beatDecay = Math.Clamp(_beatDecay, 0f, 1f);
            }

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = Interpolate(_base, _flash, _beatDecay * State.Intensity);
            }
        }
    }

    // 83. Bass Reactor
    public class BassReactorEffect : BaseAudioEffect
    {
        public override string Id => "bass_reactor";
        public override string DisplayName => "Bass Reactor";

        private Color _idle = new Color(10, 0, 30); // Deep Purple
        private Color _active = new Color(255, 0, 255); // Magenta

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Bass mostly impacts the entire keyboard but center slightly more
            float bassVal = AudioData.Bass;
            float subBassVal = AudioData.SubBass;
            float totalBass = Math.Clamp((bassVal + subBassVal) * 1.5f, 0f, 1f);

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - 1.5f);
                float zoneIntensity = dist < 1.0f ? totalBass : (totalBass * 0.7f); // Outer zones 70%

                colors[i] = Interpolate(_idle, _active, zoneIntensity * State.Intensity);
            }
        }
    }

    // 84. Spectrum Wave
    public class SpectrumWaveEffect : BaseAudioEffect
    {
        public override string Id => "spectrum_wave";
        public override string DisplayName => "Spectrum Wave";

        private Color _bassCol = new Color(255, 0, 0); // Red
        private Color _lowMidCol = new Color(255, 128, 0); // Orange
        private Color _highMidCol = new Color(0, 255, 0); // Green
        private Color _trebleCol = new Color(0, 255, 255); // Cyan

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Four zones = four frequencies
            float[] intensities = new float[4] {
                AudioData.SubBass + AudioData.Bass,
                AudioData.LowMid,
                AudioData.Mid + AudioData.HighMid,
                AudioData.Treble
            };
            
            Color[] targetCols = new Color[4] { _bassCol, _lowMidCol, _highMidCol, _trebleCol };

            for (int i = 0; i < LedCount; i++)
            {
                int z = Math.Clamp(i, 0, 3);
                float intensity = Math.Clamp(intensities[z] * 1.2f, 0f, 1f);
                
                colors[i] = new Color(
                    (byte)(targetCols[z].R * intensity * State.Intensity),
                    (byte)(targetCols[z].G * intensity * State.Intensity),
                    (byte)(targetCols[z].B * intensity * State.Intensity)
                );
            }
        }
    }

    // 85. Audio Vortex
    public class AudioVortexEffect : BaseAudioEffect
    {
        public override string Id => "audio_vortex";
        public override string DisplayName => "Audio Vortex";

        private float _internalPhase = 0f;
        private Color _primary = new Color(128, 0, 255);
        private Color _secondary = new Color(0, 255, 128);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Energy speeds up the rotation
            float speedMult = 1.0f + (AudioData.Volume * 5.0f);
            _internalPhase += deltaTime * State.Speed * speedMult;
            
            float pos = _internalPhase % LedCount;
            if (pos < 0) pos += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - pos);
                if (dist > LedCount / 2f) dist = LedCount - dist;

                float intensity = 1.0f - (dist / Math.Max(0.5f, State.Width * 2f));
                intensity = Math.Clamp(intensity, 0f, 1f);
                
                // Treble shifts hue, Bass pushes brightness
                Color activeCol = Interpolate(_primary, _secondary, AudioData.Treble);
                float finalIntensity = intensity * Math.Max(0.3f, AudioData.Bass * 1.5f);

                colors[i] = new Color(
                    (byte)(activeCol.R * finalIntensity * State.Intensity),
                    (byte)(activeCol.G * finalIntensity * State.Intensity),
                    (byte)(activeCol.B * finalIntensity * State.Intensity)
                );
            }
        }
    }

    // 86. Bass Tunnel
    public class BassTunnelEffect : BaseAudioEffect
    {
        public override string Id => "bass_tunnel";
        public override string DisplayName => "Bass Tunnel";

        private Color _dark = new Color(10, 0, 10);
        private Color _core = new Color(255, 0, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float bass = Math.Clamp(AudioData.Bass * 1.5f, 0f, 1f);

            for (int i = 0; i < LedCount; i++)
            {
                float distFromCenter = Math.Abs(i - 1.5f); // 0.5 for center, 1.5 for outer
                float targetIntensity;
                
                if (distFromCenter < 1.0f) // Center
                {
                    targetIntensity = bass;
                }
                else // Outer
                {
                    targetIntensity = bass * 0.3f; // dimmer
                }

                colors[i] = Interpolate(_dark, _core, targetIntensity * State.Intensity);
            }
        }
    }

    // 87. Beat Ripple
    public class BeatRippleEffect : BaseAudioEffect
    {
        public override string Id => "beat_ripple";
        public override string DisplayName => "Beat Ripple";

        private float _lastBeatTime = -10f;
        private int _rippleOrigin = 0;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            if (AudioData.IsBeat && (time - _lastBeatTime) > 0.3f)
            {
                _lastBeatTime = time;
                _rippleOrigin = (_rippleOrigin + 1) % LedCount; // Rotate origin
            }

            float elapsed = time - _lastBeatTime;
            float speed = State.Speed * 5.0f;
            float waveRadius = elapsed * speed;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - _rippleOrigin);
                float intensity = 0f;

                if (waveRadius >= dist && waveRadius < dist + State.Width)
                {
                    intensity = 1.0f - ((waveRadius - dist) / State.Width);
                    intensity = Math.Clamp(intensity, 0f, 1f);
                }

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity * State.Intensity),
                    (byte)(State.CurrentColor.G * intensity * State.Intensity),
                    (byte)(State.CurrentColor.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 88. Frequency Flow
    public class FrequencyFlowEffect : BaseAudioEffect
    {
        public override string Id => "frequency_flow";
        public override string DisplayName => "Frequency Flow";

        private Color _bassCol = new Color(128, 0, 128); // Purple
        private Color _midCol = new Color(0, 0, 255);    // Blue
        private Color _highCol = new Color(0, 255, 255); // Cyan

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float bass = AudioData.Bass;
            float mid = AudioData.Mid;
            float high = AudioData.Treble;

            float total = bass + mid + high + 0.01f;
            float wBass = bass / total;
            float wMid = mid / total;
            float wHigh = high / total;
            
            float r = (_bassCol.R * wBass) + (_midCol.R * wMid) + (_highCol.R * wHigh);
            float g = (_bassCol.G * wBass) + (_midCol.G * wMid) + (_highCol.G * wHigh);
            float b = (_bassCol.B * wBass) + (_midCol.B * wMid) + (_highCol.B * wHigh);

            float globalIntensity = AudioData.Volume * State.Intensity * 1.5f;
            globalIntensity = Math.Clamp(globalIntensity, 0f, 1f);

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)(r * globalIntensity),
                    (byte)(g * globalIntensity),
                    (byte)(b * globalIntensity)
                );
            }
        }
    }

    // 89. Music Aurora
    public class MusicAuroraEffect : BaseAudioEffect
    {
        public override string Id => "music_aurora";
        public override string DisplayName => "Music Aurora";

        private Color _c1 = new Color(0, 100, 255);
        private Color _c2 = new Color(0, 255, 100);
        
        private float _beatGlow = 0f;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            if (AudioData.IsBeat)
            {
                _beatGlow = 1.0f;
            }
            else
            {
                _beatGlow -= deltaTime * State.Speed;
                _beatGlow = Math.Clamp(_beatGlow, 0f, 1f);
            }

            float phase = time * State.Speed * 0.5f + (AudioData.Mid * 2.0f); // Mid pushes movement

            for (int i = 0; i < LedCount; i++)
            {
                float wave = (float)Math.Sin(phase + i) * 0.5f + 0.5f;
                Color baseC = Interpolate(_c1, _c2, wave);
                
                // Bass controls wave height, overall volume sets minimum
                float intensity = Math.Max(AudioData.Volume * 0.5f, AudioData.Bass);
                intensity = Math.Clamp(intensity + (_beatGlow * 0.5f), 0.1f, 1.0f); // beat expands it

                colors[i] = new Color(
                    (byte)(baseC.R * intensity * State.Intensity),
                    (byte)(baseC.G * intensity * State.Intensity),
                    (byte)(baseC.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 90. Audio Plasma
    public class AudioPlasmaEffect : BaseAudioEffect
    {
        public override string Id => "audio_plasma";
        public override string DisplayName => "Audio Plasma";

        private Color _c1 = new Color(255, 0, 128); // Pink
        private Color _c2 = new Color(0, 128, 255); // Blue

        private float _phase = 0f;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Plasma speed driven by Mid/Treble
            float speedMult = 1.0f + (AudioData.Mid * 2.0f) + (AudioData.Treble * 4.0f);
            _phase += deltaTime * State.Speed * speedMult;
            
            float bassAmp = AudioData.Bass * 2.0f;

            for (int i = 0; i < LedCount; i++)
            {
                float v1 = (float)Math.Sin(_phase + i);
                float v2 = (float)Math.Sin(_phase * 0.7f - i * 1.5f);
                float composite = (v1 + v2) / 2f; // -1 to 1
                
                float normalized = (composite + 1f) / 2f;
                
                Color c = Interpolate(_c1, _c2, normalized);
                
                // Bass controls the amplitude, volume is the baseline
                float intensity = (normalized * bassAmp) + (AudioData.Volume * 0.5f);
                intensity = Math.Clamp(intensity, 0.05f, 1.0f); // Never fully dark

                if (AudioData.IsBeat)
                {
                    intensity = 1.0f; // Spike
                }

                colors[i] = new Color(
                    (byte)(c.R * intensity * State.Intensity),
                    (byte)(c.G * intensity * State.Intensity),
                    (byte)(c.B * intensity * State.Intensity)
                );
            }
        }
    }
}
