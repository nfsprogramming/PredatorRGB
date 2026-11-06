using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace ForgeSense.Core
{
    [SupportedOSPlatform("windows")]
    public static class WindowsServiceManager
    {
        public const string AcerLightingServiceName = "AcerLightingService";

        public static string GetServiceStatus(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                return sc.Status.ToString().ToUpperInvariant(); // RUNNING, STOPPED, START_PENDING, STOP_PENDING
            }
            catch (InvalidOperationException)
            {
                return "NOT_INSTALLED";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public static (bool Success, string Message) StopService(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    return (true, "ALREADY_STOPPED");
                }

                if (sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                    return (true, "STOPPED");
                }
                else
                {
                    RunScCommand($"stop {serviceName}");
                    return (true, "STOP_COMMAND_SENT");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    RunScCommand($"stop {serviceName}");
                    return (true, "STOP_SENT_VIA_SC");
                }
                catch (Exception scEx)
                {
                    return (false, "FAILED: " + ex.Message + " | SC: " + scEx.Message);
                }
            }
        }

        public static (bool Success, string Message) StartService(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    return (true, "ALREADY_RUNNING");
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                return (true, "RUNNING");
            }
            catch (Exception ex)
            {
                try
                {
                    RunScCommand($"start {serviceName}");
                    return (true, "START_SENT_VIA_SC");
                }
                catch (Exception scEx)
                {
                    return (false, "FAILED: " + ex.Message + " | SC: " + scEx.Message);
                }
            }
        }

        private static void RunScCommand(string args)
        {
            var psi = new ProcessStartInfo("sc.exe", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(3000);
        }
    }
}
