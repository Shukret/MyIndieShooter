using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Makes a non-AI object to arm a weapon.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerArmTrigger : MonoBehaviour
    {
        /// <summary>
        /// Weapon to arm. Value of 0 means none.
        /// </summary>
        [Tooltip("Weapon to arm. Value of 0 means none.")]
        public int WeaponToUse = 1;

        /// <summary>
        /// Trigger is ignored if the player already has a weapon. When UseForce is enabled the player always switches to the new weapon.
        /// </summary>
        [Tooltip("Trigger is ignored if the player already has a weapon. When UseForce is enabled the player always switches to the new weapon.")]
        public bool UseForce = false;

        private void OnTriggerEnter(Collider other)
        {
            var ai = other.GetComponent<AIController>();
            if (ai != null) return;

            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null) return;

            if (motor.Gun == null || WeaponToUse == 0 || UseForce)
                motor.InputWeapon(WeaponToUse);
        }
    }
}
