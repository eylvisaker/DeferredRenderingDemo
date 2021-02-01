using DeferredRendererDemo.DeferredRendering.Effects;
using DeferredRendererDemo.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeferredRendererDemo
{
    public class Sky
    {
        private readonly GraphicsDevice graphics;
        private ContentManager content;
        private Model sphere;
        private Texture2D alignment;
        private Texture2D blueSky;
        private Texture2D sunrise;
        private Texture2D nightSky;
        private BackgroundEffect effect;

        public Sky(GraphicsDevice graphics, ContentManager content)
        {
            this.graphics = graphics;
            this.content = content;

            sphere = content.Load<Model>("gbuffer/sphere");
            alignment = content.Load<Texture2D>("sky/alignment");
            blueSky = content.Load<Texture2D>("sky/bluesky");
            sunrise = content.Load<Texture2D>("sky/sunrise");
            nightSky = content.Load<Texture2D>("sky/nightsky");

            Sun.Texture = content.Load<Texture2D>("white");
            Sun.Light.EnableShadows = true;
        }

        public BackgroundEffect Effect
        {
            get => effect;
            set
            {
                effect = value;
                effect.PrepModel(sphere);
            }
        }

        public float NightSkyIntensity { get; set; } = 0.01f;

        public CelestialBody Sun { get; set; } = new CelestialBody();

        public float NightSkyRotation { get; set; }

        public void Draw(Vector3 position, float farPlane)
        {
            farPlane *= 0.99f;

            graphics.BlendState = BlendState.Opaque;
            graphics.RasterizerState = RasterizerState.CullNone;

            DrawSky(position, farPlane);
            DrawCelestialBody(position, farPlane, Sun);

            graphics.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private void DrawSky(Vector3 position, float farPlane)
        {
            DrawNightSky(position, farPlane);
            DrawDaySky(position, farPlane);
            DrawRiseSet(position, farPlane);
        }

        private void DrawRiseSet(Vector3 position, float farPlane)
        {
            float red = Sun.Red;

            graphics.BlendState = BlendState.AlphaBlend;

            if (red <= 0)
                return;

            float rotZ = (float)Math.Atan2(Sun.DirectionTo.Y, Sun.DirectionTo.X);
            Vector3 riseAxis = new Vector3(-Sun.DirectionTo.Y, Sun.DirectionTo.X, 0);
            Quaternion rise = Quaternion.CreateFromAxisAngle(riseAxis, -(float)Math.Asin(Sun.DirectionTo.Z));

            Effect.BackgroundTexture = sunrise;
            Effect.World = Matrix.Identity
                         * Matrix.CreateRotationZ(rotZ)
                         * Matrix.CreateFromQuaternion(rise)
                         * Matrix.CreateScale(farPlane)
                         * Matrix.CreateTranslation(position);

            // Don't allow src alpha to exceed 1.
            Vector4 color = Color.White.ToVector4() * Sun.RedIntensity;
            color.W = red;

            Effect.Parameters["Color"].SetValue(color);

            foreach (ModelMesh mesh in sphere.Meshes)
            {
                mesh.Draw();
            }
        }

        private void DrawDaySky(Vector3 position, float farPlane)
        {
            float blue = Sun.Blue;

            graphics.BlendState = BlendState.Additive;

            if (blue <= 0)
                return;

            Effect.BackgroundTexture = blueSky;
            Effect.World = Matrix.CreateScale(farPlane)
                         * Matrix.CreateTranslation(position);

            // Don't allow src alpha to exceed 1.
            Vector4 color = Color.White.ToVector4() * Sun.BlueIntensity;
            color.W = blue;

            Effect.Parameters["Color"].SetValue(color);

            foreach (ModelMesh mesh in sphere.Meshes)
            {
                mesh.Draw();
            }
        }

        private void DrawNightSky(Vector3 position, float farPlane)
        {
            Vector3 axis = new Vector3(0, 1, 1);
            axis.Normalize();

            Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, NightSkyRotation);
            Matrix rotationMatrix = Matrix.CreateFromQuaternion(rotation);

            Effect.BackgroundTexture = nightSky;
            Effect.World = rotationMatrix
                         * Matrix.CreateScale(farPlane)
                         * Matrix.CreateTranslation(position);

            Effect.Parameters["Color"].SetValue(Color.White.ToVector3() * NightSkyIntensity);

            foreach (ModelMesh mesh in sphere.Meshes)
            {
                mesh.Draw();
            }
        }

        private void DrawCelestialBody(Vector3 position, float farPlane, CelestialBody body)
        {
            Vector3 sunDelta = farPlane * body.DirectionTo;
            Vector3 sunPos = position + sunDelta;

            float scale = 0.01f * farPlane * body.Scale * (1 + 0.5f * body.Red);
            Vector4 color = body.Color.ToVector4();

            color *= body.BodyIntensity * 2;
            color.W = 1;

            Effect.BackgroundTexture = body.Texture;
            Effect.World = Matrix.CreateScale(scale)
                         * Matrix.CreateTranslation(sunPos);

            Effect.Parameters["Color"].SetValue(color);

            foreach (ModelMesh mesh in sphere.Meshes)
            {
                mesh.Draw();
            }
        }
    }

    public class CelestialBody
    {
        public LightDirectional Light { get; set; } = new LightDirectional();

        public Vector3 DirectionTo
        {
            get => Light.DirectionToLight;
            set
            {
                Light.DirectionToLight = Vector3.Normalize(value);

                CalcBlue();
                CalcRed();

                AmbientDown = new Color(10, 20, 30).ToVector3() * AmbientNightIntensity;
                AmbientUp = new Color(30, 40, 50).ToVector3() * AmbientNightIntensity;

                if (Blue <= 0)
                {
                    LightIntensity = 0;
                    BodyIntensity = MaxBodyIntensity * 0.1f;
                }
                else
                {
                    LightIntensity = Blue * MaxLightIntensity;

                    AmbientDown += new Color(10, 20, 30).ToVector3() * AmbientDayIntensity * Blue;
                    AmbientUp += new Color(40, 40, 40).ToVector3() * AmbientDayIntensity * Blue;

                    BodyIntensity = MaxBodyIntensity * (0.1f + 0.9f * (Math.Min(Blue, 1)));
                }

                Light.Intensity = LightIntensity;
            }
        }

        /// <summary>
        /// The intensity of the directional light provided by this celestial body.
        /// </summary>
        public float LightIntensity
        {
            get => Light.Intensity;
            private set => Light.Intensity = value;
        }

        public float AmbientDayIntensity { get; set; } = 0.15f;

        public float AmbientNightIntensity { get; set; } = 0.015f;

        public float MaxLightIntensity { get; set; } = 0.8f;

        /// <summary>
        /// The intensity of the sky at its brightest.
        /// </summary>
        public float MaxSkyIntensity { get; set; } = 0.3f;

        public Vector3 AmbientDown { get; internal set; }
        public Vector3 AmbientUp { get; internal set; }

        /// <summary>
        /// Returns how blue the sky should be if this celestial body has risen.
        /// </summary>
        /// <returns></returns>
        public float Blue { get; private set; }

        public float BlueIntensity { get; private set; }


        /// <summary>
        /// Returns how red the sky should be due to body rise/setting.
        /// </summary>
        public float Red { get; private set; }

        public float RedIntensity { get; private set; }

        /// <summary>
        /// The color of the celestial body in the sky
        /// </summary>
        public Color Color { get; set; } = new Color(255, 255, 128);
        
        public float BodyIntensity { get; private set; }
        public float MaxBodyIntensity { get; set; } = 0.9f;

        public Texture2D Texture { get; set; }

        public float Scale { get; set; } = 1f;

        private void CalcBlue()
        {
            float blueStart = -0.16f;
            float blueRange = 0.3f;

            float blueF = (DirectionTo.Z - blueStart) / blueRange;

            blueF = Math.Min(blueF, 1);
            blueF = Math.Max(blueF, 0);

            float blue = blueF * blueF;

            Blue = blue;
            BlueIntensity = blueF * MaxSkyIntensity;
        }

        private void CalcRed()
        {
            float redStart = -0.19f;
            float redRange = 0.29f;

            float redF = (DirectionTo.Z - redStart) / redRange;

            redF = Math.Min(redF, 1.1f);
            redF = Math.Max(redF, 0);

            float red = (float)Math.Sin(redF * MathHelper.Pi);
            red = Math.Max(red, 0);

            red *= red;

            redF = Math.Min(redF, 1f);

            Red = red;
            RedIntensity = redF * MaxSkyIntensity * (float)Math.Sqrt(red);
        }
    }
}