using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib
{
    public class Player
    {
        public static readonly Vector3 StartPosition = new Vector3(-10, -10, 6);

        public Vector3 Position { get; set; } = StartPosition;

        public Vector3 Facing { get; set; } = Vector3.UnitX + Vector3.UnitY;

        public Vector3 Up { get; set; } = Vector3.UnitZ;

        public void Update(GameTime gameTime, GamePadState gamePadState)
        {
            Vector2 moveInput = gamePadState.ThumbSticks.Left;
            Vector2 lookInput = gamePadState.ThumbSticks.Right;
            Vector3 facing = Facing;
            Vector3 up = Up;

            if (moveInput.LengthSquared() > 1e-4 || lookInput.LengthSquared() > 1e-4)
            {
                float moveSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 10;
                float lookSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;

                Vector3 right = Vector3.Cross(Facing, Up);
                right.Normalize();

                Position += moveSpeed * (moveInput.X * right + moveInput.Y * Facing);

                facing += lookSpeed * (lookInput.X * right + lookInput.Y * Up);
                facing.Normalize();

                up = Vector3.Cross(right, Facing);
                up.Normalize();
            }

            if (gamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                up = Vector3.UnitZ;
                facing.Z = 0;
            }
            if (gamePadState.Buttons.LeftStick == ButtonState.Pressed)
            {
                Position = StartPosition;
            }

            Facing = facing;
            Up = up;
        }
    }
}
