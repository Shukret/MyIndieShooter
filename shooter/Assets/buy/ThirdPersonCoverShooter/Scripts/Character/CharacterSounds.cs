using System.Collections;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Plays various sounds on character events.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterSounds : MonoBehaviour
    {
        /// <summary>
        /// Possible sounds to play on each footstep.
        /// </summary>
        [Tooltip("Possible sounds to play on each footstep.")]
        public AudioClip[] Footstep;

        /// <summary>
        /// Possible sounds to play when the character is hit.
        /// </summary>
        [Tooltip("Possible sounds to play when the character is hit.")]
        public AudioClip[] Hurt;

        /// <summary>
        /// Possible sounds to play when the character dies.
        /// </summary>
        [Tooltip("Possible sounds to play when the character dies.")]
        public AudioClip[] Death;

        /// <summary>
        /// Possible sounds to play at the beginning of a jump.
        /// </summary>
        [Tooltip("Possible sounds to play at the beginning of a jump.")]
        public AudioClip[] Jump;

        /// <summary>
        /// Possible sounds to play when the character lands.
        /// </summary>
        [Tooltip("Possible sounds to play when the character lands.")]
        public AudioClip[] Land;

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

        public void OnLand()
        {
            if (_fallSoundTimer <= 0 && _motor.IsAlive)
            {
                _fallSoundTimer = 0.4f;
                playSound(Land);
            }
        }

        public void OnFootstep()
        {
            if (_motor.IsAlive)
                playSound(Footstep);
        }

        public void OnDead()
        {
            playSound(Death, 0.3f);
        }

        public void OnJump()
        {
            if (_motor.IsAlive)
                playSound(Jump);
        }

        public void OnHit()
        {
            if (_hurtSoundTimer < float.Epsilon && _motor.IsAlive)
            {
                _hurtSoundTimer = 0.5f;
                playSound(Hurt, 0.1f);
            }
        }

        private void playSound(AudioClip[] clips, float delay = 0f)
        {
            if (clips.Length == 0)
                return;

            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (delay < float.Epsilon)
                AudioSource.PlayClipAtPoint(clip, transform.position);
            else
                StartCoroutine(delayedClip(clip, delay));
        }

        private IEnumerator delayedClip(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}