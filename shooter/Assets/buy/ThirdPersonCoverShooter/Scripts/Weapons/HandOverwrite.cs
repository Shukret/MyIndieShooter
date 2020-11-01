using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Defines hand position markers to overwrite the default marker.
    /// </summary>
    [Serializable]
    public struct HandOverwrite
    {
        /// <summary>
        /// Marker to use when the character is aiming.
        /// </summary>
        [Tooltip("Marker to use when the character is aiming.")]
        public Transform Aim;

        /// <summary>
        /// Marker to use when the character is standing in a low cover facing left.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a low cover facing left.")]
        public Transform LowCoverLeft;

        /// <summary>
        /// Marker to use when the character is standing in a low cover facing right.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a low cover facing right.")]
        public Transform LowCoverRight;

        /// <summary>
        /// Marker to use when the character is standing in a tall cover facing left.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a tall cover facing left.")]
        public Transform TallCoverLeft;

        /// <summary>
        /// Marker to use when the character is standing in a tall cover facing right.
        /// </summary>
        [Tooltip("Marker to use when the character is standing in a tall cover facing right.")]
        public Transform TallCoverRight;
    }
}
