using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns effects and sounds on gun events.
    /// </summary>
    [RequireComponent(typeof(Gun))]
    public class GunEffects : MonoBehaviour
    {
        /// <summary>
        /// Object to instantiate when ejecting a clip.
        /// </summary>
        [Tooltip("Object to instantiate when ejecting a clip.")]
        public GameObject Eject;

        /// <summary>
        /// Object to instantiate when a clip is put inside the gun.
        /// </summary>
        [Tooltip("Object to instantiate when a clip is put inside the gun.")]
        public GameObject Rechamber;

        /// <summary>
        /// Object to instantiate on each bullet fire.
        /// </summary>
        [Tooltip("Object to instantiate on each bullet fire.")]
        public GameObject Fire;

        /// <summary>
        /// Object to instantiate to simulate shell ejection.
        /// </summary>
        [Tooltip("Object to instantiate to simulate shell ejection.")]
        public GameObject Shell;

        private List<GameObject> _particles = new List<GameObject>();
        private Gun _gun;

        private void Awake()
        {
            _gun = GetComponent<Gun>();
        }

        /// <summary>
        /// Play a sequence of relaod effects.
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
            if (_gun != null && _gun.Aim != null)
                StartCoroutine(play(delay, Fire, _gun.Aim.transform, Vector3.zero, Quaternion.identity));

            StartCoroutine(play(delay, Shell, null, transform.position, Quaternion.identity));
        }

        private void LateUpdate()
        {
            int i = 0;
            while (i < _particles.Count)
            {
                if (_particles[i] == null)
                    _particles.RemoveAt(i);
                else
                    i++;
            }

            foreach (var particle in _particles)
                if (particle == null)
                    _particles.Remove(particle);
        }

        private void OnDisable()
        {
            foreach (var particle in _particles)
                if (particle != null)
                    GameObject.Destroy(particle);

            _particles.Clear();

            StopAllCoroutines();
        }

        private IEnumerator reloadSequence()
        {
            yield return play(0.1f, Eject, null, transform.position, Quaternion.identity);
            yield return play(0.6f, Rechamber, null, transform.position, Quaternion.identity);
        }

        private IEnumerator play(float delay, GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, float destroyAfter = 4f)
        {
            if (prefab == null)
                yield break;

            yield return new WaitForSeconds(delay);

            var particle = GameObject.Instantiate(prefab);
            particle.transform.parent = parent;
            particle.transform.localPosition = position;
            particle.transform.localRotation = rotation;
            particle.SetActive(true);
            _particles.Add(particle);
            GameObject.Destroy(particle, destroyAfter);
        }
    }
}