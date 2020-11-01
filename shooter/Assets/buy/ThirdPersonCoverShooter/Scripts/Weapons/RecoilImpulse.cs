using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Information about a recoil impulse. Used by the Gun component.
    /// </summary>
    public struct RecoilImpulse
    {
        /// <summary>
        /// Direction of the recoil in world space.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Progress ranging in 0..1 of processing this recoil.
        /// </summary>
        public float Progress;

        /// <summary>
        /// Creates a recoil impulse.
        /// </summary>
        /// <param name="direction">Direction of the recoil in world space.</param>
        public RecoilImpulse(Vector3 direction)
        {
            Direction = direction;
            Progress = 0.0f;
        }
    }
}