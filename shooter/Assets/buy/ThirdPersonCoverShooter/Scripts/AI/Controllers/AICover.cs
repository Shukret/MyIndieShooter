using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class AICover : AIBase
    {
        #region Public fields

        /// <summary>
        /// Maximum angle of a low cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a low cover relative to the enemy.")]
        public float MaxLowCoverAngle = 60;

        /// <summary>
        /// Maximum angle of a tall cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a tall cover relative to the enemy.")]
        public float MaxTallCoverAngle = 40;

        /// <summary>
        /// Maximum distance of a cover for AI to take.
        /// </summary>
        [Tooltip("Maximum distance of a cover for AI to take.")]
        public float MaxCoverDistance = 30;

        /// <summary>
        /// AI won't switch to cover positions closer than this distance.
        /// </summary>
        [Tooltip("AI won't switch to cover positions closer than this distance.")]
        public float MinSwitchDistance = 6;

        /// <summary>
        /// AI avoids taking covers that are closer to the enemy.
        /// </summary>
        [Tooltip("AI avoids taking covers that are closer to the enemy.")]
        public float AvoidDistance = 6;

        #endregion

        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private NavMeshPath _path;
        private Vector3[] _corners = new Vector3[32];

        private bool _isRunning = true;
        private Cover _targetCover;
        private Vector3 _targetPosition;
        private int _targetDirection;
        private Vector3 _threatPosition;
        private bool _hasThreat;
        private Cover _registeredCover;
        private CoverCache _covers = new CoverCache();
        private bool _hasAskedToStopMoving;
        private bool _hasTakenTheRightCover;

        #endregion

        #region Events

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        /// <param name="position"></param>
        public void OnThreatPosition(Vector3 position)
        {
            if (_hasThreat)
                _threatPosition = position;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Sets the component to run towards future covers.
        /// </summary>
        public void ToRunToCovers()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Sets the component to walk towards future covers.
        /// </summary>
        public void ToWalkToCovers()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Told by the brains to take the cloest cover.
        /// </summary>
        public void ToTakeCover(Vector3 position)
        {
            _hasThreat = false;
            _threatPosition = position;
            _targetCover = null;

            if (isActiveAndEnabled)
            {
                _covers.Reset(transform.position, MaxCoverDistance);

                foreach (var item in _covers.Items)
                {
                    takeCover(item.Cover, item.Position, item.Direction, 1);
                    Message("OnFoundCover");
                    return;
                }
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take the closest suitable cover.
        /// </summary>
        public void ToTakeCoverAgainst(Vector3 position)
        {
            _hasThreat = true;
            _threatPosition = position;
            _targetCover = null;

            if (isActiveAndEnabled)
            {
                _covers.Reset(transform.position, MaxCoverDistance);

                foreach (var item in _covers.Items)
                    if (isValidCover(item.Cover, item.Position, item.Direction, true))
                    {
                        takeCover(item.Cover, item.Position, item.Direction, 1);
                        Message("OnFoundCover");
                        return;
                    }
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take a cover closer to the enemy.
        /// </summary>
        public void ToSwitchCover()
        {
            _targetCover = null;

            if (isActiveAndEnabled)
            {
                _covers.Reset(transform.position, MaxCoverDistance);

                var currentDistance = Vector3.Distance(transform.position, _threatPosition);

                foreach (var item in _covers.Items)
                    if (Vector3.Distance(transform.position, item.Position) >= MinSwitchDistance &&
                        Vector3.Distance(item.Position, _threatPosition) < currentDistance &&
                        isValidCover(item.Cover, item.Position, item.Direction, true))
                    {
                        takeCover(item.Cover, item.Position, item.Direction, 1);
                        Message("OnFoundCover");
                        return;
                    }
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to take a cover closer to the enemy.
        /// </summary>
        public void ToTakeCoverCloseTo(AIBaseRegrouper regrouper)
        {
            _targetCover = null;

            if (isActiveAndEnabled)
            {
                var movement = regrouper.GetComponent<AIMovement>();
                var position = regrouper.transform.position;

                if (movement != null)
                    position = movement.Destination;

                _covers.Reset(position, MaxCoverDistance);

                foreach (var item in _covers.Items)
                    if (Vector3.Distance(item.Position, position) <= regrouper.Radius)
                        if (isValidCover(item.Cover, item.Position, item.Direction, true))
                        {
                            takeCover(item.Cover, item.Position, item.Direction, 1);
                            Message("OnFoundCover");
                            return;
                        }
            }

            updateRegistration();
        }

        /// <summary>
        /// Told by the brains to stop moving towards a cover.
        /// </summary>
        public void ToStopMoving()
        {
            if (_actor.Cover != _targetCover)
                _targetCover = null;
        }

        /// <summary>
        /// Told by the brains to exit cover.
        /// </summary>
        public void ToLeaveCover()
        {
            _targetCover = null;
            updateRegistration();
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();

            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (_actor == null || !_actor.IsAlive)
                return;

            updateRegistration();

            if (_motor.Cover != _targetCover)
            {
                if (_motor.PotentialCover != null &&
                    (_motor.PotentialCover == _targetCover ||
                     _motor.PotentialCover.LeftAdjacent == _targetCover ||
                     _motor.PotentialCover.RightAdjacent == _targetCover))
                {
                    _motor.InputTakeCover();
                }
                else if (_motor.Cover != null && _motor.Cover.LeftAdjacent != _targetCover && _motor.Cover.RightAdjacent != _targetCover)
                    _motor.InputLeaveCover();
                else if (Vector3.Distance(transform.position, _targetPosition) < 0.5f)
                    _motor.InputImmediateCoverSearch();
            }

            if (_targetCover != null && _hasThreat)
            {
                if (!isValidCover(_targetCover, _targetPosition, _targetDirection, false))
                    Message("OnInvalidCover");
            }

            if (_targetCover != null && _targetCover == _motor.Cover && _targetCover.IsTall)
            {
                if (_targetDirection > 0)
                {
                    _motor.InputStandRight();

                    if (!_motor.IsNearRightCorner)
                        _motor.InputMovement(new CharacterMovement(_targetCover.Right, 0.5f));
                }
                else
                {
                    _motor.InputStandLeft();

                    if (!_motor.IsNearLeftCorner)
                        _motor.InputMovement(new CharacterMovement(_targetCover.Left, 0.5f));
                }
            }

            if (_targetCover != null && _motor.Cover != null && Vector3.Distance(_actor.transform.position, _targetPosition) < 0.5f &&
                (_motor.Cover == _targetCover ||
                 _motor.Cover.LeftAdjacent == _targetCover ||
                 _motor.Cover.RightAdjacent == _targetCover))
            {
                if (!_hasTakenTheRightCover)
                {
                    _hasTakenTheRightCover = true;
                    Message("OnFinishTakeCover");
                }

                if (!_hasAskedToStopMoving)
                {
                    _hasAskedToStopMoving = true;
                    Message("ToStopMoving");
                }
            }
            else
                _hasAskedToStopMoving = false;
        }

        #endregion

        #region Private methods

        private void updateRegistration()
        {
            if (_registeredCover != _targetCover)
            {
                if (_registeredCover != null)
                    _registeredCover.UnregisterUser(_actor);

                _registeredCover = _targetCover;
            }

            if (_registeredCover != null)
            {
                if (_actor.Cover == _registeredCover)
                    _registeredCover.RegisterUser(_actor, _actor.transform.position);
                else
                    _registeredCover.RegisterUser(_actor, _targetPosition);
            }
        }

        private void takeCover(Cover cover, Vector3 position, int direction, float speed)
        {
            _targetPosition = position;
            _targetCover = cover;
            _targetDirection = direction;
            _hasTakenTheRightCover = false;
            _hasAskedToStopMoving = false;
            updateRegistration();

            if (_isRunning)
                Message("ToRunTo", position);
            else
                Message("ToWalkTo", position);
        }

        private bool isValidCover(Cover cover, Vector3 position, int direction, bool checkPath)
        {
            if (!_hasThreat)
                return true;

            if (Vector3.Distance(position, _threatPosition) < AvoidDistance)
                return false;

            if (!AIUtil.IsGoodAngle(MaxTallCoverAngle,
                                    MaxLowCoverAngle,
                                    cover,
                                    position,
                                    _threatPosition,
                                    cover.IsTall))
                return false;

            if (!AIUtil.IsCoverPositionFree(cover, position, 1, _actor))
                return false;

            var aimPosition = position;

            if (cover.IsTall)
            {
                var angle = Util.AngleOfVector(_threatPosition - position);

                if (direction > 0)
                {
                    if (!cover.IsFrontField(angle, _motor.CoverSettings.Angles.TallRightCornerFront))
                        return false;

                    if (!cover.IsRight(angle, _motor.CoverSettings.Angles.RightCorner, false))
                        return false;

                    aimPosition = cover.RightCorner(cover.Bottom, _motor.CoverSettings.CornerOffset.x);
                }
                else if (direction < 0)
                {
                    if (!cover.IsFrontField(angle, _motor.CoverSettings.Angles.TallLeftCornerFront))
                        return false;

                    if (!cover.IsLeft(angle, _motor.CoverSettings.Angles.LeftCorner, false))
                        return false;

                    aimPosition = cover.LeftCorner(cover.Bottom, _motor.CoverSettings.CornerOffset.x);
                }
            }

            if (AIUtil.IsObstructed(aimPosition + (_actor.StandingTopPosition - transform.position),
                                    _threatPosition + Vector3.up * 2,
                                    100))
                return false;

            if (checkPath)
                if (Vector3.Distance(transform.position, _threatPosition) > AvoidDistance)
                {
                    if (NavMesh.CalculatePath(transform.position, position, 1, _path))
                    {
                        for (int i = 0; i < _path.GetCornersNonAlloc(_corners); i++)
                        {
                            var a = i == 0 ? transform.position : _corners[i - 1];
                            var b = _corners[i];

                            var closest = Util.FindClosestToPath(a, b, _threatPosition);

                            if (Vector3.Distance(closest, _threatPosition) < AvoidDistance)
                                return false;
                        }
                    }
                    else
                        return false;
                }

            return true;
        }

        #endregion
    }
}
