using System;
using UnityEngine;

namespace CoverShooter
{
    [Serializable]
    public struct TurnSettings
    {
        /// <summary>
        /// Maximum allowed angle between aim direction and body direction.
        /// </summary>
        [Tooltip("Maximum allowed angle between aim direction and body direction.")]
        public float MaxAimAngle;

        /// <summary>
        /// Controls whether weapons are aimed at a point in a world (the fire target when true) or at an infinite distance (the look target when false). 
        /// You want this turned off for player characters so that guns are stable and at the crosshair.
        /// Gameplay generally presents many situations where aiming at the fire target is impossible and so using it may look wrong.
        /// </summary>
        [Tooltip("Controls whether weapons are aimed at a point in a world (on) or at an infinite distance (off).")]
        public bool IsAimingPrecisely;

        /// <summary>
        /// Should the character turn towards the fire direction immediately instead of animating the turn.
        /// </summary>
        [Tooltip("Should the character turn towards the fire direction immediately instead of animating the turn.")]
        public bool ImmediateAim;

        /// <summary>
        /// How quickly the character model is orientated towards the standing direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the standing direction.")]
        [Range(0, 50)]
        public float StandingRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the running direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float RunningRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the running direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float SprintingRotationSpeed;

        /// <summary>
        /// How quickly the character model is orientated towards the throw direction.
        /// </summary>
        [Tooltip("How quickly the character model is orientated towards the throw direction.")]
        [Range(0, 50)]
        public float GrenadeRotationSpeed;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static TurnSettings Default()
        {
            var result = new TurnSettings();
            result.MaxAimAngle = 60;
            result.IsAimingPrecisely = false;
            result.ImmediateAim = false;
            result.StandingRotationSpeed = 5;
            result.RunningRotationSpeed = 5;
            result.SprintingRotationSpeed = 20;
            result.GrenadeRotationSpeed = 20;

            return result;
        }
    }
}
