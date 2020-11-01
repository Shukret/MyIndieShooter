using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    public class AIFlashlight : AIItemBase
    {
        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private int _darkzoneCount;

        private bool _isUsing;

        #endregion

        #region Commands

        /// <summary>
        /// Registers a command to take out a flashlight. Ignored if currently equipped weapon already has a flashlight attached.
        /// </summary>
        public void ToTakeFlashlight()
        {
            if (_motor.NextWeapon == 0 || _motor.Weapons[_motor.NextWeapon - 1].Flashlight == null || _motor.Weapons[_motor.CurrentWeapon - 1].Type == WeaponType.Tool)
                Equip(_motor);
        }

        /// <summary>
        /// Registers a command to hide a flashlight if taken out.
        /// </summary>
        public void ToHideFlashlight()
        {
            if (isActiveAndEnabled)
                turnOffWeaponFlashlight();

            Unequip(_motor);
            _isUsing = false;
        }

        /// <summary>
        /// Registers a command to turn on light on the currently equipped item.
        /// </summary>
        public void ToTurnOnLight()
        {
            _isUsing = true;
        }

        /// <summary>
        /// Registers a command to equip a flashlight and turn it on. Will turn on the light on a weapon if present.
        /// </summary>
        public void ToLight()
        {
            ToTakeFlashlight();
            _isUsing = true;
        }

        /// <summary>
        /// Registers a command to turn off light.
        /// </summary>
        public void ToUnlight()
        {
            _isUsing = false;

            if (isActiveAndEnabled)
                turnOffWeaponFlashlight();
        }

        /// <summary>
        /// Registers a command to disarm, turns off a weapon's flashlight before it is taken off.
        /// </summary>
        public void ToDisarm()
        {
            if (isActiveAndEnabled)
                turnOffWeaponFlashlight();
        }

        #endregion

        #region Events

        /// <summary>
        /// Notified of an entrance to a dark area.
        /// </summary>
        public void OnEnterDarkness(DarkZone zone)
        {
            _darkzoneCount++;

            if (_darkzoneCount == 1)
                Message("OnNeedLight");
        }

        /// <summary>
        /// Notified of an exit out of a dark area.
        /// </summary>
        public void OnLeaveDarkness(DarkZone zone)
        {
            _darkzoneCount--;

            if (_darkzoneCount == 0)
                Message("OnDontNeedLight");
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();

            if (AutoFindIndex)
                AutoFind(_motor, Tool.flashlight);
        }

        private void Update()
        {
            if (!_actor.IsAlive)
            {
                turnOffFlashlight();
                return;
            }

            if (!attemptManageMotorFlashlight())
                if (_isUsing && _motor.NextWeapon == Index && Index > 0)
                    _motor.InputUseTool();
        }

        private void turnOffWeaponFlashlight()
        {
            if (_motor.CurrentWeapon > 0 && _motor.Weapons[_motor.CurrentWeapon - 1].Type != WeaponType.Tool && _motor.Weapons[_motor.CurrentWeapon - 1].Flashlight != null)
                turnOffFlashlight();
        }

        private void turnOffFlashlight()
        {
            if (_motor.CurrentWeapon > 0 && _motor.Weapons[_motor.CurrentWeapon - 1].Flashlight != null)
            {
                var flashlight = _motor.Weapons[_motor.CurrentWeapon - 1].Flashlight;

                if (flashlight.IsTurnedOn)
                    flashlight.OnUsed();
            }
        }

        private bool attemptManageMotorFlashlight()
        {
            if (_motor.CurrentWeapon > 0 && _motor.Weapons[_motor.CurrentWeapon - 1].Type != WeaponType.Tool && _motor.Weapons[_motor.CurrentWeapon - 1].Flashlight != null)
            {
                var flashlight = _motor.Weapons[_motor.CurrentWeapon - 1].Flashlight;

                if (_isUsing && !flashlight.IsTurnedOn)
                    flashlight.OnStartUsing();
                else if (!_isUsing && flashlight.IsTurnedOn)
                    flashlight.OnUsed();

                return true;
            }
            else
                return false;
        }

        #endregion
    }
}
