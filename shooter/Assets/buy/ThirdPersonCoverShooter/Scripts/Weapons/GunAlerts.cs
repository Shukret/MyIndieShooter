using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts on various gun events.
    /// </summary>
    [RequireComponent(typeof(Gun))]
    public class GunAlerts : MonoBehaviour
    {
        /// <summary>
        /// Distance at which fire can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which fire can be heard. Alert is not generated if value is zero or negative.")]
        public float Fire = 20;

        /// <summary>
        /// Distance at which reloads can be heard. Alert is not generated if value is zero or negative.
        /// </summary>
        [Tooltip("Distance at which reloads can be heard. Alert is not generated if value is zero or negative.")]
        public float Reload = 10;

        private UpdatedAlert _fire;
        private UpdatedAlert _reload;

        private Gun _gun;
        private Actor _actor;
        private CharacterMotor _cachedMotor;

        private void Awake()
        {
            _gun = GetComponent<Gun>();
        }

        private void LateUpdate()
        {
            _fire.Update();
            _reload.Update();
        }

        private void OnDisable()
        {
            _fire.Kill();
            _reload.Kill();
        }

        /// <summary>
        /// Generates a land alert.
        /// </summary>
        public void OnFire()
        {
            checkActor();
            _fire.Start(transform.position, Fire, true, _actor, true);
        }

        /// <summary>
        /// Generates a hurt alert.
        /// </summary>
        public void OnReloadStart()
        {
            checkActor();
            _reload.Start(transform.position, Reload, true, _actor, true);
        }

        private void checkActor()
        {
            if (_gun.Character != _cachedMotor)
            {
                _cachedMotor = _gun.Character;
                _actor = _cachedMotor.GetComponent<Actor>();
            }
        }
    }
}
