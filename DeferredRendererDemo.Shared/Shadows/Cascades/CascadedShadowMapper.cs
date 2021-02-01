using DeferredRendererDemo.Cameras;
using DeferredRendererDemo.Lights;
using DeferredRendererDemo.Shadows.Cascades.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.Shadows
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Algorithm adapted from https://github.com/tgjones/monogame-samples
    /// </remarks>
    public class CascadedShadowMapper
    {
        public const int NumCascades = 4;

        private const int ShadowMapSize = 2048;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly ShadowSettings _settings;

        private RenderTarget2D _shadowMaps;

        private readonly float[] _cascadeSplits;
        private readonly Vector3[] _frustumCorners;

        private readonly ShadowMapEffect _shadowMapEffect;

        private readonly BoundingFrustum _boundingFrustum;


        public float[] CascadeSplits { get; private set; }
        public Vector4[] CascadeOffsets { get; private set; }
        public Vector4[] CascadeScales { get; private set; }

        /// <summary>
        /// Gets the global shadow matrix.
        /// </summary>
        public Matrix ShadowMatrix { get; private set; }

        public ShadowSettings Settings => _settings;
        public RenderTarget2D ShadowMaps => _shadowMaps;

        public CascadedShadowMapper(GraphicsDevice graphicsDevice,
                                    ShadowSettings settings,
                                    ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _settings = settings;

            _cascadeSplits = new float[4];
            _frustumCorners = new Vector3[8];

            _shadowMapEffect = new ShadowMapEffect(contentManager.Load<Effect>("CascadeShadowMap"));

            _boundingFrustum = new BoundingFrustum(Matrix.Identity);

            CreateShadowMaps();
        }

        private void CreateShadowMaps()
        {
            _shadowMaps?.Dispose();

            CascadeSplits = new float[NumCascades];
            CascadeOffsets = new Vector4[NumCascades];
            CascadeScales = new Vector4[NumCascades];

            _shadowMaps = new RenderTarget2D(_graphicsDevice,
                                             ShadowMapSize,
                                             ShadowMapSize,
                                             false,
                                             SurfaceFormat.Single,
                                             DepthFormat.Depth24,
                                             0,
                                             RenderTargetUsage.DiscardContents,
                                             false,
                                             4);
        }

        public void RenderShadowMap(GraphicsDevice graphicsDevice, LightDirectional sunLight, Camera camera, Action<Camera, ShadowMapEffect> renderScene)
        {
            // Set cascade split ratios.
            _cascadeSplits[0] = _settings.SplitDistance0;
            _cascadeSplits[1] = _settings.SplitDistance1;
            _cascadeSplits[2] = _settings.SplitDistance2;
            _cascadeSplits[3] = _settings.SplitDistance3;

            var globalShadowMatrix = MakeGlobalShadowMatrix(camera, sunLight);
            ShadowMatrix = globalShadowMatrix;

            // Render the meshes to each cascade.
            for (var cascadeIdx = 0; cascadeIdx < NumCascades; ++cascadeIdx)
            {
                // Set the shadow map as the render target
                graphicsDevice.SetRenderTarget(_shadowMaps, cascadeIdx);
                graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

                // Get the 8 points of the view frustum in world space
                ResetViewFrustumCorners();

                var prevSplitDist = cascadeIdx == 0 ? 0.0f : _cascadeSplits[cascadeIdx - 1];
                var splitDist = _cascadeSplits[cascadeIdx];

                var invViewProj = Matrix.Invert(camera.ViewProjection);
                for (var i = 0; i < 8; ++i)
                    _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();

                // Get the corners of the current cascade slice of the view frustum
                for (var i = 0; i < 4; ++i)
                {
                    var cornerRay = _frustumCorners[i + 4] - _frustumCorners[i];
                    var nearCornerRay = cornerRay * prevSplitDist;
                    var farCornerRay = cornerRay * splitDist;
                    _frustumCorners[i + 4] = _frustumCorners[i] + farCornerRay;
                    _frustumCorners[i] = _frustumCorners[i] + nearCornerRay;
                }

                // Calculate the centroid of the view frustum slice
                var frustumCenter = Vector3.Zero;
                for (var i = 0; i < 8; ++i)
                    frustumCenter = frustumCenter + _frustumCorners[i];
                frustumCenter /= 8.0f;

                // Pick the up vector to use for the light camera
                var upDir = camera.Right;

                Vector3 minExtents;
                Vector3 maxExtents;

                if (_settings.StabilizeCascades)
                {
                    // This needs to be constant for it to be stable
                    upDir = Vector3.Up;

                    // Calculate the radius of a bounding sphere surrounding the frustum corners
                    var sphereRadius = 0.0f;
                    for (var i = 0; i < 8; ++i)
                    {
                        var dist = (_frustumCorners[i] - frustumCenter).Length();
                        sphereRadius = Math.Max(sphereRadius, dist);
                    }

                    sphereRadius = (float)Math.Ceiling(sphereRadius * 16.0f) / 16.0f;

                    maxExtents = new Vector3(sphereRadius);
                    minExtents = -maxExtents;
                }
                else
                {
                    // Create a temporary view matrix for the light
                    var lightCameraPos = frustumCenter;
                    var lookAt = frustumCenter - sunLight.DirectionToLight;
                    var lightView = Matrix.CreateLookAt(lightCameraPos, lookAt, upDir);

                    // Calculate an AABB around the frustum corners
                    var mins = new Vector3(float.MaxValue);
                    var maxes = new Vector3(float.MinValue);
                    for (var i = 0; i < 8; ++i)
                    {
                        var corner = Vector4.Transform(_frustumCorners[i], lightView).ToVector3();
                        mins = Vector3.Min(mins, corner);
                        maxes = Vector3.Max(maxes, corner);
                    }

                    minExtents = mins;
                    maxExtents = maxes;

                    // Adjust the min/max to accommodate the filtering size
                    var scale = (ShadowMapSize + _settings.FixedFilterKernelSize) / (float)ShadowMapSize;
                    minExtents.X *= scale;
                    minExtents.Y *= scale;
                    maxExtents.X *= scale;
                    maxExtents.Y *= scale;
                }

                var cascadeExtents = maxExtents - minExtents;

                // Get position of the shadow camera
                var shadowCameraPos = frustumCenter + sunLight.DirectionToLight * -minExtents.Z;

                // Come up with a new orthographic camera for the shadow caster
                var shadowCamera = new OrthographicCamera(
                    minExtents.X, minExtents.Y, maxExtents.X, maxExtents.Y,
                    0.0f, cascadeExtents.Z);
                shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

                if (_settings.StabilizeCascades)
                {
                    // Create the rounding matrix, by projecting the world-space origin and determining
                    // the fractional offset in texel space
                    var shadowMatrixTemp = shadowCamera.ViewProjection;
                    var shadowOrigin = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                    shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
                    shadowOrigin = shadowOrigin * (ShadowMapSize / 2.0f);

                    var roundedOrigin = Vector4Utility.Round(shadowOrigin);
                    var roundOffset = roundedOrigin - shadowOrigin;
                    roundOffset = roundOffset * (2.0f / ShadowMapSize);
                    roundOffset.Z = 0.0f;
                    roundOffset.W = 0.0f;

                    var shadowProj = shadowCamera.Projection;
                    //shadowProj.r[3] = shadowProj.r[3] + roundOffset;
                    shadowProj.M41 += roundOffset.X;
                    shadowProj.M42 += roundOffset.Y;
                    shadowProj.M43 += roundOffset.Z;
                    shadowProj.M44 += roundOffset.W;
                    shadowCamera.Projection = shadowProj;
                }

                // Draw the mesh with depth only, using the new shadow camera
                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = CreateShadowMapRasterizerState;

                _shadowMapEffect.View = shadowCamera.View;
                _shadowMapEffect.Projection = shadowCamera.Projection;
                renderScene(shadowCamera, _shadowMapEffect);

                // Apply the scale/offset matrix, which transforms from [-1,1]
                // post-projection space to [0,1] UV space
                var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f)
                    * Matrix.CreateTranslation(0.5f, 0.5f, 0.0f);
                var shadowMatrix = shadowCamera.ViewProjection;
                shadowMatrix = shadowMatrix * texScaleBias;

                // Store the split distance in terms of view space depth
                var clipDist = camera.FarZ - camera.NearZ;

                CascadeSplits[cascadeIdx] = camera.NearZ + splitDist * clipDist;

                // Calculate the position of the lower corner of the cascade partition, in the UV space
                // of the first cascade partition
                var invCascadeMat = Matrix.Invert(shadowMatrix);
                var cascadeCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
                cascadeCorner = Vector4.Transform(cascadeCorner, globalShadowMatrix).ToVector3();

                // Do the same for the upper corner
                var otherCorner = Vector4.Transform(Vector3.One, invCascadeMat).ToVector3();
                otherCorner = Vector4.Transform(otherCorner, globalShadowMatrix).ToVector3();

                // Calculate the scale and offset
                var cascadeScale = Vector3.One / (otherCorner - cascadeCorner);
                CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
                CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
            }
        }

        private void ResetViewFrustumCorners()
        {
            _frustumCorners[0] = new Vector3(-1.0f, 1.0f, 0.0f);
            _frustumCorners[1] = new Vector3(1.0f, 1.0f, 0.0f);
            _frustumCorners[2] = new Vector3(1.0f, -1.0f, 0.0f);
            _frustumCorners[3] = new Vector3(-1.0f, -1.0f, 0.0f);
            _frustumCorners[4] = new Vector3(-1.0f, 1.0f, 1.0f);
            _frustumCorners[5] = new Vector3(1.0f, 1.0f, 1.0f);
            _frustumCorners[6] = new Vector3(1.0f, -1.0f, 1.0f);
            _frustumCorners[7] = new Vector3(-1.0f, -1.0f, 1.0f);
        }

        /// <summary>
        /// Makes the "global" shadow matrix used as the reference point for the cascades.
        /// </summary>
        private Matrix MakeGlobalShadowMatrix(Camera camera, LightDirectional sunLight)
        {
            // Get the 8 points of the view frustum in world space
            ResetViewFrustumCorners();

            var invViewProj = Matrix.Invert(camera.ViewProjection);
            var frustumCenter = Vector3.Zero;
            for (var i = 0; i < 8; i++)
            {
                _frustumCorners[i] = Vector4.Transform(_frustumCorners[i], invViewProj).ToVector3();
                frustumCenter += _frustumCorners[i];
            }

            frustumCenter /= 8.0f;

            // Pick the up vector to use for the light camera
            var upDir = camera.Right;

            // This needs to be constant for it to be stable
            if (_settings.StabilizeCascades)
                upDir = Vector3.Up;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter + sunLight.DirectionToLight * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            var shadowCamera = new OrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

            var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f);
            texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
            return shadowCamera.ViewProjection * texScaleBias;
        }

        //public void Render(GraphicsDevice graphicsDevice, Camera camera, Matrix worldMatrix)
        //{
        //    // Render scene.

        //    graphicsDevice.BlendState = BlendState.Opaque;
        //    graphicsDevice.DepthStencilState = DepthStencilState.Default;
        //    graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        //    graphicsDevice.SamplerStates[0] = ShadowMapSamplerState;

        //    _meshEffect.VisualizeCascades = _settings.VisualizeCascades;
        //    _meshEffect.FilterAcrossCascades = _settings.FilterAcrossCascades;
        //    _meshEffect.FilterSize = _settings.FixedFilterSize;
        //    _meshEffect.Bias = _settings.Bias;
        //    _meshEffect.OffsetScale = _settings.OffsetScale;

        //    _meshEffect.ViewProjection = camera.ViewProjection;
        //    _meshEffect.CameraPosWS = camera.Position;

        //    _meshEffect.ShadowMap = _shadowMap;

        //    _meshEffect.LightDirection = _settings.LightDirection;
        //    _meshEffect.LightColor = _settings.LightColor;

        //    _boundingFrustum.Matrix = camera.ViewProjection;

        //    // Draw all meshes.
        //    foreach (var mesh in _scene.Meshes)
        //    {
        //        if (!_boundingFrustum.Intersects(mesh.BoundingSphere))
        //            continue;

        //        foreach (var meshPart in mesh.MeshParts)
        //            if (meshPart.PrimitiveCount > 0)
        //            {
        //                var basicEffect = (BasicEffect)meshPart.Effect;

        //                _meshEffect.DiffuseColor = basicEffect.DiffuseColor;
        //                _meshEffect.World = _sceneTransforms[mesh.ParentBone.Index] * worldMatrix;
        //                _meshEffect.Apply();

        //                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
        //                graphicsDevice.Indices = meshPart.IndexBuffer;

        //                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
        //                    meshPart.VertexOffset, 0, meshPart.PrimitiveCount);
        //            }
        //    }
        //}


        private static readonly RasterizerState CreateShadowMapRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            DepthClipEnable = false
        };

        public static readonly SamplerState ShadowMapSamplerState = new SamplerState
        {
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            Filter = TextureFilter.Linear,
            ComparisonFunction = CompareFunction.LessEqual,
            FilterMode = TextureFilterMode.Comparison
        };
    }
}
