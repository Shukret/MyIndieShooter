using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Maintains information about the gun and it's ammo.
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class GunAmmo : MonoBehaviour
    {
        /// <summary>
        /// Character whose gun information is displayed.
        /// </summary>
        [Tooltip("")]
        public CharacterMotor Motor;

        /// <summary>
        /// Determines if the display is hidden when the motor is dead.
        /// </summary>
        [Tooltip("Determines if the display is hidden when the motor is dead.")]
        public bool HideWhenDead = true;

        /// <summary>
        /// Determines if the display is hidden when there is no gun.
        /// </summary>
        [Tooltip("Determines if the display is hidden when there is no gun.")]
        public bool HideWhenNone = false;

        private Text _text;
        int bul;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }


        private void LateUpdate()
        {
            if (Motor != null && Motor.Gun != null)
            {
                bul = Motor.Gun.ClipSize-Motor.Gun.Clip;
                _text.text = Motor.Gun.Name + " " + Motor.Gun.Clip.ToString() + " | " + bul.ToString();
            }

            if (Application.isPlaying)
            {
                var isVisible = true;

                if (Motor == null)
                    isVisible = !HideWhenNone;
                else
                    isVisible = (Motor.IsAlive || !HideWhenDead) && (Motor.Gun != null || !HideWhenNone);

                _text.enabled = isVisible;
            }
        }
    }
}