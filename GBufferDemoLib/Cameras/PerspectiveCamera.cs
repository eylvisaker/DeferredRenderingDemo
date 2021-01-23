using Microsoft.Xna.Framework;

namespace GBufferDemoLib.Cameras
{
    public class PerspectiveCamera : Camera
    {
        private float _fieldOfView;
        private float _aspectRatio;

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                CreateProjection();
            }
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                _aspectRatio = value;
                CreateProjection();
            }
        }

        public PerspectiveCamera(float fieldOfView, float aspectRatio, float nearZ, float farZ)
            : base(nearZ, farZ)
        {
            _fieldOfView = fieldOfView;
            _aspectRatio = aspectRatio;

            CreateProjection();
        }

        protected override void CreateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, NearZ, FarZ);
        }
    }
}