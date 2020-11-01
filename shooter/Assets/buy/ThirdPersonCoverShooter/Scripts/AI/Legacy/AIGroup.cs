using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Information about a target and AI assigned to it.
    /// </summary>
    class AIGroupTarget
    {
        public Actor Target;
        public List<AssignedAI> Unforced = new List<AssignedAI>();
        public List<AssignedAI> Forced = new List<AssignedAI>();

        public AIGroupTarget(Actor target)
        {
            Target = target;
        }

        /// <summary>
        /// Adds an ai to the assignee list. Removes other AI if they are further away and the list has reached the max.
        /// </summary>
        public void CheckIn(AIController ai, int max, bool isForced)
        {
            if (max <= 0)
            {
                Unforced.Clear();
                return;
            }

            var distance = Vector3.Distance(ai.transform.position, Target.transform.position);

            if (isForced)
                Forced.Add(new AssignedAI(ai, distance));
            else
            {
                while (Unforced.Count >= max)
                {
                    if (Unforced[Unforced.Count - 1].Distance > distance)
                        Unforced.RemoveAt(Unforced.Count - 1);
                    else
                        return;
                }

                Unforced.Add(new AssignedAI(ai, distance));
            }
        }
    }

    struct AssignedAI
    {
        public AIController AI;
        public float Distance;

        public AssignedAI(AIController ai, float distance)
        {
            AI = ai;
            Distance = distance;
        }
    }

    public class AIGroup : MonoBehaviour
    {
        /// <summary>
        /// Returns a list of all registered AI members.
        /// </summary>
        public IEnumerable<AIController> Members
        {
            get { return _list; }
        }

        /// <summary>
        /// Returns a list of all aggressive AI members.
        /// </summary>
        public IEnumerable<AIController> Aggressive
        {
            get { return _aggressive; }
        }

        /// <summary>
        /// Maximum number of aggressive attackers allowed at the same time.
        /// </summary>
        [Tooltip("Maximum number of aggressive attackers allowed at the same time.")]
        public int MaxAggressive = 3;

        /// <summary>
        /// How long should the AI be kept aggressive no matter what.
        /// </summary>
        [Tooltip("How long should the AI be kept aggressive no matter what.")]
        public float Sustain = 3;

        private List<AIController> _list = new List<AIController>();
        private List<AIController> _aggressive = new List<AIController>();
        private List<AIController> _potentialAggressive = new List<AIController>();
        private Dictionary<Actor, AIGroupTarget> _targets = new Dictionary<Actor, AIGroupTarget>();
        private Dictionary<AIController, float> _aggressiveDuration = new Dictionary<AIController, float>();
        private List<Actor> _targetsToRemove = new List<Actor>();

        private void LateUpdate()
        {
            foreach (var target in _targets.Values)
            {
                target.Forced.Clear();
                target.Unforced.Clear();
            }

            foreach (var ai in _aggressive)
                if (_aggressiveDuration.ContainsKey(ai))
                {
                    var value = _aggressiveDuration[ai];

                    if (value >= Sustain)
                        _aggressiveDuration.Remove(ai);
                    else
                        _aggressiveDuration[ai] = value + Time.deltaTime;
                }
                else
                    _aggressiveDuration[ai] = 0;

            _aggressive.Clear();

            foreach (var ai in _potentialAggressive)
            {
                var enemy = ai.Situation.Threat;

                if (enemy != null)
                {
                    if (!_targets.ContainsKey(enemy))
                        _targets[enemy] = new AIGroupTarget(enemy);

                    _targets[enemy].CheckIn(ai, MaxAggressive, _aggressiveDuration.ContainsKey(ai));
                }
            }

            foreach (var target in _targets.Values)
            {
                if (target.Forced.Count == 0 && target.Unforced.Count == 0)
                    _targetsToRemove.Add(target.Target);
                else
                {
                    foreach (var assignee in target.Forced)
                        _aggressive.Add(assignee.AI);

                    var left = MaxAggressive - target.Forced.Count;

                    if (left > target.Unforced.Count)
                        left = target.Unforced.Count;

                    for (int i = 0; i < left; i++)
                        _aggressive.Add(target.Unforced[i].AI);
                }
            }

            foreach (var target in _targetsToRemove)
                _targets.Remove(target);

            _targetsToRemove.Clear();
            _potentialAggressive.Clear();
        }

        public bool IsAggressive(AIController ai)
        {
            return _aggressive.Contains(ai);
        }

        /// <summary>
        /// Tell the group that the AI wants to be aggressive the next frame.
        /// </summary>
        public void MarkAsPotentialAggressive(AIController ai, bool isForced)
        {
            if (!_potentialAggressive.Contains(ai))
                _potentialAggressive.Add(ai);

            if (isForced)
                _aggressiveDuration[ai] = 0;
        }

        /// <summary>
        /// Registers an AI as part of the group.
        /// </summary>
        public void Register(AIController ai)
        {
            if (!_list.Contains(ai))
                _list.Add(ai);
        }

        /// <summary>
        /// Removes the AI from the registered group.
        /// </summary>
        public void Unregister(AIController ai)
        {
            if (_list.Contains(ai))
                _list.Remove(ai);
        }
    }
}
