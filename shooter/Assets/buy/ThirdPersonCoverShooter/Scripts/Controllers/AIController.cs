using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Makes decisions based on the gameplay situation and gives input to CharacterMotor.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Actor))]
    public class AIController : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// All the alerted AI.
        /// </summary>
        public static IEnumerable<AIController> AlertedAI
        {
            get { return _alertedControllers; }
        }

        /// <summary>
        /// Returns true if the AI is thinking and acting.
        /// </summary>
        public bool IsAlerted
        {
            get { return _situation.IsAlerted; }
        }

        /// <summary>
        /// Current AI situation.
        /// </summary>
        public AISituation Situation
        {
            get { return _situation; }
        }

        /// <summary>
        /// Current AI state.
        /// </summary>
        public AIState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Reason the AI is in the current state.
        /// </summary>
        public AIStateReason StateReason
        {
            get { return _stateReason; }
        }

        /// <summary>
        /// Motor controlled by the AI.
        /// </summary>
        public CharacterMotor Motor
        {
            get { return _motor; }
        }

        /// <summary>
        /// Health component attached to the object. Can be null.
        /// </summary>
        public CharacterHealth Health
        {
            get { return _health; }
        }

        /// <summary>
        /// Actor component attached to the object.
        /// </summary>
        public Actor Actor
        {
            get { return _actor; }
        }

        /// <summary>
        /// Navmesh agent attached to the object.
        /// </summary>
        public NavMeshAgent Agent
        {
            get { return _agent; }
        }

        /// <summary>
        /// Is AI currently intending to aim.
        /// </summary>
        public bool IsAiming
        {
            get { return _aim.Target; }
        }

        /// <summary>
        /// Is AI currently intending to sprint.
        /// </summary>
        public bool IsSprinting
        {
            get { return _sprint.Target; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Group the AI belongs to. Groups manage AI group behaviour.
        /// </summary>
        [Tooltip("Group the AI belongs to. Groups manage AI group behaviour.")]
        public AIGroup Group;

        /// <summary>
        /// Settings for AI behaviour.
        /// </summary>
        [Tooltip("Settings for AI behaviour.")]
        public AIBehaviourSettings Behaviour = AIBehaviourSettings.Default();

        /// <summary>
        /// Settings for AI patrol.
        /// </summary>
        [Tooltip("Settings for AI patrol.")]
        public AIPatrolSettings Patrol = AIPatrolSettings.Default();

        /// <summary>
        /// Settings for AI fighting and aiming.
        /// </summary>
        [Tooltip("Settings for AI fighting and aiming.")]
        public AIFightingSettings Fighting = AIFightingSettings.Default();

        /// <summary>
        /// Settings for AI grenades.
        /// </summary>
        [Tooltip("Settings for AI fighting and aiming.")]
        public AIGrenadeSettings Grenades = AIGrenadeSettings.Default();

        /// <summary>
        /// Settings for distances AI tries to maintain.
        /// </summary>
        [Tooltip("Settings for distances AI tries to maintain.")]
        public AIDistanceSettings Distances = AIDistanceSettings.Default();

        /// <summary>
        /// AI cover settings.
        /// </summary>
        [Tooltip("AI cover settings.")]
        public AICoverSettings Cover = AICoverSettings.Default();

        /// <summary>
        /// Settings for AI to notice changes in the world.
        /// </summary>
        [Tooltip("Settings for AI to notice changes in the world.")]
        public AIViewSettings View = AIViewSettings.Default();

        /// <summary>
        /// Settings for bursts of fire when not walking in a cover.
        /// </summary>
        [Tooltip("Settings for bursts of fire when not walking in a cover.")]
        public Bursts WalkingBursts = Bursts.Default();

        /// <summary>
        /// Settings for bursts of fire when approaching but not intending to fight in covers.
        /// </summary>
        [Tooltip("Settings for bursts of fire when approaching but not intending to fight in covers.")]
        public AICoverBurstSettings CoveredApproachBursts = AICoverBurstSettings.DefaultApproach();

        /// <summary>
        /// Settings for bursts of fire when fighting in covers.
        /// </summary>
        [Tooltip("Settings for bursts of fire when fighting in covers.")]
        public AICoverBurstSettings CoveredFightingBursts = AICoverBurstSettings.DefaultCovered();

        /// <summary>
        /// Points to visit when patrolling.
        /// </summary>
        [Tooltip("Points to visit when patrolling.")]
        [HideInInspector]
        public Waypoint[] Waypoints;

        #endregion

        #region Private fields

        private CharacterMotor _motor;
        private CharacterHealth _health;
        private NavMeshAgent _agent;
        private Actor _actor;

        private AIGroup _registeredGroup;

        private float _updateTimer = 0;
        private AIState _state;
        private AIStateReason _stateReason;
        private AISituation _previousSituation;
        private AISituation _situation;
        private float _stateTime;        

        private float _walkingBurstWait = 0;

        private int _lastLookPoint = 0;
        private float _lookPointTimer = 0;

        private Vector3[] _path = new Vector3[64];
        private int _pathLength;
        private int _currentPathIndex;
        private bool _isWaitingForPath;

        private float _grenadeTimer;
        private float _grenadeCheckTimer;
        private bool _hasThrowFirstGrenade;
        private Vector3[] _grenadePath = new Vector3[128];

        private SustainedValue _aim;
        private SustainedValue _sprint;

        private Cover _lastTargetCover;

        private float _aimAngle;

        private static List<AIController> _alertedControllers = new List<AIController>();
        private bool _hasAddedToAlertedControllers;

        private bool _wasJustHit;

        #endregion

        #region Events

        /// <summary>
        /// React to attacks and notify other AI about it.
        /// </summary>
        public void OnHit(Hit hit)
        {
            _situation.IsAlerted = true;

            if (hit.Attacker != null)
            {
                var threat = hit.Attacker.GetComponent<Actor>();

                if (threat != null && threat.Side != _actor.Side)
                {
                    _wasJustHit = true;
                    _situation.ReadEnemyState(threat);
                }
            }

            AIUtil.NotifyFriends(_actor, "OnFriendHit", _actor);
        }

        /// <summary>
        /// React to attacks on other AI.
        /// </summary>
        public void OnFriendHit(Actor friend)
        {
            if (friend == null)
                return;

            var vector = friend.transform.position - transform.position;

            if (vector.magnitude < View.CommunicationDistance)
                _situation.IsAlerted = true;
            else if (vector.magnitude < View.SightDistance(IsAlerted) && !IsAlerted)
                if (AIUtil.IsInSight(this, friend.TopPosition))
                    _situation.IsAlerted = true;

            if (_situation.IsAlerted && _situation.Threat == null)
            {
                var ai = friend.GetComponent<AIController>();

                if (ai != null)
                    _situation.TakeEnemyState(ai);
            }
        }

        #endregion

        #region Behaviour

        private void OnDisable()
        {
            unregister();
        }

        private void OnDestroy()
        {
            unregister();
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _motor = GetComponent<CharacterMotor>();
            _health = GetComponent<CharacterHealth>();
            _actor = GetComponent<Actor>();
        }

        private void Update()
        {
            if (_situation.IsAlerted)
            {
                if (!_hasAddedToAlertedControllers)
                {
                    _hasAddedToAlertedControllers = true;
                    _alertedControllers.Add(this);
                }
            }
            else if (_hasAddedToAlertedControllers)
                _alertedControllers.Remove(this);

            _stateTime += Time.deltaTime;

            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.autoRepath = true;
            _agent.autoBraking = false;

            var wasJustHit = _wasJustHit;
            _wasJustHit = false;

            _aim.Target = false;
            _sprint.Target = false;

            if (!_motor.IsAlive)
                _situation.TargetCover = null;

            if (_lastTargetCover != _situation.TargetCover)
            {
                if (_lastTargetCover != null)
                    _lastTargetCover.UnregisterUser(_actor);

                _lastTargetCover = _situation.TargetCover;

                if (_lastTargetCover != null)
                    _lastTargetCover.RegisterUser(_actor, _situation.TargetPosition);
            }

            if (_isWaitingForPath && !_agent.pathPending)
            {
                _pathLength = _agent.path.GetCornersNonAlloc(_path);
                _currentPathIndex = 0;
                _isWaitingForPath = false;
            }

            if (!_motor.IsAlive)
            {
                _state = AIState.none;
                _agent.enabled = false;
                return;
            }
            else
                _agent.enabled = true;

            if (_state == AIState.reload)
            {
                // Maintain current cover state
            }
            else if (_state == AIState.fireInCover || _state == AIState.hideInCover)
            {
                if (_motor.PotentialCover != null)
                    _motor.InputTakeCover();
            }
            else if (AIUtil.IsTakeCoverState(_state))
            {
                if (_motor.IsCornerAiming || (_motor.IsInTallCover && _motor.Cover != _situation.TargetCover))
                    _motor.InputLeaveCover();
                else
                {
                    if (_motor.Cover != _situation.TargetCover && Vector3.Distance(_situation.CurrentPosition, _situation.TargetPosition) < 1)
                        _motor.InputImmediateCoverSearch();

                    _motor.InputTakeCover();

                    if (_motor.Cover == _situation.TargetCover)
                        _sprint.Current = false;
                }
            }
            else
                _motor.InputLeaveCover();

            if (_situation.IsAlerted)
            {
                if (!_motor.IsInCover)
                    _motor.InputAim();

                // Make the AI pick up a weapon.
                if (Fighting.WeaponToUse <= 0)
                    _motor.InputWeapon(0);
                else if (Fighting.WeaponToUse - 1 < _motor.Weapons.Length)
                    _motor.InputWeapon(Fighting.WeaponToUse);
                else
                    _motor.InputWeapon(_motor.Weapons.Length);

                if (_motor.Cover == null && Behaviour.AlwaysAim)
                    _motor.InputAim();

                _situation.IsAllowedToBeAggressive = !_situation.WouldLikeToRetreat && _stateReason != AIStateReason.cantSeeAndCantFindABetterCover;

                ///
                /// POTENTIALLY FORGET ABOUT THE ENEMY. CHECK IF CAN BE AGGRESSIVE.
                ///
                if (_situation.Threat != null)
                {
                    if (_situation.Threat.IsAlive)
                    {
                        if (_registeredGroup != null && _situation.IsAllowedToBeAggressive)
                        {
                            _registeredGroup.MarkAsPotentialAggressive(this, wasJustHit);
                            _situation.IsAllowedToBeAggressive = _registeredGroup.IsAggressive(this);
                        }
                    }
                    else
                        _situation.RemoveEnemyState();
                }

                if (!_situation.IsAllowedToBeAggressive)
                {
                    if (_state == AIState.retreat && _stateReason == AIStateReason.couldntFindACoverToRetreatTo)
                        _situation.IsAllowedToBeAggressive = true;
                    else
                        _situation.IsAllowedToBeAggressive = Vector3.Distance(_situation.CurrentPosition, _situation.ThreatGroundPosition) < Distances.MaxFightBack;
                }

                ///
                /// AIM AND FIRE
                /// 
                {
                    var canSeeTheThreat = _situation.Threat != null && (_situation.CanSeeTheThreat || _situation.IsThreatInCover);

                    // Process grenade throwing
                    if (canSeeTheThreat && _situation.ThrownGrenadeCount < Grenades.GrenadeCount)
                    {
                        var doThrow = false;

                        if (_situation.IsAllowedToBeAggressive)
                        {
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
                        }

                        if (doThrow && _motor.PotentialGrenade != null)
                        {
                            if (_grenadeCheckTimer <= float.Epsilon)
                            {
                                GrenadeDescription desc;
                                desc.Gravity = _motor.Grenade.Gravity;
                                desc.Duration = _motor.PotentialGrenade.Timer;
                                desc.Bounciness = _motor.PotentialGrenade.Bounciness;

                                var length = GrenadePath.Calculate(GrenadePath.Origin(_motor, Util.AngleOfVector(_situation.ThreatGroundPosition - transform.position)),
                                                                   _situation.ThreatGroundPosition,
                                                                   _motor.Grenade.MaxVelocity,
                                                                   desc,
                                                                   _grenadePath,
                                                                   _motor.Grenade.Step);

                                if (Vector3.Distance(_grenadePath[length - 1], _situation.ThreatGroundPosition) > Grenades.MaxRadius ||
                                    Vector3.Distance(_grenadePath[length - 1], _situation.CurrentPosition) < Grenades.AvoidDistance)
                                    _grenadeCheckTimer = Grenades.CheckInterval;
                                else
                                {
                                    _motor.InputThrowGrenade(_grenadePath, length, _motor.Grenade.Step);
                                    _situation.ThrownGrenadeCount++;

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

                    // Position in world space that is best fit to aim at.
                    Vector3 perfectTarget;

                    if (_state == AIState.fireInCover)
                    {
                        _aim.Current = true;
                        _aim.Target = true;
                    }
                    else if (_situation.IsAllowedToBeAggressive)
                        _aim.Target = true;
                    else if (!_situation.WouldLikeToRetreat && AIUtil.IsMovingState(_state) && Vector3.Distance(_situation.CurrentPosition, _situation.TargetPosition) < 2)
                        _aim.Target = true;

                    if (_aim.Current)
                    {
                        perfectTarget = _situation.ThreatGroundPosition + (_situation.ThreatTopPosition - _situation.ThreatGroundPosition) * 0.7f;

                        if (_situation.IsThreatInCover)
                        {
                            var dot = Vector3.Dot(_situation.ThreatCoverForward, (_situation.ThreatGroundPosition - transform.position).normalized);

                            if (dot < -0.1f)
                                perfectTarget = _situation.ThreatStandingTopPosition;
                        }
                    }
                    else
                        perfectTarget = _situation.TargetPosition + Vector3.up;

                    var aimTarget = perfectTarget;

                    if (_aim.Current)
                        _aimAngle = Util.AngleOfVector(perfectTarget - _situation.CurrentPosition);
                    else
                    {
                        if (Vector3.Distance(perfectTarget, _situation.CurrentPosition) > 3)
                            _aimAngle = Util.AngleOfVector(perfectTarget - _situation.CurrentPosition);

                        var base_ = transform.eulerAngles.y;
                        var delta = Mathf.DeltaAngle(base_, _aimAngle);

                        if (delta > 45)
                            _aimAngle = base_ + 45;
                        else if (delta < -45)
                            _aimAngle = base_ - 45;

                        aimTarget = _situation.CurrentPosition + Quaternion.AngleAxis(_aimAngle, Vector3.up) * Vector3.forward * 100;
                        aimTarget.y = _situation.CurrentPosition.y + 1.0f;
                    }

                    // Make the AI look at the target.
                    {
                        var vector = aimTarget - _motor.transform.position;
                        vector.y = 0;

                        if (vector.magnitude < 2)
                            _motor.SetLookTarget(aimTarget + vector * 2);
                        else
                            _motor.SetLookTarget(aimTarget);

                        _motor.SetBodyLookTarget(aimTarget);
                    }

                    // Aim the gun at the inprecise target.
                    var targetRadius = Fighting.TargetRadius.Get(Vector3.Distance(_situation.ThreatGroundPosition, transform.position));
                    _motor.SetFireTarget(perfectTarget + new Vector3(UnityEngine.Random.Range(-1, 1) * targetRadius,
                                                                     UnityEngine.Random.Range(-1, 1) * targetRadius,
                                                                      UnityEngine.Random.Range(-1, 1) * targetRadius));

                    // We want AI too look at the corner when standing near it.
                    if (_motor.Cover != null && _situation.CurrentCover == _situation.TargetCover && _state == AIState.fireInCover)
                    {
                        if (_situation.TargetDirection < 0)
                        {
                            _motor.InputStandLeft();

                            if (!_motor.IsNearLeftCorner)
                                _motor.InputMovement(new CharacterMovement(_motor.Cover.Left, 1.0f));
                        }
                        else if (_situation.TargetDirection > 0)
                        {
                            _motor.InputStandRight();

                            if (!_motor.IsNearRightCorner)
                                _motor.InputMovement(new CharacterMovement(_motor.Cover.Right, 1.0f));
                        }
                    }

                    // Fire
                    if (Behaviour.CanFire)
                    {
                        var isAllowed = true;
                        var burst = Behaviour.IsFightingUsingCovers ? CoveredFightingBursts : CoveredApproachBursts;

                        if (_state == AIState.fireInCover)
                        {
                            _motor.InputAim();
                            isAllowed = _stateTime >= burst.IntroDuration && _stateTime <= burst.IntroDuration + burst.Duration;
                        }
                        else
                        {
                            if (_situation.Threat == null || !AIUtil.IsFireState(_state) || !_aim.Current)
                                isAllowed = false;
                            else if (AIUtil.IsMovingState(_state))
                                isAllowed = _walkingBurstWait <= WalkingBursts.Duration || _situation.WouldLikeToRetreat;

                            if (isAllowed)
                            {
                                var aimPoint = transform.position;

                                if (_motor.Cover != null && _motor.CanPeekLeftCorner)
                                    aimPoint = _motor.Cover.LeftCorner(transform.position.y, _motor.CoverSettings.CornerOffset.x);
                                else if (_motor.Cover != null && _motor.CanPeekRightCorner)
                                    aimPoint = _motor.Cover.RightCorner(transform.position.y, _motor.CoverSettings.CornerOffset.x);

                                // See if the motor can hit the standing enemy after potentially peeking.
                                if (!AIUtil.IsInSight(this, aimPoint, _situation.ThreatStandingTopPosition))
                                    isAllowed = false;
                            }
                        }

                        if (isAllowed)
                            _motor.InputFireOnCondition(_actor.Side);

                        _lookPointTimer = 0;
                    }
                }
            }
            else
            {
                _situation.IsAllowedToBeAggressive = false;

                if (Behaviour.AlwaysAim && _motor.Cover == null)
                {
                    // Make the AI pick up a weapon.
                    if (Fighting.WeaponToUse <= 0)
                        _motor.InputWeapon(0);
                    else if (Fighting.WeaponToUse - 1 < _motor.Weapons.Length)
                        _motor.InputWeapon(Fighting.WeaponToUse);
                    else
                        _motor.InputWeapon(_motor.Weapons.Length);

                    _motor.InputAim();
                }
                else
                    _motor.InputWeapon(0);

                if (_state == AIState.patrol || _state == AIState.patrolPause)
                {
                    Vector3 forward;

                    if (Waypoints.Length > 1)
                    {
                        var next = Waypoints[_situation.PatrolPoint];

                        forward = next.Position - transform.position;

                        if (forward.magnitude < 0.5f)
                        {
                            var previous = _situation.PatrolPoint == 0 ? Waypoints[Waypoints.Length - 1] : Waypoints[_situation.PatrolPoint - 1];
                            var line = next.Position - previous.Position;

                            if (Vector2.Dot(line, forward) > 0)
                                forward = line;
                        }
                    }
                    else
                        forward = transform.forward;

                    lookAround(forward);
                }
                else
                    _lookPointTimer = 0;
            }

            var positionToMoveTo = transform.position;

            ///
            /// STATE MANAGEMENT
            /// 
            {
                var updateSituationInDetail = false;

                {
                    // Update the update delay timer.
                    _updateTimer -= Time.deltaTime;

                    if (_updateTimer < 0)
                    {
                        _updateTimer = Fighting.ReactionTime;
                        updateSituationInDetail = true;
                    }
                }

                // Update standing burst timer.
                {
                    _walkingBurstWait += Time.deltaTime;

                    if (_walkingBurstWait > WalkingBursts.Duration + WalkingBursts.Wait)
                        _walkingBurstWait = 0;
                }

                // Try to ask for reload input
                if (_state == AIState.reload && !_motor.IsGunReady)
                    _motor.InputReload();

                {
                    _situation.Update(this, _previousSituation, updateSituationInDetail);

                    var newState = calcState(_situation, _state, _stateReason, _stateTime, wasJustHit);
                    _stateReason = newState.Reason;

                    if (newState.State != _state || newState.ShouldRestart)
                        updateSituation(newState.State);
                }
            }

            ///
            /// WALK
            ///
            if (!_motor.IsInCornerAimState && AIUtil.IsMovingState(_state))
            {
                var toTarget = _situation.TargetPosition - transform.position;
                toTarget.y = 0;

                // If patrolling, set the body to rotate towards the target.
                if (_state == AIState.patrol)
                    if (toTarget.magnitude > 0.1f)
                    {
                        var normalized = toTarget.normalized;
                        var position = transform.position + normalized * 4;
                        var dot = Vector3.Dot(toTarget.normalized, _motor.transform.forward);

                        if (Waypoints[_situation.PatrolPoint].Run)
                            _motor.SetBodyLookTarget(position, 8);
                        else
                        {
                            if (dot > 0)
                                _motor.SetBodyLookTarget(position, 1.6f);
                            else if (dot > -0.5f)
                                _motor.SetBodyLookTarget(position, 2.5f);
                            else
                                _motor.SetBodyLookTarget(position, 4f);
                        }
                    }

                // Move the last meter without using the agent.
                if (toTarget.magnitude >= 0.5f || !_motor.IsFree(toTarget.normalized))
                {
                    var vectorToAgent = _agent.nextPosition - transform.position;
                    var distanceToAgent = vectorToAgent.magnitude;

                    if (distanceToAgent > float.Epsilon)
                        vectorToAgent /= distanceToAgent;

                    // If agent moved too far, reset it.
                    if (distanceToAgent > 1f)
                        updateAgent(_situation.TargetPosition);

                    float walkSpeed;

                    if (_state == AIState.patrol && !Patrol.IsAlwaysRunning && (Waypoints.Length == 0 || !Waypoints[_situation.PatrolPoint].Run))
                        walkSpeed = 0.5f;
                    else
                    {
                        if (Behaviour.CanSprint && !_situation.IsAllowedToBeAggressive && !_aim.Current && Vector3.Distance(_situation.CurrentPosition, _situation.TargetPosition) > 1)
                            _sprint.Target = true;

                        if (_sprint.Current)
                            walkSpeed = 2.0f;
                        else
                            walkSpeed = 1.0f;
                    }

                    var point = _currentPathIndex + 1 < _pathLength ? _path[_currentPathIndex + 1] : _situation.TargetPosition;

                    var vectorToPoint = point - transform.position;
                    var distanceToPoint = vectorToPoint.magnitude;

                    if (distanceToPoint > float.Epsilon)
                        vectorToPoint /= distanceToPoint;

                    if (distanceToPoint < 0.1f && _currentPathIndex + 1 < _pathLength)
                        _currentPathIndex++;

                    _agent.speed = 0.1f;
                    _motor.InputMovement(new CharacterMovement(vectorToPoint, walkSpeed));
                }
                else
                {
                    if (toTarget.magnitude > 0.05f)
                    {
                        if (_motor.IsInCover)
                            _motor.InputMovement(new CharacterMovement(toTarget.normalized, 1.0f));
                        else
                            _motor.InputMovement(new CharacterMovement(toTarget.normalized, 0.5f));
                    }
                    else
                        _motor.transform.position = _situation.TargetPosition;
                }
            }

            _aim.Update(4);
            _sprint.Update(4);
        }

        #endregion

        #region Private methods

        private void unregister()
        {
            if (_registeredGroup != null)
            {
                _registeredGroup.Unregister(this);
                _registeredGroup = null;
            }
        }

        private void register(AIGroup value)
        {
            if (_registeredGroup != value)
            {
                unregister();

                if (value != null)
                {
                    _registeredGroup = value;
                    _registeredGroup.Register(this);
                }
            }
        }

        private void lookAround(Vector3 forward)
        {
            _motor.InputLeaveCover();

            forward.y = 0;
            forward.Normalize();

            _lookPointTimer -= Time.deltaTime;

            if (_lookPointTimer <= float.Epsilon)
            {
                Vector3 vector;
                var timer = Patrol.LookDuration;

                if (_lastLookPoint == 0)
                {
                    vector = forward;
                    _lastLookPoint = 1;
                    timer = 1.0f;
                }
                else if (_lastLookPoint > 0)
                {
                    vector = Quaternion.AngleAxis(Patrol.LookAngle, Vector3.up) * forward;
                    _lastLookPoint = -1;
                }
                else
                {
                    vector = Quaternion.AngleAxis(-Patrol.LookAngle, Vector3.up) * forward;
                    _lastLookPoint = 1;
                }

                var target = transform.position + vector * 1000;
                _motor.SetHeadLookTargetOverride(target, Patrol.LookSpeed);
                _lookPointTimer = timer;
            }
            else if (Vector3.Dot(transform.forward, forward) < 0.5f)
            {
                var target = transform.position + forward * 1000;
                _motor.SetHeadLookTargetOverride(target, Patrol.LookSpeed * 2);
            }
        }

        /// <summary>
        /// Sets a new state and takes necessary actions.
        /// </summary>
        private void updateSituation(AIState state)
        {
            if (_registeredGroup != Group)
            {
                if (Group != null)
                    register(Group);
                else
                    unregister();
            }

            setNewState(state, _situation);

            switch (_state)
            {
                case AIState.approach:
                    if (_situation.IsThreatInCover)
                        Positioning.ApproachACovered(this, ref _situation);
                    else
                        Positioning.ApproachAFree(this, ref _situation, _agent, Distances.MaxWalkingFight, true);
                    break;

                case AIState.retreat:
                    Positioning.Retreat(this, ref _situation, _agent);
                    break;

                case AIState.avoidGrenade:
                    Positioning.AvoidGrenade(this, ref _situation, _agent);
                    break;

                case AIState.follow:
                    _situation.HasInvestigatedTheLatestAlert = false;
                    _situation.IsThreatPositionANewAlert = false;
                    _situation.HasAnInvestigatedAlert = false;
                    _situation.WasTheLastInvestigatedAlertTheFirst = false;

                    _situation.TargetCover = null;
                    _situation.TargetPosition = _situation.ThreatGroundPosition;
                    _situation.InvestigationPosition = _situation.ThreatGroundPosition;

                    _situation.DirectionOfInvestigation = _situation.ThreatGroundPosition - transform.position;
                    _situation.DirectionOfInvestigation.y = 0;
                    _situation.DirectionOfInvestigation.Normalize();
                    break;

                case AIState.investigate:
                    _situation.HasInvestigatedTheLatestAlert = false;
                    _situation.IsThreatPositionANewAlert = false;

                    if (_situation.HasAnInvestigatedAlert)
                    {
                        if (_situation.WasTheLastInvestigatedAlertTheFirst)
                        {
                            if (!Positioning.FindNewThreatPositionInDirection(_situation.DirectionOfInvestigation, this, ref _situation, _agent))
                                Positioning.FindNewThreatPosition(this, ref _situation, _agent);
                        }
                        else
                            Positioning.FindNewThreatPosition(this, ref _situation, _agent);
                    }

                    _situation.TargetCover = null;
                    _situation.TargetPosition = _situation.ThreatGroundPosition;
                    _situation.InvestigationPosition = _situation.ThreatGroundPosition;

                    _situation.DirectionOfInvestigation = _situation.ThreatGroundPosition - transform.position;
                    _situation.DirectionOfInvestigation.y = 0;
                    _situation.DirectionOfInvestigation.Normalize();
                    break;

                case AIState.takeBetterCover:
                case AIState.takeAnyCover:
                    int previousTargetDirection = _situation.TargetDirection;

                    if (!Positioning.TakeCover(this, ref _situation, _state == AIState.takeBetterCover))
                    {
                        if (_situation.WouldLikeToRetreat)
                        {
                            updateSituation(AIState.retreat);
                            _stateReason = AIStateReason.couldntFindACoverToRetreatTo;
                        }
                        else if (!Behaviour.IsFightingUsingCovers || !Behaviour.IsApproachingUsingCovers)
                        {
                            _stateReason = AIStateReason.couldntFindAGoodCoverInsteadOfApproach;
                            updateSituation(AIState.approach);
                        }
                        else if (_situation.CurrentCover != null)
                        {
                            if (_stateReason == AIStateReason.previousTargetCoverNotSuitable)
                            {
                                _stateReason = AIStateReason.couldntFindACover;
                                updateSituation(AIState.approach);
                            }
                            else
                            {
                                if (!_situation.CanSeeFromCurrentPosition)
                                    _stateReason = AIStateReason.cantSeeAndCantFindABetterCover;
                                else
                                    _stateReason = AIStateReason.couldntFindABetterCover;

                                _situation.TargetDirection = previousTargetDirection;
                                _situation.TargetCover = _situation.CurrentCover;
                                _situation.TargetPosition = _situation.CurrentPosition;
                                updateSituation(AIState.hideInCover);
                            }
                        }
                        else
                        {
                            _stateReason = AIStateReason.couldntFindABetterCover;
                            updateSituation(AIState.approach);
                        }
                    }

                    break;

                case AIState.reload:
                    _motor.InputReload();
                    break;

                case AIState.patrol:
                    Positioning.Patrol(this, ref _situation);
                    break;
            }

            updateAgent(_situation.TargetPosition);
            _previousSituation = _situation;
        }

        /// <summary>
        /// Calculates and returns best state to move to depending on the given situation.
        /// </summary>
        private EnterAIState calcState(AISituation situation, AIState currentState, AIStateReason currentReason, float time, bool wasJustHit)
        {
            EnterAIState result;
            result.State = AIState.none;
            result.Reason = AIStateReason.none;
            result.ShouldRestart = false;

            if (situation.IsAlerted)
            {
                if (situation.IsNearGrenade)
                {
                    result.State = AIState.avoidGrenade;
                    result.Reason = AIStateReason.avoidGrenade;
                }
                else if (situation.WouldLikeToRetreat)
                {
                    if (Behaviour.IsRetreatingUsingCovers)
                    {
                        if (currentReason == AIStateReason.couldntFindACoverToRetreatTo && time < 4)
                        {
                            result.State = currentState;
                            result.Reason = currentReason;
                        }
                        else
                        {
                            result.State = AIState.takeAnyCover;

                            if ((situation.TargetCover != null && !situation.IsTargetCoverGood && time > 1) || wasJustHit)
                            {
                                result.Reason = AIStateReason.previousTargetCoverNotSuitableForRetreat;
                                result.ShouldRestart = true;
                            }
                            else
                                result.Reason = AIStateReason.retreatingUsingCovers;
                        }
                    }
                    else
                    {
                        result.State = AIState.retreat;
                        result.Reason = AIStateReason.retreatingWithoutUsingCovers;
                    }
                }
                else if (situation.Threat == null)
                {
                    result.State = AIState.investigate;

                    if (currentState == AIState.investigate)
                    {
                        result.ShouldRestart = situation.HasInvestigatedTheLatestAlert || situation.IsThreatPositionANewAlert;
                        result.Reason = AIStateReason.investigateNewPosition;
                    }
                    else
                        result.Reason = AIStateReason.alertedButNoKnownThreat;
                }
                else if (situation.CanSeeThatNoThreatAtLastPosition && situation.IsAllowedToBeAggressive)
                {
                    if (currentState == AIState.investigate || (currentState == AIState.follow && situation.HasInvestigatedTheLatestAlert) || situation.IsThreatPositionANewAlert)
                    {
                        result.State = AIState.investigate;
                        result.ShouldRestart = situation.HasInvestigatedTheLatestAlert || situation.IsThreatPositionANewAlert;
                        result.Reason = AIStateReason.investigateNewPosition;
                    }
                    else
                    {
                        result.State = AIState.follow;
                        result.Reason = AIStateReason.enemyDisappeared;
                    }
                }
                else if (AIUtil.CouldntFindCover(currentReason) && AIUtil.IsRelatedToNotFindingACover(currentState) && time < 2)
                {
                    result.State = currentState;
                    result.Reason = currentReason;
                }
                else if (!situation.Threat.IsAggressive)
                {
                    result.State = AIState.approach;
                    result.Reason = AIStateReason.threatIsNotAttacking;
                    result.ShouldRestart = time > 1 && Vector3.Distance(situation.TargetPosition, situation.ThreatGroundPosition) < Distances.MinEnemy;
                }
                else if (situation.IsIrritated)
                {
                    result.State = AIState.approach;
                    result.Reason = AIStateReason.isIrritated;
                    result.ShouldRestart = time > 1 && Vector3.Distance(situation.TargetPosition, situation.ThreatGroundPosition) < Distances.MinEnemy;
                }
                else if (!Behaviour.IsFightingUsingCovers && Vector3.Distance(situation.CurrentPosition, situation.ThreatGroundPosition) < Distances.MaxApproach)
                {
                    if (situation.IsAllowedToBeAggressive)
                    {
                        result.State = AIState.approach;
                        result.Reason = AIStateReason.approachedCloseEnoughToStopUsingCovers;
                        result.ShouldRestart = time > 1 && Vector3.Distance(situation.TargetPosition, situation.ThreatGroundPosition) < Distances.MinEnemy;
                    }
                    else
                    {
                        result.Reason = AIStateReason.wantsToAttackButNotAllowedByGroup;

                        if (situation.CurrentCover != null)
                            result.State = AIState.hideInCover;
                        else
                            result.State = AIState.retreat;
                    }
                }
                else if (Behaviour.IsApproachingUsingCovers || Behaviour.IsFightingUsingCovers)
                {
                    if (situation.TargetCover == null)
                    {
                        result.Reason = AIStateReason.needsCover;
                        result.State = AIState.takeAnyCover;
                        result.ShouldRestart = true;
                    }
                    else if (AIUtil.IsCoverState(currentState) && !situation.IsTargetCoverGood)
                    {
                        result.Reason = AIStateReason.previousTargetCoverNotSuitable;
                        result.State = AIState.takeAnyCover;
                        result.ShouldRestart = time > 1;
                    }
                    else if (AIUtil.IsCoverState(currentState) && situation.TargetCover == situation.CurrentCover && !situation.CanSeeFromCurrentPosition)
                    {
                        result.Reason = AIStateReason.couldntSeeFromPreviousCover;
                        result.State = AIState.takeAnyCover;
                        result.ShouldRestart = time > 1;
                    }
                    else if (situation.CurrentCover != situation.TargetCover)
                    {
                        result.Reason = AIStateReason.needsCover;

                        if (AIUtil.IsTakeCoverState(currentState))
                            result.State = currentState;
                        else
                            result.State = AIState.takeAnyCover;
                    }
                    else if (!situation.IsAllowedToBeAggressive)
                    {
                        result.State = AIState.hideInCover;
                        result.Reason = AIStateReason.wantsToAttackButNotAllowedByGroup;
                    }
                    else
                    {
                        var burst = Behaviour.IsFightingUsingCovers ? CoveredFightingBursts : CoveredApproachBursts;

                        if (!situation.IsGunReady)
                        {
                            result.State = AIState.reload;
                            result.Reason = AIStateReason.gunNeedsReloading;
                        }
                        else if (currentState == AIState.fireInCover)
                        {
                            if (time <= burst.TotalPeekDuration)
                            {
                                result.State = AIState.fireInCover;
                                result.Reason = AIStateReason.takeAPeek;
                            }
                            else if (situation.BurstCount < burst.Count)
                            {
                                result.State = AIState.hideInCover;
                                result.Reason = AIStateReason.waitingBeforeTakingAPeek;
                            }
                            else
                            {
                                result.State = AIState.takeBetterCover;
                                result.Reason = AIStateReason.tookEnoughPeeksAndLookingForNewCover;
                                result.ShouldRestart = true;
                            }
                        }
                        else if (currentState == AIState.hideInCover && time < burst.Wait)
                        {
                            result.State = AIState.hideInCover;
                            result.Reason = AIStateReason.waitingBeforeTakingAPeek;
                        }
                        else if (situation.BurstCount < burst.Count)
                        {
                            result.State = AIState.fireInCover;
                            result.Reason = AIStateReason.takeAPeek;
                        }
                        else
                        {
                            result.State = AIState.takeBetterCover;
                            result.Reason = AIStateReason.tookEnoughPeeksAndLookingForNewCover;
                            result.ShouldRestart = true;
                        }
                    }
                }
                else
                {
                    result.State = AIState.approach;
                    result.Reason = AIStateReason.notUsingCovers;
                    result.ShouldRestart = time > 1 && Vector3.Distance(situation.TargetPosition, situation.ThreatGroundPosition) < Distances.MinEnemy;
                }
            }
            else
            {
                const float WaypointThreshold = 0.7f;

                if (Waypoints != null && Waypoints.Length > 1)
                {
                    if (_situation.PatrolPoint >= Waypoints.Length)
                    {
                        result.State = AIState.patrol;
                        result.Reason = AIStateReason.patrol;
                        result.ShouldRestart = true;
                    }
                    else if (Vector3.Distance(transform.position, Waypoints[_situation.PatrolPoint].Position) < WaypointThreshold)
                    {
                        var duration = Waypoints[_situation.PatrolPoint].Pause;

                        if (duration <= float.Epsilon)
                        {
                            result.State = AIState.patrol;
                            result.Reason = AIStateReason.patrol;
                            result.ShouldRestart = true;
                        }
                        else if (currentState == AIState.patrol)
                        {
                            result.State = AIState.patrolPause;
                            result.Reason = AIStateReason.waitAtAWaypoint;
                        }
                        else if (currentState == AIState.patrolPause && time < duration)
                        {
                            result.State = AIState.patrolPause;
                            result.Reason = AIStateReason.waitAtAWaypoint;
                        }
                        else
                        {
                            result.State = AIState.patrol;
                            result.Reason = AIStateReason.patrol;
                            result.ShouldRestart = true;
                        }
                    }
                    else
                    {
                        result.State = AIState.patrol;
                        result.Reason = AIStateReason.patrol;
                    }
                }
                else if (Waypoints != null && Waypoints.Length == 1)
                {
                    if (Vector3.Distance(transform.position, Waypoints[0].Position) < WaypointThreshold)
                    {
                        result.State = AIState.patrolPause;
                        result.Reason = AIStateReason.waitAtAWaypoint;
                    }
                    else
                    {
                        result.State = AIState.patrol;
                        result.Reason = AIStateReason.patrol;
                    }
                }
                else
                {
                    result.State = AIState.patrolPause;
                    result.Reason = AIStateReason.noWaypoints;
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the current state machine state. Resets the state and burst timers and counters.
        /// </summary>
        private void setNewState(AIState value, AISituation situation)
        {
            _situation = situation;

            if (value != AIState.reload && _state != AIState.reload)
                _stateTime = 0;

            if (_state != value && value == AIState.fireInCover)
                _situation.BurstCount++;
            else if (AIUtil.IsMovingState(_state))
                _situation.BurstCount = 0;

            _state = value;
            _walkingBurstWait = 0;
        }

        /// <summary>
        /// Sets up the navigation agent to move to the givent position.
        /// </summary>
        private void updateAgent(Vector3 value)
        {
            if (!_agent.isActiveAndEnabled)
                return;

            _agent.Warp(transform.position);
            _agent.ResetPath();
            _agent.SetDestination(value);
            _isWaitingForPath = true;
        }

        #endregion
    }
}