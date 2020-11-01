using UnityEngine;

namespace CoverShooter
{
    public static class CameraManager
    {
        public static Camera Main
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;

                return _camera;
            }
        }

        private static Camera _camera;

        public static void Update()
        {
            _camera = Camera.main;
        }
    }
}
