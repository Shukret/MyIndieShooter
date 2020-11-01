using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Makes a non-AI object hide their weapon.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerDisarmTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var ai = other.GetComponent<AIController>();
            if (ai != null) return;

            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null) return;

            motor.InputWeapon(0);
        }
    }
}
