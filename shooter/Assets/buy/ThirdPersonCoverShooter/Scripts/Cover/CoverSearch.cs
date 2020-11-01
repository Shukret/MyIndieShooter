using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages a search for covers.
    /// </summary>
    public class CoverSearch
    {
        private Vector3 _position;
        private Vector3 _head;
        private CoverState _current;
        private float _radius;
        private CoverSettings _settings;

        private Collider[] _colliders = new Collider[16];
        private Cover[] _covers = new Cover[16];
        private int _colliderCount;

        private static Dictionary<GameObject, Cover> _coverMap = new Dictionary<GameObject, Cover>();

        /// <summary>
        /// Optimised way to get a cover component.
        /// </summary>
        public static Cover GetCover(GameObject gameObject)
        {
            if (!_coverMap.ContainsKey(gameObject))
                _coverMap[gameObject] = gameObject.GetComponent<Cover>();

            return _coverMap[gameObject];
        }

        /// <summary>
        /// Updates cover search.
        /// </summary>
        public void Update(CoverState current, Vector3 position, Vector3 head, float searchRadius, float capsuleRadius, CoverSettings settings)
        {
            _current = current;
            _colliderCount = Physics.OverlapSphereNonAlloc(position, searchRadius, _colliders);
            _position = position;
            _head = head;
            _radius = capsuleRadius;
            _settings = settings;

            for (int i = 0; i < _colliderCount; i++)
                _covers[i] = GetCover(_colliders[i].gameObject);
        }

        /// <summary>
        /// Find a cover closest to the character.
        /// </summary>
        public Cover FindClosest()
        {
            Cover result = null;

            for (int i = 0; i < _colliderCount; i++)
            {
                var cover = _covers[i];

                if (cover != null && cover == _current.Main && doesCoverFit(cover))
                    result = cover;
            }

            for (int i = 0; i < _colliderCount; i++)
            {
                var cover = _covers[i];

                if (cover != null && cover != _current.Main && doesCoverFit(cover))
                {
                    if (result == null)
                        result = cover;
                    else if (_current.MainChangeAge >= 1)
                    {
                        _head.y = _position.y;
                        var headDistance = Vector3.Distance(_head, cover.ClosestPointTo(_head, 0, 0));

                        if (headDistance < 0.3f)
                            result = cover;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Check if the given cover is fitting.
        /// </summary>
        private bool doesCoverFit(Cover cover)
        {
            if (cover == null || cover.Top < _position.y + 0.5f)
                return false;

            var position = _position + cover.Forward * _radius;
            var distance = Vector3.Distance(position, cover.ClosestPointTo(position, 0, 0));

            float radius;

            if (cover == _current.Main)
                radius = cover.CheckTall(_position.y) ? _settings.TallSideLeaveRadius : _settings.LowSideLeaveRadius;
            else
                radius = cover.CheckTall(_position.y) ? _settings.TallSideEnterRadius : _settings.LowSideEnterRadius;

            var isInFront = cover.IsInFront(_position, cover == _current.Main) &&
                            (cover.IsInFront(_position + cover.Right * radius, cover == _current.Main) || cover.RightAdjacent != null) &&
                            (cover.IsInFront(_position + cover.Left * radius, cover == _current.Main) || cover.LeftAdjacent != null);

            var isOld = isInFront && distance <= _settings.LeaveDistance && cover == _current.Main;
            if (isOld) return true;

            var isNew = isInFront && distance <= _settings.EnterDistance && cover != _current.Main;
            if (isNew) return true;

            return false;
        }
    }
}