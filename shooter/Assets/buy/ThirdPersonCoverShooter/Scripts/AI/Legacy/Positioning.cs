using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    public static class Positioning
    {
        /// <summary>
        /// Updates target situation to visit a patrol point.
        /// </summary>
        public static void Patrol(AIController controller, ref AISituation situation)
        {
            situation.TargetCover = null;
            situation.TargetPosition = situation.CurrentPosition;

            if (controller.Waypoints != null && controller.Waypoints.Length > 0)
            {
                if (controller.Waypoints.Length == 1)
                    situation.PatrolPoint = 0;
                else
                {
                    situation.PatrolPoint = situation.PatrolPoint % controller.Waypoints.Length;

                    if (Vector3.Distance(controller.transform.position, controller.Waypoints[situation.PatrolPoint].Position) < 1)
                    {
                        situation.PatrolPoint++;
                        situation.PatrolPoint = situation.PatrolPoint % controller.Waypoints.Length;
                    }
                }

                situation.TargetPosition = controller.Waypoints[situation.PatrolPoint].Position;
            }
        }

        /// <summary>
        /// Updates the target situation to approach an enemy that's in cover.
        /// </summary>
        public static void ApproachACovered(AIController controller, ref AISituation situation)
        {
            var path = new NavMeshPath();
            var corners = new Vector3[16];

            var resultPosition = situation.ThreatGroundPosition;
            var resultDistance = 9999f;

            var coverAngle = Util.AngleOfVector(situation.ThreatCoverForward);

            for (int a = 0; a < 18; a++)
            {
                var angle = coverAngle - 180f + a * 10;
                var position = situation.ThreatGroundPosition + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * controller.Distances.MinEnemy;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas) &&
                    AIUtil.IsInSight(controller, hit.position, situation.ThreatStandingTopPosition))
                {
                    NavMesh.CalculatePath(controller.transform.position, hit.position, NavMesh.AllAreas, path);

                    if (path.status == NavMeshPathStatus.PathComplete &&
                        !AIUtil.IsPositionTooCloseToFriends(controller, hit.position))
                    {
                        var dist = 0f;

                        for (int i = 1; i < path.GetCornersNonAlloc(corners); i++)
                            dist += Vector3.Distance(corners[i - 1], corners[i]);

                        if (dist < resultDistance)
                        {
                            resultDistance = dist;
                            resultPosition = position;
                        }
                    }
                }
            }

            situation.TargetPosition = resultPosition;
            situation.TargetCover = null;
        }

        /// <summary>
        /// Updates the target situation to approach an enemy that's not in cover.
        /// </summary>
        public static void ApproachAFree(AIController controller, ref AISituation situation, NavMeshAgent agent, float maxDistance, bool approachStanding)
        {
            situation.TargetCover = null;
            situation.TargetPosition = situation.ThreatGroundPosition;

            var path = new NavMeshPath();
            agent.CalculatePath(situation.ThreatGroundPosition, path);

            var corners = new Vector3[16];
            var count = path.GetCornersNonAlloc(corners);

            if (count < 2)
                return;

            var i = 0;
            var p0 = corners[i];
            var p1 = corners[i + 1];
            var f = 1f;

            var targetPosition = approachStanding ? situation.ThreatStandingTopPosition : situation.ThreatGroundPosition;

            {
                var distLeft = Vector3.Distance(controller.transform.position, situation.ThreatGroundPosition);

                while (distLeft > maxDistance)
                {
                    var pd = Vector3.Distance(p0, p1);
                    var left = distLeft - maxDistance;
                    distLeft -= pd;

                    if (pd >= left)
                    {
                        f = left / pd;
                        break;
                    }
                    else
                    {
                        i++;

                        if (i + 1 >= count)
                        {
                            i = 0;
                            break;
                        }
                        else
                        {
                            p0 = corners[i];
                            p1 = corners[i + 1];
                        }
                    }
                }
            }

            while (i + 1 < count)
            {
                var p = p0 + (p1 - p0) * f;

                if (AIUtil.IsInSight(controller, p, targetPosition))
                {
                    situation.TargetPosition = p;
                    break;
                }

                f += 0.2f;

                if (f >= 1f)
                {
                    if (AIUtil.IsInSight(controller, p1, targetPosition))
                    {
                        situation.TargetPosition = p1;
                        break;
                    }

                    f = 0;
                    i++;
                }
            }
        }

        /// <summary>
        /// Looks for a position to investigate in given direction.
        /// </summary>
        public static bool FindNewThreatPositionInDirection(Vector3 direction, AIController controller, ref AISituation situation, NavMeshAgent agent)
        {
            var corners = new Vector3[32];

            float extra = 0f;

            while (extra < 20f)
            {
                var p = situation.CurrentPosition + direction * (controller.Distances.MinSearch + extra);

                var path = new NavMeshPath();
                agent.CalculatePath(p, path);

                if (path.status != NavMeshPathStatus.PathInvalid)
                {
                    int count = path.GetCornersNonAlloc(corners);

                    if (count > 1)
                    {
                        situation.SetThreatPosition(corners[count - 1], true);
                        return true;
                    }
                }

                extra += 1f;
            }

            return false;
        }

        /// <summary>
        /// Looks for a position to investigate around the character.
        /// </summary>
        public static void FindNewThreatPosition(AIController controller, ref AISituation situation, NavMeshAgent agent, int attempts = 0)
        {
            var vec = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * Vector3.forward;

            if (!FindNewThreatPositionInDirection(vec, controller, ref situation, agent))
            {
                if (attempts < 3)
                    FindNewThreatPosition(controller, ref situation, agent, attempts + 1);
            }
        }

        /// <summary>
        /// Updates the target situation to retreat from an enemy.
        /// </summary>
        public static void Retreat(AIController controller, ref AISituation situation, NavMeshAgent agent)
        {
            situation.TargetCover = null;
            situation.TargetPosition = situation.CurrentPosition;

            var vec = (controller.transform.position - situation.ThreatGroundPosition).normalized;

            float extra = 0f;

            while (extra < 20f)
            {
                var p = situation.ThreatGroundPosition + vec * (controller.Distances.MinRetreat + extra);

                var path = new NavMeshPath();
                agent.CalculatePath(p, path);

                if (path.status != NavMeshPathStatus.PathInvalid)
                {
                    situation.TargetPosition = p;
                    break;
                }
                else
                    extra += 1f;
            }
        }

        /// <summary>
        /// Updates the target situation to retreat from an enemy.
        /// </summary>
        public static void AvoidGrenade(AIController controller, ref AISituation situation, NavMeshAgent agent)
        {
            situation.TargetCover = null;
            situation.TargetPosition = situation.CurrentPosition;

            var vec = controller.transform.position - situation.NearestGrenadePosition;
            vec.y = 0;
            vec.Normalize();

            situation.TargetPosition = situation.NearestGrenadePosition + vec * controller.Grenades.AvoidDistance;
        }

        /// <summary>
        /// Updates the target situation to take cover. Returns true if a cover was found.
        /// </summary>
        public static bool TakeCover(AIController controller, ref AISituation situation, bool ignoreTooClose)
        {
            var currentVectorToTarget = situation.ThreatGroundPosition - situation.CurrentPosition;
            var currentDistanceToTarget = currentVectorToTarget.magnitude;

            Cover result = null;
            float resultPathDistance = 0;
            int resultDirection = 0;
            Vector3 resultPosition = situation.CurrentPosition;

            var path = new NavMeshPath();
            var corners = new Vector3[32];

            var isAlreadyTooClose = currentDistanceToTarget <= controller.Distances.MinEnemy || currentDistanceToTarget <= controller.Cover.MinCoverToEnemyDistance;

            var covers = new List<CoverItem>();

            var segmenta = situation.ThreatGroundPosition;
            var segmentVector = situation.IsThreatInCover ? situation.ThreatCoverForward : (situation.CurrentPosition - situation.ThreatGroundPosition).normalized;
            var segmentb = segmenta + segmentVector * 1000;

            foreach (var collider in Physics.OverlapSphere(controller.transform.position, controller.Cover.MaxDistance, 0x1 << 8, QueryTriggerInteraction.Collide))
            {
                if (!collider.isTrigger)
                    continue;

                var cover = CoverSearch.GetCover(collider.gameObject);

                if (cover == null || cover == situation.CurrentCover)
                    continue;

                var item = new CoverItem();
                item.Cover = cover;

                if (cover.IsTall)
                    item.Direction = cover.ClosestCornerToSegment(segmenta, segmentb, controller.Motor.CoverSettings.CornerAimTriggerDistance, out item.Position);
                else
                    item.Position = cover.ClosestPointTo(situation.CurrentPosition, controller.Motor.CoverSettings.LowSideEnterRadius, 0.3f);

                if (float.IsNaN(item.Position.x) || float.IsNaN(item.Position.z))
                    continue;

                item.Position.y = cover.Bottom;
                item.Distance = Vector3.Distance(controller.transform.position, item.Position);

                covers.Add(item);
            }

            foreach (var item in covers.OrderBy(o => o.Distance))
            {
                var isTall = item.Cover.IsTall;
                var cover = item.Cover;
                var point = item.Position;
                var coverDirection = item.Direction;

                if (!AIUtil.IsGoodAngle(controller, cover, point, situation.ThreatGroundPosition, isTall))
                    continue;

                var distanceToTarget = Vector3.Distance(situation.ThreatGroundPosition, point);

                if (distanceToTarget < controller.Distances.MinEnemy ||
                    distanceToTarget < controller.Cover.MinCoverToEnemyDistance)
                    continue;

                if (ignoreTooClose)
                    if (situation.CurrentCover != null && Vector3.Distance(situation.CurrentPosition, point) < controller.Cover.MinSwitchDistance)
                        continue;

                if ((controller.Health == null || controller.Health.Health > controller.Fighting.MinHealth))
                {
                    if (isTall)
                    {
                        Vector3 aimPoint;
                        coverDirection = cover.ClosestCornerTo(point, -controller.Motor.CoverSettings.CornerOffset.x, out aimPoint);

                        if (!AIUtil.IsInSight(controller, aimPoint, situation.ThreatStandingTopPosition))
                            continue;
                    }
                    else if (!AIUtil.IsInSight(controller, point, situation.ThreatStandingTopPosition))
                        continue;
                }

                var distanceToOrigin = Vector3.Distance(situation.CurrentPosition, point);

                if (situation.CurrentCover == null)
                {
                    if (distanceToOrigin > controller.Cover.MaxDistance)
                        continue;
                }
                else
                    if (distanceToOrigin > controller.Cover.MaxSwitchDistance)
                    continue;

                var areThereFriends = false;

                {
                    var hasChangedPosition = false;

                    Vector3 side;

                    if (Vector3.Dot((point - situation.CurrentPosition).normalized, cover.Right) > 0)
                        side = cover.Right;
                    else
                        side = cover.Left;

                    do
                    {
                        hasChangedPosition = false;

                        if (AIUtil.IsCoverPositionTooCloseToFriends(cover, controller, point))
                        {
                            var next = point + side * 0.5f;

                            if (cover.IsInFront(next, false))
                            {
                                point = next;
                                hasChangedPosition = true;
                            }
                            else
                                areThereFriends = true;
                        }
                    }
                    while (hasChangedPosition);
                }

                if (areThereFriends)
                    continue;

                var isOk = false;

                NavMesh.CalculatePath(situation.CurrentPosition, point, NavMesh.AllAreas, path);

                float pathDistance = 0f;

                var count = path.GetCornersNonAlloc(corners);

                if (count < 2)
                    continue;

                var isTooCloseToEnemy = false;

                for (int i = 1; i < count; i++)
                {
                    pathDistance += Vector3.Distance(corners[i - 1], corners[i]);

                    if (!isAlreadyTooClose)
                        if (Util.DistanceToSegment(situation.ThreatGroundPosition, corners[i - 1], corners[i]) <= controller.Distances.MinPassing)
                        {
                            isTooCloseToEnemy = true;
                            break;
                        }
                }

                if (isTooCloseToEnemy)
                    continue;

                if (situation.CurrentCover == null)
                    isOk = result == null || pathDistance < resultPathDistance;
                else if (controller.Health == null || controller.Health.Health > controller.Fighting.MinHealth)
                {
                    isOk = (isAlreadyTooClose || distanceToTarget < currentDistanceToTarget) &&
                           (result == null || pathDistance < resultPathDistance);
                }
                else
                    isOk = distanceToTarget > currentDistanceToTarget && (result == null || pathDistance < resultPathDistance);

                if (isOk)
                {
                    result = cover;
                    resultPosition = point;
                    resultPathDistance = pathDistance;
                    resultDirection = coverDirection;
                    break;
                }
            }

            if (result == null)
                return false;
            else
            {
                situation.IsNewCover = true;
                situation.TargetCover = result;
                situation.TargetPosition = resultPosition;
                situation.TargetDirection = resultDirection;

                return true;
            }
        }
    }
}
