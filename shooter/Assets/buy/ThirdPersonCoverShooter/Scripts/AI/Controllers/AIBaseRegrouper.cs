using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    public abstract class AIBaseRegrouper : AIBase
    {
        public List<Actor> Friends
        {
            get { return _friends; }
        }

        /// <summary>
        /// Maximum distance at which the regrouped actors will stand next to the regrouper.
        /// </summary>
        [Tooltip("Maximum distance at which the regrouped actors will stand next to the regrouper.")]
        public float Radius = 8;

        /// <summary>
        /// Time in seconds the units will maintain their new uncovered positions before searching for better spots.
        /// </summary>
        [Tooltip("Time in seconds the units will maintain their new uncovered positions before searching for better spots.")]
        public float UncoveredDuration = 8;

        /// <summary>
        /// Distance at which to search for friendly AI that will be regrouped.
        /// </summary>
        [Tooltip("Distance at which to search for friendly AI that will be regrouped.")]
        public float CallDistance = 20;

        /// <summary>
        /// Maximum number of regrouped units.
        /// </summary>
        [Tooltip("Maximum number of regrouped units.")]
        public int Limit = 6;

        private Actor _actor;

        private Collider[] _colliders = new Collider[128];
        private List<Actor> _friends = new List<Actor>();
        private List<Vector3> _takenPositions = new List<Vector3>();

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        public void TakePosition(Vector3 value)
        {
            _takenPositions.Add(value);
        }

        public bool IsPositionTaken(Vector3 position, float threshold = 1.0f)
        {
            foreach (var taken in _takenPositions)
                if (Vector3.Distance(taken, position) < threshold)
                    return true;

            return false;
        }

        public void Regroup()
        {
            _friends.Clear();
            _takenPositions.Clear();

            var count = Physics.OverlapSphereNonAlloc(_actor.transform.position, CallDistance, _colliders, 0x1, QueryTriggerInteraction.Ignore);
            var limit = Limit;

            for (int i = 0; i < count; i++)
            {
                var friend = Actors.Get(_colliders[i].gameObject);

                if (friend != null && friend != _actor && friend.IsAlive && friend.Side == _actor.Side)
                {
                    var distance = Vector3.Distance(_actor.transform.position, friend.transform.position);

                    if (distance < CallDistance)
                    {
                        if (limit <= 0)
                            break;
                        else
                            limit--;

                        friend.SendMessage("ToLeaveCover");
                        _friends.Add(friend);
                    }
                }
            }

            foreach (var friend in _friends)
                friend.SendMessage("ToRegroupAround", this);
        }
    }
}
