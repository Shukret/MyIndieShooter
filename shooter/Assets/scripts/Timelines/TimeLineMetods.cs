using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TimeLineMetods : MonoBehaviour
{
    [SerializeField] private CinemachineBrain camera;

    public void DisCamera()
    {
        camera.enabled = false;
    }
}
