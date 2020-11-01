using UnityEngine;

namespace CoverShooter
{
    public class Grenade : MonoBehaviour
    {
        /// <summary>
        /// Object that activated the grenade.
        /// </summary>
        public Actor Attacker
        {
            get { return _attacker; }
        }

        /// <summary>
        /// Explosion prefab to instantiate on detonation.
        /// </summary>
        [Tooltip("Explosion prefab to instantiate on detonation.")]
        public GameObject Explosion;

        /// <summary>
        /// Distance from the explosion center where damage reaches 0.
        /// </summary>
        [Tooltip("Distance from the explosion center where damage reaches 0.")]
        public float ExplosionRadius = 4.5f;

        /// <summary>
        /// Damage done at the center of explosion.
        /// </summary>
        [Tooltip("Damage done at the center of explosion.")]
        public float CenterDamage = 150;

        /// <summary>
        /// Fraction of velocity retained when bouncing from a surface.
        /// </summary>
        [Tooltip("Fraction of velocity retained when bouncing from a surface.")]
        public float Bounciness = 0.3f;

        /// <summary>
        /// Explosion timer in seconds counted from activation.
        /// </summary>
        [Tooltip("Explosion timer in seconds counted from activation.")]
        public float Timer = 3;

        /// <summary>
        /// Prefab to instantiate to display grenade explosion preview.
        /// </summary>
        [Tooltip("Prefab to instantiate to display grenade explosion preview.")]
        public GameObject ExplosionPreview;

        /// <summary>
        /// Time in seconds since the activation to display the explosion preview.
        /// </summary>
        [Tooltip("Time in seconds since the activation to display the explosion preview.")]
        public float PreviewTime;

        private bool _hasExploded = false;

        private bool _isActivated = false;
        private bool _isActivating = false;
        private float _activateTimer = 0;
        private Actor _attacker;

        private float _timer = 0;

        private bool _isFlying;
        private Vector3 _velocity;
        private Vector3 _offset;
        private float _gravity;

        private GameObject _preview;

        private void OnEnable()
        {
            if (_isActivated)
                GrenadeList.Register(this);
        }

        private void OnDisable()
        {
            if (_preview != null)
            {
                GameObject.Destroy(_preview);
                _preview = null;
            }

            GrenadeList.Unregister(this);
        }

        /// <summary>
        /// Launched the grenade on flight with giver parameters.
        /// </summary>
        public void Fly(Vector3 origin, Vector3 velocity, float gravity)
        {
            _isFlying = true;

            _offset = transform.position - origin;

            if (_offset.magnitude > 0.5f)
                _offset = _offset.normalized * 0.5f;

            _velocity = velocity;
            _gravity = gravity;
        }

        /// <summary>
        /// Sets the grenade to explode on collision.
        /// </summary>
        public void Activate(Actor attacker, float delay = 0)
        {
            if (delay <= float.Epsilon)
            {
                _isActivated = true;
                _isActivating = false;
                _timer = Timer;
                GrenadeList.Register(this);
            }
            else
            {
                _isActivating = true;
                _activateTimer = delay;
            }

            _attacker = attacker;
        }

        private void Update()
        {
            if (_isActivating)
            {
                _activateTimer -= Time.deltaTime;

                if (_activateTimer < float.Epsilon)
                {
                    _isActivating = false;
                    _isActivated = true;
                    GrenadeList.Register(this);
                    _timer = Timer;
                }
            }

            if (_isActivated)
            {
                if (_timer < float.Epsilon)
                    explode();
                else
                {
                    _timer -= Time.deltaTime;

                    if (_timer < Timer - PreviewTime)
                    {
                        if (_preview == null)
                        {
                            _preview = GameObject.Instantiate(ExplosionPreview);
                            _preview.transform.parent = null;
                            _preview.SetActive(true);
                        }

                        if (_preview != null)
                        {
                            _preview.transform.localScale = Vector3.one * ExplosionRadius * 2;
                            _preview.transform.position = transform.position;
                        }
                    }
                    else if (_preview != null)
                    {
                        GameObject.Destroy(_preview);
                        _preview = null;
                    }
                }
            }

            if (_isFlying)
            {
                var position = transform.position;

                var previousOffset = _offset;
                _offset = Vector3.Lerp(_offset, Vector3.zero, Time.deltaTime * 2);
                position += _offset - previousOffset;

                GrenadePath.Step(ref position, ref _velocity, Time.deltaTime, _gravity, Bounciness);

                transform.position = position;
            }
        }

        private void explode()
        {
            if (_hasExploded || !_isActivated)
                return;

            _hasExploded = true;

            if (Explosion != null)
            {
                var alert = Explosion.GetComponent<Alert>();

                if (alert != null)
                    alert.Generator = _attacker;

                var particle = GameObject.Instantiate(Explosion);
                particle.transform.parent = null;
                particle.transform.position = transform.position;
                particle.SetActive(true);
            }

            var colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);

            foreach (var collider in colliders)
                if (!collider.isTrigger)
                {
                    var part = collider.GetComponent<BodyPartHealth>();

                    if (part == null)
                    {
                        var closest = collider.transform.position;

                        if (collider.GetType() == typeof(MeshCollider))
                            if (((MeshCollider)collider).convex)
                                closest = collider.ClosestPoint(transform.position);

                        var vector = transform.position - closest;
                        var distance = vector.magnitude;

                        if (distance < ExplosionRadius)
                        {
                            Vector3 normal;

                            if (distance > float.Epsilon)
                                normal = vector / distance;
                            else
                                normal = (closest - collider.transform.position).normalized;

                            var hit = new Hit(closest, normal, CenterDamage * (1 - distance / ExplosionRadius), null, collider.gameObject);
                            collider.gameObject.SendMessage("OnHit", hit, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }

            GameObject.Destroy(gameObject);
        }
    }
}
