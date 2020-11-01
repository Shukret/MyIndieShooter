using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class NPS : MonoBehaviour
{
    //таймлиния
    [SerializeField] private PlayableDirector timelineDialoge;
    [SerializeField] private CinemachineBrain camera;
    [SerializeField] private GameObject player;
    //в зоне ли игрок
    bool zone;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (zone && Input.GetKeyDown(KeyCode.E))
        {
            timelineDialoge.Play();
            camera.enabled = true;
            player.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            zone = false;
            camera.enabled = false;
            timelineDialoge.Stop();
            player.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            zone = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            zone = false;
        }
    }
}
