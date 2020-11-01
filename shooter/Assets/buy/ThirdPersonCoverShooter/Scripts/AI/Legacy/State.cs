using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Name of the current AI state.
    /// </summary>
    public enum AIState
    {
        none,
        patrol,
        patrolPause,
        takeAnyCover,
        takeBetterCover,
        fireInCover,
        hideInCover,
        reload,
        investigate,
        follow,
        approach,
        retreat,
        avoidGrenade
    }

    /// <summary>
    /// Reasons for entering an AI state.
    /// </summary>
    public enum AIStateReason
    {
        none,
        alertedButNoKnownThreat,
        threatIsNotAttacking,
        retreatingUsingCovers,
        retreatingWithoutUsingCovers,
        approachedCloseEnoughToStopUsingCovers,
        isIrritated,
        wantsToAttackButNotAllowedByGroup,
        needsCover,
        avoidGrenade,
        previousTargetCoverNotSuitable,
        previousTargetCoverNotSuitableForRetreat,
        couldntSeeFromPreviousCover,
        notUsingCovers,
        gunNeedsReloading,
        waitingBeforeTakingAPeek,
        takeAPeek,
        tookEnoughPeeksAndLookingForNewCover,
        waitAtAWaypoint,
        patrol,
        noWaypoints,
        couldntFindACoverToRetreatTo,
        couldntFindAGoodCoverInsteadOfApproach,
        couldntFindABetterCover,
        couldntFindACover,
        cantSeeAndCantFindABetterCover,
        investigateNewPosition,
        enemyDisappeared
    }

    /// <summary>
    /// An AI decision, includes a new state, reason for entering it.
    /// </summary>
    struct EnterAIState
    {
        public AIState State;
        public AIStateReason Reason;
        public bool ShouldRestart;
    }
}
