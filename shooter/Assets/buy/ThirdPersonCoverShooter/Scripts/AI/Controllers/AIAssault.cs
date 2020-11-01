using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    public class AIAssault : AIBase
    {
        #region Public fields

        /// <summary>
        /// Distance at which the AI stops it's assault.
        /// </summary>
        [Tooltip("Distance at which the AI stops it's assault.")]
        public float StopDistance = 8;

        /// <summary>
        /// Chance the AI takes cover after assaulting an enemy.
        /// </summary>
        [Tooltip("Chance the AI takes cover after assaulting an enemy.")]
        [Range(0, 1)]
        public float TakeCoverChance = 0.5f;

        #endregion

        #region Private fields

        private bool _isAssaulting;
        private Vector3 _threatPosition;

        #endregion

        #region 

        /// <summary>
        /// Responds with an answer to a brain enquiry.
        /// </summary>
        public void AssaultCheck(Vector3 position)
        {
            if (isActiveAndEnabled && Vector3.Distance(transform.position, position) > StopDistance)
                Message("AssaultResponse");
        }

        /// <summary>
        /// Notified by the brains of a new threat position.
        /// </summary>
        public void OnThreatPosition(Vector3 position)
        {
            if (!_isAssaulting || !isActiveAndEnabled)
                return;

            if (Vector3.Distance(position, _threatPosition) > 0.5f)
            {
                _threatPosition = position;
                Message("ToRunTo", _threatPosition);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Commanded to start an assault towards a threat position.
        /// </summary>
        public void ToStartAssault(Vector3 position)
        {
            var wasAssaulting = _isAssaulting;
            _threatPosition = position;

            if (isActiveAndEnabled && Vector3.Distance(transform.position, position) > StopDistance)
            {
                _isAssaulting = true;

                Message("ToRunTo", position);

                if (!wasAssaulting)
                    Message("OnAssaultStart");
            }
        }

        /// <summary>
        /// Commanded to stop an assault.
        /// </summary>
        public void ToStopAssault()
        {
            if (_isAssaulting)
            {
                _isAssaulting = false;
                Message("ToStopMoving");

                if (Random.Range(0f, 1f) <= TakeCoverChance)
                    Message("ToFindCover");

                Message("OnAssaultStop");
            }
        }

        #endregion

        #region Behaviour

        private void Update()
        {
            if (!_isAssaulting)
                return;

            if (Vector3.Distance(transform.position, _threatPosition) <= StopDistance)
                ToStopAssault();
        }

        #endregion
    }
}
