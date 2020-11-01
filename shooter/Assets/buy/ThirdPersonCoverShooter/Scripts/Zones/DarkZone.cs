using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Collider))]
    public class DarkZone : MonoBehaviour
    {
        /// <summary>
        /// Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.")]
        public VisibilityType Type = VisibilityType.constant;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI is not alerted. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float DefaultValue = 4;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI knows about the threat. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float AlertValue = 10;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterDarkness", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveDarkness", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnEnable()
        {
            DarkZones.Register(this);
        }

        private void OnDisable()
        {
            DarkZones.Unregister(this);
        }

        private void OnDestroy()
        {
            DarkZones.Unregister(this);
        }
    }

    public static class DarkZones
    {
        public static IEnumerable<DarkZone> All
        {
            get { return _list; }
        }

        private static List<DarkZone> _list = new List<DarkZone>();
        private static Dictionary<GameObject, DarkZone> _map = new Dictionary<GameObject, DarkZone>();

        public static DarkZone Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static void Register(DarkZone zone)
        {
            if (!_list.Contains(zone))
                _list.Add(zone);

            _map[zone.gameObject] = zone;
        }

        public static void Unregister(DarkZone zone)
        {
            if (_list.Contains(zone))
                _list.Remove(zone);

            if (_map.ContainsKey(zone.gameObject))
                _map.Remove(zone.gameObject);
        }
    }
}
