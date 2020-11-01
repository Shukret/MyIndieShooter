using UnityEngine;

namespace CoverShooter
{
    public class AIItemBase : AIBase
    {
        /// <summary>
        /// Is the weapon discovered automatically inside the character motor.
        /// </summary>
        [Tooltip("Is the weapon discovered automatically inside the character motor.")]
        public bool AutoFindIndex = true;

        /// <summary>
        /// Weapon index to use when auto-find is turned off.
        /// </summary>
        [Tooltip("Weapon index to use when auto-find is turned off.")]
        public int Index = 0;

        /// <summary>
        /// Equips the item if possible.
        /// </summary>
        protected bool Equip(CharacterMotor motor)
        {
            if (Index > 0 && isActiveAndEnabled)
            {
                motor.InputWeapon(Index);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Unequips the item if it is currently used.
        /// </summary>
        protected bool Unequip(CharacterMotor motor)
        {
            if (Index > 0 && motor.NextWeapon == Index && isActiveAndEnabled)
            {
                motor.InputWeapon(0);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Finds an item index of a weapon. Prefers the given type. Returns true if a weapon was found.
        /// </summary>
        protected bool AutoFind(CharacterMotor motor, WeaponType type)
        {
            for (int i = 0; i < motor.Weapons.Length; i++)
                if (motor.Weapons[i].Type == type)
                {
                    Index = i + 1;
                    return true;
                }

            for (int i = 0; i < motor.Weapons.Length; i++)
                if (motor.Weapons[i].Type != WeaponType.Tool)
                {
                    Index = i + 1;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Finds an item index of a tool.
        /// </summary>
        protected bool AutoFind(CharacterMotor motor, Tool tool)
        {
            for (int i = 0; i < motor.Weapons.Length; i++)
                if (motor.Weapons[i].Type == WeaponType.Tool && motor.Weapons[i].Tool == tool)
                {
                    Index = i + 1;
                    return true;
                }

            if (tool == Tool.flashlight)
                for (int i = 0; i < motor.Weapons.Length; i++)
                    if (motor.Weapons[i].Flashlight != null)
                    {
                        Index = i + 1;
                        return true;
                    }

            return false;
        }
    }
}
