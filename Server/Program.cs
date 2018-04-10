using System;
using System.Diagnostics;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = Convert.ToInt32(args[0]);

            Server server = new Server(10000, "connect_key");
            server.Start(port);

            int tickRate = 10;
            double timePerTick = 1000.0D / tickRate;

            Stopwatch stopwatch = new Stopwatch();

            bool active = true;
            while (active)
            {
                stopwatch.Reset();
                stopwatch.Start();

                server.Update();

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

            server.Stop();

            Console.WriteLine("Quit. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
