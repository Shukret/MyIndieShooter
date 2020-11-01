using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Light))]
    public class Flashlight : MonoBehaviour
    {
        /// <summary>
        /// Is the flashlight turned on.
        /// </summary>
        public bool IsTurnedOn
        {
            get { return _isOn; }
        }

        private bool _isOn;
        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        /// <summary>
        /// Turns on the flashlight.
        /// </summary>
        public void OnStartUsing()
        {
            _isOn = true;
        }

        /// <summary>
        /// Turns off the flashlight. Usually called by the end of an animation.
        /// </summary>
        public void OnUsed()
        {
            _isOn = false;
        }

        private void Update()
        {
            _light.enabled = _isOn;
        }
    }
}
