using UnityEngine;

namespace CoverShooter
{
    public class BaseBrain : AIBase
    {
        /// <summary>
        /// Actor component found in the same object.
        /// </summary>
        public Actor Actor
        {
            get
            {
                if (_actor == null)
                    _actor = GetComponent<Actor>();

                return _actor;
            }
        }

        /// <summary>
        /// Actor component of the threating character.
        /// </summary>
        public Actor Threat
        {
            get { return _threat; }
        }

        /// <summary>
        /// Time in seconds since the threat was last seen.
        /// </summary>
        public float ThreatAge
        {
            get { return Time.timeSinceLevelLoad - _lastSeenThreatTime; }
        }

        /// <summary>
        /// Last known position of the assigned threat.
        /// </summary>
        public Vector3 LastKnownThreatPosition
        {
            get { return _lastKnownThreatPosition; }
        }

        /// <summary>
        /// Time since level load that the threat was seen.
        /// </summary>
        public float LastSeenThreatTime
        {
            get { return _lastSeenThreatTime; }
        }

        /// <summary>
        /// Is the threat currently seen.
        /// </summary>
        public bool CanSeeTheThreat
        {
            get { return _canSeeTheThreat; }
        }

        /// <summary>
        /// Cover that is currently known to be used by the threat.
        /// </summary>
        public Cover ThreatCover
        {
            get { return _threatCover; }
        }

        /// <summary>
        /// Was the last known threat position a position the threat occupied or was it just an indirect explosion.
        /// </summary>
        public bool IsActualThreatPosition
        {
            get { return _isActualThreatPosition; }
        }

        /// <summary>
        /// Was any threat seen at least once.
        /// </summary>
        public bool HasSeenTheEnemy
        {
            get { return _hasSeenTheEnemy; }
        }

        private Actor _actor;

        private Actor _threat;
        private Vector3 _lastKnownThreatPosition;
        private float _lastSeenThreatTime;
        private Cover _threatCover;
        private bool _canSeeTheThreat;
        private bool _isActualThreatPosition;
        private bool _hasSeenTheEnemy;

        protected void RemoveThreat()
        {
            _threat = null;
            _canSeeTheThreat = false;
        }

        /// <summary>
        /// Sets the threat state to unseen.
        /// </summary>
        protected void UnseeThreat()
        {
            _canSeeTheThreat = false;
        }

        /// <summary>
        /// Updates threat state. Implicitly marks it as an unseen threat.
        /// </summary>
        protected void SetUnseenThreat(bool isDirect, Actor threat, Vector3 position, Cover threatCover)
        {
            SetThreat(false, isDirect, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        /// <summary>
        /// Updates threat state. Implicitly marks it as a visible threat.
        /// </summary>
        protected void SetThreat(Actor threat, Vector3 position, Cover threatCover)
        {
            SetThreat(true, true, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        /// <summary>
        /// Updates threat state. Time is given since level load.
        /// </summary>
        protected void SetThreat(bool isVisible, bool isActual, Actor threat, Vector3 position, Cover threatCover, float time)
        {
            var lastThreat = _threat;

            _lastSeenThreatTime = time;
            _lastKnownThreatPosition = position;
            _threatCover = threatCover;
            _threat = threat;
            _canSeeTheThreat = isVisible;
            _isActualThreatPosition = isActual;

            if (_canSeeTheThreat)
                _hasSeenTheEnemy = true;

            if (_threat != lastThreat)
                Message("OnThreat", _threat);

            if (threat != null)
                Message("OnThreatPosition", _lastKnownThreatPosition);
        }
    }
}
