using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo
{
    public static class Kbrd
    {
        private static KeyboardState lastKeyboard;
        private static KeyboardState keyboard;

        public static void Update()
        {
            lastKeyboard = keyboard;
            keyboard = Keyboard.GetState();
        }

        public static bool KeyReleased(Keys key)
        {
            return (lastKeyboard.IsKeyDown(key) && !keyboard.IsKeyDown(key));
        }

        public static bool IsKeyDown(Keys key) => keyboard.IsKeyDown(key);
    }
}
