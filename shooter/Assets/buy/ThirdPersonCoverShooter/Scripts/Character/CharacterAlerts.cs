using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts on various character events.
    /// </summary>
    public class CharacterAlerts : MonoBehaviour
    {
        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Step = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Hurt = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Death = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Jump = 10;

        /// <summary>
        /// Distance at which step can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which step can be heard. Alert is not generated if value is zero or negative.")]
        public float Land = 10;

        private UpdatedAlert _step;
        private UpdatedAlert _hurt;
        private UpdatedAlert _death;
        private UpdatedAlert _jump;
        private UpdatedAlert _land;
        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        private void LateUpdate()
        {
            _step.Update();
            _hurt.Update();
            _death.Update();
            _jump.Update();
            _land.Update();
        }

        private void OnDisable()
        {
            _step.Kill();
            _hurt.Kill();
            _death.Kill();
            _jump.Kill();
            _land.Kill();
        }

        /// <summary>
        /// Generates a land alert.
        /// </summary>
        public void OnLand()
        {
            _land.Start(transform.position, Land, false, _actor, true);
        }

        /// <summary>
        /// Generates a hurt alert.
        /// </summary>
        public void OnHit(Hit hit)
        {
            _hurt.Start(transform.position, Hurt, true, _actor, true);
        }

        /// <summary>
        /// Generates a step alert.
        /// </summary>
        public void OnFootstep(Vector3 position)
        {
            _step.Start(transform.position, Step, false, _actor, true);
        }

        /// <summary>
        /// Generates a death alert.
        /// </summary>
        public void OnDead()
        {
            _death.Start(transform.position, Death, true, _actor, true);
        }

        /// <summary>
        /// Generates a jump alert.
        /// </summary>
        public void OnJump()
        {
            _jump.Start(transform.position, Jump, false, _actor, true);
        }
    }
}
