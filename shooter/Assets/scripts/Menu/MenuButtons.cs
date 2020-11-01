using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private GameObject load;
    public void NextLevel()
    {
        load.SetActive(true);
    }
}
