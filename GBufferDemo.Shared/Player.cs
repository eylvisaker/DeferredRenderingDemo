using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo
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
            Vector3 right = Vector3.Cross(Facing, Up);

            Vector3 moveFacing = Facing;
            Vector3 moveRight = right;

            moveFacing.Z = 0;
            moveRight.Z = 0;

            float moveSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 40;

            if (moveFacing.LengthSquared() > 1e-4)
            {
                moveFacing.Normalize();
                moveRight.Normalize();

                Position += moveSpeed * (moveInput.X * moveRight + moveInput.Y * moveFacing);
            }

            Position = new Vector3(Position.X,
                                   Position.Y,
                                   Position.Z + 0.5f * moveSpeed * (gamePadState.Triggers.Right - gamePadState.Triggers.Left));

            if (lookInput.LengthSquared() > 1e-4)
            {
                float lookSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4.2f;

                right.Z = 0;
                right.Normalize();

                facing += lookSpeed * (lookInput.X * right + lookInput.Y * Vector3.UnitZ);
                facing.Normalize();

                up = Vector3.Cross(right, Facing);
                up.Normalize();
            }

            if (gamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                up = Vector3.UnitZ;
                facing = Vector3.UnitX + Vector3.UnitY;
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
