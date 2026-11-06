using System;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;

namespace ForgeSense.Core.DSP
{
    public enum AudioSourceType
    {
        SystemAudio,
        Microphone,
        Disabled
    }

    public class AudioAnalyzer : IDisposable
    {
        private IWaveIn _capture;
        private AudioSourceType _currentSource = AudioSourceType.Disabled;

        public AudioFeatureState Features { get; } = new AudioFeatureState();
        private AudioFeatureState _rawFeatures = new AudioFeatureState();

        // DSP Params
        public float Sensitivity { get; set; } = 1.0f;
        public float Smoothing { get; set; } = 0.8f;
        public float BeatSensitivity { get; set; } = 1.5f;

        private Complex[] _fftBuffer = new Complex[1024];
        private int _fftPos = 0;
        private int _mLog2FftSize = 10; // 1024 points

        private float _energyHistoryAverage = 0f;
        private float _maxHistoricPeak = 0.01f;

        public void SetAudioSource(AudioSourceType type)
        {
            if (_currentSource == type) return;

            Stop();
            _currentSource = type;

            if (type == AudioSourceType.Disabled) return;

            try
            {
                if (type == AudioSourceType.SystemAudio)
                {
                    var enumerator = new MMDeviceEnumerator();
                    var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    _capture = new WasapiLoopbackCapture(device);
                }
                else if (type == AudioSourceType.Microphone)
                {
                    _capture = new WaveInEvent();
                    ((WaveInEvent)_capture).WaveFormat = new WaveFormat(44100, 16, 1);
                }

                _capture.DataAvailable += OnDataAvailable;
                _capture.RecordingStopped += (s, e) => 
                { 
                    if (_currentSource != AudioSourceType.Disabled)
                    {
                        var source = _currentSource;
                        _currentSource = AudioSourceType.Disabled; // Allow restart
                        Task.Delay(1000).ContinueWith(_ => 
                        {
                            SetAudioSource(source);
                        });
                    }
                };
                _capture.StartRecording();
                Console.WriteLine($"[AudioAnalyzer] Started capturing: {type}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioAnalyzer] Failed to start {type}: {ex.Message}");
                _currentSource = AudioSourceType.Disabled;
            }
        }

        public void Stop()
        {
            if (_capture != null)
            {
                try { _capture.StopRecording(); } catch { }
                try { _capture.Dispose(); } catch { }
                _capture = null;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            int bytesPerSample = _capture.WaveFormat.BitsPerSample / 8;
            int samples = e.BytesRecorded / bytesPerSample;
            int channels = _capture.WaveFormat.Channels;

            float maxPeak = 0f;
            float sumSq = 0f;
            int monoSamples = samples / channels;

            for (int i = 0; i < samples; i += channels)
            {
                float sample = 0;
                if (bytesPerSample == 4)
                {
                    sample = BitConverter.ToSingle(e.Buffer, i * bytesPerSample);
                }
                else if (bytesPerSample == 2)
                {
                    sample = BitConverter.ToInt16(e.Buffer, i * bytesPerSample) / 32768f;
                }

                float absSample = Math.Abs(sample);
                if (absSample > maxPeak) maxPeak = absSample;
                sumSq += sample * sample;

                // Fill FFT Buffer (Windowing)
                _fftBuffer[_fftPos].X = (float)(sample * FastFourierTransform.HannWindow(_fftPos, 1024));
                _fftBuffer[_fftPos].Y = 0;
                _fftPos++;

                if (_fftPos >= 1024)
                {
                    ProcessFft();
                    _fftPos = 0;
                }
            }

            float rms = (float)Math.Sqrt(sumSq / monoSamples);

            // Auto-gain
            _maxHistoricPeak = Math.Max(_maxHistoricPeak * 0.999f, maxPeak);
            if (_maxHistoricPeak < 0.01f) _maxHistoricPeak = 0.01f;

            _rawFeatures.Peak = Math.Clamp((maxPeak / _maxHistoricPeak) * Sensitivity, 0f, 1f);
            _rawFeatures.RMS = Math.Clamp((rms / _maxHistoricPeak) * Sensitivity, 0f, 1f);
            _rawFeatures.Volume = _rawFeatures.Peak;

            ApplySmoothing();
        }

        private void ProcessFft()
        {
            FastFourierTransform.FFT(true, _mLog2FftSize, _fftBuffer);

            float[] magnitudes = new float[512];
            for (int i = 0; i < 512; i++)
            {
                magnitudes[i] = (float)Math.Sqrt(_fftBuffer[i].X * _fftBuffer[i].X + _fftBuffer[i].Y * _fftBuffer[i].Y);
            }

            // Extract Bands (Assuming 44100Hz / 1024 = 43Hz per bin)
            _rawFeatures.SubBass = GetBandEnergy(magnitudes, 0, 1);    // 0-43Hz
            _rawFeatures.Bass = GetBandEnergy(magnitudes, 1, 5);       // 43-215Hz
            _rawFeatures.LowMid = GetBandEnergy(magnitudes, 5, 11);    // 215-473Hz
            _rawFeatures.Mid = GetBandEnergy(magnitudes, 11, 46);      // 473-2000Hz
            _rawFeatures.HighMid = GetBandEnergy(magnitudes, 46, 92);  // 2000-4000Hz
            _rawFeatures.Treble = GetBandEnergy(magnitudes, 92, 511);  // 4000+Hz

            // Simple Beat Detection (Energy variance)
            float instantEnergy = _rawFeatures.Bass + _rawFeatures.SubBass;
            _energyHistoryAverage = (_energyHistoryAverage * 0.95f) + (instantEnergy * 0.05f);
            
            if (instantEnergy > _energyHistoryAverage * BeatSensitivity && instantEnergy > 0.1f)
            {
                _rawFeatures.IsBeat = true;
                _rawFeatures.BeatConfidence = 1.0f;
            }
            else
            {
                _rawFeatures.IsBeat = false;
                _rawFeatures.BeatConfidence = Math.Max(0, _rawFeatures.BeatConfidence - 0.05f);
            }

            // Populate downsampled spectrum for UI
            for(int i=0; i<64; i++)
            {
                int startBin = (int)((i / 64f) * 128); // Focus on lower half of spectrum for better visuals
                int endBin = (int)(((i + 1) / 64f) * 128);
                if (endBin == startBin) endBin++;
                _rawFeatures.Spectrum[i] = GetBandEnergy(magnitudes, startBin, endBin);
            }
        }

        private float GetBandEnergy(float[] mags, int startBin, int endBin)
        {
            float max = 0;
            for (int i = startBin; i <= endBin && i < mags.Length; i++)
            {
                if (mags[i] > max) max = mags[i];
            }
            return Math.Clamp((max / _maxHistoricPeak) * Sensitivity, 0f, 1f);
        }

        private void ApplySmoothing()
        {
            // Lock to safely copy over
            lock (Features)
            {
                Features.Volume = ApplyFilter(Features.Volume, _rawFeatures.Volume, Smoothing);
                Features.Peak = ApplyFilter(Features.Peak, _rawFeatures.Peak, Smoothing);
                Features.RMS = ApplyFilter(Features.RMS, _rawFeatures.RMS, Smoothing);
                
                Features.SubBass = ApplyFilter(Features.SubBass, _rawFeatures.SubBass, Smoothing);
                Features.Bass = ApplyFilter(Features.Bass, _rawFeatures.Bass, Smoothing);
                Features.LowMid = ApplyFilter(Features.LowMid, _rawFeatures.LowMid, Smoothing);
                Features.Mid = ApplyFilter(Features.Mid, _rawFeatures.Mid, Smoothing);
                Features.HighMid = ApplyFilter(Features.HighMid, _rawFeatures.HighMid, Smoothing);
                Features.Treble = ApplyFilter(Features.Treble, _rawFeatures.Treble, Smoothing);

                Features.IsBeat = _rawFeatures.IsBeat;
                Features.BeatConfidence = _rawFeatures.BeatConfidence;

                for (int i = 0; i < 64; i++)
                {
                    Features.Spectrum[i] = ApplyFilter(Features.Spectrum[i], _rawFeatures.Spectrum[i], Smoothing);
                }
            }
        }

        private float ApplyFilter(float current, float target, float smoothing)
        {
            // Faster attack, slower decay
            if (target > current) return target; // Instant attack
            return (current * smoothing) + (target * (1.0f - smoothing));
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
