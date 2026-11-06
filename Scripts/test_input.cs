using System;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    static void Main()
    {
        Console.WriteLine("Press 'A' key (Virtual Key 0x41) to test GetAsyncKeyState...");
        int count = 0;
        while (count < 100) // Run for 10 seconds (100 * 100ms)
        {
            short state = GetAsyncKeyState(0x41);
            if ((state & 0x8000) != 0)
            {
                Console.WriteLine("A key is pressed!");
                break;
            }
            Thread.Sleep(100);
            count++;
        }
        Console.WriteLine("Test finished.");
    }
}
