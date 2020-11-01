using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    public class AIInvestigation : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance to the investigation position for it to be marked as investigated.
        /// </summary>
        [Tooltip("Distance to the investigation position for it to be marked as investigated.")]
        public float VerifyDistance = 10;

        /// <summary>
        /// At which height the AI confirms the point as investigated.
        /// </summary>
        [Tooltip("At which height the AI confirms the point as investigated.")]
        public float VerifyHeight = 0.3f;

        /// <summary>
        /// Radius of an investigation point when it's close to a cover. AI tries to verify all of it is clear of enemies when investigating. Aiming is done at the central point so keep the radius small.
        /// </summary>
        [Tooltip("Radius of an investigation point when it's close to a cover. AI tries to verify all of it is clear of enemies when investigating. Aiming is done at the central point so keep the radius small.")]
        [HideInInspector]
        public float VerifyRadius = 1;

        /// <summary>
        /// Field of view when checking an investigation position.
        /// </summary>
        [Tooltip("Field of view when checking an investigation position.")]
        public float FieldOfView = 90;

        /// <summary>
        /// Distance to a cover to maintain when approaching to see behind it.
        /// </summary>
        [Tooltip("Distance to a cover to maintain when approaching to see behind it.")]
        public float CoverOffset = 2;

        /// <summary>
        /// Cover search radius around an investigation point. Closest cover will be checked when investigating.
        /// </summary>
        [Tooltip("Cover search radius around an investigation point. Closest cover will be checked when investigating.")]
        [HideInInspector]
        public float CoverSearchDistance = 3;

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _isInvestigating;
        private Vector3 _position;
        private Cover _cover;
        private bool _hasReachedCoverLine;
        private Vector3 _approachPosition;
        private float _verifyDistance;

        private NavMeshPath _path;
        private Vector3[] _corners = new Vector3[32];
        private Collider[] _colliders = new Collider[32];

        #endregion

        #region Commands

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void InvestigationCheck()
        {
            if (isActiveAndEnabled)
                Message("InvestigationResponse");
        }

        /// <summary>
        /// Told by the brains to investigate a position.
        /// </summary>
        /// <param name="position"></param>
        public void ToInvestigatePosition(Vector3 position)
        {
            _isInvestigating = true;

            _position = position;
            _cover = null;
            var minDistance = 0f;

            for (int i = 0; i < Physics.OverlapSphereNonAlloc(position, CoverSearchDistance, _colliders, 0x1 << 8, QueryTriggerInteraction.Collide); i++)
            {
                var cover = CoverSearch.GetCover(_colliders[i].gameObject);

                if (cover != null)
                {
                    var point = cover.ClosestPointTo(position, 0.3f, 0.3f);
                    var distance = Vector3.Distance(position, point);

                    if (distance < minDistance || _cover == null)
                    {
                        _cover = cover;
                        _position = point;
                        minDistance = distance;
                    }
                }
            }

            _verifyDistance = Util.GetViewDistance(_position, VerifyDistance, true);

            if (_cover == null)
            {
                _hasReachedCoverLine = false;

                if (isActiveAndEnabled)
                {
                    Message("ToWalkTo", position);
                    Message("OnInvestigationStart");
                }
            }
            else
            {
                var vector = _position - transform.position;
                _hasReachedCoverLine = Vector3.Dot(_cover.Forward, vector) > 0;

                if (_hasReachedCoverLine)
                {
                    if (isActiveAndEnabled)
                    {
                        Message("ToWalkTo", _position);
                        Message("OnInvestigationStart");
                    }
                }
                else
                {
                    var left = _cover.LeftCorner(_cover.Bottom, CoverOffset) - _cover.Forward * 1.0f;
                    var right = _cover.RightCorner(_cover.Bottom, CoverOffset) - _cover.Forward * 1.0f;

                    AIUtil.Path(ref _path, transform.position, left);
                    var leftLength = 0f;

                    if (_path.status == NavMeshPathStatus.PathInvalid)
                        leftLength = 999999f;
                    else
                        for (int i = 1; i < _path.GetCornersNonAlloc(_corners); i++)
                            leftLength += Vector3.Distance(_corners[i], _corners[i - 1]);

                    AIUtil.Path(ref _path, transform.position, right);
                    var rightLength = 0f;

                    if (_path.status == NavMeshPathStatus.PathInvalid)
                        rightLength = 999999f;
                    else
                        for (int i = 1; i < _path.GetCornersNonAlloc(_corners); i++)
                            rightLength += Vector3.Distance(_corners[i], _corners[i - 1]);

                    if (leftLength < rightLength)
                        _approachPosition = left;
                    else
                        _approachPosition = right;

                    var distance = Vector3.Distance(_approachPosition, _position);

                    if (distance + VerifyRadius > _verifyDistance)
                        _approachPosition = _position + Vector3.Normalize(_approachPosition - _position) * (_verifyDistance + VerifyRadius - 0.1f);

                    if (isActiveAndEnabled)
                    {
                        Message("ToWalkTo", _approachPosition);
                        Message("OnInvestigationStart");
                    }
                }
            }
        }

        /// <summary>
        /// Told by the brains to stop investigating.
        /// </summary>
        public void ToStopInvestigation()
        {
            _isInvestigating = false;
            Message("OnInvestigationStop");
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _path = new NavMeshPath();
        }

        private void Update()
        {
            if (!_isInvestigating)
                return;

            if (_cover != null && !_hasReachedCoverLine && Vector3.Distance(_approachPosition, transform.position) < 0.5f)
            {
                _hasReachedCoverLine = true;
                Message("ToWalkTo", _position);
            }

            if (_cover != null)
            {
                if (verify(_position) &&
                     verify(_position + _cover.Right * VerifyRadius) &&
                     verify(_position - _cover.Left * VerifyRadius))
                    Message("ToMarkPointInspected", _position);
            }
            else if (verify(_position))
                Message("ToMarkPointInspected", _position);
        }

        private bool verify(Vector3 position)
        {
            var distance = Vector3.Distance(transform.position, position);

            return (_cover != null && AIUtil.IsInSight(_actor, position + Vector3.up * VerifyHeight, _verifyDistance, FieldOfView)) || distance < VerifyRadius || (distance < _verifyDistance && _verifyDistance < VerifyRadius);
        }

        #endregion
    }
}
