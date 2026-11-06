using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 51. Rage
    public class RageEffect : BaseEffect
    {
        public override string Id => "rage";
        public override string DisplayName => "Rage";

        private Color _darkRed = new Color(50, 0, 0);
        private Color _red = new Color(255, 0, 0);
        private Color _orange = new Color(255, 69, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 2.0f;
            
            // Fast pulsing energy
            float phase = (time * speed) % 1.0f;

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * 0.25f;
                float wave = (float)Math.Sin((phase + offset) * Math.PI * 2) * 0.5f + 0.5f;
                
                // Irregular fluctuation
                float fluctuation = (float)Math.Sin(time * speed * 3.5f + i) * 0.2f;
                wave = Math.Clamp(wave + fluctuation, 0f, 1f);

                Color c;
                if (wave < 0.5f) c = Interpolate(_darkRed, _red, wave / 0.5f);
                else c = Interpolate(_red, _orange, (wave - 0.5f) / 0.5f);

                colors[i] = new Color(
                    (byte)(c.R * State.Intensity),
                    (byte)(c.G * State.Intensity),
                    (byte)(c.B * State.Intensity)
                );
            }
        }
    }

    // 52. Critical Hit
    public class CriticalHitEffect : BaseEffect
    {
        public override string Id => "critical_hit";
        public override string DisplayName => "Critical Hit";

        private Color _idle = new Color(30, 0, 0);
        private Color _impact = new Color(255, 200, 200); // White-red
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "CriticalHit")
            {
                _lastTrigger = -1f; // forces a trigger to be registered next frame
            }
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time; // Set trigger time properly

            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;
            float intensity = 0f;

            if (elapsed < 0.05f) // Attack
                intensity = elapsed / 0.05f;
            else if (elapsed < 0.8f) // Decay
                intensity = (float)Math.Pow(1.0f - ((elapsed - 0.05f) / 0.75f), 2.0);

            for (int i = 0; i < LedCount; i++)
                colors[i] = Interpolate(_idle, _impact, intensity * State.Intensity);
        }
    }

    // 53. Damage Flash
    public class DamageFlashEffect : BaseEffect
    {
        public override string Id => "damage_flash";
        public override string DisplayName => "Damage Flash";

        private Color _idle = new Color(40, 0, 0);
        private Color _damage = new Color(255, 50, 50);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "Damage") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;

            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;
            float intensity = 0f;

            if (elapsed < 0.08f)
                intensity = elapsed / 0.08f;
            else if (elapsed < 0.5f)
                intensity = 1.0f - ((elapsed - 0.08f) / 0.42f); // linear sharp decay

            for (int i = 0; i < LedCount; i++)
                colors[i] = Interpolate(_idle, _damage, intensity * State.Intensity);
        }
    }

    // 54. Health Pulse
    public class HealthPulseEffect : BaseEffect
    {
        public override string Id => "health_pulse";
        public override string DisplayName => "Health Pulse";

        private Color _green = new Color(0, 255, 0);
        private Color _yellow = new Color(255, 255, 0);
        private Color _orange = new Color(255, 128, 0);
        private Color _red = new Color(255, 0, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float h = Math.Clamp(State.HealthPercent, 0f, 100f);
            
            // Interpolate color based on health
            Color targetC;
            if (h > 75f) targetC = Interpolate(_yellow, _green, (h - 75f) / 25f);
            else if (h > 50f) targetC = Interpolate(_orange, _yellow, (h - 50f) / 25f);
            else if (h > 25f) targetC = Interpolate(_red, _orange, (h - 25f) / 25f);
            else targetC = _red;

            // Pulse speed inversely proportional to health
            float pulseSpeed = 1.0f + ((100f - h) / 100f) * 4.0f; // 1x to 5x speed
            
            float pulse = (float)Math.Sin(time * State.Speed * pulseSpeed * Math.PI) * 0.3f + 0.7f; // 0.4 to 1.0

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)(targetC.R * pulse),
                    (byte)(targetC.G * pulse),
                    (byte)(targetC.B * pulse)
                );
            }
        }
    }

    // 55. Combo
    public class ComboEffect : BaseEffect
    {
        public override string Id => "combo";
        public override string DisplayName => "Combo";

        private Color[] _comboColors = new[] {
            new Color(0, 0, 255),   // 0: Blue
            new Color(0, 255, 255), // Cyan
            new Color(128, 0, 128), // Purple
            new Color(255, 20, 147),// Pink
            new Color(255, 69, 0)   // Red/Orange (Max)
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Scale combo 0-30+ to 0-4
            float c = Math.Clamp(State.ComboCount / 7.5f, 0f, 4f); 
            
            int idx1 = (int)Math.Floor(c);
            int idx2 = Math.Min(idx1 + 1, _comboColors.Length - 1);
            Color targetC = Interpolate(_comboColors[idx1], _comboColors[idx2], c - idx1);

            float speed = State.Speed * (1.0f + c * 0.5f); // Faster at high combo
            float intensity = 0.5f + (c / 4f) * 0.5f; // Brighter at high combo

            float wave = (float)Math.Sin(time * speed * Math.PI) * 0.2f + 0.8f;

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)(targetC.R * wave * intensity),
                    (byte)(targetC.G * wave * intensity),
                    (byte)(targetC.B * wave * intensity)
                );
            }
        }
    }

    // 56. Level Up
    public class LevelUpEffect : BaseEffect
    {
        public override string Id => "level_up";
        public override string DisplayName => "Level Up";

        private Color _idle = new Color(30, 20, 0);
        private Color _gold = new Color(255, 215, 0);
        private Color _white = new Color(255, 255, 255);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "LevelUp") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;
            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;

            // Phase 1: Center outwards (0 to 0.5s)
            // Phase 2: Flash (0.5s to 0.7s)
            // Phase 3: Fade (0.7s to 2.0s)
            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - 1.5f); // Center
                Color c = _idle;

                if (elapsed < 0.5f)
                {
                    float spread = (elapsed / 0.5f) * 2.0f; // expands 0 to 2.0
                    if (dist <= spread) c = _gold;
                }
                else if (elapsed < 0.7f)
                {
                    c = Interpolate(_gold, _white, (elapsed - 0.5f) / 0.2f);
                }
                else if (elapsed < 2.0f)
                {
                    c = Interpolate(_white, _idle, (elapsed - 0.7f) / 1.3f);
                }

                colors[i] = c;
            }
        }
    }

    // 57. Victory
    public class VictoryEffect : BaseEffect
    {
        public override string Id => "victory";
        public override string DisplayName => "Victory";

        private Color _idle = new Color(10, 10, 0);
        private Color _gold = new Color(255, 215, 0);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "Victory") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;
            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;

            float intensity = 0f;
            if (elapsed < 0.5f) intensity = elapsed / 0.5f; // Ramp up
            else if (elapsed < 2.5f) // Pulsing gold
                intensity = (float)Math.Sin((elapsed - 0.5f) * Math.PI * 2) * 0.2f + 0.8f;
            else if (elapsed < 4.0f) // Fade
                intensity = 1.0f - ((elapsed - 2.5f) / 1.5f);

            for (int i = 0; i < LedCount; i++)
                colors[i] = Interpolate(_idle, _gold, Math.Clamp(intensity, 0f, 1f));
        }
    }

    // 58. Defeat
    public class DefeatEffect : BaseEffect
    {
        public override string Id => "defeat";
        public override string DisplayName => "Defeat";

        private Color _idle = new Color(20, 0, 0);
        private Color _red = new Color(255, 0, 0);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "Defeat") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;
            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;

            float intensity = 0f;
            if (elapsed < 0.2f) intensity = elapsed / 0.2f; // Quick pulse
            else if (elapsed < 0.8f) intensity = 1.0f - ((elapsed - 0.2f) / 0.6f); // Fade
            else intensity = 0f; // Very dark red

            for (int i = 0; i < LedCount; i++)
                colors[i] = Interpolate(new Color(5,0,0), _red, Math.Clamp(intensity, 0f, 1f));
        }
    }

    // 59. Respawn
    public class RespawnEffect : BaseEffect
    {
        public override string Id => "respawn";
        public override string DisplayName => "Respawn";

        private Color _dark = new Color(0, 0, 10);
        private Color _blue = new Color(0, 150, 255);
        private Color _cyan = new Color(0, 255, 255);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "Respawn") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;
            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - 1.5f);
                Color c = _dark;

                if (elapsed < 0.8f)
                {
                    float wave = (elapsed / 0.8f) * 2.5f;
                    if (Math.Abs(dist - wave) < 0.5f) c = _cyan; // Expanding energy ring
                    else if (dist < wave) c = _blue; // Filled center
                }
                else if (elapsed < 1.5f)
                {
                    c = Interpolate(_cyan, _blue, (elapsed - 0.8f) / 0.7f);
                }
                else
                {
                    c = _blue;
                }

                colors[i] = c;
            }
        }
    }

    // 60. Game Over
    public class GameOverEffect : BaseEffect
    {
        public override string Id => "game_over";
        public override string DisplayName => "Game Over";

        private Color _idle = new Color(5, 0, 0);
        private Color _red = new Color(255, 0, 0);
        private float _lastTrigger = -10f;

        public override void Trigger(string eventType)
        {
            if (eventType == "GameOver") _lastTrigger = -1f;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_lastTrigger == -1f) _lastTrigger = time;
            colors = new Color[LedCount];
            float elapsed = time - _lastTrigger;

            float intensity = 0f;
            if (elapsed < 0.3f) intensity = (float)Math.Sin(elapsed * Math.PI / 0.3f); // heartbeat 1
            else if (elapsed > 0.4f && elapsed < 0.8f) intensity = (float)Math.Sin((elapsed - 0.4f) * Math.PI / 0.4f); // heartbeat 2
            else if (elapsed > 1.0f && elapsed < 3.0f) intensity = 1.0f - ((elapsed - 1.0f) / 2.0f); // Long fade

            for (int i = 0; i < LedCount; i++)
                colors[i] = Interpolate(_idle, _red, Math.Clamp(intensity, 0f, 1f));
        }
    }
}
