using System;
using System.Diagnostics;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = args[0];
            int port = Convert.ToInt32(args[1]);

            Client client = new Client(1, "connect_key", host, port);
            client.Start();

            int tickRate = 10;
            double timePerTick = 1000.0D / tickRate;

            Stopwatch stopwatch = new Stopwatch();

            bool active = true;
            while (active)
            {
                stopwatch.Reset();
                stopwatch.Start();

                client.Update();

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        active = false;
                    }
                }

                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;

                if (elapsed < timePerTick)
                {
                    System.Threading.Thread.Sleep((int)(timePerTick - elapsed));
                }
            }

            client.Stop();

            Console.WriteLine("Quit. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
