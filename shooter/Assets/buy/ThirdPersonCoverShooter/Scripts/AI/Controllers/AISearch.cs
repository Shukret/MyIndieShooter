using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    [Serializable]
    public struct AdvancedAISearchSettings
    {
        /// <summary>
        /// Search points belong in the same search block if they are closer to each other than this distance.
        /// </summary>
        [Tooltip("Search points belong in the same search block if they are closer to each other than this distance.")]
        public float BlockThreshold;

        /// <summary>
        /// A search point is considered to belong in a block if it is closer than this value to it's center.
        /// </summary>
        [Tooltip("A search point is considered to belong in a block if it is closer than this value to it's center.")]
        public float BlockCenterThreshold;

        public static AdvancedAISearchSettings Default()
        {
            var settings = new AdvancedAISearchSettings();
            settings.BlockThreshold = 3;
            settings.BlockCenterThreshold = 6;

            return settings;
        }
    }

    [RequireComponent(typeof(Actor))]
    public class AISearch : AIBase
    {
        #region Public fields

        /// <summary>
        /// Maximum distance of a location for AI to search.
        /// </summary>
        [Tooltip("Maximum distance of a location for AI to search.")]
        public float MaxDistance = 100;

        /// <summary>
        /// Distance to target location the AI has to reach for it to be marked as investigated.
        /// </summary>
        [Tooltip("Distance to target location the AI has to reach for it to be marked as investigated.")]
        public float VerifyDistance = 16;

        /// <summary>
        /// At which height the AI confirms the point as investigated.
        /// </summary>
        [Tooltip("At which height the AI confirms the point as investigated.")]
        public float VerifyHeight = 0.7f;

        /// <summary>
        /// Offset from cover the AI keeps when approaching it from a side.
        /// </summary>
        [Tooltip("Offset from cover the AI keeps when approaching it from a side.")]
        public float CoverOffset = 2;

        /// <summary>
        /// Field of sight to register the search position.
        /// </summary>
        [Tooltip("Field of sight to register the search position.")]
        public float FieldOfView = 90;

        /// <summary>
        /// Distance at which AI turns from running to walking to safely investigate the position.
        /// </summary>
        [Tooltip("Distance at which AI turns from running to walking to safely investigate the position.")]
        public float WalkDistance = 8;

        /// <summary>
        /// Time in seconds for the AI to think that the position is safe. Used when communicating with other AIs so that they don't check it themselves.
        /// </summary>
        [Tooltip("Time in seconds for the AI to think that the position is safe. Used when communicating with other AIs so that they don't check it themselves.")]
        public float MaxInvestigationAge = 30;

        /// <summary>
        /// Advanced settings for how the search is conducted.
        /// </summary>
        [Tooltip("Advanced settings for how the search is conducted.")]
        public AdvancedAISearchSettings Advanced = AdvancedAISearchSettings.Default();

        /// <summary>
        /// Should a line to the intended search point be drawn in the editor.
        /// </summary>
        [Tooltip("Should a line to the intended search point be drawn in the editor.")]
        public bool DebugTarget = false;

        /// <summary>
        /// Should information about search points be displayed.
        /// </summary>
        [Tooltip("Should information about search points be displayed.")]
        public bool DebugPoints = false;

        #endregion

        #region Private fields

        private Actor _actor;

        private bool _hasSearchDirection;
        private bool _hasBlockDirection;
        private Vector3 _searchDirection;
        private Vector3 _blockDirection;
        private Vector3 _searchPosition;

        private bool _hasPoint;
        private int _pointIndex;
        private SearchPoint _point;

        private bool _hasPreviousPoint;
        private int _previousPointIndex;

        private bool _isSearching;
        private bool _wasRunning;
        private bool _hasApproached;

        private SearchPointData _points = new SearchPointData();
        private SearchBlock _block;
        private SearchBlockCache _blockCache;
        private List<SearchBlock> _investigatedBlocks = new List<SearchBlock>();
        private List<SearchBlock> _blocks = new List<SearchBlock>();

        private HashSet<Cover> _usedCovers = new HashSet<Cover>();

        private CoverCache _coverCache = new CoverCache();
        private SearchZoneCache _zoneCache = new SearchZoneCache();

        private List<InvestigatedPoint> _investigated = new List<InvestigatedPoint>();

        private List<AISearch> _friends = new List<AISearch>();

        private float _timeOfReset;

        private float _checkWait;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to start search at the current location.
        /// </summary>
        public void ToSearch()
        {
            startSearch();
            _searchPosition = transform.position;
            _hasSearchDirection = false;
        }

        /// <summary>
        /// Told by the brains to start searching a ta position.
        /// </summary>>
        public void ToSearchAt(SearchPoint point)
        {
            startSearch();
            setPoint(addPoint(point));
            _hasBlockDirection = _hasSearchDirection;
            _blockDirection = _searchDirection;
        }

        /// <summary>
        /// Told by the brains to stop searching.
        /// </summary>
        public void ToStopSearch()
        {
            _isSearching = false;
        }

        /// <summary>
        /// Told by the brains to force mark a position as investigated.
        /// </summary>
        public void ToMarkPointInspected(Vector3 position)
        {
            if (!isActiveAndEnabled)
                return;

            var point = new InvestigatedPoint(position);

            if (!considerPoint(point))
                markInvestigated(point);

            foreach (var friend in _friends)
                friend.considerPoint(point);
        }

        /// <summary>
        /// Told by the brains to forget all search history.
        /// </summary>
        public void ToClearSearchHistory()
        {
            _timeOfReset = Time.timeSinceLevelLoad;
            _investigated.Clear();
            _blocks.Clear();
            _block.Clear();
            _coverCache.Items.Clear();
        }

        #endregion

        #region Events

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void SearchCheck()
        {
            if (isActiveAndEnabled)
                Message("SearchResponse");
        }

        /// <summary>
        /// Notified that a friend was found.
        /// </summary>
        public void OnFoundFriend(Actor friend)
        {
            var search = friend.GetComponent<AISearch>();

            if (search != null && !_friends.Contains(search))
            {
                if (isActiveAndEnabled)
                    foreach (var investigated in search._investigated)
                        considerPoint(investigated);

                _friends.Add(search);
            }
        }

        /// <summary>
        /// Notified that a friend got out of range.
        /// </summary>
        /// <param name="friend"></param>
        public void OnLostFriend(Actor friend)
        {
            var search = friend.GetComponent<AISearch>();

            if (search != null && _friends.Contains(search))
                _friends.Remove(search);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _block = new SearchBlock(_points);
            _blockCache = new SearchBlockCache(_points);
            _actor = GetComponent<Actor>();
        }

        private void Update()
        {
            for (int i = _investigated.Count - 1; i >= 0; i--)
                if (_investigated[i].Time < Time.timeSinceLevelLoad - MaxInvestigationAge)
                    _investigated.RemoveAt(i);

            if (!_isSearching)
                return;

            if (_blocks.Count == 0 && !_hasPoint)
            {
                ToClearSearchHistory();

                _hasPreviousPoint = false;
                _points.Clear();

                foreach (var block in _investigatedBlocks)
                    _blockCache.Give(block);

                _investigatedBlocks.Clear();
                _usedCovers.Clear();
                _coverCache.Reset(transform.position, MaxDistance, false);

                foreach (var item in _coverCache.Items)
                {
                    if (_usedCovers.Contains(item.Cover))
                        continue;

                    var cover = item.Cover;

                    while (cover.LeftAdjacent != null && !_usedCovers.Contains(cover.LeftAdjacent))
                        cover = cover.LeftAdjacent;

                    var index = -1;

                    while (cover != null)
                    {
                        _usedCovers.Add(cover);

                        var left = cover.LeftCorner(cover.Bottom) - cover.Forward * 0.25f;
                        var right = cover.RightCorner(cover.Bottom) - cover.Forward * 0.25f;
                        var vector = right - left;
                        var length = vector.magnitude;

                        var leftApproach = left;
                        var rightApproach = right;

                        {
                            NavMeshHit hit;
                            var position = left + cover.Left * CoverOffset;

                            if (NavMesh.Raycast(left, position, out hit, 1))
                                leftApproach = left;
                            else
                                leftApproach = position;
                        }

                        {
                            NavMeshHit hit;
                            var position = right + cover.Right * CoverOffset;

                            if (NavMesh.Raycast(right, position, out hit, 1))
                                rightApproach = right;
                            else
                                rightApproach = position;
                        }

                        if (cover.LeftAdjacent != null && cover.RightAdjacent != null)
                        {
                            leftApproach = left;
                            rightApproach = right;
                        }
                        else if (cover.LeftAdjacent != null)
                            leftApproach = rightApproach;
                        else if (cover.RightAdjacent != null)
                            rightApproach = leftApproach;

                        possiblyAddRightPoint(ref index, new SearchPoint(left, leftApproach, -cover.Forward, false));

                        if (length > Advanced.BlockThreshold * 2)
                        {
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.2f, leftApproach, -cover.Forward, false));
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.4f, leftApproach, -cover.Forward, false));
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.6f, rightApproach, -cover.Forward, false));
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.8f, rightApproach, -cover.Forward, false));
                        }
                        else if (length > Advanced.BlockThreshold)
                        {
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.33f, leftApproach, -cover.Forward, false));
                            possiblyAddRightPoint(ref index, new SearchPoint(left + vector * 0.66f, rightApproach, -cover.Forward, false));
                        }

                        possiblyAddRightPoint(ref index, new SearchPoint(right, rightApproach, -cover.Forward, false));

                        if (cover.RightAdjacent != null && !_usedCovers.Contains(cover.RightAdjacent))
                            cover = cover.RightAdjacent;
                        else
                            cover = null;
                    }
                }

                _zoneCache.Reset(transform.position, MaxDistance);

                foreach (var block in _zoneCache.Items)
                    foreach (var position in block.Points(Advanced.BlockThreshold))
                        addPoint(new SearchPoint(position, false));

                mergeBlocks();
            }

            if (DebugPoints)
            {
                foreach (var block in _blocks)
                    debugBlock(block);

                foreach (var block in _investigatedBlocks)
                    debugBlock(block);
            }

            if (_blocks.Count == 0 && !_hasPoint)
                return;

            if (_block.Empty && !_hasPoint)
            {
                var pickedIndex = -1;
                var previousValue = 0f;

                for (int i = 0; i < _blocks.Count; i++)
                {
                    var vector = _searchPosition - _blocks[i].Center;
                    var distance = vector.magnitude;
                    var direction = vector / distance;

                    var value = distance;

                    if (_hasBlockDirection)
                        value *= -Vector3.Dot(direction, _blockDirection) * 0.5f + 1.5f;
                    else
                        value *= -Vector3.Dot(direction, _actor.HeadDirection) * 0.5f + 1.5f;

                    if (pickedIndex < 0 || value < previousValue)
                    {
                        pickedIndex = i;
                        previousValue = value;
                    }
                }

                _block = _blocks[pickedIndex];
                _blocks.RemoveAt(pickedIndex);
                _investigatedBlocks.Add(_block);

                _hasBlockDirection = true;
                _blockDirection = (_block.Center - _searchPosition).normalized;
            }

            if (!_hasPoint)
            {
                int index;
                float value;
                findBestPoint(_block, out index, out value);

                setPoint(_block.Indices[index]);
                _block.Investigate(index);
            }

            if (!_hasApproached && !shouldApproach(_point))
            {
                _hasApproached = true;

                if (_wasRunning)
                    run();
                else
                    walk();
            }

            if (_wasRunning && !shouldRunTo(_point.Position))
                walk();

            _checkWait -= Time.deltaTime;

            if (_checkWait <= float.Epsilon)
            {
                glimpse(_block);

                for (int b = _blocks.Count - 1; b >= 0; b--)
                {
                    glimpse(_blocks[b]);

                    if (_blocks[b].Empty)
                    {
                        _investigatedBlocks.Add(_blocks[b]);
                        _blocks.RemoveAt(b);
                    }
                }

                _checkWait = 0.25f;
            }

            if (DebugTarget)
                Debug.DrawLine(transform.position, _point.Position, Color.yellow);

            if (canBeInvestigated(_point))
            {
                var point = new InvestigatedPoint(_point.Position);
                _hasPoint = false;

                markInvestigated(point);

                foreach (var friend in _friends)
                    friend.considerPoint(point);
            }
        }

        #endregion

        #region Private methods

        private void debugBlock(SearchBlock block)
        {
            var color = Color.white;

            switch (block.Index % 5)
            {
                case 0: color = Color.red; break;
                case 1: color = Color.green; break;
                case 2: color = Color.blue; break;
                case 3: color = Color.yellow; break;
                case 4: color = Color.cyan; break;
            }

            for (int i = 0; i < block.Count; i++)
                debugPoint(block.Get(i), false, color);

            foreach (var index in block.InvestigatedIndices)
                debugPoint(_points.Points[index], !_hasPoint || index != _pointIndex, color);    
        }

        private void debugPoint(SearchPoint point, bool wasInvestigated, Color color)
        {
            Debug.DrawLine(point.Position, point.Position + Vector3.up * (wasInvestigated ? 0.2f : 0.75f), color);

            //if (point.Left >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Left].Position - point.Position) * 0.25f, Color.white);
            //if (point.Right >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Right].Position - point.Position) * 0.25f, Color.magenta);
        }

        private void findBestPoint(SearchBlock block, out int pointIndex, out float pointValue)
        {
            var pickedIndex = -1;
            var previousValue = 0f;

            var previousLeft = -1;
            var previousRight = -1;

            if (_hasPreviousPoint)
            {
                var previousPoint = _points.Points[_previousPointIndex];
                previousLeft = previousPoint.Left;
                previousRight = previousPoint.Right;
            }

            for (int i = 0; i < block.Count; i++)
            {
                var index = block.Indices[i];
                var point = block.Get(i);

                var vector = _searchPosition - point.Position;
                var distance = vector.magnitude;
                var direction = vector / distance;

                var value = distance;

                if (_hasPreviousPoint && (index == previousLeft || index == previousRight))
                    value *= -1;
                else
                {
                    if (_hasSearchDirection)
                        value *= -Vector3.Dot(direction, _searchDirection) * 0.5f + 1.5f;
                    else
                        value *= -Vector3.Dot(direction, _actor.HeadDirection) * 0.5f + 1.5f;
                }

                if (pickedIndex < 0 || (value > 0 && value < previousValue) || (value < 0 && previousValue < 0 && value > previousValue) || (value < 0 && previousValue > 0))
                {
                    pickedIndex = i;
                    previousValue = value;
                }
            }

            pointIndex = pickedIndex;
            pointValue = previousValue;
        }

        private void mergeBlocks()
        {
            for (int a = 0; a < _blocks.Count - 1; a++)
            {
                RESTART:

                for (int b = _blocks.Count - 1; b > a; b--)
                {
                    foreach (var ap in _blocks[a].Indices)
                        if (_blocks[b].IsClose(_points.Points[ap], Advanced.BlockThreshold, Advanced.BlockCenterThreshold))
                            goto SUCCESS;

                    continue;

                    SUCCESS:

                    foreach (var index in _blocks[b].Indices)
                        _blocks[a].Add(index);

                    _blockCache.Give(_blocks[b]);
                    _blocks.RemoveAt(b);

                    goto RESTART;
                }
            }

            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blocks[i];
                block.Index = i;
                _blocks[i] = block;
            }
        }

        private void possiblyAddRightPoint(ref int index, SearchPoint point)
        {
            NavMeshHit hit;

            if (!NavMesh.SamplePosition(point.Position, out hit, 0.2f, 1))
                return;
            else
                point.Position = hit.position;

            var new_ = addPoint(point);

            if (index >= 0)
                _points.LinkRight(index, new_);

            index = new_;
        }

        private int addPoint(SearchPoint point)
        {
            point.CalcVisibility(VerifyDistance, false);
            var index = _points.Add(point);

            if (!_block.Empty)
                if (_block.IsClose(point, Advanced.BlockThreshold, Advanced.BlockCenterThreshold))
                {
                    _block.Add(index);
                    return index;
                }

            for (int i = 0; i < _blocks.Count; i++)
                if (_blocks[i].IsClose(point, Advanced.BlockThreshold, Advanced.BlockCenterThreshold))
                {
                    _blocks[i].Add(index);
                    return index;
                }

            var new_ = _blockCache.Take();
            new_.Add(index);
            _blocks.Add(new_);

            return index;
        }

        private bool canBeInvestigated(SearchPoint point)
        {
            var position = point.Position + Vector3.up * VerifyHeight;
            var distanceToPoint = Vector3.Distance(transform.position, position);

            var checkDistance = VerifyDistance;

            if (point.Visibility < checkDistance)
                checkDistance = point.Visibility;

            if (distanceToPoint < checkDistance &&
                (distanceToPoint < 1 ||
                 AIUtil.IsInSight(_actor, position, checkDistance, FieldOfView)))
                return !point.RequiresReaching || distanceToPoint < 1.1f;

            return false;
        }

        private void glimpse(SearchBlock block)
        {
            for (int i = block.Count - 1; i >= 0; i--)
            {
                var p = block.Get(i);

                if (canBeInvestigated(p))
                {
                    var point = new InvestigatedPoint(p.Position);
                    markInvestigated(point);

                    foreach (var friend in _friends)
                        friend.considerPoint(point);

                    block.Investigate(i);
                }
            }
        }

        private bool considerPoint(InvestigatedPoint point)
        {
            if (point.Time < _timeOfReset)
                return false;

            if (_hasPoint && areCloseEnough(point, _point))
            {
                _hasPoint = false;
                markInvestigated(point);
                return true;
            }

            if (considerPoint(_block, point))
                return true;

            for (int i = 0; i < _blocks.Count; i++)
                if (considerPoint(_blocks[i], point))
                {
                    if (_blocks[i].Empty)
                    {
                        _investigatedBlocks.Add(_blocks[i]);
                        _blocks.RemoveAt(i);
                    }

                    return true;
                }

            return false;
        }

        private bool considerPoint(SearchBlock block, InvestigatedPoint point)
        {
            for (int i = 0; i < block.Count; i++)
                if (areCloseEnough(point, block.Get(i)))
                {
                    block.Investigate(i);
                    markInvestigated(point);
                    return true;
                }

            return false;
        }

        private void markInvestigated(InvestigatedPoint point)
        {
            _investigated.Add(point);
            Message("OnPointInvestigated", point.Position);
        }

        private bool areCloseEnough(InvestigatedPoint a, SearchPoint b)
        {
            if (Vector3.Distance(a.Position, b.Position) < 0.5f)
                return true;

            return false;
        }

        private bool shouldRunTo(Vector3 position)
        {
            var distance = Vector3.Distance(transform.position, position);

            if (distance > WalkDistance || (distance > VerifyDistance && !AIUtil.IsInSight(_actor, position, VerifyDistance, 360)))
                return true;
            else
                return false;
        }

        private void setPoint(int index)
        {
            _pointIndex = index;
            _point = _points.Points[index];
            _searchPosition = _point.Position;
            _hasPoint = true;

            _hasPreviousPoint = true;
            _previousPointIndex = index;

            _hasSearchDirection = true;
            _searchDirection = (_point.Position - transform.position).normalized;

            _hasApproached = !shouldApproach(_point);

            if (shouldRunTo(_point.Position))
                run();
            else
                walk();
        }

        private bool shouldApproach(SearchPoint point)
        {
            return Vector3.Dot(point.Normal, point.Position - transform.position) > 0;
        }

        private void walk()
        {
            _wasRunning = false;
            Message("ToSlowlyAimAt", _point.Position);
            Message("ToTurnAt", _point.Position);

            if (!_hasApproached)
                Message("ToWalkTo", _point.ApproachPosition);
            else
                Message("ToWalkTo", _point.Position);
        }

        private void run()
        {
            _wasRunning = true;
            Message("ToFaceWalkDirection");

            if (!_hasApproached)
                Message("ToRunTo", _point.ApproachPosition);
            else
                Message("ToRunTo", _point.Position);
        }

        private void startSearch()
        {
            _isSearching = true;
            _hasPoint = false;
        }

        #endregion
    }
}
