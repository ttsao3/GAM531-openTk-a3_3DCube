using System;
using WindowOpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace WindowOpenTk
{
    //Main entry point for c# console
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "GAM531 midterm game",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var game = new Game(GameWindowSettings.Default, nativeWindowSettings))
            {
                game.Run();
            }
        }
    }
}