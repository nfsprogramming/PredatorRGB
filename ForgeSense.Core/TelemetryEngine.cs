using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace ForgeSense.Core
{
    public class TelemetryEngine : IDisposable
    {
        private Computer _computer;
        private CancellationTokenSource _cts;
        private Task _monitorTask;

        public float CpuTemperature { get; private set; } = 0;
        public float GpuTemperature { get; private set; } = 0;
        public float BatteryLevel { get; private set; } = 100;

        public TelemetryEngine()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false, // Disabled due to NVML Access Violation (0xC0000005) on Optimus laptops
                IsBatteryEnabled = true
            };
            
            try 
            {
                _computer.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Warning: LibreHardwareMonitor requires Administrator privileges to read all sensors. " + ex.Message);
            }
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoop(_cts.Token));
        }

        private void MonitorLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Refresh hardware data
                    foreach (var hardware in _computer.Hardware)
                    {
                        hardware.Update();

                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Package"));
                            if (tempSensor != null && tempSensor.Value.HasValue)
                                CpuTemperature = tempSensor.Value.Value;
                        }
                        else if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                        {
                            var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Core"));
                            if (tempSensor != null && tempSensor.Value.HasValue)
                                GpuTemperature = tempSensor.Value.Value;
                        }
                        else if (hardware.HardwareType == HardwareType.Battery)
                        {
                            var levelSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Level);
                            if (levelSensor != null && levelSensor.Value.HasValue)
                                BatteryLevel = levelSensor.Value.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Telemetry Error: " + ex.Message);
                }

                // Poll every 1 second
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _monitorTask?.Wait();
            _computer?.Close();
        }
    }
}
