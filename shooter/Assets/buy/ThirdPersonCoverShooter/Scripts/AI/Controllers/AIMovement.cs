using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    [RequireComponent(typeof(CharacterMotor))]
    public class AIMovement : AIBase
    {
        #region Properties

        /// <summary>
        /// Position the AI is walking towards. Returns current position when circling or retreating.
        /// </summary>
        public Vector3 Destination
        {
            get
            {
                if (_mode == Mode.toPosition)
                    return _target;
                else
                    return transform.position;
            }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Should a line to destination be drawn in the editor.
        /// </summary>
        [Tooltip("Should a line to destination be drawn in the editor.")]
        public bool DebugDestination = false;

        /// <summary>
        /// Should a path to destination be drawn in the editor.
        /// </summary>
        [Tooltip("Should a path to destination be drawn in the editor.")]
        public bool DebugPath = false;

        #endregion

        #region Private fields

        enum Mode
        {
            none,
            toPosition,
            fromPosition,
            circle
        }

        private CharacterMotor _motor;

        private Mode _mode;
        private Vector3 _target;

        private float _speed = 1.0f;

        private NavMeshPath _path;
        private Vector3[] _pathPoints = new Vector3[64];
        private int _pathLength;
        private int _currentPathIndex;

        private Vector3 _direction;

        private bool _isCrouching;

        private int _side;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="value"></param>
        public void OnThreatPosition(Vector3 value)
        {
            if (_mode == Mode.circle || _mode == Mode.fromPosition)
                _target = value;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Enter crouch mode.
        /// </summary>
        public void ToCrouch()
        {
            _isCrouching = true;
        }

        /// <summary>
        /// Exit crouch mode.
        /// </summary>
        public void ToStopCrouching()
        {
            _isCrouching = false;
        }

        /// <summary>
        /// Told by the brains to circle around a threat.
        /// </summary>
        public void ToCircle(Vector3 threat)
        {
            _mode = Mode.circle;
            _target = threat;
            _speed = 0.5f;
            _side = 0;
        }

        /// <summary>
        /// Told by the brains to walk to a destination position.
        /// </summary>
        public void ToWalkTo(Vector3 destination)
        {
            moveTo(destination, 0.5f);
        }

        /// <summary>
        /// Told by the brains to run to a destination position.
        /// </summary>
        public void ToRunTo(Vector3 destination)
        {
            moveTo(destination, 1.0f);
        }

        /// <summary>
        /// Told by the brains to sprint to a destination position.
        /// </summary>
        public void ToSprintTo(Vector3 destination)
        {
            moveTo(destination, 2.0f);
        }

        /// <summary>
        /// Told by the brains to walk away from a position.
        /// </summary>
        public void ToWalkFrom(Vector3 target)
        {
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to run away from a position.
        /// </summary>
        public void ToRunFrom(Vector3 target)
        {
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to sprint away from a position.
        /// </summary>
        public void ToSprintFrom(Vector3 target)
        {
            moveFrom(target, 0.5f);
        }

        /// <summary>
        /// Told by the brains to walk to stop moving.
        /// </summary>
        public void ToStopMoving()
        {
            _mode = Mode.none;
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();

            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (_motor == null || !_motor.IsAlive)
                _mode = Mode.none;

            if (_mode == Mode.none)
                return;

            if (DebugDestination)
                Debug.DrawLine(transform.position, _target, Color.blue);

            if (DebugPath)
                for (int i = 0; i < _pathLength - 1; i++)
                {
                    if (i == _currentPathIndex)
                    {
                        Debug.DrawLine(_pathPoints[i], _pathPoints[i + 1], Color.cyan);
                        Debug.DrawLine(_pathPoints[i + 1], _pathPoints[i + 1] + Vector3.up, Color.cyan);
                    }
                    else
                        Debug.DrawLine(_pathPoints[i], _pathPoints[i + 1], Color.green);
                }

            var vector = _target - transform.position;
            vector.y = 0;

            var direction = vector.normalized;
            var side = Vector3.Cross(direction, Vector3.up);

            if (_isCrouching)
                _motor.InputCrouch();

            switch (_mode)
            {
                case Mode.toPosition:
                    var isCloseToThePath = _currentPathIndex <= _pathLength - 1 && Util.DistanceToSegment(transform.position, _pathPoints[_currentPathIndex], _pathPoints[_currentPathIndex + 1]) < 0.5f;

                    if (!isCloseToThePath)
                        updatePath();

                    var isLastStepOnPartialPath = _currentPathIndex >= _pathLength - 2 && _path.status == NavMeshPathStatus.PathPartial;

                    if (vector.magnitude >= 0.3f && !isLastStepOnPartialPath)
                    {
                        if (_path.status == NavMeshPathStatus.PathInvalid)
                            updatePath();

                        var point = _currentPathIndex + 1 < _pathLength ? _pathPoints[_currentPathIndex + 1] : _target;

                        direction = point - transform.position;
                        direction.y = 0;

                        var distanceToPoint = direction.magnitude;

                        if (distanceToPoint > float.Epsilon)
                            direction /= distanceToPoint;

                        if (distanceToPoint < 0.2f && _currentPathIndex + 1 < _pathLength)
                        {
                            var index = _currentPathIndex;

                            if (distanceToPoint > 0.07f && _currentPathIndex + 2 < _pathLength)
                            {
                                if (Vector3.Dot(point - transform.position, _pathPoints[_currentPathIndex + 2] - transform.position) <= 0.1f)
                                    _currentPathIndex++;
                            }
                            else
                                _currentPathIndex++;

                            if (index < _currentPathIndex)
                                updateDirection(((_currentPathIndex + 1 < _pathLength ? _pathPoints[_currentPathIndex + 1] : _target) - _pathPoints[_currentPathIndex]).normalized, true);
                        }

                        _motor.InputMovement(new CharacterMovement(direction, _speed));
                    }
                    else
                    {
                        if (vector.magnitude > 0.05f)
                        {
                            if (_motor.IsInCover)
                                _motor.InputMovement(new CharacterMovement(direction, 1.0f));
                            else
                                _motor.InputMovement(new CharacterMovement(direction, 0.5f));
                        }
                        else
                        {
                            _motor.transform.position = _target;
                            _mode = Mode.none;
                        }
                    }
                    break;

                case Mode.fromPosition:
                    _pathLength = 0;
                    direction = -direction;

                    if (_motor.IsFree(direction, 0.5f, 0.25f))
                        _motor.InputMovement(new CharacterMovement(direction, 1.0f));
                    else
                    {
                        if (_side == 0)
                        {
                            if (Random.Range(0, 10) < 5 && _motor.IsFree(side, 0.5f, 0.25f))
                                _side = 1;
                            else
                                _side = -1;
                        }

                        if (!_motor.IsFree(side * _side, 0.5f, 0.25f))
                        {
                            if (!_motor.IsFree(-side * _side, 0.5f, 0.25f))
                                Message("OnMoveFromFail");
                            else
                                _side = -_side;
                        }

                        _motor.InputMovement(new CharacterMovement(side * _side, 1.0f));
                    }

                    updateDirection(direction, false);
                    break;

                case Mode.circle:
                    _pathLength = 0;

                    if (_side == 0)
                    {
                        if (Random.Range(0, 10) < 5 && isValidSide(side))
                            _side = 1;
                        else
                            _side = -1;
                    }

                    if (!isValidSide(side * _side))
                    {
                        if (!isValidSide(-side * _side))
                            Message("OnCircleFail");
                        else
                            _side = -_side;
                    }

                    direction = side * _side;
                    _motor.InputMovement(new CharacterMovement(direction, 1.0f));
                    updateDirection(direction, false);
                    break;
            }
        }

        #endregion

        #region Private methods

        private void updateDirection(Vector3 value, bool force)
        {
            if (force || Vector3.Dot(_direction, value) < 0.95f)
            {
                _direction = value;
                Message("OnWalkDirection", _direction);
            }
        }

        private bool isValidSide(Vector3 vector)
        {
            if (!_motor.IsFree(vector, 0.5f, 0.1f))
                return false;

            if (AIUtil.IsObstructed(transform.position + Vector3.up * 2, _target + Vector3.up * 2, 1000))
                return false;

            return true;
        }

        private void moveTo(Vector3 destination, float speed)
        {
            _mode = Mode.toPosition;
            _target = destination;
            _speed = speed;
            updatePath();
        }

        private void moveFrom(Vector3 target, float speed)
        {
            _mode = Mode.fromPosition;
            _target = target;
            _speed = speed;
            _side = 0;
        }

        /// <summary>
        /// Sets up the navigation agent to move to the givent position.
        /// </summary>
        private void updatePath()
        {
            AIUtil.Path(ref _path, transform.position, _target);

            _pathLength = _path.GetCornersNonAlloc(_pathPoints);
            _currentPathIndex = 0;

            if (_pathLength > 1)
                updateDirection((_pathPoints[1] - _pathPoints[0]).normalized, true);
        }

        #endregion
    }
}
