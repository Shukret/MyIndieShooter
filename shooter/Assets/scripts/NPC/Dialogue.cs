using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Playables;
using Cinemachine;
using CoverShooter;
public class Dialogue : MonoBehaviour
{
    //таймлиния
    [SerializeField] private PlayableDirector timelineDialoge;
    [SerializeField] private CinemachineBrain camera;
    [SerializeField] private GameObject player;
    
    public DialogueNode[] node;
    public int _currentNode;
    public bool triger = false;
    public bool triger2;
    public int AnsNum = 0;
    
    public Quest quest;
    public Inventory inventory;
    public Text titleText;
    public Text descriptionText;


    [SerializeField] private MouseLock mouseLock;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            triger = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            triger = false;
        }
    }

    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            triger2 = true;
        }
         if (triger && triger2)
         {
                mouseLock.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                timelineDialoge.Play();
                camera.enabled = true;
                player.SetActive(false); 
                GUI.Box(new Rect(Screen.width / 2 - 300, Screen.height - 150, 600, 250), "");
                GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height - 140, 500, 90), node[_currentNode].NpcText);
                for (int i = 0; i < node[_currentNode].PlayerAnswer.Length; i++)
                {
                    if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height - 100 + 25 * i, 500, 25), node[_currentNode].PlayerAnswer[i].Text))
                    {
                        if (node[_currentNode].PlayerAnswer[i].SpeakEnd)
                        {
                            triger = false;
                            triger2 = false;
                            mouseLock.enabled = true;
                            camera.enabled = false;
                            timelineDialoge.Stop();
                            player.SetActive(true);
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                        if(node[_currentNode].PlayerAnswer[i].GiveQuest)
                        {
                            quest.isActive = true;
                            inventory.quest = quest;  
                            titleText.text = quest.title;
                            descriptionText.text = quest.description;
                        }
                        if(node[_currentNode].PlayerAnswer[i].QuestWin == false)
                        {
                            i++;
                        }
                        _currentNode = node[_currentNode].PlayerAnswer[i].ToNode;
                }
            }
        }
    }
}


[System.Serializable]
public class DialogueNode
{
    public string NpcText;
    public Answer[] PlayerAnswer;
}


[System.Serializable]
public class Answer
{
    public bool GiveQuest;
    public bool QuestWin = true;
    public int ToNum;
    public string Text;
    public int ToNode;
    public bool SpeakEnd;
}

