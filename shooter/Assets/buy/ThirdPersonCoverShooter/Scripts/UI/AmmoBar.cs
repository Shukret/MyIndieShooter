using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Displays ammo information in a bar and allows selection of the weapon by touch.
    /// </summary>
    [ExecuteInEditMode]
    public class AmmoBar : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// Gun oject whose ammo is displayed on the ammo bar.
        /// </summary>
        [Tooltip("Gun object whose ammo is displayed on the ammo bar.")]
        public Gun Target;

        /// <summary>
        /// Motor which will be assigned the weapon when pressed.
        /// </summary>
        [Tooltip("Motor which will be assigned the weapon when pressed.")]
        public CharacterMotor Motor;

        /// <summary>
        /// Current value of the ammo bar.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Current value of the health bar.")]
        public float Value = 1.0f;

        /// <summary>
        /// Determines if the ammo bar is hidden when there is no target.
        /// </summary>
        [Tooltip("Determines if the ammo bar is hidden when there is no target.")]
        public bool HideWhenNone = false;

        /// <summary>
        /// Link to the object that draws the background of the ammo bar.
        /// </summary>
        [Tooltip("Link to the object that draws the background of the ammo bar.")]
        public RectTransform BackgroundRect;

        /// <summary>
        /// Link to the object that draws the ammo bar.
        /// </summary>
        [Tooltip("Link to the object that draws the ammo bar.")]
        public RectTransform FillRect;

        /// <summary>
        /// Link to the icon object.
        /// </summary>
        [Tooltip("Link to the icon object.")]
        public RectTransform Icon;

        /// <summary>
        /// Link to the object that will be used to display a gun's name.
        /// </summary>
        [Tooltip("Link to the object that will be used to display a gun's name.")]
        public Text Name;

        /// <summary>
        /// Color to use on elements when the gun is selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is selected.")]
        public Color ActiveColor = Color.white;

        /// <summary>
        /// Color to use on elements when the gun is not selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is not selected.")]
        public Color InactiveColor = new Color(1, 1, 1, 0.6f);

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Motor != null)
            {
                Motor.InputCancelGrenade();
                Motor.Gun = Target;
            }
        }

        private void LateUpdate()
        {
            if (Target != null)
            {
                if (Target != null)
                    Value = (float)Target.Clip / (float)Target.ClipSize;

                if (Name != null)
                    Name.text = Target.name;
            }

            if (FillRect != null)
                FillRect.anchorMax = new Vector2(Value, 1);

            if (Application.isPlaying)
            {
                var isVisible = !HideWhenNone || Target != null;
                updateElement(FillRect, isVisible);
                updateElement(BackgroundRect, isVisible);
                updateElement(Icon, isVisible);
                updateElement(Name, isVisible);
            }
        }

        private void updateElement(RectTransform obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);

            if (isVisible)
            {
                var image = obj.GetComponent<Image>();

                if (Motor != null)
                    image.color = (Motor.Gun == Target && !Motor.HasGrenadeInHand) ? ActiveColor : InactiveColor;
            }
        }

        private void updateElement(Text obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);
        }
    }
}