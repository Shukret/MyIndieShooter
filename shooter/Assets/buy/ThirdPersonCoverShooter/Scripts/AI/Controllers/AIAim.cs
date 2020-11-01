using UnityEngine;

namespace CoverShooter
{
    public struct ActorTarget
    {
        public Vector3 Position;
        public Vector3 RelativeTopPosition;

        public ActorTarget(Vector3 position, Vector3 relativeTopPosition)
        {
            Position = position;
            RelativeTopPosition = relativeTopPosition;
        }
    }

    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIAim : AIBase
    {
        #region Enums

        enum BodyMode
        {
            none,
            actor,
            position,
            direction,
            walk
        }

        enum AimMode
        {
            none,
            actor,
            position,
            direction,
            walk
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Speed at which the AI turns.
        /// </summary>
        [Tooltip("Speed at which the AI turns.")]
        public float Speed = 6;

        /// <summary>
        /// Speed at which the AI turns when in slow mode.
        /// </summary>
        [Tooltip("Speed at which the AI turns when in slow mode.")]
        public float SlowSpeed = 2;

        /// <summary>
        /// Position of the enemy the AI is aiming at.
        /// </summary>
        [Tooltip("Position of the enemy the AI is aiming at.")]
        public AITargetSettings Target = new AITargetSettings(0.5f, 0.8f);

        /// <summary>
        /// Settings for the AI accuracy. Lower values translate to better aiming.
        /// </summary>
        [Tooltip("Settings for the AI accuracy. Lower values translate to better aiming.")]
        public AIAccuracySettings Accuracy = new AIAccuracySettings(0, 1, 3, 20);

        /// <summary>
        /// Should a debug rays be displayed.
        /// </summary>
        [Tooltip("Should a debug rays be displayed.")]
        public bool DebugAim = false;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private Vector3 _body;
        private Vector3 _aim;
        private ActorTarget _target;
        private bool _hasBodyAim;
        private bool _hasAim;

        private bool _isAimingSlowly;
        private bool _isTurningSlowly;

        private Vector3 _currentAim;

        private BodyMode _bodyMode;
        private AimMode _aimMode;

        private Vector3 _walkDirection;

        private float _aimDelay = 0;

        private float _currentTargetHeight;
        private float _targetHeight;
        private float _targetHeightTime;

        #endregion

        #region Events

        /// <summary>
        /// Notified of a new walk direction.
        /// </summary>
        public void OnWalkDirection(Vector3 value)
        {
            _walkDirection = value;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Told to turn both body and arms towards the walk direction.
        /// </summary>
        public void ToFaceWalkDirection()
        {
            _bodyMode = BodyMode.walk;
            _aimMode = AimMode.walk;
            _hasBodyAim = true;
            _hasAim = true;
        }

        /// <summary>
        /// Told to turn the body towards the walk direction.
        /// </summary>
        public void ToTurnToWalkDirection()
        {
            _bodyMode = BodyMode.walk;
            _hasBodyAim = true;
        }

        /// <summary>
        /// Told by the brains to turn the body at a position.
        /// </summary>
        public void ToTurnAt(Vector3 position)
        {
            _body = position;
            _bodyMode = BodyMode.position;
            _hasBodyAim = true;
            _isTurningSlowly = false;

            if (!_hasAim)
                ToAimAt(position);
        }

        /// <summary>
        /// Told by the brains to turn the body to a direection.
        /// </summary>
        public void ToTurnTo(Vector3 direction)
        {
            _body = direction;
            _bodyMode = BodyMode.direction;
            _hasBodyAim = true;
            _isTurningSlowly = false;

            if (!_hasAim)
                ToAimTo(direction);
        }

        /// <summary>
        /// Told by the brains to aim the gun at a position.
        /// </summary>
        public void ToAimAt(Vector3 position)
        {
            aimAt(position);
            _isAimingSlowly = false;
            _aimDelay = 0;
        }

        /// <summary>
        /// Told by the brains to aim the gun to a direection.
        /// </summary>
        public void ToAimTo(Vector3 direction)
        {
            aimTo(direction);
            _isAimingSlowly = false;
            _aimDelay = 0;
        }

        public void ToTarget(ActorTarget target)
        {
            _target = target;
            _bodyMode = BodyMode.actor;
            _aimMode = AimMode.actor;
            _hasBodyAim = true;
            _hasAim = true;
            _isTurningSlowly = false;
        }

        /// <summary>
        /// Told by the brains to slowly turn the body at a position.
        /// </summary>
        public void ToSlowlyTurnAt(Vector3 position)
        {
            ToTurnAt(position);
            _isTurningSlowly = true;
        }

        /// <summary>
        /// Told by the brains to slowly turn the body to a direction.
        /// </summary>
        public void ToSlowlyTurnTo(Vector3 direction)
        {
            ToTurnTo(direction);
            _isTurningSlowly = true;
        }

        /// <summary>
        /// Told by the brains to slowly aim the gun at a position.
        /// </summary>
        public void ToSlowlyAimAt(Vector3 position)
        {
            aimAt(position);
            _isAimingSlowly = true;
            _aimDelay = 0.5f;
        }

        /// <summary>
        /// Told by the brains to slowly aim the gun to a direction.
        /// </summary>
        public void ToSlowlyAimTo(Vector3 direction)
        {
            aimTo(direction);
            _isAimingSlowly = true;
            _aimDelay = 0.5f;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
            _walkDirection = transform.position;
            _currentAim = transform.forward;

            _targetHeight = Random.Range(Target.Min, Target.Max);
            _currentTargetHeight = _targetHeight;
            _targetHeightTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            if (Time.timeSinceLevelLoad - _targetHeightTime > 3)
            {
                _targetHeight = Random.Range(Target.Min, Target.Max);
                _targetHeightTime = Time.timeSinceLevelLoad;
            }

            _currentTargetHeight = Mathf.Lerp(_currentTargetHeight, _targetHeight, Time.deltaTime);

            if (!_hasBodyAim)
                ToTurnTo(transform.forward);

            switch (_bodyMode)
            {
                case BodyMode.actor: _motor.SetBodyLookTarget(_target.Position + _target.RelativeTopPosition * _currentTargetHeight, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.position: _motor.SetBodyLookTarget(_body, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.direction: _motor.SetBodyLookTarget(transform.position + _body * 8, _isTurningSlowly ? SlowSpeed : Speed); break;
                case BodyMode.walk: _motor.SetBodyLookTarget(transform.position + _walkDirection * 8, (_isTurningSlowly ? SlowSpeed : Speed) * 2); break;
            }

            switch (_aimMode)
            {
                case AimMode.actor: turn(ref _currentAim, _target.Position + _target.RelativeTopPosition * _currentTargetHeight, _isAimingSlowly); break;
                case AimMode.position: turn(ref _currentAim, _aim, _isAimingSlowly); break;
                case AimMode.direction: turn(ref _currentAim, transform.position + _aim * 8, _isAimingSlowly); break;
                case AimMode.walk: turn(ref _currentAim, transform.position + _walkDirection * 8, _isAimingSlowly); break;
            }

            aimMotorAt(_currentAim);

            if (_aimDelay >= 0)
                _aimDelay -= Time.deltaTime;
        }

        #endregion

        #region Helpers

        public void aimAt(Vector3 position)
        {
            _aim = position;
            _aimMode = AimMode.position;
            _hasAim = true;

            if (!_hasBodyAim)
                ToTurnAt(position);
        }

        public void aimTo(Vector3 direction)
        {
            _aim = direction;
            _aimMode = AimMode.direction;
            _hasAim = true;

            if (!_hasBodyAim)
                ToTurnTo(direction);
        }

        private void turn(ref Vector3 current, Vector3 target, bool isSlow, float multiplier = 1.0f)
        {
            if (_motor.IsInCover && !_motor.IsAimingGun && !_motor.IsAimingTool)
                current = target;
            else if (_aimDelay <= float.Epsilon)
            {
                float speed = (isSlow ? SlowSpeed : Speed) * multiplier;

                var currentVector = current - transform.position;
                var targetVector = target - transform.position;

                var currentAngle = Util.AngleOfVector(currentVector);
                var targetAngle = Util.AngleOfVector(targetVector);

                var move = speed * Time.deltaTime * 90;
                var delta = Mathf.DeltaAngle(currentAngle, targetAngle);

                if (Mathf.Abs(delta) < move)
                    currentAngle = targetAngle;
                else if (delta > 0)
                    currentAngle += move;
                else
                    currentAngle -= move;

                var length = Mathf.Lerp(currentVector.magnitude, targetVector.magnitude, Time.deltaTime * speed);

                var position = transform.position + Quaternion.AngleAxis(currentAngle, Vector3.up) * Vector3.forward * length;
                position.y = Mathf.Lerp(current.y, target.y, Time.deltaTime * speed);

                current = position;
            }

            if (DebugAim)
            {
                Debug.DrawLine(transform.position, current, Color.magenta);
                Debug.DrawLine(transform.position, target, Color.green);
            }
        }

        private void aimMotorAt(Vector3 position)
        {
            if (_motor.IsInTallCover && !_motor.IsCornerAiming)
            {
                var vector = position - transform.position;

                if (Vector3.Dot(_motor.Cover.Forward, vector) > 0)
                {
                    var isNearLeft = _motor.IsNearLeftCorner;
                    var isNearRight = _motor.IsNearRightCorner;

                    if (isNearLeft && isNearRight)
                    {
                        if (Vector3.Dot(_motor.Cover.Left, vector) > 0)
                        {
                            position = transform.position + (_motor.Cover.Forward + _motor.Cover.Left).normalized * 8;
                            _motor.InputStandLeft();
                        }
                        else
                        {
                            position = transform.position + (_motor.Cover.Forward + _motor.Cover.Right).normalized * 8;
                            _motor.InputStandRight();
                        }
                    }
                    else if (isNearLeft)
                    {
                        position = transform.position + (_motor.Cover.Forward + _motor.Cover.Left).normalized * 8;
                        _motor.InputStandLeft();
                    }
                    else
                    {
                        position = transform.position + (_motor.Cover.Forward + _motor.Cover.Right).normalized * 8;
                        _motor.InputStandRight();
                    }
                }
            }

            _motor.SetLookTarget(position);

            if (DebugAim)
                Debug.DrawLine(_motor.GunOrigin, position, Color.red);

            var targetRadius = Accuracy.Get(Vector3.Distance(position, transform.position));
            _motor.SetFireTarget(position + new Vector3(Random.Range(-1, 1) * targetRadius,
                                                        Random.Range(-1, 1) * targetRadius,
                                                        Random.Range(-1, 1) * targetRadius));
        }

        #endregion
    }
}
