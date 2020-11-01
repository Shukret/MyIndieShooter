using UnityEngine;

namespace CoverShooter
{
    public struct SustainedValue
    {
        public bool Current;
        public bool Target;
        public float Shift;

        public void Update(float speed)
        {
            if (Current != Target)
            {
                Shift += Time.deltaTime * speed;

                if (Shift >= 1)
                {
                    Current = Target;
                    Shift = 0;
                }
            }
            else
                Shift = 0;
        }
    }
}
