using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIPhone : AIItemBase
    {
        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private bool _isFilming;
        private bool _wantsToCall;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to start filming.
        /// </summary>
        public void ToStartFilming()
        {
            if (isActiveAndEnabled)
                ToTakePhone();

            _isFilming = true;
        }

        /// <summary>
        /// Told by the brains to stop filming.
        /// </summary>
        public void ToStopFilming()
        {
            _isFilming = false;
        }

        /// <summary>
        /// Told by the brains to take a weapon to arms.
        /// </summary>
        public void ToTakePhone()
        {
            Equip(_motor);
        }

        /// <summary>
        /// Told by the brains to disarm any weapon.
        /// </summary>
        public void ToHidePhone()
        {
            Unequip(_motor);
            _isFilming = false;
        }

        /// <summary>
        /// Told by the brains to initiate a call.
        /// </summary>
        public void ToCall()
        {
            ToStopFilming();
            ToTakePhone();
            _wantsToCall = true;
        }

        /// <summary>
        /// Told by the brains to initiate a phone call.
        /// </summary>
        public void ToPhoneCall()
        {
            ToStopFilming();
            ToTakePhone();
            _wantsToCall = true;
        }

        #endregion

        #region Events

        public void OnToolUsedAlternate()
        {
            if (isActiveAndEnabled && _wantsToCall && _motor.CurrentWeapon == Index)
            {
                Message("OnCallMade");
                _wantsToCall = false;
            }
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();

            if (AutoFindIndex)
                AutoFind(_motor, Tool.phone);
        }

        private void Update()
        {
            if (!_actor.IsAlive || _motor.CurrentWeapon != Index)
                return;

            if (_wantsToCall)
                _motor.InputUseToolAlternate();
            else if (_isFilming)
                _motor.InputUseTool();
        }

        #endregion
    }
}
