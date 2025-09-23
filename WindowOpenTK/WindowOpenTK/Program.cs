using System;
using WindowOpenTk;
using WindowOpenTK;

namespace WindowOpenTk
{
    //Main entry point for c# console
    class Program
    {
        static void Main(String[] args)
        {
            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
}