using GBufferDemoLib;
using System;

namespace GBufferDemoDesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameMain())
            {
                game.OpenGL = true;
                game.Run();
            }
        }
    }
}
