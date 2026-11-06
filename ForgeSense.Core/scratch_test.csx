using System;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;

var enumerator = new MMDeviceEnumerator();
var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
Console.WriteLine($"Default Device: {device.FriendlyName}");

var capture = new WasapiLoopbackCapture(device);
int packets = 0;
capture.DataAvailable += (s, e) => {
    packets++;
};
capture.StartRecording();
Console.WriteLine("Started recording...");
Thread.Sleep(3000);
capture.StopRecording();
Console.WriteLine($"Recorded {packets} packets in 3 seconds.");
