using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestGoal
{
    public GoalType goalType;

    public int requiredAmount;
    public int currentAmount;

    public bool IsReached()
    {
        return(currentAmount>=requiredAmount);
    }

    public void KillQuest()
    {
        if (goalType == GoalType.Kill)
            currentAmount++;
    }

    public void GatheringQuest()
    {
        if (goalType == GoalType.Gathering)
            currentAmount++;
    }

    public void FindQuest()
    {
        if (goalType == GoalType.Find)
            currentAmount++;
    }
}

public enum GoalType
{
    //найти диалог
    Find,
    //собирание
    Gathering,
    //убийство
    Kill
}
