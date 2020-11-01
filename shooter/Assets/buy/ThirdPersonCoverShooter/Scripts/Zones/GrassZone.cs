using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Collider))]
    public class GrassZone : MonoBehaviour
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
        public float DefaultValue = 1;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used when the AI knows about the threat. Can be either a distance or a multiplier for the AI view distance depending on the Type.")]
        public float AlertValue = 6;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterGrass", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveGrass", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnEnable()
        {
            GrassZones.Register(this);
        }

        private void OnDisable()
        {
            GrassZones.Unregister(this);
        }

        private void OnDestroy()
        {
            GrassZones.Unregister(this);
        }
    }

    public static class GrassZones
    {
        public static IEnumerable<GrassZone> All
        {
            get { return _list; }
        }

        private static List<GrassZone> _list = new List<GrassZone>();
        private static Dictionary<GameObject, GrassZone> _map = new Dictionary<GameObject, GrassZone>();

        public static GrassZone Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static void Register(GrassZone zone)
        {
            if (!_list.Contains(zone))
                _list.Add(zone);

            _map[zone.gameObject] = zone;
        }

        public static void Unregister(GrassZone zone)
        {
            if (_list.Contains(zone))
                _list.Remove(zone);

            if (_map.ContainsKey(zone.gameObject))
                _map.Remove(zone.gameObject);
        }
    }
}
