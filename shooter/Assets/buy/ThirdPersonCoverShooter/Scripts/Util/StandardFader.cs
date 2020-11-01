using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Renderer))]
    public class StandardFader : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void OnFade()
        {
            _renderer.material.renderQueue = 3000;
            _renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _renderer.material.SetInt("_ZWrite", 0);
            _renderer.material.DisableKeyword("_ALPHATEST_ON");
            _renderer.material.EnableKeyword("_ALPHABLEND_ON");
            _renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        public void OnUnfade()
        {
            _renderer.material.renderQueue = -1;
            _renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            _renderer.material.SetInt("_ZWrite", 1);
            _renderer.material.DisableKeyword("_ALPHATEST_ON");
            _renderer.material.DisableKeyword("_ALPHABLEND_ON");
            _renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
}
