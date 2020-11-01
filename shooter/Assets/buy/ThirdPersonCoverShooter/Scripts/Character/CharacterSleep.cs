using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterSleep : MonoBehaviour
    {
        /// <summary>
        /// Distance to the player when to activate the character.
        /// </summary>
        [Tooltip("Distance to the player when to activate the character.")]
        public float On = 20;

        /// <summary>
        /// Distance to the player when to deactivate the character.
        /// </summary>
        [Tooltip("Distance to the player when to deactivate the character.")]
        public float Off = 60;

        private CharacterMotor _motor;
        private Rigidbody _body;
        private Animator _animator;
        private CapsuleCollider _capsule;

        private AIController _ai;
        private AIBase[] _ais;

        private bool _isFirst;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _body = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _capsule = GetComponent<CapsuleCollider>();

            _ai = GetComponent<AIController>();
            _ais = GetComponents<AIBase>();

            _isFirst = true;
        }

        private void on()
        {
            _motor.enabled = true;
            _animator.enabled = true;
            _capsule.enabled = true;
            _body.isKinematic = false;
            _body.detectCollisions = true;

            if (_ai != null)
                _ai.enabled = true;

            if (_ais != null)
                foreach (var ai in _ais)
                    ai.enabled = true;
        }

        private void off()
        {
            _motor.enabled = false;
            _animator.enabled = false;
            _capsule.enabled = false;
            _body.isKinematic = true;
            _body.detectCollisions = false;

            if (_ai != null)
                _ai.enabled = false;

            if (_ais != null)
                foreach (var ai in _ais)
                    ai.enabled = false;
        }

        private void Update()
        {
            if (_isFirst)
            {
                _isFirst = false;

                if (Characters.MainPlayer.Object != null &&
                    Vector3.Distance(transform.position, Characters.MainPlayer.Object.transform.position) > On)
                    off();
            }
            else
            {
                var isOn = _motor.isActiveAndEnabled;

                if (isOn)
                {
                    var turnOff = false;

                    if (Characters.MainPlayer.Object == null)
                        turnOff = true;
                    else if (Vector3.Distance(transform.position, Characters.MainPlayer.Object.transform.position) > Off)
                        turnOff = true;

                    if (turnOff)
                        off();
                }
                else
                {
                    if (Characters.MainPlayer.Object != null &&
                        Vector3.Distance(transform.position, Characters.MainPlayer.Object.transform.position) < On)
                        on();
                }
            }
        }
    }
}
