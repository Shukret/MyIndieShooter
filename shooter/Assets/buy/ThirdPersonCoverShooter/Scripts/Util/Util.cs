using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    public sealed class Util
    {
        // Cache
        private static RaycastHit[] _hits = new RaycastHit[64];
        private static Collider[] _colliders = new Collider[64];
        private static List<DarkZone> _dark = new List<DarkZone>();
        private static List<LightZone> _light = new List<LightZone>();
        private static List<GrassZone> _grass = new List<GrassZone>();

        public static float GetViewDistance(Vector3 position, float viewDistance, bool isAlerted)
        {
            var count = Physics.OverlapSphereNonAlloc(position, 1, _colliders, 0x1 << 8, QueryTriggerInteraction.Collide);

            _dark.Clear();
            _light.Clear();
            _grass.Clear();

            for (int i = 0; i < count; i++)
            {
                {
                    var zone = DarkZones.Get(_colliders[i].gameObject);
                    if (zone != null) _dark.Add(zone);
                }
                {
                    var zone = LightZones.Get(_colliders[i].gameObject);
                    if (zone != null) _light.Add(zone);
                }
                {
                    var zone = GrassZones.Get(_colliders[i].gameObject);
                    if (zone != null) _grass.Add(zone);
                }
            }

            return GetViewDistance(viewDistance, _dark, _light, _grass, isAlerted);
        }

        public static float GetViewDistance(float viewDistance, IEnumerable<DarkZone> dark, IEnumerable<LightZone> light, IEnumerable<GrassZone> grass, bool isAlerted)
        {
            var value = viewDistance;

            if (dark != null)
                foreach (var zone in dark)
                {
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = isAlerted ? zone.AlertValue : zone.DefaultValue; break;
                        case VisibilityType.multiplier: newValue = viewDistance * (isAlerted ? zone.AlertValue : zone.DefaultValue); break;
                    }

                    if (newValue < value)
                        value = newValue;
                }

            if (light != null)
                foreach (var zone in light)
                {
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = zone.Value; break;
                        case VisibilityType.multiplier: newValue = viewDistance * zone.Value; break;
                    }

                    if (newValue > value)
                        value = newValue;
                }

            if (grass != null)
                foreach (var zone in grass)
                {
                    var newValue = 0f;

                    switch (zone.Type)
                    {
                        case VisibilityType.constant: newValue = isAlerted ? zone.AlertValue : zone.DefaultValue; break;
                        case VisibilityType.multiplier: newValue = viewDistance * (isAlerted ? zone.AlertValue : zone.DefaultValue); break;
                    }

                    if (newValue < value)
                        value = newValue;
                }

            return value;
        }

        public static Vector3 GetClosestHit(Vector3 origin, Vector3 target, float minDistance, GameObject ignore)
        {
            var vector = (target - origin).normalized;
            var maxDistance = Vector3.Distance(origin, target);
            var closestHit = target;

            for (int i = 0; i < Physics.RaycastNonAlloc(origin, vector, _hits); i++)
            {
                var hit = _hits[i];

                if (hit.collider.gameObject != ignore && !hit.collider.isTrigger)
                    if (hit.distance > minDistance && hit.distance < maxDistance)
                    {
                        maxDistance = hit.distance;
                        closestHit = hit.point;
                    }
            }

            return closestHit;
        }

        public static void Lerp(ref float Value, float Target, float speed)
        {
            if (Target > Value)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        public static void LerpAngle(ref float Value, float Target, float speed)
        {
            var delta = Mathf.DeltaAngle(Value, Target);

            if (delta > 0)
            {
                if (Value + speed > Target)
                    Value = Target;
                else if (speed > 0)
                    Value += speed;
            }
            else
            {
                if (Value - speed < Target)
                    Value = Target;
                else if (speed > 0)
                    Value -= speed;
            }
        }

        /// <summary>
        /// Is the given target object is inside the parent hierarchy.
        /// </summary>
        public static bool InHiearchyOf(GameObject target, GameObject parent)
        {
            var obj = target;

            while (obj != null)
            {
                if (obj == parent)
                    return true;

                if (obj.transform.parent != null)
                    obj = obj.transform.parent.gameObject;
                else
                    obj = null;
            }

            return false;
        }

        /// <summary>
        /// Delta of a point on AB line closest to the given point.
        /// </summary>
        public static float FindDeltaPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return t;
        }

        /// <summary>
        /// Position of a point on AB line closest to the given .point.
        /// </summary>
        public static Vector3 FindClosestToPath(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float ab2 = ab.x * ab.x + +ab.z * ab.z;
            float ap_ab = ap.x * ab.x + ap.z * ab.z;
            float t = ap_ab / ab2;

            return a + ab * Mathf.Clamp01(t);
        }

        /// <summary>
        /// Calculates horizontal angle of the given vector.
        /// </summary>
        public static float AngleOfVector(Vector3 vector)
        {
            var v = new Vector2(vector.z, vector.x);

            if (v.sqrMagnitude > 0.01f)
                v.Normalize();

            var sign = (v.y < 0) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, v) * sign;
        }

        /// <summary>
        /// An utility function to calculate a distance between a point and a segment.
        /// </summary>
        public static float DistanceToSegment(Vector3 point, Vector3 p0, Vector3 p1)
        {
            var lengthSqr = (p1 - p0).sqrMagnitude;
            if (lengthSqr <= float.Epsilon) return Vector3.Distance(point, p0);

            var t = Mathf.Clamp01(((point.x - p0.x) * (p1.x - p0.x) +
                                   (point.y - p0.y) * (p1.y - p0.y) +
                                   (point.z - p0.z) * (p1.z - p0.z)) / lengthSqr);

            return Vector3.Distance(point, p0 + (p1 - p0) * t);
        }
    }
}