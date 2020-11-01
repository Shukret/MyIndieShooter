using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    public class AISight : AIBase
    {
        #region Properties

        /// <summary>
        /// All currently visible actors
        /// </summary>
        public IEnumerable<Actor> Visible
        {
            get { return _visible; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Distance for AI to see objects in the world.
        /// </summary>
        [Tooltip("Distance for AI to see objects in the world.")]
        public float Distance = 25;

        /// <summary>
        /// Field of sight to notice changes in the world.
        /// </summary>
        [Tooltip("Field of sight to notice changes in the world.")]
        public float FieldOfView = 160;

        /// <summary>
        /// Time in seconds between each visibility update.
        /// </summary>
        [Tooltip("Time in seconds between each visibility update.")]
        public float UpdateDelay = 0.1f;

        /// <summary>
        /// Should a debug graphic be drawn to show the field of view.
        /// </summary>
        [Tooltip("Should a debug graphic be drawn to show the field of view.")]
        public bool DebugFOV = false;

        #endregion

        #region Private fields

        private Actor _actor;

        private HashSet<Actor> _visible = new HashSet<Actor>();
        private HashSet<Actor> _oldVisible = new HashSet<Actor>();

        private HashSet<Actor> _knownAlive = new HashSet<Actor>();
        private HashSet<Actor> _oldKnownAlive = new HashSet<Actor>();

        private Collider[] _colliders = new Collider[128];

        private float _wait = 0;

        private bool _isAlerted;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains that a threat is known.
        /// </summary>
        public void OnThreat(Actor threat)
        {
            _isAlerted = true;

            if (!_knownAlive.Contains(threat))
                _knownAlive.Add(threat);
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            _wait -= Time.deltaTime;

            if (_wait > float.Epsilon)
                return;

            _wait = Random.Range(UpdateDelay * 0.8f, UpdateDelay * 1.2f);

            _oldVisible.Clear();

            foreach (var actor in _visible)
                _oldVisible.Add(actor);

            _visible.Clear();

            foreach (var actor in _oldVisible)
                if (actor != null && actor.IsAlive && AIUtil.IsInSight(_actor, actor.TopPosition, actor.GetViewDistance(Distance, _isAlerted), FieldOfView))
                    _visible.Add(actor);
                else
                {
                    Message("OnUnseeActor", actor);

                    if (!actor.IsAlive)
                    {
                        _knownAlive.Remove(actor);
                        Message("OnSeeDeath", actor);
                    }
                }

            var count = Physics.OverlapSphereNonAlloc(_actor.TopPosition, Distance, _colliders, 0x1, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var actor = Actors.Get(_colliders[i].gameObject);

                if (actor != null && actor != _actor && actor.IsAlive && !_oldVisible.Contains(actor))
                    if (AIUtil.IsInSight(_actor, actor.TopPosition, actor.GetViewDistance(Distance, _isAlerted), FieldOfView))
                    {
                        _visible.Add(actor);
                        Message("OnSeeActor", actor);
                    }
            }

            _oldKnownAlive.Clear();

            foreach (var known in _knownAlive)
                _oldKnownAlive.Add(known);

            _knownAlive.Clear();

            foreach (var actor in _oldKnownAlive)
                if (_visible.Contains(actor))
                    _knownAlive.Add(actor);
                else if (AIUtil.IsInSight(_actor, actor.TopPosition, actor.GetViewDistance(Distance, _isAlerted), FieldOfView))
                    Message("OnSeeDeath", actor);
                else
                    _knownAlive.Add(actor);
        }

        private void OnDrawGizmos()
        {
            if (DebugFOV)
                drawFOV();
        }

        private void OnDrawGizmosSelected()
        {
            if (DebugFOV)
                drawFOV();
        }

        private void drawFOV()
        {
            var direction = _actor == null ? transform.forward : _actor.HeadDirection;
            var head = _actor == null ? (transform.position + Vector3.up * 2) : _actor.StandingTopPosition;

            direction = Quaternion.AngleAxis(-FieldOfView * 0.5f, Vector3.up) * direction;

            Handles.color = new Color(0.5f, 0.3f, 0.3f, 0.1f);
            Handles.DrawSolidArc(head, Vector3.up, direction, FieldOfView, Distance);

            Handles.color = new Color(1, 0.3f, 0.3f, 0.75f);
            Handles.DrawWireArc(head, Vector3.up, direction, FieldOfView, Distance);
            Handles.DrawLine(head, head + direction * Distance);
            Handles.DrawLine(head, head + Quaternion.AngleAxis(FieldOfView, Vector3.up) * direction * Distance);
        }

        #endregion
    }
}
