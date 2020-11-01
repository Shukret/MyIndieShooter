using UnityEngine;
using UnityEngine.Profiling;

namespace CoverShooter
{
    public struct GrenadeDescription
    {
        public float Gravity;
        public float Duration;
        public float Bounciness;
    }

    public static class GrenadePath
    {
        public static Vector3 Origin(CharacterMotor motor, float lookAngle)
        {
            var origin = Vector3.zero;

            if (motor.IsInLowCover)
                origin = motor.Grenade.CrouchOrigin;
            else
                origin = motor.Grenade.StandingOrigin;

            if (motor.IsThrowingLeft)
                origin.x *= -1;

            float angle;

            if (motor.IsLookingBackInTallCover)
                angle = motor.Cover.Angle - 180;
            else
                angle = lookAngle;

            origin = Quaternion.AngleAxis(angle, Vector3.up) * origin;

            if (motor.IsInTallCover)
            {
                if (motor.CanPeekLeftCorner)
                    origin += Quaternion.AngleAxis(motor.Cover.Angle, Vector3.up) * (-motor.CoverSettings.CornerOffset);
                else if (motor.CanPeekRightCorner)
                    origin += Quaternion.AngleAxis(motor.Cover.Angle, Vector3.up) * motor.CoverSettings.CornerOffset;
            }

            return origin + motor.transform.position;
        }

        public static void Step(ref Vector3 position, ref Vector3 velocity, float step, float gravity, float bounciness)
        {
            var v = velocity.magnitude;
            var vector = velocity / v;

            RaycastHit hit;
            if (Physics.Raycast(position - vector * 0.1f, vector, out hit, v * step + 0.1f, 1, QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
                velocity = Vector3.Reflect(velocity, hit.normal) * bounciness;
            }
            else
                position += velocity * step;

            velocity -= Vector3.up * gravity * step;
        }

        public static int Calculate(Vector3 start, float horizontalAngle, float angleInDegrees, float velocity, GrenadeDescription desc, Vector3[] buffer, float step = 0.05f)
        {
            if (buffer.Length == 0)
                return 0;

            buffer[0] = start;
            var count = 1;

            var horizontal = Quaternion.AngleAxis(horizontalAngle, Vector3.up) * Vector3.forward;
            var distance = horizontal.magnitude;
            var angle = angleInDegrees * Mathf.Deg2Rad;

            var currentVelocity = (horizontal.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * velocity;
            var position = start;
            var time = 0f;

            Profiler.BeginSample("Grenade path");

            while (count < buffer.Length && time < desc.Duration && currentVelocity.magnitude > 0.1f)
            {
                Step(ref position, ref currentVelocity, step, desc.Gravity, desc.Bounciness);

                time += step;
                buffer[count++] = position;
            }

            Profiler.EndSample();

            return count;
        }

        public static Vector3 GetVelocity(Vector3 start, Vector3 target, float maxVelocity, GrenadeDescription desc)
        {
            var horizontal = new Vector3(target.x, 0, target.z) - new Vector3(start.x, 0, start.z);
            var height = target.y - start.y;
            var distance = horizontal.magnitude;

            float angle = Mathf.Deg2Rad * 45;
            float velocity = maxVelocity;

            if (getAngle(height, distance, velocity, -desc.Gravity, ref angle))
                if (angle > Mathf.Deg2Rad * 45)
                {
                    angle = Mathf.Deg2Rad * 45;
                    velocity = getVelocity(height, distance, -desc.Gravity, angle);
                }

            return (horizontal.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * velocity;
        }

        public static int Calculate(Vector3 start, Vector3 target, float maxVelocity, GrenadeDescription desc, Vector3[] buffer, float step = 0.05f)
        {
            if (buffer.Length == 0)
                return 0;

            buffer[0] = start;
            var count = 1;

            var currentVelocity = GetVelocity(start, target, maxVelocity, desc);
            var position = start;
            var time = 0f;

            Profiler.BeginSample("Grenade path");

            while (count < buffer.Length && time < desc.Duration && currentVelocity.magnitude > 0.1f)
            {
                Step(ref position, ref currentVelocity, step, desc.Gravity, desc.Bounciness);

                time += step;
                buffer[count++] = position;
            }

            Profiler.EndSample();

            return count;
        }

        private static float getVelocity(float height, float distance, float gravity, float angle)
        {
            var tan = Mathf.Tan(angle);
            var top = Mathf.Sqrt(distance) * Mathf.Sqrt(Mathf.Abs(gravity)) * Mathf.Sqrt(tan * tan + 1);
            var bottom = Mathf.Sqrt(-((2 * height) / distance - 2 * tan));

            return top / bottom;
        }

        private static bool getAngle(float height, float distance, float velocity, float gravity, ref float angle)
        {
            var sqrt = velocity * velocity * velocity * velocity - (gravity * (gravity * distance * distance - 2 * height * velocity * velocity));

            if (sqrt > 0)
            {
                sqrt = Mathf.Sqrt(sqrt);
                angle = Mathf.Atan((velocity * velocity + sqrt) / (-gravity * distance));
                return true;
            }
            else
                return false;
        }
    }
}
