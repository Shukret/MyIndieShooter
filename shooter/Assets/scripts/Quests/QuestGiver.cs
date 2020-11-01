using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    public Quest quest;

    public Inventory inventory;

    public Text titleText;
    public Text descriptionText;

    public void ActiveQuest()
    {
        quest.isActive = true;
        inventory.quest = quest;  
        titleText.text = quest.title;
        descriptionText.text = quest.description;
    }
}
