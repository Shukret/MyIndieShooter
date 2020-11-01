using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Description of a weapon/tool held by a CharacterMotor. 
    /// </summary>
    [Serializable]
    public struct WeaponDescription
    {
        /// <summary>
        /// Link to the weapon object containg a Gun component.
        /// </summary>
        [Tooltip("Link to the weapon object containg a Gun component.")]
        public GameObject Item;

        /// <summary>
        /// Link to the holstered weapon object which is made visible when the weapon is not used.
        /// </summary>
        [Tooltip("Link to the holstered weapon object which is made visible when the weapon is not used.")]
        public GameObject Holster;

        /// <summary>
        /// Defines character animations used with this weapon.
        /// </summary>
        [Tooltip("Defines character animations used with this weapon.")]
        public WeaponType Type;

        /// <summary>
        /// Animations to use for a tool. Relevant when weapon type is set to 'tool'.
        /// </summary>
        [Tooltip("Animations to use for a tool. Relevant when weapon type is set to 'tool'.")]
        public Tool Tool;

        /// <summary>
        /// Link to the flashlight attached to the weapon.
        /// </summary>
        public Flashlight Flashlight
        {
            get
            {
                if (_cacheItem == Item)
                    return _cachedFlashlight;
                else
                {
                    cache();
                    return _cachedFlashlight;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the gun component of the Item.
        /// </summary>
        public Gun Gun
        {
            get
            {
                if (_cacheItem == Item)
                    return _cachedGun;
                else
                {
                    cache();
                    return _cachedGun;
                }
            }
        }

        /// <summary>
        /// Checks if the weapon is a tool that requires character aiming.
        /// </summary>
        public bool IsAnAimableTool(bool useAlternate)
        {
            return Type == WeaponType.Tool && ToolDescription.Defaults[(int)Tool].HasAiming(useAlternate);
        }

        /// <summary>
        /// Checks if the weapon is a tool that's not single action and used continuously instead.
        /// </summary>
        public bool IsAContinuousTool(bool useAlternate)
        {
            return Type == WeaponType.Tool && ToolDescription.Defaults[(int)Tool].IsContinuous(useAlternate);
        }

        private Gun _cachedGun;
        private Flashlight _cachedFlashlight;

        private GameObject _cacheItem;

        private void cache()
        {
            _cacheItem = Item;
            _cachedGun = Item == null ? null : Item.GetComponent<Gun>();

            _cachedFlashlight = Item == null ? null : Item.GetComponent<Flashlight>();

            if (_cachedFlashlight == null && Item != null)
                _cachedFlashlight = Item.GetComponentInChildren<Flashlight>();
        }
    }

    /// <summary>
    /// Defines character animations used with a weapon.
    /// </summary>
    public enum WeaponType
    {
        Pistol,
        Rifle,
        Tool,
    }

    /// <summary>
    /// Defines a set of character animator states used with a weapon.
    /// </summary>
    public struct WeaponAnimationStates
    {
        /// <summary>
        /// Name of the reload animator state.
        /// </summary>
        public string Reload;

        /// <summary>
        /// Name of the melee hit animator state.
        /// </summary>
        public string Hit;

        /// <summary>
        /// Name of animator states when a weapon is in full use.
        /// </summary>
        public string[] Common;

        /// <summary>
        /// Name of equip animator state.
        /// </summary>
        public string Equip;

        /// <summary>
        /// Name of use animator state.
        /// </summary>
        public string Use;

        /// <summary>
        /// Name of alternate use animator state.
        /// </summary>
        public string AlternateUse;

        /// <summary>
        /// Returns animator state names.
        /// </summary>
        public static WeaponAnimationStates Default()
        {
            var states = new WeaponAnimationStates();
            states.Reload = "Reload";
            states.Hit = "Hit";
            states.Common = new string[] { "Idle", "Aim", "Cover", "Empty state", "Jump", "Use", "Alternate Use", "Reload" };
            states.Equip = "Equip";
            states.Use = "Use";
            states.AlternateUse = "Alternate Use";

            return states;
        }
    }
}