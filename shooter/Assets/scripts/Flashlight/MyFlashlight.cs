using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
public class MyFlashlight : MonoBehaviour
{
    bool on;
    [SerializeField] private Light light;


    [SerializeField] private ProgressBar flashPowerPB;
    float flashlight = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (on && flashlight > 0)
            light.intensity -= 0.0012f;
        else if (on && flashlight <= 0)
        {
            light.intensity = 0; 
            StartCoroutine(OFF());
        }
        if (flashPowerPB.gameObject.activeSelf)
        {
            flashPowerPB.currentPercent = flashlight;
            flashlight -= Time.deltaTime * 1.4f;
        }
        if (!on && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ON());
        }
        if (on && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(OFF());
        }
    }

    IEnumerator ON()
    {
        light.enabled = true;
        flashPowerPB.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        on = true;
    }

    IEnumerator OFF()
    {
        light.enabled = false;
        flashPowerPB.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.01f);
        on = false;
    }

}
