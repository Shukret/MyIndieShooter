using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIFire : AIItemBase
    {
        #region Properties

        /// <summary>
        /// Is the AI currently firing.
        /// </summary>
        public bool IsFiring
        {
            get { return _isFiring; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Weapon type to look for, if not present any other weapon will be picked. Used only when AutoFindIndex is enabled.
        /// </summary>
        [Tooltip("Weapon type to look for, if not present any other weapon will be picked. Used only when AutoFindIndex is enabled.")]
        public WeaponType AutoFindType = WeaponType.Pistol;

        /// <summary>
        /// Should the AI always be aiming.
        /// </summary>
        [Tooltip("Should the AI always be aiming.")]
        public bool AlwaysAim;

        /// <summary>
        /// Settings for bursts of fire when in a cover.
        /// </summary>
        [Tooltip("Settings for bursts of fire when in a cover.")]
        public Bursts Bursts = Bursts.Default();

        /// <summary>
        /// Settings for bursts of fire when in a cover.
        /// </summary>
        [Tooltip("Settings for bursts of fire when in a cover.")]
        public CoverBursts CoverBursts = CoverBursts.Default();

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private bool _isFiring;
        private bool _isAiming;

        private float _fireCycle;
        private bool _isReloading;
        private bool _isFiringABurst;
        private int _burstBulletCount;
        private int _bulletCountAtStart;
        private Gun _gunBurstsWereCalculatedFor;

        private bool _isAimingAtAPosition;
        private Vector3 _aim;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the actor that a cover was entered.
        /// </summary>
        public void OnEnterCover()
        {
            _fireCycle = 0;
        }

        /// <summary>
        /// Notified by the actor that a cover was left.
        /// </summary>
        public void OnLeaveCover()
        {
            _fireCycle = 0;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to aim the gun at a position.
        /// </summary>
        public void ToAimAt(Vector3 position)
        {
            _aim = position;
            _isAimingAtAPosition = true;
        }

        /// <summary>
        /// Told by the brains to aim the gun to a direection.
        /// </summary>
        public void ToAimTo(Vector3 direction)
        {
            _isAimingAtAPosition = false;
        }

        /// <summary>
        /// Told by the brains to start aiming and firing.
        /// </summary>
        public void ToOpenFire()
        {
            ToArm();
            _isFiring = true;
            _isAiming = true;
            _fireCycle = 0;
        }

        /// <summary>
        /// Told by the brains to stop firing.
        /// </summary>
        public void ToCloseFire()
        {
            _isFiring = false;
        }

        /// <summary>
        /// Told by the brains to start aiming, doesn't start firing.
        /// </summary>
        public void ToStartAiming()
        {
            _isAiming = true;
        }

        /// <summary>
        /// Told by the brains to stop both aiming and firing.
        /// </summary>
        public void ToStopAiming()
        {
            _isAiming = false;
            _isFiring = false;
        }

        /// <summary>
        /// Told by the brains to take a weapon to arms.
        /// </summary>
        public void ToArm()
        {
            Equip(_motor);
        }

        /// <summary>
        /// Told by the brains to disarm any weapon.
        /// </summary>
        public void ToDisarm()
        {
            Unequip(_motor);
            _isFiring = false;
            _isAiming = false;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();

            if (AutoFindIndex)
                AutoFind(_motor, AutoFindType);
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            var wantsToAlwaysAim = AlwaysAim && _motor.Cover == null;

            if (wantsToAlwaysAim)
                Equip(_motor);

            if (_motor.Gun == null)
                return;

            if (_isReloading)
                _isReloading = !_motor.IsGunReady;
            else if (_motor.Gun != null && _motor.Gun.IsClipEmpty)
            {
                _isReloading = true;
                _motor.InputReload();
            }

            var isObstructed = false;

            if (_isFiring && _isAimingAtAPosition)
            {
                var origin = transform.position;

                if (_motor.Cover != null && _motor.Cover.IsTall)
                {
                    if (_motor.CoverDirection > 0 && _motor.IsNearRightCorner)
                        origin = _motor.Cover.RightCorner(transform.position.y, _motor.CoverSettings.CornerOffset.x);
                    else if (_motor.CoverDirection < 0 && _motor.IsNearLeftCorner)
                        origin = _motor.Cover.LeftCorner(transform.position.y, _motor.CoverSettings.CornerOffset.x);
                }

                var start = origin + Vector3.up * 2;
                var distance = Vector3.Distance(start, _aim);

                if (distance > 1.5f)
                    isObstructed = AIUtil.IsObstructed(start, _aim, distance - 1.5f);
            }

            if (_motor.Cover == null && (_isAiming || wantsToAlwaysAim) && !_isReloading)
                _motor.InputAim();

            if (_isFiring && !_isReloading && !isObstructed)
            {
                var cycleDuration = _motor.Cover != null ? CoverBursts.CycleDuration : Bursts.CycleDuration;

                if (_fireCycle >= cycleDuration)
                {
                    if (_isFiringABurst)
                    {
                        _gunBurstsWereCalculatedFor = _motor.Gun;
                        _burstBulletCount = _bulletCountAtStart - _motor.Gun.Clip;
                    }

                    _isFiringABurst = false;
                    _fireCycle -= cycleDuration;

                    if (_gunBurstsWereCalculatedFor == _motor.Gun && _motor.Gun.Clip < _burstBulletCount)
                    {
                        _isReloading = true;
                        _motor.InputReload();
                    }
                }

                if (_motor.Cover != null)
                {
                    if (_fireCycle >= CoverBursts.Wait)
                    {
                        _motor.InputAim();

                        if (_fireCycle >= CoverBursts.Wait + CoverBursts.IntroDuration &&
                            _fireCycle < CoverBursts.CycleDuration - CoverBursts.OutroDuration)
                        {
                            if (!_isFiringABurst)
                            {
                                _bulletCountAtStart = _motor.Gun.Clip;
                                _isFiringABurst = true;
                            }

                            if (_motor.IsGunReady)
                                _motor.InputFireOnCondition(_actor.Side);
                        }
                    }
                }
                else
                {
                    _motor.InputAim();

                    if (_fireCycle >= Bursts.Wait)
                    {
                        if (!_isFiringABurst)
                        {
                            _bulletCountAtStart = _motor.Gun.Clip;
                            _isFiringABurst = true;
                        }

                        if (_motor.IsGunReady)
                            _motor.InputFireOnCondition(_actor.Side);
                    }
                }

                _fireCycle += Time.deltaTime;
            }
            else
            {
                _fireCycle = 0;
                _isFiringABurst = false;
            }
        }

        #endregion
    }
}
