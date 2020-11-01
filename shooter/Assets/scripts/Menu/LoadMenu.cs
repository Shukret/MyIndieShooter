using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMenu : MonoBehaviour
{
    [SerializeField] private string scene;

    [SerializeField] private Slider slider;
    [SerializeField] private Text progressTxt;

    [SerializeField] GameObject[] rand;
    int i;
    void Start()
    {
        i = Random.Range(0,7);
        rand[i].SetActive(true);
        StartCoroutine(AsyncLoad());
    }

    void Next()
    {
        i = Random.Range(0,7);
        rand[i].SetActive(true);
    }

    IEnumerator AsyncLoad()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        while (!operation.isDone)
        {
            float progress = operation.progress / 0.9f;
            slider.value = progress;
            progressTxt.text = string.Format("{0:0}%", progress * 100);
            yield return null;
        }
    }
}
