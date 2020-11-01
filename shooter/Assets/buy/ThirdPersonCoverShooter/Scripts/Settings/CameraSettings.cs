using System;
using UnityEngine;

namespace CoverShooter
{
    [Serializable]
    public struct CameraStates
    {
        /// <summary>
        /// Camera state used when the character is unarmed.
        /// </summary>
        [Tooltip("Camera state used when the character is unarmed.")]
        public CameraState Default;

        /// <summary>
        /// Camera state to use when the character is standing and aiming.
        /// </summary>
        [Tooltip("Camera state to use when the character is standing and aiming.")]
        public CameraState Aim;

        /// <summary>
        /// Camera state to use when the character is crouching.
        /// </summary>
        [Tooltip("Camera state to use when the character is crouching.")]
        public CameraState Crouch;

        /// <summary>
        /// Camera state to use when the character is in low cover and not aiming.
        /// </summary>
        [Tooltip("Camera state to use when the character is in low cover and not aiming.")]
        public CameraState LowCover;

        /// <summary>
        /// Camera state to use when the character is low cover and grenade mode.
        /// </summary>
        [Tooltip("Camera state to use when the character is low cover and grenade mode.")]
        public CameraState LowCoverGrenade;

        /// <summary>
        /// Camera state to use when the character is in tall cover and facing left.
        /// </summary>
        [Tooltip("Camera state to use when the character is in tall cover and facing left.")]
        public CameraState TallCoverLeft;

        /// <summary>
        /// Camera state to use when the character is in tall cover and facing right.
        /// </summary>
        [Tooltip("Camera state to use when the character is in tall cover and facing right.")]
        public CameraState TallCoverRight;

        /// <summary>
        /// Camera state to use when the character is in tall cover and camera is facing back.
        /// </summary>
        [Tooltip("Camera state to use when the character is in tall cover and camera is facing back.")]
        public CameraState TallCoverBack;

        /// <summary>
        /// Camera state to use when the character is ready to fire from the left corner of a cover.
        /// </summary>
        [Tooltip("Camera state to use when the character is ready to fire from the left corner of a cover.")]
        public CameraState LeftCorner;

        /// <summary>
        /// Camera state to use when the character is ready to fire from the right corner of a cover.
        /// </summary>
        [Tooltip("Camera state to use when the character is ready to fire from the right corner of a cover.")]
        public CameraState RightCorner;

        /// <summary>
        /// Camera state to use when the character is climbing.
        /// </summary>
        [Tooltip("Camera state to use when the character is climbing.")]
        public CameraState Climb;

        /// <summary>
        /// Camera state to use when the character is dead.
        /// </summary>
        [Tooltip("Camera state to use when the character is dead.")]
        public CameraState Dead;

        /// <summary>
        /// Camera state to use when the character is standing and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is standing and using zoom.")]
        public CameraState Zoom;

        /// <summary>
        /// Camera state to use when the character is crouching and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is crouching and using zoom.")]
        public CameraState CrouchZoom;

        /// <summary>
        /// Camera state to use when the character is in low cover, facing towards it and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is in low cover, facing towards it and using zoom.")]
        public CameraState LowCoverZoom;

        /// <summary>
        /// Camera state to use when the character is in low cover, facing away from it and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is in low cover, facing away from it and using zoom.")]
        public CameraState LowCoverBackZoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a left corner in low cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a left corner in low cover and using zoom.")]
        public CameraState LeftLowCornerZoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a right corner in low cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a right corner in low cover and using zoom.")]
        public CameraState RightLowCornerZoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a left corner in tall cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a left corner in tall cover and using zoom.")]
        public CameraState LeftTallCornerZoom;

        /// <summary>
        /// Camera state to use when the character is aiming from a right corner in tall cover and using zoom.
        /// </summary>
        [Tooltip("Camera state to use when the character is aiming from a right corner in tall cover and using zoom.")]
        public CameraState RightTallCornerZoom;

        /// <summary>
        /// Default camera settings.
        /// </summary>
        public static CameraStates GetDefault()
        {
            var states = new CameraStates();
            states.Default = CameraState.Default();
            states.Aim = CameraState.Aim();
            states.Crouch = CameraState.Crouch();
            states.LowCover = CameraState.LowCover();
            states.LowCoverGrenade = CameraState.LowCoverGrenade();
            states.TallCoverLeft = CameraState.TallCoverLeft();
            states.TallCoverRight = CameraState.TallCoverRight();
            states.TallCoverBack = CameraState.TallCoverBack();
            states.LeftCorner = CameraState.LeftCorner();
            states.RightCorner = CameraState.RightCorner();
            states.Climb = CameraState.Climb();
            states.Dead = CameraState.Dead();
            states.Zoom = CameraState.Zoom();
            states.CrouchZoom = CameraState.CrouchZoom();
            states.LowCoverZoom = CameraState.CoverZoom();
            states.LowCoverBackZoom = CameraState.LowCoverBackZoom();
            states.LeftLowCornerZoom = CameraState.LeftLowCornerZoom();
            states.RightLowCornerZoom = CameraState.RightLowCornerZoom();
            states.LeftTallCornerZoom = CameraState.LeftTallCornerZoom();
            states.RightTallCornerZoom = CameraState.RightTallCornerZoom();

            return states;
        }
    }

    /// <summary>
    /// Defines camera state for a single gameplay situation.
    /// </summary>
    [Serializable]
    public struct CameraState
    {
        /// <summary>
        /// Position around which the camera is rotated.
        /// </summary>
        [Tooltip("Position around which the camera is rotated.")]
        public Vector3 Pivot;

        /// <summary>
        /// Offset from the pivot. The offset is rotated using camera's Horizontal and Vertical values.
        /// </summary>
        [Tooltip("Offset from the pivot. The offset is rotated using camera's Horizontal and Vertical values.")]
        public Vector3 Offset;

        /// <summary>
        /// Final rotation of the camera once it is in position.
        /// </summary>
        [Tooltip("Final rotation of the camera once it is in position.")]
        public Vector3 Orientation;

        /// <summary>
        /// Field of view.
        /// </summary>
        [Tooltip("Field of view.")]
        [Range(0, 360)]
        public float FOV;

        public static CameraState Default()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.64f, 0.1f, -2.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Aim()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1.7f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Crouch()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.8f, -1f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState LowCover()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.5f, -0.3f, -1.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState LowCoverGrenade()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.23f, -0.23f, -2.2f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState TallCoverLeft()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(-0.5f, -0.3f, -1.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState TallCoverRight()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.5f, -0.3f, -1.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState TallCoverBack()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.6f, 0);
            settings.Offset = new Vector3(0.65f, -0.3f, -1.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState LeftCorner()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(-1.5f, -0.44f, -2.4f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState RightCorner()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(1.5f, -0.44f, -2.4f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Climb()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2f, 0);
            settings.Offset = new Vector3(0.6f, -0.75f, -1.75f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Dead()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 0, 0);
            settings.Offset = new Vector3(0f, 1f, -2.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Zoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2f, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState CrouchZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2f, 0);
            settings.Offset = new Vector3(0.5f, -0.78f, -1f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState CoverZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2.2f, 0);
            settings.Offset = new Vector3(0.4f, -0.4f, -0.6f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState LowCoverZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.5f, -0.25f, -1f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState LowCoverBackZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2.15f, 0);
            settings.Offset = new Vector3(0.5f, -0.8f, -0.6f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState LeftLowCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(-0.55f, -0.4f, -0.95f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState RightLowCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.8f, -0.4f, -0.95f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState LeftTallCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(-0.4f, -0.4f, -0.95f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState RightTallCornerZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 1.75f, 0);
            settings.Offset = new Vector3(0.8f, -0.4f, -0.95f);
            settings.FOV = 40;

            return settings;
        }
    }
}