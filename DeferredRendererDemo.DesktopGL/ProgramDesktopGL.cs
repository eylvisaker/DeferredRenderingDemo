using DeferredRendererDemo;
using System;

namespace DeferredRendererDemoDesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameMain())
            {
                game.Run();
            }
        }
    }
}
