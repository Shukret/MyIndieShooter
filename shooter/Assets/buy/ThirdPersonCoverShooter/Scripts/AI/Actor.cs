using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Collider))]
    public class Actor : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Is the object alive.
        /// </summary>
        public bool IsAlive
        {
            get { return _isAlive; }
        }

        /// <summary>
        /// Does the character have a weapon in their hands.
        /// </summary>
        public bool IsArmed
        {
            get { return _motor != null && _motor.CurrentWeapon > 0 && _motor.Weapons[_motor.CurrentWeapon - 1].Type != WeaponType.Tool; }
        }

        /// <summary>
        /// Cover the threat is hiding behind. Null if there isn't any.
        /// </summary>
        public Cover Cover
        {
            get { return _cover; }
        }

        /// <summary>
        /// Top position when the actor is standing.
        /// </summary>
        public Vector3 RelativeStandingTopPosition
        {
            get
            {
                if (_hasStandingHeight)
                    return Vector3.up * _standingHeight;
                else
                    return Vector3.up * _height;
            }
        }

        /// <summary>
        /// Current top position.
        /// </summary>
        public Vector3 RelativeTopPosition
        {
            get { return Vector3.up * _height; }
        }

        /// <summary>
        /// Top position when the actor is standing.
        /// </summary>
        public Vector3 StandingTopPosition
        {
            get
            {
                if (_hasStandingHeight)
                    return transform.position + Vector3.up * _standingHeight;
                else
                    return transform.position + Vector3.up * _height;
            }
        }

        /// <summary>
        /// Current top position.
        /// </summary>
        public Vector3 TopPosition
        {
            get { return transform.position + Vector3.up * _height; }
        }

        /// <summary>
        /// Collider attached to the object.
        /// </summary>
        public Collider Collider
        {
            get { return _collider; }
        }

        /// <summary>
        /// AI component attached to the object. Can be null.
        /// </summary>
        public AIController AI
        {
            get { return _ai; }
        }

        /// <summary>
        /// Current look direction of the actor's head.
        /// </summary>
        public Vector3 HeadDirection
        {
            get
            {
                if (_motor == null)
                    return transform.forward;
                else
                    return _motor.HeadForward;
            }
        }

        /// <summary>
        /// Is the AI attached to the actor alerted.
        /// </summary>
        public bool IsAlerted
        {
            get { return _isAlerted; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Team number used by the AI.
        /// </summary>
        [Tooltip("Team number used by the AI.")]
        public int Side = 0;

        /// <summary>
        /// Is the actor aggresive. Value used by the AI. Owning AI usually overwrites the value if present.
        /// </summary>
        [Tooltip("Is the actor aggresive. Value used by the AI. Owning AI usually overwrites the value if present.")]
        public bool IsAggressive = true;

        #endregion

        #region Private fields

        private bool _isAlive = true;
        private Cover _cover;
        private bool _hasStandingHeight;
        private float _standingHeight;
        private float _height;
        private Collider _collider;
        private AIController _ai;
        private CharacterMotor _motor;

        private HashSet<DarkZone> _darkZones = new HashSet<DarkZone>();
        private HashSet<LightZone> _lightZones = new HashSet<LightZone>();
        private HashSet<GrassZone> _grassZones = new HashSet<GrassZone>();

        private bool _isAlerted;

        #endregion

        #region Events

        /// <summary>
        /// The actor enters a flashlight or any similar object.
        /// </summary>
        public void OnEnterGrass(GrassZone zone)
        {
            if (!_grassZones.Contains(zone))
                _grassZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a lighted area.
        /// </summary>
        public void OnLeaveGrass(GrassZone zone)
        {
            if (_grassZones.Contains(zone))
                _grassZones.Remove(zone);
        }

        /// <summary>
        /// The actor enters a flashlight or any similar object.
        /// </summary>
        public void OnEnterLight(LightZone zone)
        {
            if (!_lightZones.Contains(zone))
                _lightZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a lighted area.
        /// </summary>
        public void OnLeaveLight(LightZone zone)
        {
            if (_lightZones.Contains(zone))
                _lightZones.Remove(zone);
        }

        /// <summary>
        /// The actor enters a dark area.
        /// </summary>
        public void OnEnterDarkness(DarkZone zone)
        {
            if (!_darkZones.Contains(zone))
                _darkZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a dark area.
        /// </summary>
        public void OnLeaveDarkness(DarkZone zone)
        {
            if (_darkZones.Contains(zone))
                _darkZones.Remove(zone);
        }

        /// <summary>
        /// Notify the component of the standing height (used when in cover).
        /// </summary>
        public void OnStandingHeight(float value)
        {
            _hasStandingHeight = true;
            _standingHeight = value;
        }

        /// <summary>
        /// Notified by components that the actor is no longer alive.
        /// </summary>
        public void OnDead()
        {
            _isAlive = false;
            Actors.Unregister(this);
        }

        /// <summary>
        /// Tell the threat to mark itself as being behind the given cover.
        /// </summary>
        public void OnEnterCover(Cover cover)
        {
            _cover = cover;
        }

        /// <summary>
        /// Tell the threat to mark itself as out of cover.
        /// </summary>
        public void OnLeaveCover()
        {
            _cover = null;
        }

        /// <summary>
        /// Notified by an AI that the actor is alerted.
        /// </summary>
        public void OnAlerted()
        {
            _isAlerted = true;
        }

        #endregion

        #region Behaviour

        private void Update()
        {
            _height = _collider.bounds.extents.y * 2;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _ai = GetComponent<AIController>();
            _motor = GetComponent<CharacterMotor>();

            _height = _collider.bounds.extents.y * 2;
        }

        private void OnEnable()
        {
            Actors.Register(this);
        }

        private void OnDisable()
        {
            Actors.Unregister(this);
        }

        private void OnDestroy()
        {
            Actors.Unregister(this);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Calculates the view distance when looking at this actor.
        /// </summary>
        public float GetViewDistance(float viewDistance, bool isAlerted)
        {
            return Util.GetViewDistance(viewDistance, _darkZones, _lightZones, _motor.IsCrouching ? _grassZones : null, isAlerted);
        }

        #endregion
    }

    public static class Actors
    {
        public static IEnumerable<Actor> All
        {
            get { return _list; }
        }

        private static List<Actor> _list = new List<Actor>();
        private static Dictionary<GameObject, Actor> _map = new Dictionary<GameObject, Actor>();

        public static Actor Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static void Register(Actor actor)
        {
            if (!_list.Contains(actor))
                _list.Add(actor);

            _map[actor.gameObject] = actor;
        }

        public static void Unregister(Actor actor)
        {
            if (_list.Contains(actor))
                _list.Remove(actor);

            if (_map.ContainsKey(actor.gameObject))
                _map.Remove(actor.gameObject);
        }
    }
}
