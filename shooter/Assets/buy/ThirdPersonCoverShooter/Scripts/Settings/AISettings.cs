using System;
using UnityEngine;

namespace CoverShooter
{
    public enum AIStartMode
    {
        patrol,
        idle,
        searchAround,
        searchPosition,
        investigate
    }

    [Serializable]
    public struct AITargetSettings
    {
        /// <summary>
        /// Minimum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.
        /// </summary>
        [Tooltip("Minimum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.")]
        [Range(0, 1)]
        public float Min;

        /// <summary>
        /// Maximum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.
        /// </summary>
        [Tooltip("Maximum possible position going from the feet up that the AI is aiming at. Value of 0 means feet, value of 1 means top of the head.")]
        [Range(0, 1)]
        public float Max;

        public AITargetSettings(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    [Serializable]
    public struct AIApproximationSettings
    {
        /// <summary>
        /// Minimal possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.
        /// </summary>
        [Tooltip("Minimal possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.")]
        public float Min;

        /// <summary>
        /// Maximum possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.
        /// </summary>
        [Tooltip("Maximum possible error the AI makes when approximating the position of an enemy. Error is measured in meters around the target. Smaller values mean the AI is better at guessing the position.")]
        public float Max;

        /// <summary>
        /// Distance at which the AI is using Min value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Min value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.")]
        public float MinDistance;

        /// <summary>
        /// Distance at which the AI is using Max value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Max value for guessing. If a target is at a greater distance the value is interpolated between Min and Max.")]
        public float MaxDistance;

        /// <summary>
        /// Constructs a new distance range.
        /// </summary>
        public AIApproximationSettings(float min, float max, float minDistance, float maxDistance)
        {
            Min = min;
            Max = max;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        /// <summary>
        /// Returns value based on the given position. The distance is calculated between the main camera and the given position.
        /// </summary>
        public float Get(Vector3 position)
        {
            if (CameraManager.Main == null || CameraManager.Main.transform == null)
                return Min;
            else
                return Get(Vector3.Distance(CameraManager.Main.transform.position, position));
        }

        /// <summary>
        /// Returns value based on the given distance.
        /// </summary>
        public float Get(float distance)
        {
            float t = Mathf.Clamp01((distance - MinDistance) / (MaxDistance - MinDistance));
            return Mathf.Lerp(Min, Max, t);
        }
    }

    [Serializable]
    public struct AIAccuracySettings
    {
        /// <summary>
        /// How precise the AI is when at Min Distance or closer to the enemy. Error is measured in meters around the target. Greater values translate to greater errors in AI's aiming.
        /// </summary>
        [Tooltip("How precise the AI is when at Min Distance or closer to the enemy. Error is measured in meters around the target. Greater values translate to greater errors in AI's aiming.")]
        public float Min;

        /// <summary>
        /// How precise the AI is when at Max Distance or greater to the enemy. Error is measured in meters around the target. Greater values translate to greater errors in AI's aiming.
        /// </summary>
        [Tooltip("How precise the AI is when at Max Distance or greater to the enemy. Error is measured in meters around the target. Greater values translate to greater errors in AI's aiming.")]
        public float Max;

        /// <summary>
        /// Distance at which the AI is using Min value for aiming. If a target is at a greater distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Min value for aiming. If a target is at a greater distance the value is interpolated between Min and Max.")]
        public float MinDistance;

        /// <summary>
        /// Distance at which the AI is using Max value for aiming. If a target is at a lesser distance the value is interpolated between Min and Max.
        /// </summary>
        [Tooltip("Distance at which the AI is using Max value for aiming. If a target is at a lesser distance the value is interpolated between Min and Max.")]
        public float MaxDistance;

        /// <summary>
        /// Constructs a new distance range.
        /// </summary>
        public AIAccuracySettings(float min, float max, float minDistance, float maxDistance)
        {
            Min = min;
            Max = max;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        /// <summary>
        /// Returns value based on the given position. The distance is calculated between the main camera and the given position.
        /// </summary>
        public float Get(Vector3 position)
        {
            if (CameraManager.Main == null || CameraManager.Main.transform == null)
                return Min;
            else
                return Get(Vector3.Distance(CameraManager.Main.transform.position, position));
        }

        /// <summary>
        /// Returns value based on the given distance.
        /// </summary>
        public float Get(float distance)
        {
            float t = Mathf.Clamp01((distance - MinDistance) / (MaxDistance - MinDistance));
            return Mathf.Lerp(Min, Max, t);
        }
    }

    /// <summary>
    /// Settings for AI start.
    /// </summary>
    [Serializable]
    public struct AIStartSettings
    {
        /// <summary>
        /// Mode in which the AI starts.
        /// </summary>
        [Tooltip("Mode in which the AI starts.")]
        public AIStartMode Mode;

        /// <summary>
        /// Position the AI should investigate if the mode is set to searchPosition or investigate.
        /// </summary>
        [Tooltip("Position the AI should investigate if the mode is set to searchPosition or investigate.")]
        public Vector3 Position;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIStartSettings Default()
        {
            var result = new AIStartSettings();
            result.Mode = AIStartMode.patrol;
            result.Position = Vector3.zero;

            return result;
        }
    }

    /// <summary>
    /// Results of an AI call.
    /// </summary>
    [Serializable]
    public struct AICall
    {
        /// <summary>
        /// Target object that will receive the message.
        /// </summary>
        [Tooltip("Target object that will receive the message.")]
        public GameObject Target;

        /// <summary>
        /// Function name in a script that belongs in the target object.
        /// </summary>
        [Tooltip("Function name in a script that belongs in the target object.")]
        public string Message;

        /// <summary>
        /// Should the calling Actor component be passed to the called function as an argument.
        /// </summary>
        [Tooltip("Should the calling Actor component be passed to the called function as an argument.")]
        public bool PassCaller;

        public void Make(Actor caller)
        {
            if (Target != null)
            {
                if (PassCaller)
                    Target.SendMessage(Message, caller, SendMessageOptions.RequireReceiver);
                else
                    Target.SendMessage(Message, SendMessageOptions.RequireReceiver);
            }
        }

        public static AICall Default()
        {
            var result = new AICall();
            result.Target = null;
            result.Message = "Spawn";
            result.PassCaller = true;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI behaviour.
    /// </summary>
    [Serializable]
    public struct AIBehaviourSettings
    {
        /// <summary>
        /// Can the AI sprint when not aiming at the enemy.
        /// </summary>
        [Tooltip("Can the AI sprint when not aiming at the enemy.")]
        public bool CanSprint;

        /// <summary>
        /// Is AI approaching the enemy from cover to cover.
        /// </summary>
        [Tooltip("Is AI approaching the enemy from cover to cover.")]
        public bool IsApproachingUsingCovers;

        /// <summary>
        /// Is AI always staying in cover to fight.
        /// </summary>
        [Tooltip("Is AI always staying in cover to fight.")]
        public bool IsFightingUsingCovers;

        /// <summary>
        /// Is AI using covers to retreat.
        /// </summary>
        [Tooltip("Is AI using covers to retreat.")]
        public bool IsRetreatingUsingCovers;

        /// <summary>
        /// Can AI pull the trigger.
        /// </summary>
        [Tooltip("Can AI pull the trigger.")]
        public bool CanFire;

        /// <summary>
        /// Does AI only fire when it is seen by the main camera.
        /// </summary>
        [Tooltip("Does AI only fire when it is seen by the main camera.")]
        public bool OnlyFireWhenSeen;

        /// <summary>
        /// Should the AI always aim at something.
        /// </summary>
        [Tooltip("Should the AI always aim at something.")]
        public bool AlwaysAim;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIBehaviourSettings Default()
        {
            var result = new AIBehaviourSettings();
            result.CanSprint = true;
            result.IsApproachingUsingCovers = true;
            result.IsFightingUsingCovers = true;
            result.IsRetreatingUsingCovers = true;
            result.CanFire = true;
            result.OnlyFireWhenSeen = false;
            result.AlwaysAim = false;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI patrolling.
    /// </summary>
    [Serializable]
    public struct AIPatrolSettings
    {
        /// <summary>
        /// Should the AI always run when patrolling.
        /// </summary>
        [Tooltip("Should the AI always run when patrolling.")]
        public bool IsAlwaysRunning;

        /// <summary>
        /// Duration in seconds of a head sweep to one direction.
        /// </summary>
        [Tooltip("Duration in seconds of a head sweep to one direction.")]
        public float LookDuration;

        /// <summary>
        /// Angle in degrees of a head sweep to left or right.
        /// </summary>
        [Tooltip("Angle in degrees of a head sweep to left or right.")]
        public float LookAngle;

        /// <summary>
        /// Speed of head turning when patrolling.
        /// </summary>
        [Tooltip("Speed of head turning when patrolling.")]
        public float LookSpeed;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIPatrolSettings Default()
        {
            var result = new AIPatrolSettings();
            result.IsAlwaysRunning = false;
            result.LookDuration = 1.5f;
            result.LookAngle = 60f;
            result.LookSpeed = 2f;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI fighting and aiming.
    /// </summary>
    [Serializable]
    public struct AIFightingSettings
    {
        /// <summary>
        /// AI retreats when its health is lower than this value.
        /// </summary>
        [Tooltip("AI retreats when its health is lower than this value.")]
        public float MinHealth;

        /// <summary>
        /// AI manually reloads a gun when the bullets lower than a fraction of the clip size.
        /// </summary>
        [Tooltip("AI manually reloads a gun when the bullets lower than a fraction of the clip size.")]
        public float ReloadFraction;

        /// <summary>
        /// Time in seconds for AI to react to changes in the world other than grenades.
        /// </summary>
        [Tooltip("Time in seconds for AI to react to changes in the world.")]
        public float ReactionTime;

        /// <summary>
        /// Time in seconds for AI to react to grenades.
        /// </summary>
        [Tooltip("Time in seconds for AI to react to grenades.")]
        public float GrenadeReactionTime;

        /// <summary>
        /// Weapon index for the AI to use. Will fallback to a lower one when not available.
        /// </summary>
        [Tooltip("Weapon index for the AI to use. Will fallback to a lower one when not available.")]
        public int WeaponToUse;

        /// <summary>
        /// Aiming precision. Lower values translate to better aiming.
        /// </summary>
        [Tooltip("Aiming precision. Lower values translate to better aiming.")]
        public DistanceRange TargetRadius;

        /// <summary>
        /// How long the AI will wait for the enemy to appear again.
        /// </summary>
        [Tooltip("How long the AI will wait for the enemy to appear again.")]
        public float Patience;

        /// <summary>
        /// How long the AI will stay close to the enemy after discovering them.
        /// </summary>
        [Tooltip("How long the AI will stay close to the enemy after discovering them.")]
        public float Irritation;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIFightingSettings Default()
        {
            var result = new AIFightingSettings();
            result.MinHealth = 40;
            result.ReloadFraction = 0.3f;
            result.ReactionTime = 0.3f;
            result.GrenadeReactionTime = 0.6f;
            result.TargetRadius = new DistanceRange(0, 1, 3, 20);
            result.WeaponToUse = 2;
            result.Patience = 20;
            result.Irritation = 10;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI grenades.
    /// </summary>
    [Serializable]
    public struct AIGrenadeSettings
    {
        /// <summary>
        /// Number of grenades the AI will throw.
        /// </summary>
        [Tooltip("Number of grenades the AI will throw.")]
        public int GrenadeCount;

        /// <summary>
        /// Time in seconds since becoming alerted to wait before throwing a grenade.
        /// </summary>
        [Tooltip("Time in seconds since becoming alerted to wait before throwing a grenade.")]
        public float FirstCheckDelay;

        /// <summary>
        /// AI will only throw a grenade if it can hit the enemy. CheckInterval defines the time between checks.
        /// </summary>
        [Tooltip("AI will only throw a grenade if it can hit the enemy. CheckInterval defines the time between checks.")]
        public float CheckInterval;

        /// <summary>
        /// Time in seconds to wait before throwing a grenade after already having thrown one.
        /// </summary>
        [Tooltip("Time in seconds to wait before throwing a grenade after already having thrown one.")]
        public float Interval;

        /// <summary>
        /// Maximum allowed distance between a landed grenade and the enemy. Throws with greater result distance are cancelled.
        /// </summary>
        [Tooltip("Maximum allowed distance between a landed grenade and the enemy. Throws with greater result distance are cancelled.")]
        public float MaxRadius;

        /// <summary>
        /// Distance to maintain against grenades.
        /// </summary>
        [Tooltip("Distance to maintain against grenades.")]
        public float AvoidDistance;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIGrenadeSettings Default()
        {
            var result = new AIGrenadeSettings();
            result.GrenadeCount = 1;
            result.FirstCheckDelay = 5;
            result.Interval = 10;
            result.CheckInterval = 2;
            result.MaxRadius = 5;
            result.AvoidDistance = 8;

            return result;
        }
    }

    /// <summary>
    /// Settings for distances AI tries to maintain between characters.
    /// </summary>
    [Serializable]
    public struct AIDistanceSettings
    {
        /// <summary>
        /// Distance to maintain when retreating from an enemy.
        /// </summary>
        [Tooltip("Distance to maintain when retreating from an enemy.")]
        public float MinRetreat;

        /// <summary>
        /// AI will avoid taking paths that are too close to the enemy.
        /// </summary>
        [Tooltip("AI will avoid taking paths that are too close to the enemy.")]
        public float MinPassing;

        /// <summary>
        /// Distance to friends in cover for the AI to maintain.
        /// </summary>
        [Tooltip("Distance to friends in cover for the AI to maintain.")]
        public float MinFriend;

        /// <summary>
        /// AI will keep firing back at the enemy no matter what if closer than this distance.
        /// </summary>
        [Tooltip("AI will keep firing back at the enemy no matter what if closer than this distance.")]
        public float MaxFightBack;

        /// <summary>
        /// AI tries to stand closer to it's enemy than this distance when fighting away from covers.
        /// </summary>
        [Tooltip("AI tries to stand closer to it's enemy than this distance when fighting away from covers.")]
        public float MaxWalkingFight;

        /// <summary>
        /// If enemy doesn't intend to always fight in covers it stops using them when closer to the enemy than this value.
        /// </summary>
        [Tooltip("If enemy doesn't intend to always fight in covers it stops using them when closer to the enemy than this value.")]
        public float MaxApproach;

        /// <summary>
        /// AI avoids being closer to it's enemy than this value.
        /// </summary>
        [Tooltip("AI avoids being closer to it's enemy than this value.")]
        public float MinEnemy;

        /// <summary>
        /// AI looks for enemies in random direction by at least this distance.
        /// </summary>
        [Tooltip("AI looks for enemies in random direction by at least this distance.")]
        public float MinSearch;

        /// <summary>
        /// AI counts threat as investigated once it comes closer than this value.
        /// </summary>
        [Tooltip("AI counts threat as investigated once it comes closer than this value.")]
        public float ThreatInvestigation;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIDistanceSettings Default()
        {
            var result = new AIDistanceSettings();
            result.MinRetreat = 40;
            result.MinPassing = 6;
            result.MinFriend = 4;
            result.MaxFightBack = 6;
            result.MaxWalkingFight = 6;
            result.MaxApproach = 12;
            result.MinEnemy = 3;
            result.MinSearch = 20;
            result.ThreatInvestigation = 4;

            return result;
        }
    }

    /// <summary>
    /// AI cover settings.
    /// </summary>
    [Serializable]
    public struct AICoverSettings
    {
        /// <summary>
        /// AI won't take covers that are closer to the enemy.
        /// </summary>
        [Tooltip("AI won't take covers that are closer to the enemy.")]
        public float MinCoverToEnemyDistance;

        /// <summary>
        /// Maximum distance of a cover for AI to take.
        /// </summary>
        [Tooltip("Maximum distance of a cover for AI to take.")]
        public float MaxDistance;

        /// <summary>
        /// Maximum distance of a cover for AI to switch to when already in other cover.
        /// </summary>
        [Tooltip("Maximum distance of a cover for AI to switch to when already in other cover.")]
        public float MaxSwitchDistance;

        /// <summary>
        /// AI avoids to switch to covers that are closer than this value.
        /// </summary>
        [Tooltip("AI avoids to switch to covers that are closer than this value.")]
        public float MinSwitchDistance;

        /// <summary>
        /// Maximum angle of a low cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a low cover relative to the enemy.")]
        public float MaxLowAngle;

        /// <summary>
        /// Maximum angle of a tall cover relative to the enemy.
        /// </summary>
        [Tooltip("Maximum angle of a tall cover relative to the enemy.")]
        public float MaxTallAngle;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AICoverSettings Default()
        {
            var result = new AICoverSettings();
            result.MinCoverToEnemyDistance = 6;
            result.MaxDistance = 50;
            result.MaxSwitchDistance = 30;
            result.MinSwitchDistance = 4;
            result.MaxLowAngle = 60;
            result.MaxTallAngle = 40;

            return result;
        }
    }

    /// <summary>
    /// Settings for AI to notice changes in the world.
    /// </summary>
    [Serializable]
    public struct AIViewSettings
    {
        /// <summary>
        /// Field of sight to notice changes in the world.
        /// </summary>
        [Tooltip("Field of sight to notice changes in the world.")]
        public float FieldOfView;

        /// <summary>
        /// Distance for AI to notice visible threat or a teammate being attacked when not alerted
        /// </summary>
        [Tooltip("Distance for AI to notice visible threat or a teammate being attacked when not alerted.")]
        public float CalmSightDistance;

        /// <summary>
        /// Distance for AI to notice visible threat or a teammate being attacked when alerted.
        /// </summary>
        [Tooltip("Distance for AI to notice visible threat or a teammate being attacked when alerted.")]
        public float AlertedSightDistance;

        /// <summary>
        /// Distance in any direction for AI to communicate between each other.
        /// </summary>
        [Tooltip("Distance in any direction for AI to communicate between each other.")]
        public float CommunicationDistance;

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AIViewSettings Default()
        {
            var result = new AIViewSettings();
            result.FieldOfView = 160;
            result.CalmSightDistance = 20;
            result.AlertedSightDistance = 30;
            result.CommunicationDistance = 12;

            return result;
        }

        /// <summary>
        /// Returns appropriate sight distance depending on alertness.
        /// </summary>
        /// <param name="isAlerted"></param>
        /// <returns></returns>
        public float SightDistance(bool isAlerted)
        {
            return isAlerted ? AlertedSightDistance : CalmSightDistance;
        }
    }

    /// <summary>
    /// Settings for AI burst of fire when in cover.
    /// </summary>
    [Serializable]
    public struct AICoverBurstSettings
    {
        /// <summary>
        /// Number of bursts before moving to another cover.
        /// </summary>
        [Tooltip("Number of bursts before moving to another cover.")]
        public int Count;

        /// <summary>
        /// Time in seconds to wait in cover for another burst of fire.
        /// </summary>
        [Tooltip("Time in seconds to wait in cover for another burst of fire.")]
        public float Wait;

        /// <summary>
        /// Duration in seconds of a burst.
        /// </summary>
        [Tooltip("Duration in seconds of a burst.")]
        public float Duration;

        /// <summary>
        /// Time in seconds for AI to stand without firing before a burst.
        /// </summary>
        [Tooltip("Time in seconds for AI to stand without firing before a burst.")]
        public float IntroDuration;

        /// <summary>
        /// Time in seconds for AI to stand without firing after a burst.
        /// </summary>
        [Tooltip("Time in seconds for AI to stand without firing after a burst.")]
        public float OutroDuration;

        /// <summary>
        /// Total duration of peeking.
        /// </summary>
        public float TotalPeekDuration { get { return Duration + IntroDuration + OutroDuration; } }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AICoverBurstSettings DefaultApproach()
        {
            var result = new AICoverBurstSettings();
            result.Count = 2;
            result.Wait = 1.0f;
            result.Duration = 0.9f;
            result.IntroDuration = 0.35f;
            result.OutroDuration = 0.35f;

            return result;
        }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static AICoverBurstSettings DefaultCovered()
        {
            var result = new AICoverBurstSettings();
            result.Count = 4;
            result.Wait = 1.0f;
            result.Duration = 0.9f;
            result.IntroDuration = 0.5f;
            result.OutroDuration = 0.5f;

            return result;
        }
    }
}
