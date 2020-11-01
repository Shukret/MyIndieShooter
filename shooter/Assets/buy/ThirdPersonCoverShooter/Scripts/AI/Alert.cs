using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts.
    /// </summary>
    public class Alert : MonoBehaviour
    {
        /// <summary>
        /// Range of the alert.
        /// </summary>
        [Tooltip("Range of the alert.")]
        public float Range = 20;

        /// <summary>
        /// Should the alert be activate when enabling the object.
        /// </summary>
        [Tooltip("Should the alert be activate when enabling the object.")]
        public bool AutoActivate = true;

        /// <summary>
        /// Is threat regarded as hostile by civilians.
        /// </summary>
        [Tooltip("Is threat regarded as hostile by civilians.")]
        public bool IsHostile;

        [HideInInspector]
        public Actor Generator;

        private UpdatedAlert _alert;
        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        /// <summary>
        /// Activates the alert and resets the timer.
        /// </summary>
        public void Activate()
        {
            _alert.Start(transform.position, Range, IsHostile, _actor == null ? Generator : _actor, _actor != null);
        }

        /// <summary>
        /// Deactivates the alert.
        /// </summary>
        public void Deactivate()
        {
            _alert.Kill();
        }

        private void Update()
        {
            _alert.Update();
        }

        private void OnEnable()
        {
            if (AutoActivate)
                Activate();
        }

        private void OnDisable()
        {
            Deactivate();
        }
    }

    /// <summary>
    /// Used for inside objects.
    /// </summary>
    public struct UpdatedAlert
    {
        public bool IsGenerated;
        public int Frames;
        public int ID;

        /// <summary>
        /// Registers the alert and starts counting frames.
        /// </summary>
        public void Start(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            if (!IsGenerated)
            {
                IsGenerated = true;
                ID = Alerts.Register(position, range, isHostile, actor, isDirect);
            }

            Frames = 2;
        }

        /// <summary>
        /// Unregisters the alert.
        /// </summary>
        public void Kill()
        {
            if (IsGenerated)
            {
                Alerts.Unregister(ID);
                IsGenerated = false;
            }
        }

        /// <summary>
        /// Updates the alert. Kills few frames after starting.
        /// </summary>
        public void Update()
        {
            if (IsGenerated)
            {
                Frames--;

                if (Frames <= 0)
                    Kill();
            }
        }
    }

    public struct GeneratedAlert
    {
        /// <summary>
        /// Position of the alert.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Range of the alert.
        /// </summary>
        public float Range;

        /// <summary>
        /// Is threat regarded as hostile by civilians.
        /// </summary>
        public bool IsHostile;

        /// <summary>
        /// Object that generated the alert.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// Is the actor at the position of the alert.
        /// </summary>
        public bool IsDirect;

        public GeneratedAlert(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            Position = position;
            Range = range;
            IsHostile = isHostile;
            Actor = actor;
            IsDirect = isDirect;
        }
    }

    public static class Alerts
    {
        public static IEnumerable<GeneratedAlert> All
        {
            get { return _alerts.Values; }
        }

        private static Dictionary<int, GeneratedAlert> _alerts = new Dictionary<int, GeneratedAlert>();
        private static Collider[] _colliders = new Collider[1024];
        private static int _latest;

        /// <summary>
        /// Registers a new alert and returns its ID.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            var alert = new GeneratedAlert(position, range, isHostile, actor, isDirect);
            var count = Physics.OverlapSphereNonAlloc(position, range, _colliders, 0x1, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var listener = AIListeners.Get(_colliders[i].gameObject);

                if (listener != null && listener.isActiveAndEnabled && Vector3.Distance(listener.transform.position, position) < range * listener.Hearing)
                    listener.SendMessage("OnAlert", alert, SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Registers a new alert and returns its ID.
        /// </summary>
        public static int Register(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            var alert = new GeneratedAlert(position, range, isHostile, actor, isDirect);

            var count = Physics.OverlapSphereNonAlloc(position, range, _colliders, 0x1, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var listener = AIListeners.Get(_colliders[i].gameObject);

                if (listener != null && listener.isActiveAndEnabled && Vector3.Distance(listener.transform.position, position) < range * listener.Hearing)
                    listener.SendMessage("OnAlert", alert, SendMessageOptions.DontRequireReceiver);
            }

            _alerts[++_latest] = alert;
            return _latest;
        }

        /// <summary>
        /// Removes an alert with the given ID.
        /// </summary>
        public static void Unregister(int id)
        {
            if (_alerts.ContainsKey(id))
                _alerts.Remove(id);
        }
    }
}
