using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    public struct Character
    {
        /// <summary>
        /// Cached value of Motor not being null.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Link to the game object of the character.
        /// </summary>
        public GameObject Object;

        /// <summary>
        /// Link to the character motor of the character.
        /// </summary>
        public CharacterMotor Motor;
        
        /// <summary>
        /// Link to the possible AI component of the object.
        /// </summary>
        public AIController AI;

        public bool IsInSight(float height, float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint(height);
            return position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0;
        }

        public bool IsAnyInSight(float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint();
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            position = ViewportPoint(2);
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            return false;
        }

        public Vector3 ViewportPoint(float height = 0)
        {
            if (Object == null || CameraManager.Main == null)
                return Vector2.zero;

            return CameraManager.Main.WorldToViewportPoint(Object.transform.position);
        }
    }

    public static class Characters
    {
        /// <summary>
        /// All alive characters during the last update.
        /// </summary>
        public static IEnumerable<Character> All
        {
            get
            {
                foreach (var character in list)
                    if (character.IsValid && character.Motor.IsAlive)
                        yield return character;
            }
        }

        public static Character MainPlayer;

        private static Dictionary<GameObject, Character> dictionary = new Dictionary<GameObject, Character>();
        private static List<Character> list = new List<Character>();

        public static void Register(CharacterMotor motor)
        {
            if (motor == null)
                return;

            var build = Build(motor);
            dictionary[motor.gameObject] = build;

            if (MainPlayer.Object == null)
                if (motor.GetComponent<ThirdPersonController>() || motor.GetComponent<MobileController>())
                    MainPlayer = build;

            var isContained = false;

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list[i] = Build(motor);
                    isContained = true;
                }

            if (!isContained)
                list.Add(build);
        }

        public static void Unregister(CharacterMotor motor)
        {
            if (motor != null && dictionary.ContainsKey(motor.gameObject))
                dictionary.Remove(motor.gameObject);

            for (int i = 0; i < list.Count; i++)
                if (list[i].Motor == motor)
                {
                    list.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        /// Returns cached character description for the given object.
        /// </summary>
        public static Character Get(GameObject gameObject)
        {
            if (!dictionary.ContainsKey(gameObject))
                dictionary[gameObject] = Build(gameObject.GetComponent<CharacterMotor>());

            return dictionary[gameObject];
        }

        /// <summary>
        /// Creates and returns character description for the given object.
        /// </summary>
        public static Character Build(CharacterMotor motor)
        {
            Character character;

            if (motor != null)
            {
                character.IsValid = true;
                character.Object = motor.gameObject;
                character.Motor = motor;
                character.AI = motor.GetComponent<AIController>();
            }
            else
            {
                character.IsValid = false;
                character.Object = null;
                character.Motor = null;
                character.AI = null;
            }

            return character;
        }
    }
}
