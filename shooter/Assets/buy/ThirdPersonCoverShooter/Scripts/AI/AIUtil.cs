using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    public static class AIUtil
    {
        public static void ClosestStandablePosition(ref Vector3 position)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 3, 1))
                position = hit.position;
        }

        public static void Path(ref NavMeshPath path, Vector3 source, Vector3 target)
        {
            if (path == null)
                path = new NavMeshPath();

            ClosestStandablePosition(ref source);
            ClosestStandablePosition(ref target);

            NavMesh.CalculatePath(source, target, 1, path);
        }

        /// <summary>
        /// Checks if the given state involves moving.
        /// </summary>
        public static bool IsMovingState(AIState state)
        {
            return state == AIState.takeAnyCover || 
                   state == AIState.takeBetterCover ||
                   state == AIState.approach ||
                   state == AIState.investigate ||
                   state == AIState.follow ||
                   state == AIState.retreat ||
                   state == AIState.patrol ||
                   state == AIState.avoidGrenade;
        }

        /// <summary>
        /// Checks if the given state involves moving.
        /// </summary>
        public static bool IsFireState(AIState state)
        {
            return state == AIState.fireInCover ||
                   state == AIState.takeAnyCover ||
                   state == AIState.takeBetterCover ||
                   state == AIState.approach ||
                   state == AIState.retreat;
        }

        /// <summary>
        /// Checks if the given state is related to covers.
        /// </summary>
        public static bool IsCoverState(AIState state)
        {
            return state == AIState.takeAnyCover ||
                   state == AIState.takeBetterCover ||
                   state == AIState.fireInCover ||
                   state == AIState.hideInCover;
        }

        /// <summary>
        /// Is state involving taking a cover.
        /// </summary>
        public static bool IsTakeCoverState(AIState state)
        {
            return state == AIState.takeAnyCover ||
                   state == AIState.takeBetterCover;
        }

        /// <summary>
        /// Checks if the reason was a cover unavailability.
        /// </summary>
        public static bool CouldntFindCover(AIStateReason reason)
        {
            return reason == AIStateReason.couldntFindACoverToRetreatTo ||
                   reason == AIStateReason.couldntFindAGoodCoverInsteadOfApproach ||
                   reason == AIStateReason.couldntFindABetterCover ||
                   reason == AIStateReason.couldntFindACover ||
                   reason == AIStateReason.cantSeeAndCantFindABetterCover;
        }

        /// <summary>
        /// Checks if the state is a possibility of not finding a cover.
        /// </summary>
        public static bool IsRelatedToNotFindingACover(AIState state)
        {
            return state == AIState.hideInCover ||
                   state == AIState.approach ||
                   state == AIState.retreat;
        }

        /// <summary>
        /// Sends a message to all team members.
        /// </summary>
        public static void NotifyFriends(Actor actor, string message, object argument)
        {
            foreach (var friend in Actors.All)
                if (friend != actor && friend.Side == actor.Side)
                    friend.SendMessage(message, argument, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(AIController controller, Vector3 target)
        {
            return IsInSight(controller, controller.transform.position, target);
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(Actor actor, Vector3 target, float maxDistance, float fieldOfView)
        {
            var motorTop = actor.StandingTopPosition;
            var vector = target - motorTop;

            if (vector.magnitude > maxDistance)
                return false;

            vector.y = 0;

            var angle = Mathf.Abs(Mathf.DeltaAngle(0, Mathf.Acos(Vector3.Dot(vector.normalized, actor.HeadDirection)) * Mathf.Rad2Deg));
            if (angle > fieldOfView * 0.5f)
                return false;

            return !IsObstructed(motorTop, target, maxDistance);
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(AIController controller, Vector3 origin, Vector3 target)
        {
            var motorTop = origin + Vector3.up * controller.Motor.StandingHeight;
            var vector = target - motorTop;
            var distance = vector.magnitude;

            if (distance > controller.View.SightDistance(controller.IsAlerted))
                return false;

            var angle = Mathf.Abs(Mathf.DeltaAngle(0, Mathf.Acos(Vector3.Dot(vector / distance, controller.Motor.HeadForward)) * Mathf.Rad2Deg));
            if (angle > controller.View.FieldOfView * 0.5f)
                return false;

            return !IsObstructed(motorTop, target, controller.View.SightDistance(controller.IsAlerted));
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(CharacterMotor motor, AIViewSettings view, bool isAlerted, Vector3 origin, Vector3 target)
        {
            var motorTop = origin + Vector3.up * motor.StandingHeight;
            var vector = target - motorTop;
            var distance = vector.magnitude;

            if (distance > view.SightDistance(isAlerted))
                return false;

            var angle = Mathf.Abs(Mathf.DeltaAngle(0, Mathf.Acos(Vector3.Dot(vector / distance, motor.HeadForward)) * Mathf.Rad2Deg));
            if (angle > view.FieldOfView * 0.5f)
                return false;

            return !IsObstructed(motorTop, target, view.SightDistance(isAlerted));
        }

        /// <summary>
        /// Returns true if there is no unobstructed line between the given origin and the target.
        /// </summary>        
        public static bool IsObstructed(Vector3 origin, Vector3 target, float distance)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, (target - origin).normalized, out hit, distance, int.MaxValue, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Distance(origin, hit.point) > Vector3.Distance(origin, target) || Vector3.Distance(hit.point, target) < 0.5f)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if the given position on the cover protects the character from the enemy.
        /// </summary>
        public static bool IsGoodAngle(AIController controller, Cover cover, Vector3 positionOnCover, Vector3 enemy, bool isTall)
        {
            return IsGoodAngle(controller.Cover.MaxTallAngle, controller.Cover.MaxLowAngle, cover, positionOnCover, enemy, isTall);
        }

        /// <summary>
        /// Returns true if the given position on the cover protects the character from the enemy.
        /// </summary>
        public static bool IsGoodAngle(float maxTallAngle, float maxLowAngle, Cover cover, Vector3 positionOnCover, Vector3 enemy, bool isTall)
        {
            var dot = Vector3.Dot((enemy - positionOnCover).normalized, cover.Forward);

            if (isTall)
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxTallAngle)
                    return false;
            }
            else
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxLowAngle)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsCoverPositionFree(Cover cover, Vector3 position, float threshold, Actor newcomer)
        {
            if (!IsJustThisCoverPositionFree(cover, position, threshold, newcomer))
                return false;

            if (cover.LeftAdjacent != null && !IsJustThisCoverPositionFree(cover.LeftAdjacent, position, threshold, newcomer))
                return false;

            if (cover.RightAdjacent != null && !IsJustThisCoverPositionFree(cover.RightAdjacent, position, threshold, newcomer))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given position is free for taking.
        /// </summary>
        public static bool IsJustThisCoverPositionFree(Cover cover, Vector3 position, float threshold, Actor newcomer)
        {
            foreach (var user in cover.Users)
                if (user.Actor != newcomer && Vector3.Distance(user.Position, position) <= threshold)
                    return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsCoverPositionTooCloseToFriends(Cover cover, AIController controller, Vector3 position)
        {
            if (IsJustThisCoverTooCloseToFriends(cover, controller, position))
                return true;

            if (cover.LeftAdjacent != null && IsJustThisCoverTooCloseToFriends(cover.LeftAdjacent, controller, position))
                return true;

            if (cover.RightAdjacent != null && IsJustThisCoverTooCloseToFriends(cover.RightAdjacent, controller, position))
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsJustThisCoverTooCloseToFriends(Cover cover, AIController controller, Vector3 position)
        {
            foreach (var user in cover.Users)
            {
                if (controller.Actor != user.Actor && controller.Actor.Side == user.Actor.Side)
                {
                    if (Vector3.Distance(user.Actor.transform.position, controller.transform.position) <= controller.View.CommunicationDistance &&
                        Vector3.Distance(user.Position, position) <= controller.Distances.MinFriend)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsPositionTooCloseToFriends(AIController controller, Vector3 position)
        {
            foreach (var other in Actors.All)
                if (other != controller.Actor && other.Side == controller.Actor.Side)
                {
                    var targetPosition = other.AI == null ? other.transform.position : other.AI.Situation.TargetPosition;

                    if (Vector3.Distance(other.transform.position, controller.transform.position) <= controller.View.CommunicationDistance &&
                        Vector3.Distance(targetPosition, position) <= controller.Distances.MinFriend)
                        return true;
                }

            return false;
        }
    }
}
