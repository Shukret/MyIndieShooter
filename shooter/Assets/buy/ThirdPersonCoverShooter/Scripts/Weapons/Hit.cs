using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Description of a bullet hit. Used when passed to OnHit events.
    /// </summary>
    public struct Hit
    {
        /// <summary>
        /// Position of the hit in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Normal of the hit in world space. Faces outwards from the hit object.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Damage dealt to the impacted object.
        /// </summary>
        public float Damage;

        /// <summary>
        /// Owner of the weapon that caused the hit.
        /// </summary>
        public GameObject Attacker;

        /// <summary>
        /// Object that was hit.
        /// </summary>
        public GameObject Target;

        /// <summary>
        /// Create a bullet hit description.
        /// </summary>
        /// <param name="position">Position of the hit in world space.</param>
        /// <param name="normal">Normal of the hit in world space. Faces outwards from the hit object.</param>
        /// <param name="damage">Damage dealt to the impacted object.</param>
        /// <param name="attacker">Owner of the weapon that caused the hit.</param>
        /// <param name="target">Object that was hit.</param>
        public Hit(Vector3 position, Vector3 normal, float damage, GameObject attacker, GameObject target)
        {
            Position = position;
            Normal = normal;
            Damage = damage;
            Attacker = attacker;
            Target = target;
        }
    }
}