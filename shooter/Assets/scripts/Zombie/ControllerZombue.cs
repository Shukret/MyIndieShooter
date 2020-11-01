using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class ControllerZombue : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;
    
    [SerializeField] private Transform head;
    Ray ray;
    [SerializeField] private float maxDistance = 10;
    public bool angry;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(head.position, head.forward*maxDistance, Color.red, 1000);
        ray  = new Ray(head.position, new Vector3(0,0,90));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.transform.gameObject.tag == "Player")
                angry = true;
            if (angry)
                agent.destination = target.position;
        }
    }
}
