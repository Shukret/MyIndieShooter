using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Character roll settings.
    /// </summary>
    [Serializable]
    public struct RollSettings
    {
        /// <summary>
        /// Character's capsule height during a roll.
        /// </summary>
        [Tooltip("Character's capsule height during a roll.")]
        [Range(0, 10)]
        public float CapsuleHeight;

        /// <summary>
        /// Duration of character's capsule height adjustment.
        /// </summary>
        [Tooltip("Duration of character's capsule height adjustment.")]
        [Range(0, 10)]
        public float HeightDuration;

        /// <summary>
        /// How fast the character turns towards the roll direction.
        /// </summary>
        [Tooltip("How fast the character turns towards the roll direction.")]
        public float RotationSpeed;

        /// <summary>
        /// Fraction of the roll animation that has to be played before the character can aim again. Applied when not in cover.
        /// </summary>
        [Tooltip("Fraction of the roll animation that has to be played before the character can aim again. Applied when not in cover.")]
        [Range(0, 1)]
        public float AnimationLock;

        /// <summary>
        /// Default jump settings.
        /// </summary>
        public static RollSettings Default()
        {
            RollSettings settings;
            settings.CapsuleHeight = 1.0f;
            settings.HeightDuration = 0.75f;
            settings.RotationSpeed = 10;
            settings.AnimationLock = 0.8f;

            return settings;
        }
    }
}