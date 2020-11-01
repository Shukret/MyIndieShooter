using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Listens for events comign from the CharacterMotor and instantiates effects.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterEffects : MonoBehaviour
    {
        /// <summary>
        /// Effect prefab to instantiate on each character step.
        /// </summary>
        [Tooltip("Effect prefab to instantiate on each character step.")]
        public GameObject Step;

        /// <summary>
        /// Effect prefab to instantiate when the character is hit.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character is hit.")]
        public GameObject Hurt;

        /// <summary>
        /// Effect prefab to instantiate when the character dies.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character dies.")]
        public GameObject Death;

        /// <summary>
        /// Effect prefab to instantiate at the beginning of a jump.
        /// </summary>
        [Tooltip("Effect prefab to instantiate at the beginning of a jump.")]
        public GameObject Jump;

        /// <summary>
        /// Effect prefab to instantiate when the character lands on ground.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character lands on ground.")]
        public GameObject Land;

        private CharacterMotor _motor;
        private float _hurtSoundTimer;
        private float _fallSoundTimer;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
        }

        private void LateUpdate()
        {
            if (_hurtSoundTimer > -float.Epsilon)
                _hurtSoundTimer -= Time.deltaTime;

            if (_fallSoundTimer > -float.Epsilon)
                _fallSoundTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Instantiates the landing effect when the character lands.
        /// </summary>
        public void OnLand()
        {
            if (_fallSoundTimer <= 0 && _motor.IsAlive)
            {
                _fallSoundTimer = 0.4f;
                instantiate(Land, transform.position);
            }
        }

        /// <summary>
        /// Instantiates the hit effect when the character is hit.
        /// </summary>
        public void OnHit(Hit hit)
        {
            if (_hurtSoundTimer < float.Epsilon && _motor.IsAlive)
            {
                _hurtSoundTimer = 0.5f;
                instantiate(Hurt, hit.Position);
            }
        }

        /// <summary>
        /// Instantiates the step effect.
        /// </summary>
        public void OnFootstep(Vector3 position)
        {
            if (_motor.IsAlive)
                instantiate(Step, position);
        }

        /// <summary>
        /// Instantiates the death effect.
        /// </summary>
        public void OnDead()
        {
            instantiate(Death, transform.position);
        }

        /// <summary>
        /// Instantiates the jump effect.
        /// </summary>
        public void OnJump()
        {
            if (_motor.IsAlive)
                instantiate(Jump, transform.position);
        }

        /// <summary>
        /// Helper function to instantiate effect prefabs.
        /// </summary>
        private void instantiate(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return;

            var obj = GameObject.Instantiate(prefab);
            obj.transform.parent = null;
            obj.transform.position = position;
            obj.SetActive(true);

            GameObject.Destroy(obj, 3);
        }
    }
}
