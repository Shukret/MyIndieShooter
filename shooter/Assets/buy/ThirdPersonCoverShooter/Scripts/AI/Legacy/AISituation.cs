using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Status of the AI situation in single a frame.
    /// </summary>
    public struct AISituation
    {
        public bool IsAlerted;
        public bool IsGettingAlerted;
        public float AlertReaction;

        public bool IsAllowedToBeAggressive;
        public bool IsIrritated;
        public float IrritationTime;

        public float GrenadeReaction;
        public bool IsNoticingGrenade;
        public int ThrownGrenadeCount;

        public bool HasInvestigatedTheLatestAlert;
        public bool IsThreatPositionANewAlert;
        public bool HasAnInvestigatedAlert;
        public bool WasTheLastInvestigatedAlertTheFirst;
        public float InvestigatedAlertAge;
        public Vector3 InvestigatedThreatPosition;
        public Vector3 InvestigationPosition;
        public Vector3 DirectionOfInvestigation;

        public bool IsNearGrenade;
        public Vector3 NearestGrenadePosition;

        public Vector3 ThreatGroundPosition;
        public Vector3 ThreatTopPosition;
        public Vector3 ThreatStandingTopPosition;
        public bool IsThreatPositionInvestigative;

        public Actor Threat;
        public float LastSeenThreatTime;
        public float NoThreatVisibilityTime;
        public bool CanSeeTheThreat;
        public bool CanSeeThatNoThreatAtLastPosition;
        public bool IsThreatInCover;
        public Vector3 ThreatCoverForward;

        public Vector3 TargetPosition;
        public Vector3 CurrentPosition;

        public int PatrolPoint;

        public Cover TargetCover;
        public Cover CurrentCover;
        public bool IsNewCover;
        public bool IsTargetCoverGood;
        public bool CanSeeFromCurrentPosition;
        public int TargetDirection;

        public int BurstCount;
        public bool IsGunReady;

        public bool WouldLikeToRetreat;

        /// <summary>
        /// Marks the threat as investigated.
        /// </summary>
        public void MarkInvestigated()
        {
            if (HasInvestigatedTheLatestAlert)
                return;

            WasTheLastInvestigatedAlertTheFirst = !HasAnInvestigatedAlert;
            HasInvestigatedTheLatestAlert = true;
            HasAnInvestigatedAlert = true;
            InvestigatedThreatPosition = ThreatGroundPosition;
        }

        /// <summary>
        /// Checks if this situation as better info than the other.
        /// </summary>
        public bool HasBetterThreatInfo(AIController me, ref AISituation other)
        {
            if (Threat == null || IsThreatPositionInvestigative)
                return false;

            if (other.Threat == null || other.IsThreatPositionInvestigative)
                if (LastSeenThreatTime - 1 > other.LastSeenThreatTime)
                    return true;

            if (LastSeenThreatTime - 0.005f > other.LastSeenThreatTime)
                return true;

            return false;
        }

        /// <summary>
        /// Sets threat position at all heights to the same given value.
        /// </summary>
        public void SetThreatPosition(Vector3 position, bool isInvestigative)
        {
            ThreatGroundPosition = position;
            ThreatTopPosition = position + Vector3.up;
            ThreatStandingTopPosition = ThreatTopPosition;
            IsThreatPositionInvestigative = isInvestigative;
        }

        /// <summary>
        /// Sets new ground and both top standing positions.
        /// </summary>
        public void SetThreatPosition(Vector3 ground, Vector3 top)
        {
            ThreatGroundPosition = ground;
            ThreatTopPosition = top;
            ThreatStandingTopPosition = top;
        }
        
        /// <summary>
        /// Forgets about the enemy.
        /// </summary>
        public void RemoveEnemyState()
        {
            Threat = null;
            CanSeeTheThreat = false;
            HasInvestigatedTheLatestAlert = false;
            WasTheLastInvestigatedAlertTheFirst = false;
            HasAnInvestigatedAlert = false;
        }

        /// <summary>
        /// Copy enemy info from a friend. Ignored if an enemy is already known.
        /// </summary>
        public void TakeEnemyState(AIController other)
        {
            if (other.Situation.Threat == null)
                return;

            Threat = other.Situation.Threat;
            IsThreatPositionANewAlert = false;
            CanSeeTheThreat = other.Situation.CanSeeTheThreat;
            LastSeenThreatTime = other.Situation.LastSeenThreatTime;
            NoThreatVisibilityTime = other.Situation.NoThreatVisibilityTime;
            IsThreatInCover = other.Situation.IsThreatInCover;
            ThreatGroundPosition = other.Situation.ThreatGroundPosition;
            ThreatTopPosition = other.Situation.ThreatTopPosition;
            ThreatStandingTopPosition = other.Situation.ThreatStandingTopPosition;
            ThreatCoverForward = other.Situation.ThreatCoverForward;
            CanSeeThatNoThreatAtLastPosition = other.Situation.CanSeeThatNoThreatAtLastPosition;
            IsThreatPositionInvestigative = other.Situation.IsThreatPositionInvestigative;
        }

        /// <summary>
        /// Copies enemy state from the given actor.
        /// </summary>
        public void ReadEnemyState(Actor actor)
        {
            if (actor == null)
                return;

            Threat = actor;
            CanSeeTheThreat = true;
            NoThreatVisibilityTime = 0;
            LastSeenThreatTime = Time.timeSinceLevelLoad;
            ThreatGroundPosition = Threat.transform.position;
            ThreatTopPosition = Threat.TopPosition;
            IsThreatInCover = Threat.Cover != null;
            ThreatStandingTopPosition = Threat.StandingTopPosition;
            CanSeeThatNoThreatAtLastPosition = false;
            IsThreatPositionInvestigative = false;
            IsThreatPositionANewAlert = false;

            if (IsThreatInCover)
                ThreatCoverForward = Threat.Cover.Forward;
        }

        /// <summary>
        /// Returns an updated situation struct.
        /// </summary>
        public void Update(AIController controller, AISituation previous, bool updateInDetail)
        {
            if (!IsAlerted)
                HasInvestigatedTheLatestAlert = false;
            else if (!HasInvestigatedTheLatestAlert && Vector3.Distance(CurrentPosition, InvestigationPosition) < controller.Distances.ThreatInvestigation)
                MarkInvestigated();

            if (HasAnInvestigatedAlert)
                InvestigatedAlertAge += Time.deltaTime;

            if (IsIrritated)
            {
                if (IrritationTime > controller.Fighting.Irritation)
                {
                    IrritationTime = 0;
                    IsIrritated = false;
                }
                else
                    IrritationTime += Time.deltaTime;
            }
            else
                IrritationTime = 0;

            // Check grenades
            {
                IsNearGrenade = false;
                float minDist = 1000;

                foreach (var grenade in GrenadeList.All)
                {
                    var vec = grenade.transform.position - controller.transform.position;
                    var dist = vec.magnitude;

                    if (dist < grenade.ExplosionRadius)
                        if (!IsNearGrenade || dist < minDist)
                        {
                            minDist = dist;
                            IsNearGrenade = true;
                            NearestGrenadePosition = grenade.transform.position;

                            if (Threat == null)
                            {
                                HasInvestigatedTheLatestAlert = false;
                                HasAnInvestigatedAlert = false;
                                IsThreatPositionANewAlert = true;
                                SetThreatPosition(grenade.transform.position, true);
                            }
                        }
                }
            }

            // Check friends and enemies.
            if (Threat == null || (!IsAlerted && !IsGettingAlerted))
            {
                foreach (var actor in Actors.All)
                    if (actor != controller.Actor)
                    {
                        if (actor.Side != controller.Actor.Side)
                        {
                            if (AIUtil.IsInSight(controller, actor.TopPosition))
                            {
                                IsAlerted = true;
                                ReadEnemyState(actor);
                                break;
                            }
                        }
                        else if (actor.AI != null && actor.AI.IsAlerted)
                        {
                            var vector = actor.transform.position - controller.transform.position;

                            if (vector.magnitude < controller.View.CommunicationDistance)
                            {
                                IsAlerted = true;

                                if (actor.AI.State != AIState.investigate)
                                    if (actor.AI.Situation.Threat != null && actor.AI.Situation.HasBetterThreatInfo(actor.AI, ref this))
                                    {
                                        TakeEnemyState(actor.AI);
                                        break;
                                    }
                            }
                        }
                    }
            }

            // Check friends if they had investigated the same position
            if (IsAlerted && !HasInvestigatedTheLatestAlert)
                foreach (var friend in Actors.All)
                    if (friend != controller.Actor &&
                        friend.Side == controller.Actor.Side &&
                        friend.AI != null &&
                        friend.AI.IsAlerted &&
                        friend.AI.Situation.HasAnInvestigatedAlert && 
                        friend.AI.Situation.InvestigatedAlertAge < 4 &&
                        Vector3.Distance(friend.transform.position, controller.transform.position) < controller.View.CommunicationDistance &&
                        Vector3.Distance(friend.AI.Situation.InvestigatedThreatPosition, InvestigationPosition) < controller.Distances.ThreatInvestigation)
                    {
                        MarkInvestigated();
                        break;
                    }

            var isCheckingAThreatInCover = Threat != null && IsThreatInCover && !CanSeeTheThreat;

            // Check threats
            if (Threat == null || CanSeeThatNoThreatAtLastPosition || isCheckingAThreatInCover)
            {
                var minDist = 100000f;

                foreach (var alert in Alerts.All)
                {
                    bool isOk;
                    Actor newThreat = null;

                    if (Threat != null)
                    {
                        if (alert.Actor == null)
                            isOk = NoThreatVisibilityTime > 6;
                        else if (alert.Actor.Side != controller.Actor.Side)
                        {
                            isOk = true;
                            newThreat = alert.Actor;
                        }
                        else if (alert.Actor.AI != null)
                            isOk = NoThreatVisibilityTime > 2 && alert.Actor.AI.Situation.NoThreatVisibilityTime < 1;
                        else
                            isOk = NoThreatVisibilityTime > 6;

                    }
                    else
                        isOk = true;

                    if (isOk)
                    {
                        var dist = Vector3.Distance(controller.transform.position, alert.Position);

                        if (dist < alert.Range)
                            if (dist < minDist)
                            {
                                minDist = dist;
                                IsAlerted = true;

                                HasAnInvestigatedAlert = false;
                                HasInvestigatedTheLatestAlert = false;

                                if (newThreat != null)
                                    ReadEnemyState(newThreat);
                                else
                                {
                                    IsThreatPositionANewAlert = true;
                                    SetThreatPosition(alert.Position, true);
                                }
                            }
                    }
                }
            }

            // React to grenades
            if (IsNoticingGrenade)
            {
                if (GrenadeReaction < float.Epsilon)
                    IsNoticingGrenade = false;
                else
                {
                    GrenadeReaction -= Time.deltaTime;
                    IsNearGrenade = false;
                }
            }
            else if (IsNearGrenade && !previous.IsNearGrenade)
            {
                GrenadeReaction = controller.Fighting.GrenadeReactionTime;
                IsNoticingGrenade = true;
                IsNearGrenade = false;
            }

            if (IsNearGrenade)
                IsAlerted = true;

            // React to being alerted.
            if (IsGettingAlerted)
            {
                if (AlertReaction < float.Epsilon)
                {
                    IsGettingAlerted = false;
                    IsAlerted = true;
                }
                else
                {
                    AlertReaction -= Time.deltaTime;
                    IsAlerted = false;
                }
            }
            else if (IsAlerted && !previous.IsAlerted)
            {
                AlertReaction = controller.Fighting.ReactionTime;
                IsGettingAlerted = true;
                IsAlerted = false;
            }

            if (previous.TargetCover != null && 
                (controller.Motor.LeftCover == previous.TargetCover || controller.Motor.RightCover == previous.TargetCover || controller.Motor.Cover == previous.TargetCover))
                CurrentCover = previous.TargetCover;
            else
                CurrentCover = controller.Motor.Cover;

            CurrentPosition = controller.transform.position;
            IsGunReady = controller.Motor.IsGunReady && controller.Motor.Gun.Clip >= controller.Motor.Gun.ClipSize * controller.Fighting.ReloadFraction;

            if (Threat == null || Threat.IsAggressive)
                WouldLikeToRetreat = controller.Health.Health <= controller.Fighting.MinHealth;
            else
                WouldLikeToRetreat = false;

            if (IsAlerted)
            {
                var couldSeeTheEnemy = CanSeeTheThreat;
                CanSeeTheThreat = false;

                if (Threat != null)
                {
                    var noPatience = NoThreatVisibilityTime > controller.Fighting.Patience;

                    if (couldSeeTheEnemy || updateInDetail)
                        CanSeeTheThreat = AIUtil.IsInSight(controller, Threat.TopPosition);

                    if (CanSeeTheThreat)
                    {
                        ReadEnemyState(Threat);

                        if (!couldSeeTheEnemy && noPatience)
                            IsIrritated = true;
                    }
                    else
                    {
                        if (noPatience || (NoThreatVisibilityTime > 2 && (!IsThreatInCover || Vector3.Dot(ThreatCoverForward, ThreatGroundPosition - CurrentPosition) > 0)))
                            if (!IsThreatInCover ||
                                noPatience ||
                                Vector3.Distance(CurrentPosition, ThreatGroundPosition) < controller.Distances.ThreatInvestigation ||
                                AIUtil.IsInSight(controller, (ThreatGroundPosition + ThreatTopPosition) * 0.5f))
                                CanSeeThatNoThreatAtLastPosition = true;

                        NoThreatVisibilityTime += Time.deltaTime;

                        if (updateInDetail)
                        {
                            // Check friends.
                            foreach (var friend in Actors.All)
                                if (friend != controller.Actor &&
                                    friend.Side == controller.Actor.Side &&
                                    friend.AI != null &&
                                    friend.AI.IsAlerted &&
                                    friend.AI.State != AIState.investigate &&
                                    friend.AI.Situation.Threat == Threat &&
                                    friend.AI.Situation.HasBetterThreatInfo(friend.AI, ref this))
                                {
                                    var vector = friend.transform.position - controller.transform.position;

                                    if (vector.magnitude < controller.View.CommunicationDistance)
                                        TakeEnemyState(friend.AI);
                                }
                        }
                    }
                }

                if (TargetCover != null && updateInDetail)
                {
                    var distanceToThreat = Vector3.Distance(TargetPosition, ThreatGroundPosition);

                    IsTargetCoverGood = distanceToThreat >= controller.Distances.MinEnemy &&
                                        distanceToThreat >= controller.Cover.MinCoverToEnemyDistance &&
                                        AIUtil.IsGoodAngle(controller,
                                                           TargetCover,
                                                           TargetPosition,
                                                           ThreatGroundPosition,
                                                           TargetCover.IsTall) &&
                                        !AIUtil.IsCoverPositionTooCloseToFriends(TargetCover, controller, TargetPosition);
                }

                if (updateInDetail)
                {
                    if (IsThreatInCover)
                    {
                        if (CurrentCover != null && CurrentCover.IsTall)
                        {
                            var aimPoint = Vector3.zero;
                            var isGood = true;

                            if (TargetDirection < 0)
                            {
                                isGood = CurrentCover.IsLeft(Util.AngleOfVector(ThreatStandingTopPosition - CurrentCover.LeftCorner(0)), controller.Motor.CoverSettings.Angles.LeftCorner, false);

                                if (isGood)
                                    aimPoint = CurrentCover.LeftCorner(0, controller.Motor.CoverSettings.CornerOffset.x);
                            }
                            else
                            {
                                isGood = CurrentCover.IsRight(Util.AngleOfVector(ThreatStandingTopPosition - CurrentCover.RightCorner(0)), controller.Motor.CoverSettings.Angles.LeftCorner, false);

                                if (isGood)
                                    aimPoint = CurrentCover.RightCorner(0, controller.Motor.CoverSettings.CornerOffset.x);
                            }

                            CanSeeFromCurrentPosition = AIUtil.IsInSight(controller, aimPoint, ThreatStandingTopPosition);
                        }
                        else
                            CanSeeFromCurrentPosition = AIUtil.IsInSight(controller, CurrentPosition, ThreatStandingTopPosition);
                    }
                    else
                    {
                        CanSeeFromCurrentPosition = CanSeeTheThreat;
                    }
                }
            }
            else
            {
                if (TargetCover != null && updateInDetail)
                    IsTargetCoverGood = !AIUtil.IsCoverPositionTooCloseToFriends(TargetCover, controller, TargetPosition);
            }
        }
    }
}
