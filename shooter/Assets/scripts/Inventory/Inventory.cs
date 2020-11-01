using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;

public class Inventory : MonoBehaviour
{
    public Quest quest;
    [Header("Other things")]
    public int coins;
    [Header("Activate Inventory")]
    [SerializeField] private GameObject InventoryPanel;
    public bool active;

    [Header("Sliders,Bars and more")]
    [SerializeField] private Image[] Rag;
    [SerializeField] private Image[] Alcohol;
    [SerializeField] private Image[] Duct_tape;
    [SerializeField] private Image[] Blade;
    [SerializeField] private Image[] Box_of_nails;
    [Header("Materials")]
    //тряпка
    public int rag = 0;
    //алкоголь
    public int alcohol = 0;
    //скотч
    public int duct_tape = 0;
    //лезвие
    public int blade = 0;
    //коробка с гвоздями
    public int box_of_nails =0;

    [Header("Things")]
    //оружие ближнего боя (если 0 - его нет, 1 - бита, 2 - палка, 3 - топор)
    public int melee = 0;
    //нож
    public int knife = 0;
    //коктейль молотова
    public int cocktail = 0;
    //аптечка
    public int kit = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !active)
        {
            InventoryPanel.SetActive(true);
            StartCoroutine(activePanel());
        }
        else if (Input.GetKeyDown(KeyCode.I) && active)
        {
            InventoryPanel.SetActive(false);
            StartCoroutine(closePanel());
        }
    }

    IEnumerator activePanel()
    {
        active = true;
        switch(rag)
        {
            case 0:
                Rag[0].fillAmount = 0f;
                Rag[1].fillAmount = 0f;
                Rag[2].fillAmount = 0f;
                break;
            case 1:
                Rag[0].fillAmount = 0.25f;
                Rag[1].fillAmount = 0f;
                Rag[2].fillAmount = 0f;
                break;
            case 2:
                Rag[0].fillAmount = 0.50f;
                Rag[1].fillAmount = 0f;
                Rag[2].fillAmount = 0f;
                break;
            case 3:
                Rag[0].fillAmount = 0.75f;
                Rag[1].fillAmount = 0f;
                Rag[2].fillAmount = 0f;
                break;
            case 4:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 0f;
                Rag[2].fillAmount = 0f;
                break;
            case 5:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 0.25f;
                Rag[2].fillAmount = 0f;
                break;
            case 6:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 0.50f;
                Rag[2].fillAmount = 0f;
                break;
            case 7:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 0.75f;
                Rag[2].fillAmount = 0f;
                break;
            case 8:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 1f;
                Rag[2].fillAmount = 0f;
                break;
            case 9:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 1f;
                Rag[2].fillAmount = 0.25f;
                break;
            case 10:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 1f;
                Rag[2].fillAmount = 0.5f;
                break;
            case 11:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 1f;
                Rag[2].fillAmount = 0.75f;
                break;
            case 12:
                Rag[0].fillAmount = 1f;
                Rag[1].fillAmount = 1f;
                Rag[2].fillAmount = 1f;
                break;
        } 
        switch(alcohol)
        {
            case 0:
                Alcohol[0].fillAmount = 0f;
                Alcohol[1].fillAmount = 0f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 1:
                Alcohol[0].fillAmount = 0.25f;
                Alcohol[1].fillAmount = 0f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 2:
                Alcohol[0].fillAmount = 0.50f;
                Alcohol[1].fillAmount = 0f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 3:
                Alcohol[0].fillAmount = 0.75f;
                Alcohol[1].fillAmount = 0f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 4:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 0f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 5:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 0.25f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 6:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 0.50f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 7:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 0.75f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 8:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 1f;
                Alcohol[2].fillAmount = 0f;
                break;
            case 9:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 1f;
                Alcohol[2].fillAmount = 0.25f;
                break;
            case 10:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 1f;
                Alcohol[2].fillAmount = 0.5f;
                break;
            case 11:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 1f;
                Alcohol[2].fillAmount = 0.75f;
                break;
            case 12:
                Alcohol[0].fillAmount = 1f;
                Alcohol[1].fillAmount = 1f;
                Alcohol[2].fillAmount = 1f;
                break;
        }
        switch(blade)
        {
            case 0:
                Blade[0].fillAmount = 0f;
                Blade[1].fillAmount = 0f;
                Blade[2].fillAmount = 0f;
                break;
            case 1:
                Blade[0].fillAmount = 0.25f;
                Blade[1].fillAmount = 0f;
                Blade[2].fillAmount = 0f;
                break;
            case 2:
                Blade[0].fillAmount = 0.50f;
                Blade[1].fillAmount = 0f;
                Blade[2].fillAmount = 0f;
                break;
            case 3:
                Blade[0].fillAmount = 0.75f;
                Blade[1].fillAmount = 0f;
                Blade[2].fillAmount = 0f;
                break;
            case 4:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 0f;
                Blade[2].fillAmount = 0f;
                break;
            case 5:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 0.25f;
                Blade[2].fillAmount = 0f;
                break;
            case 6:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 0.50f;
                Blade[2].fillAmount = 0f;
                break;
            case 7:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 0.75f;
                Blade[2].fillAmount = 0f;
                break;
            case 8:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 1f;
                Blade[2].fillAmount = 0f;
                break;
            case 9:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 1f;
                Blade[2].fillAmount = 0.25f;
                break;
            case 10:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 1f;
                Blade[2].fillAmount = 0.5f;
                break;
            case 11:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 1f;
                Blade[2].fillAmount = 0.75f;
                break;
            case 12:
                Blade[0].fillAmount = 1f;
                Blade[1].fillAmount = 1f;
                Blade[2].fillAmount = 1f;
                break;
        } 
        switch(duct_tape)
        {
            case 0:
                Duct_tape[0].fillAmount = 0f;
                Duct_tape[1].fillAmount = 0f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 1:
                Duct_tape[0].fillAmount = 0.25f;
                Duct_tape[1].fillAmount = 0f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 2:
                Duct_tape[0].fillAmount = 0.50f;
                Duct_tape[1].fillAmount = 0f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 3:
                Duct_tape[0].fillAmount = 0.75f;
                Duct_tape[1].fillAmount = 0f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 4:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 0f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 5:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 0.25f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 6:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 0.50f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 7:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 0.75f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 8:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 1f;
                Duct_tape[2].fillAmount = 0f;
                break;
            case 9:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 1f;
                Duct_tape[2].fillAmount = 0.25f;
                break;
            case 10:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 1f;
                Duct_tape[2].fillAmount = 0.5f;
                break;
            case 11:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 1f;
                Duct_tape[2].fillAmount = 0.75f;
                break;
            case 12:
                Duct_tape[0].fillAmount = 1f;
                Duct_tape[1].fillAmount = 1f;
                Duct_tape[2].fillAmount = 1f;
                break;
        }
        switch(box_of_nails)
        {
           case 0:
                Box_of_nails[0].fillAmount = 0f;
                Box_of_nails[1].fillAmount = 0f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 1:
                Box_of_nails[0].fillAmount = 0.25f;
                Box_of_nails[1].fillAmount = 0f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 2:
                Box_of_nails[0].fillAmount = 0.50f;
                Box_of_nails[1].fillAmount = 0f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 3:
                Box_of_nails[0].fillAmount = 0.75f;
                Box_of_nails[1].fillAmount = 0f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 4:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 0f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 5:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 0.25f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 6:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 0.50f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 7:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 0.75f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 8:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 1f;
                Box_of_nails[2].fillAmount = 0f;
                break;
            case 9:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 1f;
                Box_of_nails[2].fillAmount = 0.25f;
                break;
            case 10:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 1f;
                Box_of_nails[2].fillAmount = 0.5f;
                break;
            case 11:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 1f;
                Box_of_nails[2].fillAmount = 0.75f;
                break;
            case 12:
                Box_of_nails[0].fillAmount = 1f;
                Box_of_nails[1].fillAmount = 1f;
                Box_of_nails[2].fillAmount = 1f;
                break;
        }   
        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator closePanel()
    {
        active = false;
        yield return new WaitForSeconds(0.01f);
    }
}
