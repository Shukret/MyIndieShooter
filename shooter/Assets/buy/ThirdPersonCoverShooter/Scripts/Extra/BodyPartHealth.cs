using UnityEngine;

namespace CoverShooter
{
    public class BodyPartHealth : MonoBehaviour
    {
        /// <summary>
        /// By default target is the first found parent object with CharacterHealth. Setting TargetOverride overrides it.
        /// </summary>
        [Tooltip("By default target is the first found parent object with CharacterHealth. Setting TargetOverride overrides it.")]
        public CharacterHealth TargetOveride;

        /// <summary>
        /// Multiplies taken damage before applying it to the target CharacterHealth.
        /// </summary>
        [Tooltip("Multiplies taken damage before applying it to the target CharacterHealth.")]
        public float DamageScale = 1.0f;

        private CharacterHealth _target;

        /// <summary>
        /// Reduce health on bullet hit.
        /// </summary>
        public void OnHit(Hit hit)
        {
            var target = TargetOveride;

            if (target == null)
                target = _target;

            if (target == null)
            {
                var obj = transform;

                while (obj != null && target == null)
                {
                    target = obj.GetComponent<CharacterHealth>();
                    obj = obj.transform.parent;
                }

                _target = target;
            }

            if (target == null)
                return;

            hit.Damage *= DamageScale;

            target.SendMessage("OnHit",
                               hit,
                               SendMessageOptions.DontRequireReceiver);
        }
    }
}
