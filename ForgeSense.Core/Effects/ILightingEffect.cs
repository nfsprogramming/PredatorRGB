using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public interface ILightingEffect : IDisposable
    {
        string Id { get; }
        string DisplayName { get; }

        void Initialize(int ledCount);
        void Start();
        void Update(float time, float deltaTime, out Color[] colors);
        void Stop();
        
        /// <summary>
        /// Updates the authoritative state for the effect. 
        /// Implementations should respect the version number to drop stale asynchronous updates.
        /// </summary>
        void SetState(RgbState state);

        /// <summary>
        /// Triggers a specific event inside the effect (e.g. "CriticalHit")
        /// </summary>
        void Trigger(string eventType);
    }
}
