using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Recoil settings for a gun.
    /// </summary>
    [Serializable]
    public struct RecoilSettings
    {
        /// <summary>
        /// Strength of a single bullet fire.
        /// </summary>
        [Tooltip("Strength of a single bullet fire.")]
        [Range(0, 1)]
        public float Strength;

        /// <summary>
        /// Time in seconds it takes to move the right hand by recoil.
        /// </summary>
        [Tooltip("Time in seconds it takes to move the right hand by recoil.")]
        [Range(0, 1)]
        public float AttackTime;

        /// <summary>
        /// Time in seconds it takes for the right hand to return to its position.
        /// </summary>
        [Tooltip("Time in seconds it takes for the right hand to return to its position.")]
        [Range(0, 1)]
        public float DecayTime;

        /// <summary>
        /// How much the gun is lifted up.
        /// </summary>
        [Tooltip("How much the gun is lifted up.")]
        [Range(-5, 5)]
        public float UpForce;

        /// <summary>
        /// Limits the total amount of recoil applied to the gun.
        /// </summary>
        [Tooltip("Limits the total amount of recoil applied to the gun.")]
        [Range(0, 1)]
        public float Limit;

        /// <summary>
        /// Default recoil settings.
        /// </summary>
        public static RecoilSettings Default()
        {
            var settings = new RecoilSettings();
            settings.Strength = 0.1f;
            settings.AttackTime = 0.05f;
            settings.DecayTime = 0.25f;
            settings.UpForce = 0.5f;
            settings.Limit = 0.1f;

            return settings;
        }
    }
}