using System;
using UnityEngine;

namespace CoverShooter
{
    [Serializable]
    public struct GrenadeSettings
    {
        /// <summary>
        /// Grenade to turn on and off and clone when throwing from left hand.
        /// </summary>
        [Tooltip("Grenade to turn on and off and clone when throwing from left hand.")]
        public GameObject Left;

        /// <summary>
        /// Grenade to turn on and off and clone when throwing from right hand.
        /// </summary>
        [Tooltip("Grenade to turn on and off and clone when throwing from right hand.")]
        public GameObject Right;

        /// <summary>
        /// Maximum allowed initial grenade velocity.
        /// </summary>
        [Tooltip("Maximum allowed initial grenade velocity.")]
        public float MaxVelocity;

        /// <summary>
        /// Gravity applied to the grenade. Positive values point down.
        /// </summary>
        [Tooltip("Gravity applied to the grenade. Positive values point down.")]
        public float Gravity;

        /// <summary>
        /// Time in seconds between each calculated point in the arc.
        /// </summary>
        [Tooltip("Time in seconds between each calculated point in the arc.")]
        public float Step;

        /// <summary>
        /// Origin of grenade path relative to the feet when standing. Inverted for the left hand.
        /// </summary>
        [Tooltip("Origin of grenade path relative to the feet when standing. Inverted for the left hand.")]
        public Vector3 StandingOrigin;

        /// <summary>
        /// Origin of grenade path relative to the feet when crouching. Inverted for the left hand.
        /// </summary>
        [Tooltip("Origin of grenade path relative to the feet when crouching. Inverted for the left hand.")]
        public Vector3 CrouchOrigin;

        /// <summary>
        /// Default grenade settings.
        /// </summary>
        public static GrenadeSettings Default()
        {
            var settings = new GrenadeSettings();
            settings.MaxVelocity = 10;
            settings.Gravity = 12.5f;
            settings.Step = 0.05f;
            settings.StandingOrigin = new Vector3(0.33f, 1.88f, 0);
            settings.CrouchOrigin = new Vector3(0.5f, 1.43f, 0);

            return settings;
        }
    }
}
