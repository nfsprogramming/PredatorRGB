using System;
using System.Collections.Generic;

namespace ForgeSense.Core.Effects
{
    public class EffectRegistry
    {
        private readonly Dictionary<string, ILightingEffect> _effects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["wave"] = "rainbow_wave",
            ["aura"] = "aurora",
            ["heartbeat"] = "pulse",
            ["starlight"] = "pulse",
            ["meteor"] = "meteor_shower",
            ["ripple"] = "energy_ripple",
            ["reactive"] = "zone_reactive",
            ["reactive_zone"] = "zone_reactive",
            ["music"] = "audio_pulse",
            ["spectrum"] = "spectrum_flow"
        };

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return key.Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
        }

        public void Register(ILightingEffect effect)
        {
            _effects[effect.Id] = effect;
            string norm = NormalizeKey(effect.Id);
            if (!string.IsNullOrEmpty(norm) && !_effects.ContainsKey(norm))
            {
                _effects[norm] = effect;
            }
        }

        public ILightingEffect GetEffect(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            // 1. Direct match
            if (_effects.TryGetValue(id, out var effect)) return effect;

            // 2. Normalized match (e.g. "dualcolor" -> "dual_color")
            string norm = NormalizeKey(id);
            if (_effects.TryGetValue(norm, out effect)) return effect;

            // 3. Alias match (e.g. "wave" -> "rainbow_wave")
            if (_aliases.TryGetValue(id, out var targetId) || _aliases.TryGetValue(norm, out targetId))
            {
                if (_effects.TryGetValue(targetId, out effect) || _effects.TryGetValue(NormalizeKey(targetId), out effect))
                {
                    return effect;
                }
            }

            return null;
        }

        public void DisposeAll()
        {
            foreach (var effect in new HashSet<ILightingEffect>(_effects.Values))
            {
                effect.Dispose();
            }
            _effects.Clear();
        }
    }
}
