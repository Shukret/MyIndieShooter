using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float rotSpeed;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotSpeed*Time.deltaTime*Vector3.up);
    }
}
