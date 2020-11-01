using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets the target of the HealthBar to the current target of a CharacterMotor.
    /// </summary>
    [RequireComponent(typeof(HealthBar))]
    public class EnemyHealth : MonoBehaviour
    {
        /// <summary>
        /// Character whose aim is used to determine the enemy.
        /// </summary>
        [Tooltip("Character whose aim is used to determine the enemy.")]
        public CharacterMotor Motor;

        /// <summary>
        /// Delay after which the health bar is hidden when there is no target.
        /// </summary>
        [Range(0, 20)]
        [Tooltip("Delay after which the health bar is hidden when there is no target.")]
        public float HideDelay = 4.0f;

        private float _timer = 10000;
        private HealthBar _healthBar;

        private void Awake()
        {
            _healthBar = GetComponent<HealthBar>();
        }

        private void LateUpdate()
        {
            if (Motor.Target == null || Motor.Target == Motor.gameObject)
            {
                _timer += Time.deltaTime;

                if (_timer >= HideDelay)
                    _healthBar.Target = null;
            }
            else
            {
                _timer = 0;
                _healthBar.Target = Motor.Target;
            }
        }
    }
}