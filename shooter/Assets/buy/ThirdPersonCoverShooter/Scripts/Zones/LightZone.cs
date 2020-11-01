using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Collider))]
    public class LightZone : MonoBehaviour
    {
        /// <summary>
        /// Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Type of visibility modification. Choices are between a constant distance or a multiplier for the AI view distance.")]
        public VisibilityType Type = VisibilityType.multiplier;

        /// <summary>
        /// Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.
        /// </summary>
        [Tooltip("Value that's used depending on the visibility type. Can be either a distance or a multiplier for the AI view distance.")]
        public float Value = 1;

        private void OnTriggerEnter(Collider other)
        {
            other.SendMessage("OnEnterLight", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerExit(Collider other)
        {
            other.SendMessage("OnLeaveLight", this, SendMessageOptions.DontRequireReceiver);
        }

        private void OnEnable()
        {
            LightZones.Register(this);
        }

        private void OnDisable()
        {
            LightZones.Unregister(this);
        }

        private void OnDestroy()
        {
            LightZones.Unregister(this);
        }
    }

    public static class LightZones
    {
        public static IEnumerable<LightZone> All
        {
            get { return _list; }
        }

        private static List<LightZone> _list = new List<LightZone>();
        private static Dictionary<GameObject, LightZone> _map = new Dictionary<GameObject, LightZone>();

        public static LightZone Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static void Register(LightZone zone)
        {
            if (!_list.Contains(zone))
                _list.Add(zone);

            _map[zone.gameObject] = zone;
        }

        public static void Unregister(LightZone zone)
        {
            if (_list.Contains(zone))
                _list.Remove(zone);

            if (_map.ContainsKey(zone.gameObject))
                _map.Remove(zone.gameObject);
        }
    }
}
