using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class craft : MonoBehaviour
{
    //скрипт инвентаря
    [SerializeField] private Inventory playerInventory;


    //цвета для слайдеров
    [SerializeField] private Color white;
    [SerializeField] private Color yellow;
    [SerializeField] private Color red;
    [SerializeField] private Color alphaMinus;

    //тексты материалов
    [SerializeField] private Text[] Materials;

    //какая кнопка нажата?
    [SerializeField] private string thingsButtonPressed;

    void Update()
    {
        if (!playerInventory.active)
        {
            StartCoroutine(WhiteAll());
        }
    }
    
    //кнопки выбора
    public void KitBtn()
    {
        thingsButtonPressed = "kit";
        if (playerInventory.rag >= 4)
            Materials[0].color = white;
        else
            Materials[0].color = red;
        if (playerInventory.alcohol >= 4)
            Materials[1].color = white;
        else
            Materials[1].color = red;
        for (int i = 2; i < Materials.Length; i++)
        {
            Materials[i].color = alphaMinus;
        }
    }   

    public void KnifeBtn()
    {
        thingsButtonPressed = "knife";
        if (playerInventory.blade >= 4)
            Materials[3].color = white;
        else
            Materials[3].color = red;
        if (playerInventory.duct_tape >= 4)
            Materials[2].color = white;
        else
            Materials[2].color = red;
        Materials[0].color = alphaMinus;
        Materials[1].color = alphaMinus;
        Materials[4].color = alphaMinus;
    } 

    public void CoctailBtn()
    {
        thingsButtonPressed = "coctail";
        if (playerInventory.rag >= 4)
            Materials[0].color = white;
        else
            Materials[0].color = red;
        if (playerInventory.alcohol >= 4)
            Materials[1].color = white;
        else
            Materials[1].color = red;
        for (int i = 2; i < Materials.Length; i++)
        {
            Materials[i].color = alphaMinus;
        }
    }  

    //кнопка крафта
    public void CraftBtn()
    {
        //крафт аптечки
        if (thingsButtonPressed == "kit" && playerInventory.rag >= 4 && playerInventory.alcohol >= 4)
        {
            playerInventory.rag-=4;
            playerInventory.alcohol-=4;
            playerInventory.kit += 1;
        }
        else if (thingsButtonPressed == "kit")
        {
            Debug.Log("Need mor materials");
        }

        //крафт ножа
        if (thingsButtonPressed == "knife" && playerInventory.blade>= 4 && playerInventory.duct_tape >= 4)
        {
            playerInventory.blade-=4;
            playerInventory.duct_tape-=4;
            playerInventory.knife += 1;
        }
        else if (thingsButtonPressed == "knife")
        {
            Debug.Log("Need mor materials");
        }

        //крафт коктейля
        if (thingsButtonPressed == "coctail" && playerInventory.rag >= 4 && playerInventory.alcohol >= 4)
        {
            playerInventory.rag-=4;
            playerInventory.alcohol-=4;
            playerInventory.cocktail += 1;
        }
        else if (thingsButtonPressed == "coctail")
        {
            Debug.Log("Need mor materials");
        }
    } 
    
     

    IEnumerator WhiteAll()
    {
        for (int i = 0; i < Materials.Length; i++)
        {
            Materials[i].color = white;
        }
        yield return new WaitForSeconds(0.01f);
    }


}
