using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Possible states for the fighting AI to take.
    /// </summary>
    public enum FighterState
    {
        none,
        idle,
        patrol,
        standAndFight,
        maintainPosition,
        takeCover,
        switchCover,
        retreatToCover,
        fightInCover,
        hideInCover,
        avoidAndFight,
        avoidGrenade,
        circle,
        assault,
        search,
        investigate,
        flee,
        call
    }

    [Serializable]
    public struct FighterGrenadeAvoidSettings
    {
        /// <summary>
        /// Time in seconds for AI to react to grenades.
        /// </summary>
        [Tooltip("Time in seconds for AI to react to grenades.")]
        public float ReactionTime;

        /// <summary>
        /// Time in seconds to keep running from a threatening grenade.
        /// </summary>
        [Tooltip("Time in seconds to keep running from a threatening grenade.")]
        public float Duration;

        public static FighterGrenadeAvoidSettings Default()
        {
            var settings = new FighterGrenadeAvoidSettings();
            settings.ReactionTime = 1;
            settings.Duration = 2;

            return settings;
        }
    }

    [Serializable]
    public struct FighterRetreatSettings
    {
        /// <summary>
        /// Health value at which the AI will retreat.
        /// </summary>
        [Tooltip("Health value at which the AI will retreat.")]
        public float Health;

        /// <summary>
        /// Duration in seconds the frightened AI will wait and hide in cover before peeking again.
        /// </summary>
        [Tooltip("Duration in seconds the frightened AI will wait and hide in cover before peeking again.")]
        public float HideDuration;

        public static FighterRetreatSettings Default()
        {
            var settings = new FighterRetreatSettings();
            settings.Health = 25;
            settings.HideDuration = 3;

            return settings;
        }
    }

    [Serializable]
    public struct FighterInvestigationWaitSettings
    {
        /// <summary>
        /// Time in seconds to wait before going to inspect last seen covered enemy position.
        /// </summary>
        [Tooltip("Time in seconds to wait before going to inspect last seen covered enemy position.")]
        public float WaitForCovered;

        /// <summary>
        /// Time in seconds to wait before going to inspect last seen uncovered enemy position.
        /// </summary>
        [Tooltip("Time in seconds to wait before going to inspect last seen uncovered enemy position.")]
        public float WaitForUncovered;

        public static FighterInvestigationWaitSettings Default()
        {
            var settings = new FighterInvestigationWaitSettings();
            settings.WaitForCovered = 10;
            settings.WaitForUncovered = 10;

            return settings;
        }
    }

    [Serializable]
    public struct FighterSpeedSettings
    {
        public bool Enabled;

        public float Patrol;
        public float TakeCover;
        public float SwitchCover;
        public float RetreatToCover;
        public float Avoid;
        public float Circle;
        public float Assault;
        public float Search;
        public float Investigate;
        public float Flee;

        public static FighterSpeedSettings Default()
        {
            var settings = new FighterSpeedSettings();
            settings.Enabled = false;
            settings.Patrol = 1.0f;
            settings.TakeCover = 1.0f;
            settings.SwitchCover = 1.0f;
            settings.RetreatToCover = 1.0f;
            settings.Avoid = 1.0f;
            settings.Circle = 1.0f;
            settings.Assault = 1.0f;
            settings.Search = 1.0f;
            settings.Investigate = 1.0f;
            settings.Flee = 1.0f;

            return settings;
        }
    }

    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class FighterBrain : BaseBrain
    {
        #region Properties

        /// <summary>
        /// Is the AI currently alarmed.
        /// </summary>
        public bool IsAlerted
        {
            get
            {
                return _state != FighterState.none &&
                       _state != FighterState.idle &&
                       _state != FighterState.patrol &&
                       (_state != FighterState.search || Threat != null);
            }
        }

        /// <summary>
        /// AI state the brain is at.
        /// </summary>
        public FighterState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Time in seconds to wait for inspecting the last seen threat position.
        /// </summary>
        public float InvestigationWait
        {
            get { return ThreatCover ? Investigation.WaitForCovered : Investigation.WaitForUncovered; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Enemy distance to trigger slow retreat.
        /// </summary>
        [Tooltip("Enemy distance to trigger slow retreat.")]
        public float AvoidDistance = 4;

        /// <summary>
        /// Duration in seconds to stand fighting before changing state.
        /// </summary>
        [Tooltip("Duration in seconds to stand fighting before changing state.")]
        public float StandDuration = 2;

        /// <summary>
        /// Time in seconds for the AI to wait before switching to a better cover.
        /// </summary>
        [Tooltip("Time in seconds for the AI to wait before switching to a better cover.")]
        public float CoverSwitchWait = 10;

        /// <summary>
        /// Distance to the uncovered out of sight enemy that triggers the AI to follow.
        /// </summary>
        [Tooltip("Distance to the uncovered enemy that triggers the AI to follow.")]
        public float FollowDistance = 20;

        /// <summary>
        /// Settings for AI startup.
        /// </summary>
        [Tooltip("Settings for AI startup.")]
        public AIStartSettings Start = AIStartSettings.Default();

        /// <summary>
        /// Speed of the motor during various AI states.
        /// </summary>
        [Tooltip("Speed of the motor during various AI states.")]
        public FighterSpeedSettings Speed = FighterSpeedSettings.Default();

        /// <summary>
        /// How accurately the AI guesses the position of an enemy.
        /// </summary>
        [Tooltip("How accurately the AI guesses the position of an enemy.")]
        public AIApproximationSettings Approximation = new AIApproximationSettings(0, 10, 5, 30);

        /// <summary>
        /// Settings for AI retreats.
        /// </summary>
        [Tooltip("Settings for AI retreats.")]
        public FighterRetreatSettings Retreat = FighterRetreatSettings.Default();

        /// <summary>
        /// Settings for how long the AI waits before investigating.
        /// </summary>
        [Tooltip("Settings for how long the AI waits before investigating.")]
        public FighterInvestigationWaitSettings Investigation = FighterInvestigationWaitSettings.Default();

        /// <summary>
        /// Settings for how the fighter avoids other grenades.
        /// </summary>
        [Tooltip("Settings for how the fighter avoids other grenades.")]
        public FighterGrenadeAvoidSettings GrenadeAvoidance = FighterGrenadeAvoidSettings.Default();

        /// <summary>
        /// Settings for AI grenades.
        /// </summary>
        [Tooltip("Settings for AI fighting and aiming.")]
        public AIGrenadeSettings Grenades = AIGrenadeSettings.Default();

        /// <summary>
        /// Should a debug line be drawn towards the current threat.
        /// </summary>
        [Tooltip("Should a debug line be drawn towards the current threat.")]
        public bool DebugThreat = false;

        #endregion

        #region Private fields

        private CharacterMotor _motor;
        private CharacterHealth _health;

        private int _thrownGrenadeCount;

        private HashSet<BaseBrain> _friends = new HashSet<BaseBrain>();
        private HashSet<Actor> _friendsThatCanSeeMe = new HashSet<Actor>();
        private HashSet<Actor> _visibleCivilians = new HashSet<Actor>();

        private FighterState _previousState;
        private FighterState _state;
        private float _stateTime;

        private Vector3 _maintainPosition;
        private float _maintainDuration;
        private bool _hasReachedMaintainPosition;
        private float _maintainPositionReachTime;
        private AIBaseRegrouper _regrouper;

        private bool _failedToAvoid;

        private Vector3 _grenadePosition;
        private float _grenadeReaction;

        private float _grenadeTimer;
        private float _grenadeCheckTimer;
        private bool _hasThrowFirstGrenade;
        private Vector3[] _grenadePath = new Vector3[128];

        private FighterState _futureSetState;
        private bool _hasFoundCover;

        private bool _wasAlerted;
        private bool _wasAlarmed;

        private bool _assaultCheck;
        private bool _investigationCheck;
        private bool _searchCheck;

        private bool _isInDarkness;

        private List<Actor> _visibleActors = new List<Actor>();

        #endregion

        #region Commands

        /// <summary>
        /// Told by a component to be scared.
        /// </summary>
        public void ToBecomeScared()
        {
            setState(FighterState.flee);
        }

        /// <summary>
        /// Told by a component to make a call.
        /// </summary>
        public void ToMakeCall()
        {
            setState(FighterState.call);
        }

        /// <summary>
        /// Told by a component to find a cover.
        /// </summary>
        public void ToFindCover()
        {
            setState(FighterState.takeCover);
        }

        /// <summary>
        /// Told by an outside command to regroup around a unit.
        /// </summary>
        public void ToRegroupAround(AIBaseRegrouper regrouper)
        {
            _hasFoundCover = false;
            _regrouper = regrouper;

            setState(FighterState.takeCover);
        }

        public void ToFindAndMaintainPosition(AIBaseRegrouper regrouper)
        {
            _regrouper = regrouper;
            _maintainDuration = regrouper.UncoveredDuration;

            var movement = regrouper.GetComponent<AIMovement>();
            var middle = regrouper.transform.position;

            if (movement != null)
                middle = movement.Destination;

            for (int i = 0; i < 6; i++)
            {
                var radius = UnityEngine.Random.Range(1.0f, regrouper.Radius);
                var angle = UnityEngine.Random.Range(0f, 360f);
                var position = middle + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * radius;
                AIUtil.ClosestStandablePosition(ref position);

                if (!regrouper.IsPositionTaken(position))
                {
                    regrouper.TakePosition(position);
                    _maintainPosition = position;
                    setState(FighterState.maintainPosition, true);
                    break;
                }
            }
        }

        #endregion

        #region Checks

        /// <summary>
        /// Registers existance of an assault component.
        /// </summary>
        public void AssaultResponse()
        {
            _assaultCheck = true;
        }

        /// <summary>
        /// Registers existance of an investigation component.
        /// </summary>
        public void InvestigationResponse()
        {
            _investigationCheck = true;
        }

        /// <summary>
        /// Registers existance of a search component.
        /// </summary>
        public void SearchResponse()
        {
            _searchCheck = true;
        }

        private bool tryAssault()
        {
            _assaultCheck = false;
            Message("AssaultCheck", LastKnownThreatPosition);
            return _assaultCheck;
        }

        private bool tryInvestigate()
        {
            _investigationCheck = false;
            Message("InvestigationCheck");
            return _investigationCheck;
        }

        private bool trySearch()
        {
            _searchCheck = false;
            Message("SearchCheck");
            return _searchCheck;
        }

        #endregion

        #region Events

        /// <summary>
        /// Notified of an end of an assault.
        /// </summary>
        public void OnAssaultStop()
        {
            if (_state == FighterState.assault)
                setState(FighterState.circle);
        }

        /// <summary>
        /// A death was witnessed.
        /// </summary>
        public void OnSeeDeath(Actor actor)
        {
            if (_visibleActors.Contains(actor))
                _visibleActors.Remove(actor);

            if (actor != Threat)
                return;

            RemoveThreat();

            var minDistance = 0f;
            Actor threat = null;

            foreach (var other in _visibleActors)
                if (other.Side != Actor.Side)
                {
                    var distance = Vector3.Distance(other.transform.position, transform.position);

                    if (distance < minDistance || threat == null)
                    {
                        threat = other;
                        minDistance = distance;
                    }
                }

            if (threat != null)
                setThreat(threat, threat.transform.position, threat.Cover);
            else if (tryInvestigate())
                setState(FighterState.investigate);
            else if (trySearch())
                setState(FighterState.search);
            else
                setState(FighterState.standAndFight);
        }

        /// <summary>
        /// Notified that the waypoint system has waypoints to visit.
        /// </summary>
        public void OnWaypointsFound()
        {
            if (_state == FighterState.patrol && _isInDarkness)
                Message("ToLight");
        }

        /// <summary>
        /// Event called during a spawning process.
        /// </summary>
        public void OnSpawn(Actor caller)
        {
            var brain = caller != null ? caller.GetComponent<BaseBrain>() : null;

            if (brain != null)
                setThreat(false, false, true, brain.Threat, brain.LastKnownThreatPosition, brain.ThreatCover, brain.LastSeenThreatTime);
            else if (caller != null)
                setThreat(false, false, false, null, caller.transform.position, null, Time.timeSinceLevelLoad);
            else if (trySearch())
                setState(FighterState.search);
            else
                setState(FighterState.patrol);
        }

        /// <summary>
        /// One of the components declares a need for a light.
        /// </summary>
        public void OnNeedLight()
        {
            _isInDarkness = true;

            if (_state == FighterState.patrol || _state == FighterState.investigate || _state == FighterState.search)
                Message("ToLight");
            else
                Message("ToTurnOnLight");
        }

        /// <summary>
        /// One of the components declares that a light is no longer needed.
        /// </summary>
        public void OnDontNeedLight()
        {
            _isInDarkness = false;

            if (_state == FighterState.patrol || _state == FighterState.investigate || _state == FighterState.search)
                Message("ToHideFlashlight");
            else
                Message("ToUnlight");
        }

        /// <summary>
        /// Registers a successful call.
        /// </summary>
        public void OnCallMade()
        {
            if (_state == FighterState.call)
                setState(_previousState);
        }

        /// <summary>
        /// Registers damage done by a weapon.
        /// </summary>
        public void OnHit(Hit hit)
        {
            if (hit.Attacker != null)
            {
                var threat = hit.Attacker.GetComponent<Actor>();

                if (threat != null && threat.Side != Actor.Side)
                {
                    if (_visibleActors.Contains(threat))
                        setThreat(threat, threat.transform.position, threat.Cover);
                    else
                        guessThreat(threat, threat.transform.position);
                }
            }

            if (_health != null && _health.Health <= Retreat.Health + float.Epsilon && _state != FighterState.flee)
            {
                if (Actor.Cover == null)
                    setState(FighterState.retreatToCover);
                else if (_state == FighterState.hideInCover && _stateTime > 0.5f)
                    setState(FighterState.fightInCover);
                else if (_state == FighterState.fightInCover && _stateTime > 0.5f)
                    setState(FighterState.hideInCover);
            }
        }

        /// <summary>
        /// Notified by cover AI that target cover was taken.
        /// </summary>
        public void OnFinishTakeCover()
        {
            switch (_state)
            {
                case FighterState.takeCover:
                case FighterState.switchCover:
                    setState(FighterState.fightInCover);
                    break;

                case FighterState.retreatToCover:
                    setState(FighterState.hideInCover);
                    break;
            }
        }

        /// <summary>
        /// Notified by an alert system of an alert.
        /// </summary>
        public void OnAlert(GeneratedAlert alert)
        {
            if (alert.Actor != null && alert.Actor.gameObject != gameObject)
            {
                if (alert.Actor.Side != Actor.Side)
                {
                    if (Threat == null ||
                        (alert.Actor == Threat && !CanSeeTheThreat && alert.IsDirect) ||
                        (InvestigationWait < ThreatAge && !CanSeeTheThreat))
                        guessThreat(alert.Actor, alert.Position);
                }
                else
                {
                    if (Threat == null || InvestigationWait < ThreatAge)
                    {
                        var brain = alert.Actor.GetComponent<BaseBrain>();

                        if (brain != null)
                        {
                            if (brain.Threat != null && brain.CanSeeTheThreat)
                                setUnseenThreat(false, false, brain.Threat, alert.Position, null);
                        }
                        else if (alert.IsHostile)
                            setUnseenThreat(false, false, null, alert.Position, null);
                    }
                }
            }
        }

        /// <summary>
        /// Notified by communication AI that a friend was found.
        /// </summary>
        public void OnFoundFriend(Actor friend)
        {
            var brain = friend.GetComponent<BaseBrain>();

            if (brain != null && !_friends.Contains(brain))
            {
                _friends.Add(brain);

                if (brain.Threat != null)
                    OnFriendFoundEnemy(friend);
            }
        }

        /// <summary>
        /// Notified by communication AI that a friend got out of range.
        /// </summary>
        public void OnLostFriend(Actor friend)
        {
            var brain = friend.GetComponent<BaseBrain>();

            if (brain != null && _friends.Contains(brain))
                _friends.Remove(brain);
        }

        /// <summary>
        /// Notified that a civilian is alerted.
        /// </summary>
        public void OnCivilianAlerted(Actor actor)
        {
            if (Threat == null)
            {
                var brain = actor.GetComponent<BaseBrain>();

                if (brain != null)
                    setThreat(false, false, false, brain.Threat, actor.transform.position, null, Time.timeSinceLevelLoad);
            }
        }

        /// <summary>
        /// Notified by a friend that they found a new enemy position.
        /// </summary>
        public void OnFriendFoundEnemy(Actor friend)
        {
            if (friend == null || friend.Side != Actor.Side)
                return;

            var brain = friend.GetComponent<FighterBrain>();
            if (brain == null)
                return;

            if (Threat != null && CanSeeTheThreat)
                return;

            var isOk = false;

            if (Threat == null)
                isOk = true;
            else if (!HasSeenTheEnemy && brain.HasSeenTheEnemy)
                isOk = true;
            else if (!HasSeenTheEnemy || brain.HasSeenTheEnemy)
            {
                if ((InvestigationWait < ThreatAge && brain.InvestigationWait > brain.ThreatAge) ||
                    (Threat == brain.Threat && ThreatAge > brain.ThreatAge + 0.01f))
                    isOk = true;
            }

            if (isOk)
                setThreat(false, brain.IsActualThreatPosition, brain.IsActualThreatPosition, brain.Threat, brain.LastKnownThreatPosition, brain.ThreatCover, brain.LastSeenThreatTime);
        }

        /// <summary>
        /// Notified by a friend that the AI is seen by them.
        /// </summary>
        public void OnSeenByFriend(Actor friend)
        {
            if (!_friendsThatCanSeeMe.Contains(friend))
                _friendsThatCanSeeMe.Add(friend);
        }

        /// <summary>
        /// Notified by a friend that the AI is no longer visible by them.
        /// </summary>
        public void OnUnseenByFriend(Actor friend)
        {
            if (_friendsThatCanSeeMe.Contains(friend))
                _friendsThatCanSeeMe.Remove(friend);
        }


        /// <summary>
        /// Notified by the sight AI that an actor has entered the view.
        /// </summary>
        public void OnSeeActor(Actor actor)
        {
            _visibleActors.Add(actor);

            if (actor.Side == Actor.Side)
            {
                if (actor.IsAggressive)
                    actor.SendMessage("OnSeenByFriend", Actor);
                else
                    _visibleCivilians.Add(actor);
            }
            else if (Threat == null || InvestigationWait < ThreatAge || Threat == actor)
                setThreat(actor, actor.transform.position, actor.Cover);
        }

        /// <summary>
        /// Notified by the sight AI that an actor has dissappeared from the view.
        /// </summary>
        public void OnUnseeActor(Actor actor)
        {
            _visibleActors.Remove(actor);

            if (actor.Side == Actor.Side)
            {
                if (actor.IsAggressive)
                    actor.SendMessage("OnUnseenByFriend", Actor);
                else
                    _visibleCivilians.Remove(actor);
            }
            else if (Threat == actor)
            {
                UnseeThreat();

                if (_state == FighterState.standAndFight)
                {
                    if (ThreatCover == null && tryInvestigate())
                        setState(FighterState.investigate);
                    else
                        takeCoverOrAssault();
                }
            }
        }

        /// <summary>
        /// Notified by the cover AI that the current cover is no longer valid.
        /// </summary>
        public void OnInvalidCover()
        {
            if (_state == FighterState.takeCover || _state == FighterState.fightInCover)
                setState(FighterState.takeCover, true);
            else if (_state == FighterState.retreatToCover || _state == FighterState.hideInCover)
                setState(FighterState.retreatToCover, true);
        }

        /// <summary>
        /// Notified by the cover AI that a cover was found.
        /// </summary>
        public void OnFoundCover()
        {
            _hasFoundCover = true;
        }

        /// <summary>
        /// Notified by the movement AI that circling of an enemy is no longer viable.
        /// </summary>
        public void OnCircleFail()
        {
            if (_state == FighterState.circle)
            {
                if (tryInvestigate())
                    setState(FighterState.investigate);
                else
                    takeCoverOrAssault();
            }
        }

        /// <summary>
        /// Notified by the movement AI that a position can no longer be retreated from.
        /// </summary>
        public void OnMoveFromFail()
        {
            if (_state == FighterState.avoidAndFight)
            {
                setState(FighterState.standAndFight);
                _failedToAvoid = true;
            }
        }

        /// <summary>
        /// Notified by the search AI that a position has been investigated.
        /// </summary>
        /// <param name="position"></param>
        public void OnPointInvestigated(Vector3 position)
        {
            if (_state == FighterState.investigate && Vector3.Distance(LastKnownThreatPosition, position) < 0.5f)
            {
                if (trySearch())
                    setState(FighterState.search);
                else
                    setState(FighterState.standAndFight);
            }
        }

        #endregion

        #region Behaviour

        private void Awake()
        {
            Actor.IsAggressive = true;

            _health = GetComponent<CharacterHealth>();
            _motor = GetComponent<CharacterMotor>();

            switch (Start.Mode)
            {
                case AIStartMode.idle:
                    _futureSetState = FighterState.idle;
                    break;

                case AIStartMode.patrol:
                    _futureSetState = FighterState.patrol;
                    break;

                case AIStartMode.searchAround:
                case AIStartMode.searchPosition:
                    _futureSetState = FighterState.search;
                    break;

                case AIStartMode.investigate:
                    _futureSetState = FighterState.investigate;
                    break;
            }
        }

        private void Update()
        {
            if (Actor == null || !Actor.IsAlive)
                return;

            _stateTime += Time.deltaTime;

            if (_futureSetState != FighterState.none)
            {
                var state = _futureSetState;
                _futureSetState = FighterState.none;
                setState(state);
            }

            if (Threat == null)
                foreach (var civilian in _visibleCivilians)
                    if (civilian.IsAlerted)
                    {
                        OnCivilianAlerted(civilian);
                        break;
                    }

            if (Threat != null && CanSeeTheThreat)
                setThreat(Threat, Threat.transform.position, Threat.Cover);

            if (DebugThreat && Threat != null)
                Debug.DrawLine(transform.position, LastKnownThreatPosition, Color.cyan);

            findGrenades();

            switch (_state)
            {
                case FighterState.none:
                    setState(FighterState.patrol);
                    break;

                case FighterState.idle:
                    break;

                case FighterState.patrol:
                    break;

                case FighterState.search:
                    break;

                case FighterState.investigate:
                    break;

                case FighterState.avoidGrenade:
                    if (_stateTime > GrenadeAvoidance.Duration)
                        setState(FighterState.retreatToCover);
                    else
                        Message("ToSprintFrom", _grenadePosition);
                    break;

                case FighterState.standAndFight:
                    if (_stateTime > StandDuration)
                        takeCoverOrAssault();
                    else
                    {
                        turnAndAimAtTheThreat();

                        if (!_failedToAvoid)
                            checkAvoidanceAndSetTheState();
                    }

                    checkInvestigationAndSetTheState(true, true);
                    checkAndThrowGrenade();
                    break;

                case FighterState.maintainPosition:
                    _regrouper = null;

                    if (!_hasReachedMaintainPosition)
                        if (Vector3.Distance(transform.position, _maintainPosition) < 1)
                        {
                            _maintainPositionReachTime = _stateTime;
                            _hasReachedMaintainPosition = true;
                            Message("ToCrouch");
                        }

                    Debug.DrawLine(transform.position, _maintainPosition, Color.red);

                    if (_hasReachedMaintainPosition && (_stateTime > _maintainDuration + _maintainPositionReachTime))
                        takeCoverOrAssault();
                    else
                    {
                        turnAndAimAtTheThreat();

                        if (!_failedToAvoid)
                            checkAvoidanceAndSetTheState();
                    }

                    checkAndThrowGrenade();
                    break;

                case FighterState.takeCover:
                case FighterState.switchCover:
                    turnAndAimAtTheThreat();
                    checkAvoidanceAndSetTheState();
                    checkInvestigationAndSetTheState(false, true);
                    break;

                case FighterState.retreatToCover:
                    turnAndAimAtTheThreat();
                    checkAvoidanceAndSetTheState();
                    break;

                case FighterState.fightInCover:
                    turnAndAimAtTheThreat();

                    if (Actor.Cover == null)
                        setState(FighterState.takeCover);
                    else if (_stateTime > CoverSwitchWait)
                        setState(FighterState.switchCover);

                    checkAvoidanceAndSetTheState();
                    checkInvestigationAndSetTheState(true, true);
                    checkAndThrowGrenade();
                    break;

                case FighterState.hideInCover:
                    if (!checkAvoidanceAndSetTheState())
                        if (_stateTime > Retreat.HideDuration)
                            setState(FighterState.fightInCover);
                    break;

                case FighterState.avoidAndFight:
                    turnAndAimAtTheThreat();

                    if (_stateTime > 2 && !checkAvoidanceAndSetTheState())
                        setState(FighterState.standAndFight);

                    checkInvestigationAndSetTheState(true, true);
                    break;

                case FighterState.circle:
                    turnAndAimAtTheThreat();
                    checkAvoidanceAndSetTheState();
                    checkInvestigationAndSetTheState(true, true);
                    checkFollowAndSetTheState();
                    break;

                case FighterState.assault:
                    turnAndAimAtTheThreat();
                    checkAvoidanceAndSetTheState();
                    checkInvestigationAndSetTheState(true, false);
                    break;
            }
        }

        #endregion

        #region State

        private void setState(FighterState state, bool forceRestart = false)
        {
            if (_state == state && !forceRestart)
                return;

            if (_state != state)
                _previousState = _state;

            _failedToAvoid = false;

            closeState(_state, state);
            _stateTime = 0;
            _state = state;
            openState(_state, _previousState);

            if (!_wasAlerted && IsAlerted)
            {
                _wasAlerted = true;
                Message("OnAlerted");
            }
        }

        private void openState(FighterState state, FighterState previous)
        {
            switch (state)
            {
                case FighterState.none:
                case FighterState.idle:
                    Message("ToDisarm");
                    Message("ToStopMoving");
                    break;

                case FighterState.flee:
                    if (Speed.Enabled) _motor.Speed = Speed.Flee;
                    Message("ToCloseFire");
                    Message("ToLeaveCover");
                    Message("ToStartFleeing", LastKnownThreatPosition);
                    Message("OnScared");
                    alarm();
                    break;

                case FighterState.patrol:
                    if (Speed.Enabled) _motor.Speed = Speed.Patrol;

                    if (Actor.Cover != null)
                        Message("ToLeaveCover");

                    Message("ToDisarm");
                    Message("ToStartVisitingWaypoints");

                    break;

                case FighterState.standAndFight:
                    Message("ToStopMoving");
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.maintainPosition:
                    _hasReachedMaintainPosition = false;
                    Message("ToRunTo", _maintainPosition);
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.takeCover:
                case FighterState.retreatToCover:
                    if (Speed.Enabled)
                    {
                        if (state == FighterState.takeCover)
                            _motor.Speed = Speed.TakeCover;
                        else
                            _motor.Speed = Speed.RetreatToCover;
                    }

                    Message("ToRunToCovers");

                    _hasFoundCover = false;

                    if (_regrouper != null)
                        Message("ToTakeCoverCloseTo", _regrouper);
                    else if (Threat != null)
                        Message("ToTakeCoverAgainst", LastKnownThreatPosition);
                    else
                        Message("ToTakeCover");

                    if (_hasFoundCover)
                    {
                        Message("ToArm");
                        Message("ToOpenFire");
                    }
                    else if (_regrouper != null)
                        ToFindAndMaintainPosition(_regrouper);
                    else if (tryAssault())
                        setState(FighterState.assault);
                    else
                        setState(FighterState.circle);

                    _regrouper = null;
                    alarm();
                    break;

                case FighterState.switchCover:
                    if (Speed.Enabled) _motor.Speed = Speed.SwitchCover;
                    _hasFoundCover = false;
                    Message("ToRunToCovers");
                    Message("ToSwitchCover");

                    if (_hasFoundCover)
                    {
                        Message("ToArm");
                        Message("ToOpenFire");
                        Message("OnCoverSwitch");
                    }
                    else
                    {
                        if (Actor.Cover != null)
                            setState(FighterState.fightInCover);
                        else if (CanSeeTheThreat)
                            setState(FighterState.standAndFight);
                        else if (tryInvestigate())
                            setState(FighterState.investigate);
                        else
                            setState(FighterState.takeCover);
                    }
                    break;

                case FighterState.investigate:
                    if (Speed.Enabled) _motor.Speed = Speed.Investigate;

                    if (!HasSeenTheEnemy && Start.Mode == AIStartMode.investigate)
                        SetThreat(false, false, null, Start.Position, null, 0);

                    Message("ToArm");
                    
                    if (_isInDarkness)
                        Message("ToLight");

                    turnAndAimAtTheThreat();
                    Message("ToStartAiming");
                    Message("ToCloseFire");

                    if (ThreatCover != null)
                    {
                        if (IsActualThreatPosition && Threat != null && HasSeenTheEnemy)
                        {
                            Message("ToHideFlashlight");
                            Message("ToArm");
                            Message("ToOpenFire");
                        }

                        Message("ToInvestigatePosition", LastKnownThreatPosition);
                    }
                    else if (HasSeenTheEnemy || Start.Mode == AIStartMode.investigate)
                        Message("ToInvestigatePosition", LastKnownThreatPosition);
                    else if (trySearch())
                        setState(FighterState.search);
                    else
                        setState(FighterState.standAndFight);
                    break;

                case FighterState.fightInCover:
                    if (Actor.Cover == null)
                        setState(FighterState.takeCover);
                    else
                    {
                        Message("ToArm");
                        turnAndAimAtTheThreat();
                        Message("ToOpenFire");
                        alarm();
                    }
                    break;

                case FighterState.hideInCover:
                    Message("ToCloseFire");
                    Message("ToStopAiming");

                    if (Actor.Cover == null)
                        setState(FighterState.retreatToCover);
                    break;

                case FighterState.avoidAndFight:
                    if (Speed.Enabled) _motor.Speed = Speed.Avoid;
                    Message("ToRunFrom", LastKnownThreatPosition);
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    alarm();
                    break;

                case FighterState.avoidGrenade:
                    if (Speed.Enabled) _motor.Speed = Speed.Avoid;
                    Message("ToCloseFire");
                    Message("ToSprintFrom", _grenadePosition);
                    alarm();
                    break;

                case FighterState.circle:
                    if (Speed.Enabled) _motor.Speed = Speed.Circle;
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    Message("ToCircle", LastKnownThreatPosition);
                    alarm();
                    break;

                case FighterState.assault:
                    if (Speed.Enabled) _motor.Speed = Speed.Assault;
                    Message("ToArm");
                    turnAndAimAtTheThreat();
                    Message("ToOpenFire");
                    Message("ToStartAssault", LastKnownThreatPosition);
                    alarm();
                    break;

                case FighterState.search:
                    if (Speed.Enabled) _motor.Speed = Speed.Search;

                    if (Threat == null)
                    {
                        if (previous == FighterState.none && Start.Mode == AIStartMode.searchPosition)
                            Message("ToSearchAt", new SearchPoint(Start.Position, (transform.position - Start.Position).normalized, false));
                        else
                            Message("ToSearch");
                    }
                    else
                        Message("ToSearchAt", new SearchPoint(LastKnownThreatPosition, ThreatCover != null ? (-ThreatCover.Forward) : (transform.position - LastKnownThreatPosition).normalized, ThreatCover == null));

                    Message("ToArm");

                    if (_isInDarkness)
                        Message("ToLight");

                    Message("ToStartAiming");
                    break;

                case FighterState.call:
                    Message("ToHideFlashlight");
                    Message("ToDisarm");
                    Message("ToTakeRadio");
                    Message("ToCall");
                    break;
            }

            if (IsAlerted && _isInDarkness)
                Message("ToTurnOnLight");
        }

        private void closeState(FighterState state, FighterState next)
        {
            switch (state)
            {
                case FighterState.search:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopSearch");
                    break;

                case FighterState.avoidGrenade:
                    Message("ToStopMoving");
                    break;

                case FighterState.maintainPosition:
                    Message("ToStopCrouching");
                    break;

                case FighterState.flee:
                    Message("ToStopFleeing");
                    break;

                case FighterState.patrol:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopVisitingWaypoints");
                    break;

                case FighterState.takeCover:
                case FighterState.switchCover:
                case FighterState.retreatToCover:
                case FighterState.fightInCover:
                    if (next != FighterState.fightInCover)
                        Message("ToLeaveCover");
                    break;

                case FighterState.investigate:
                    if (_isInDarkness)
                        Message("ToHideFlashlight");

                    Message("ToStopInvestigation");
                    break;

                case FighterState.call:
                    Message("ToHideRadio");
                    break;

                case FighterState.assault:
                    Message("ToStopAssault");
                    break;
            }

            switch (state)
            {
                case FighterState.fightInCover:
                case FighterState.standAndFight:
                case FighterState.maintainPosition:
                case FighterState.circle:
                case FighterState.investigate:
                case FighterState.assault:
                    Message("ToCloseFire");
                    break;
            }
        }

        #endregion

        #region State checks

        private void checkFollowAndSetTheState()
        {
            if (Threat == null)
                return;

            if (Vector3.Distance(transform.position, LastKnownThreatPosition) > FollowDistance)
                takeCoverOrAssault();
        }

        private void checkInvestigationAndSetTheState(bool checkVisibility, bool checkTime)
        {
            if (Threat == null)
                return;

            if ((checkTime && InvestigationWait < ThreatAge) ||
                (checkVisibility && !CanSeeTheThreat && ThreatCover == null && Vector3.Distance(transform.position, LastKnownThreatPosition) > FollowDistance))
                if (tryInvestigate())
                {
                    Message("ToClearSearchHistory");
                    setState(FighterState.investigate);
                }
        }

        private void checkAndThrowGrenade()
        {
            if (Threat == null || InvestigationWait < ThreatAge || _thrownGrenadeCount >= Grenades.GrenadeCount)
                return;

            if (!CanSeeTheThreat && ThreatCover == null && !_isInDarkness)
                return;

            var doThrow = false;

            if (_hasThrowFirstGrenade)
            {
                if (_grenadeTimer < Grenades.Interval)
                    _grenadeTimer += Time.deltaTime;
                else
                    doThrow = true;
            }
            else
            {
                if (_grenadeTimer < Grenades.FirstCheckDelay)
                    _grenadeTimer += Time.deltaTime;
                else
                    doThrow = true;
            }

            if (doThrow && _motor.PotentialGrenade != null)
            {
                if (_grenadeCheckTimer <= float.Epsilon)
                {
                    GrenadeDescription desc;
                    desc.Gravity = _motor.Grenade.Gravity;
                    desc.Duration = _motor.PotentialGrenade.Timer;
                    desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                    var length = GrenadePath.Calculate(GrenadePath.Origin(_motor, Util.AngleOfVector(LastKnownThreatPosition - transform.position)),
                                                       LastKnownThreatPosition,
                                                       _motor.Grenade.MaxVelocity,
                                                       desc,
                                                       _grenadePath,
                                                       _motor.Grenade.Step);

                    if (Vector3.Distance(_grenadePath[length - 1], LastKnownThreatPosition) > Grenades.MaxRadius ||
                        Vector3.Distance(_grenadePath[length - 1], transform.position) < Grenades.AvoidDistance)
                        _grenadeCheckTimer = Grenades.CheckInterval;
                    else
                    {
                        _motor.InputThrowGrenade(_grenadePath, length, _motor.Grenade.Step);
                        _thrownGrenadeCount++;

                        _grenadeTimer = 0;
                        _hasThrowFirstGrenade = true;
                    }
                }
                else
                    _grenadeCheckTimer -= Time.deltaTime;
            }
            else
                _grenadeCheckTimer = 0;
        }

        private void findGrenades()
        {
            foreach (var grenade in GrenadeList.All)
                if (Vector3.Distance(grenade.transform.position, transform.position) < grenade.ExplosionRadius)
                {
                    _grenadeReaction += Time.deltaTime;

                    if (_grenadeReaction >= GrenadeAvoidance.ReactionTime + float.Epsilon)
                    {
                        _grenadePosition = grenade.transform.position;
                        setState(FighterState.avoidGrenade);
                    }

                    return;
                }

            _grenadeReaction = 0;
        }

        private bool checkAvoidanceAndSetTheState()
        {
            if (Threat == null || !CanSeeTheThreat || Vector3.Distance(LastKnownThreatPosition, transform.position) > AvoidDistance)
                return false;

            setState(FighterState.avoidAndFight);
            return true;
        }

        private void takeCoverOrAssault()
        {
            if (tryAssault())
                setState(FighterState.assault);
            else
                setState(FighterState.takeCover);
        }

        private void alarm()
        {
            if (!_wasAlarmed)
            {
                _wasAlarmed = true;
                Message("OnAlarmed");
            }
        }

        #endregion

        #region Threat

        private void turnAndAimAtTheThreat()
        {
            if (Threat == null)
            {
                Message("ToTurnAt", LastKnownThreatPosition);
                Message("ToAimAt", LastKnownThreatPosition + Vector3.up * 1.0f);
            }
            else if (ThreatCover == null || Vector3.Dot(ThreatCover.Forward, LastKnownThreatPosition - transform.position) > 0)
                Message("ToTarget", new ActorTarget(LastKnownThreatPosition, Threat.RelativeTopPosition));
            else
                Message("ToTarget", new ActorTarget(LastKnownThreatPosition, Threat.RelativeStandingTopPosition));
        }

        private void guessThreat(Actor threat, Vector3 position)
        {
            var error = Approximation.Get(Vector3.Distance(transform.position, position));

            if (error < 0.25f)
                setUnseenThreat(true, false, threat, position, null);
            else
            {
                var normal = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
                var distance = UnityEngine.Random.Range(error * 0.25f, error);

                if (Vector3.Distance(LastKnownThreatPosition, position) > distance)
                    setUnseenThreat(false, false, threat, position + normal * distance, null);
            }
        }

        private void setUnseenThreat(bool isDirect, bool isSeenByFriend, Actor threat, Vector3 position, Cover threatCover)
        {
            setThreat(false, isSeenByFriend, isDirect, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        private void setThreat(Actor threat, Vector3 position, Cover threatCover)
        {
            setThreat(true, false, true, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        private void setThreat(bool isVisible, bool isVisibleByFriends, bool isActual, Actor threat, Vector3 position, Cover threatCover, float time)
        {
            var previousThreat = Threat;
            var wasVisible = CanSeeTheThreat;

            SetThreat(isVisible, isActual, threat, position, threatCover, time);

            if (CanSeeTheThreat && Threat != null)
                if (!wasVisible || previousThreat != Threat)
                {
                    foreach (var friend in _friendsThatCanSeeMe)
                        friend.SendMessage("OnFriendFoundEnemy", Actor);

                    foreach (var friend in _friends)
                        if (!_friendsThatCanSeeMe.Contains(friend.Actor))
                            friend.Message("OnFriendFoundEnemy", Actor);
                }

            if (_state == FighterState.investigate)
            {
                if (isActual)
                {
                    if (isVisible)
                        setState(FighterState.standAndFight);
                    else if (isVisibleByFriends)
                        takeCoverOrAssault();
                    else if (tryInvestigate())
                        setState(FighterState.investigate, true);
                    else
                        takeCoverOrAssault();
                }
                else if (tryInvestigate())
                    setState(FighterState.investigate, true);
                else
                    takeCoverOrAssault();
            }
            else if (_state == FighterState.search || !IsAlerted)
            {
                if (isVisible)
                    setState(FighterState.standAndFight);
                else if (isVisibleByFriends)
                    takeCoverOrAssault();
                else if (tryInvestigate())
                    setState(FighterState.investigate);
                else
                    takeCoverOrAssault();
            }
        }

        #endregion
    }
}
