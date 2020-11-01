using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Takes player input and translates that to commands to CharacterMotor.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class ThirdPersonController : MonoBehaviour
    {
        /// <summary>
        /// Is the character actively reacting to camera direction changes.
        /// </summary>
        public bool IsActivelyFacing
        {
            get { return _isActivelyFacing; }
        }

        /// <summary>
        /// Is the character using zoom.
        /// </summary>
        public bool IsZooming
        {
            get { return _motor.IsAlive && ZoomInput && _motor.IsAiming; }
        }

        /// <summary>
        /// Should a scope be displayed right now.
        /// </summary>
        public bool IsScoped
        {
            get { return IsZooming && _motor.Gun != null && _motor.Gun.Scope != null; }
        }

        /// <summary>
        /// Determines if the character takes cover automatically instead of waiting for player input.
        /// </summary>
        [Tooltip("Determines if the character takes cover automatically instead of waiting for player input.")]
        public bool AutoTakeCover = true;

        /// <summary>
        /// Is the character always aiming in camera direction when not in cover.
        /// </summary>
        [Tooltip("Is the character always aiming in camera direction when not in cover.")]
        public bool AlwaysAim = false;

        /// <summary>
        /// How long to continue aiming after no longer needed.
        /// </summary>
        [Tooltip("How long to continue aiming after no longer needed.")]
        public float AimSustain = 0.4f;

        /// <summary>
        /// Time in seconds to keep the gun down when starting to move.
        /// </summary>
        [Tooltip("Time in seconds to keep the gun down when starting to move.")]
        public float NoAimSustain = 0.14f;

        /// <summary>
        /// Can the player roll into a cover.
        /// </summary>
        [Tooltip("Can the player roll into a cover.")]
        public bool TakeCoverWhenRolling = true;

        /// <summary>
        /// Degrees to add when aiming a grenade vertically.
        /// </summary>
        [Tooltip("Degrees to add when aiming a grenade vertically.")]
        public float ThrowAngleOffset = 30;

        /// <summary>
        /// How high can the player throw the grenade.
        /// </summary>
        [Tooltip("How high can the player throw the grenade.")]
        public float MaxThrowAngle = 45;

        /// <summary>
        /// Prefab to instantiate to display grenade explosion preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade explosion preview.")]
        public GameObject ExplosionPreview;

        /// <summary>
        /// Prefab to instantiate to display grenade path preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade path preview.")]
        public GameObject PathPreview;

        /// <summary>
        /// Scope object and component that's enabled and maintained when using scope.
        /// </summary>
        [Tooltip("Scope object and component that's enabled and maintained when using scope.")]
        public Image Scope;

        /// <summary>
        /// Sets the controller to start or stop firing.
        /// </summary>
        [HideInInspector]
        public bool FireInput;

        /// <summary>
        /// Sets the controller to start and stop zooming.
        /// </summary>
        [HideInInspector]
        public bool ZoomInput;

        /// <summary>
        /// Sets the position the controller is looking at.
        /// </summary>
        [HideInInspector]
        public Vector3 LookTargetInput;

        /// <summary>
        /// Sets the position the controller is zooming at.
        /// </summary>
        [HideInInspector]
        public Vector3 FireTargetInput;

        /// <summary>
        /// Sets the horizontal angle for aiming a grenade.
        /// </summary>
        [HideInInspector]
        public float GrenadeHorizontalAngleInput;

        /// <summary>
        /// Sets the vertical angle for aiming a grenade.
        /// </summary>
        [HideInInspector]
        public float GrenadeVerticalAngleInput;

        /// <summary>
        /// Sets the movement for the controller.
        /// </summary>
        [HideInInspector]
        public CharacterMovement MovementInput;

        private bool _wasInCover;
        private float _coverTimer;

        private CharacterMotor _motor;

        private GameObject _explosionPreview;
        private GameObject _pathPreview;

        private bool _isSprinting;
        private bool _isAiming;
        private bool _isActivelyFacing;

        private float _noAimSustain;
        private float _aimSustain;
        private float _postSprintNoAutoAim;

        private Vector3[] _grenadePath = new Vector3[128];
        private int _grenadePathLength;
        private bool _hasGrenadePath;
        private bool _wantsToThrowGrenade;

        private bool _startedRollingInCover;
        private bool _wasRolling;

        private bool _wasZooming;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        public void InputThrowGrenade()
        {
            _wantsToThrowGrenade = true;
        }

        private void Update()
        {
            _isActivelyFacing = AlwaysAim && !_isSprinting;

            updateGrenadeAimAndPreview();
            updateMovement();

            if (_motor.IsRolling && !_wasRolling)
                _startedRollingInCover = _wasInCover;

            if ((AutoTakeCover || ((_startedRollingInCover || TakeCoverWhenRolling) && _motor.IsRolling)) && _motor.PotentialCover != null)
                _motor.InputTakeCover();

            if (_motor.HasGrenadeInHand)
            {
                if (_hasGrenadePath && _wantsToThrowGrenade)
                {
                    _wantsToThrowGrenade = false;
                    _isActivelyFacing = true;
                    _motor.SetLookTarget(LookTargetInput);
                    _motor.SetFireTarget(FireTargetInput);
                    _motor.InputThrowGrenade(_grenadePath, _grenadePathLength, _motor.Grenade.Step);
                }

                FireInput = false;
                ZoomInput = false;
            }
            else
            {
                if (!_isSprinting)
                {
                    if (_motor.IsWeaponReady && FireInput)
                    {
                        if (_motor.Gun != null && _motor.Gun.IsClipEmpty)
                            _motor.InputReload();
                        else
                            _motor.InputFire();

                        _isActivelyFacing = true;
                    }

                    if (_motor.IsGunScopeReady && ZoomInput)
                    {
                        _motor.InputAim();
                        _motor.InputZoom();
                        _isActivelyFacing = true;
                    }
                }
            }

            if (_isSprinting)
            {
                _isAiming = false;
                _isActivelyFacing = false;
                FireInput = false;
                ZoomInput = false;
            }

            if (_isAiming && _aimSustain >= 0)
                _aimSustain -= Time.deltaTime;

            if (_noAimSustain >= 0)
                _noAimSustain -= Time.deltaTime;

            if (!FireInput && !ZoomInput)
            {
                if (_postSprintNoAutoAim >= 0)
                    _postSprintNoAutoAim -= Time.deltaTime;
            }
            else
            {
                _postSprintNoAutoAim = 0;
                _noAimSustain = 0;
            }

            if (((AlwaysAim || _isActivelyFacing) && _postSprintNoAutoAim <= float.Epsilon) ||
                 FireInput ||
                 ZoomInput)
            {
                _isAiming = true;
                _aimSustain = AimSustain;
            }
            else if (!_isAiming)
                _noAimSustain = NoAimSustain;

            if (!AlwaysAim)
                if (_aimSustain <= float.Epsilon || _noAimSustain > float.Epsilon)
                    _isAiming = false;

            if (_isAiming && _motor.Gun != null)
            {
                if (_motor.IsInCover)
                    _motor.InputAimWhenLeavingCover();
                else
                    _motor.InputAim();
            }

            if (FireInput || ZoomInput)
                _motor.InputPossibleImmediateTurn();

            if (_isActivelyFacing || _motor.IsAiming || _motor.IsInCover)
            {
                if (!_isSprinting)
                    _motor.SetBodyLookTarget(LookTargetInput);

                _motor.SetLookTarget(LookTargetInput);
                _motor.SetFireTarget(FireTargetInput);
            }

            if (ZoomInput && !_wasZooming)
                SendMessage("OnZoom", SendMessageOptions.DontRequireReceiver);
            else if (!ZoomInput && _wasZooming)
                SendMessage("OnUnzoom", SendMessageOptions.DontRequireReceiver);

            _wasZooming = ZoomInput;
            _wasRolling = _motor.IsRolling;

            if (Scope != null)
            {
                if (Scope.gameObject.activeSelf != IsScoped)
                {
                    Scope.gameObject.SetActive(IsScoped);

                    if (Scope.gameObject.activeSelf)
                        Scope.sprite = _motor.Gun.Scope;
                }
            }
        }

        private void updateMovement()
        {
            var movement = MovementInput;

            if (_motor.IsInCover || _motor.IsLookingBackFromCover)
            {
                if (!_wasInCover)
                {
                    _wasInCover = true;
                    _coverTimer = 0;
                }
                else
                    _coverTimer += Time.deltaTime;

                if (_coverTimer < 0.5f && (movement.Direction.magnitude < 0.1f || Vector3.Dot(movement.Direction, _motor.Cover.Forward) > -0.1f))
                    movement.Direction = (movement.Direction + _motor.Cover.Forward).normalized;
            }
            else
                _wasInCover = false;

            var wasSprinting = _isSprinting;
            _isSprinting = false;

            if (movement.IsMoving)
            {
                _isActivelyFacing = true;

                // Smooth sprinting turns
                if (movement.Magnitude > 1.1f && !_motor.IsInCover)
                {
                    var lookAngle = Util.AngleOfVector(LookTargetInput - _motor.transform.position);

                    // Don't allow sprinting backwards
                    if (Mathf.Abs(Mathf.DeltaAngle(lookAngle, Util.AngleOfVector(movement.Direction))) < 100)
                    {
                        var wantedAngle = Util.AngleOfVector(movement.Direction);
                        var bodyAngle = _motor.transform.eulerAngles.y;
                        var delta = Mathf.DeltaAngle(bodyAngle, wantedAngle);

                        const float MaxSprintTurn = 60;

                        if (delta > MaxSprintTurn)
                            movement.Direction = Quaternion.AngleAxis(bodyAngle + MaxSprintTurn, Vector3.up) * Vector3.forward;
                        else if (delta < -MaxSprintTurn)
                            movement.Direction = Quaternion.AngleAxis(bodyAngle - MaxSprintTurn, Vector3.up) * Vector3.forward;

                        _motor.SetBodyLookTarget(_motor.transform.position + movement.Direction * 100);
                        _motor.InputPossibleImmediateTurn(false);

                        _isSprinting = true;
                    }
                    else
                        movement.Magnitude = 1.0f;
                }

                if (!_isSprinting && wasSprinting)
                    _postSprintNoAutoAim = 0.0f;
            }
            else if (wasSprinting)
                _postSprintNoAutoAim = 0.3f;

            _motor.InputMovement(movement);
        }

        private void updateGrenadeAimAndPreview()
        {
            if (_motor.IsAlive && _motor.IsReadyToThrowGrenade && _motor.CurrentGrenade != null)
            {
                GrenadeDescription desc;
                desc.Gravity = _motor.Grenade.Gravity;
                desc.Duration = _motor.PotentialGrenade.Timer;
                desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                var verticalAngle = Mathf.Min(GrenadeVerticalAngleInput + ThrowAngleOffset, MaxThrowAngle);

                var velocity = _motor.Grenade.MaxVelocity;

                if (verticalAngle < 45)
                    velocity *= Mathf.Clamp01((verticalAngle + 15) / 45f);

                _grenadePathLength = GrenadePath.Calculate(GrenadePath.Origin(_motor, GrenadeHorizontalAngleInput),
                                                           GrenadeHorizontalAngleInput,
                                                           verticalAngle,
                                                           velocity,
                                                           desc,
                                                           _grenadePath,
                                                           _motor.Grenade.Step);
                _hasGrenadePath = true;

                if (_explosionPreview == null && ExplosionPreview != null)
                {
                    _explosionPreview = GameObject.Instantiate(ExplosionPreview);
                    _explosionPreview.transform.parent = null;
                    _explosionPreview.SetActive(true);
                }

                if (_explosionPreview != null)
                {
                    _explosionPreview.transform.localScale = Vector3.one * _motor.PotentialGrenade.ExplosionRadius * 2;
                    _explosionPreview.transform.position = _grenadePath[_grenadePathLength - 1];
                }

                if (_pathPreview == null && PathPreview != null)
                {
                    _pathPreview = GameObject.Instantiate(PathPreview);
                    _pathPreview.transform.parent = null;
                    _pathPreview.SetActive(true);
                }

                if (_pathPreview != null)
                {
                    _pathPreview.transform.position = _grenadePath[0];

                    var path = _pathPreview.GetComponent<PathPreview>();

                    if (path != null)
                    {
                        path.Points = _grenadePath;
                        path.PointCount = _grenadePathLength;
                    }
                }
            }
            else
            {
                if (_explosionPreview != null)
                {
                    GameObject.Destroy(_explosionPreview);
                    _explosionPreview = null;
                }

                if (_pathPreview != null)
                {
                    GameObject.Destroy(_pathPreview);
                    _pathPreview = null;
                }

                _hasGrenadePath = false;
            }
        }
    }
}