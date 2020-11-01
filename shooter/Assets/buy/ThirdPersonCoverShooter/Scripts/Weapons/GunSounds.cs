using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns sounds on gun events.
    /// </summary>
    public class GunSounds : MonoBehaviour
    {
        /// <summary>
        /// Sound to play when ejecting a clip.
        /// </summary>
        [Tooltip("Sound to play when ejecting a clip.")]
        public AudioClip[] Eject;

        /// <summary>
        /// Sound to play when a clip is put inside the gun.
        /// </summary>
        [Tooltip("Sound to play when a clip is put inside the gun.")]
        public AudioClip[] Rechamber;

        /// <summary>
        /// Possible sounds to play on each bullet fire.
        /// </summary>
        [Tooltip("Possible sounds to play on each bullet fire.")]
        public AudioClip[] Fire;

        /// <summary>
        /// Play a sequence of reload sounds.
        /// </summary>
        public void OnReloadStart()
        {
            StartCoroutine(reloadSequence());
        }

        /// <summary>
        /// Play fire effects delayed by the given amount of time in seconds.
        /// </summary>
        /// <param name="delay">Time to delay the creation of effects.</param>
        public void OnFire(float delay)
        {
            StartCoroutine(play(delay, Fire));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator reloadSequence()
        {
            yield return play(0.1f, Eject);
            yield return play(0.6f, Rechamber);
        }

        private void play(AudioClip clip)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        private void play(AudioClip[] clips)
        {
            if (clips.Length > 0)
                play(clips[UnityEngine.Random.Range(0, clips.Length)]);
        }

        private IEnumerator play(float delay, AudioClip[] clips)
        {
            yield return new WaitForSeconds(delay);
            play(clips);
        }
    }
}