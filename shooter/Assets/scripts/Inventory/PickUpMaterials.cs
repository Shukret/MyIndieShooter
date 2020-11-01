using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PickUpMaterials : MonoBehaviour
{
    RectTransform img;
    [SerializeField] private GameObject TakeImage;
    bool zone;

    [Header("Material Type")]
    [SerializeField] private bool rag;
    [SerializeField] private bool alcohol;
    [SerializeField] private bool duct_tape;
    [SerializeField] private bool blade;
    [SerializeField] private bool box_of_nails;

    [SerializeField] private Inventory playerInventory;
    // Update is called once per frame
    void Update()
    {
        if (zone && Input.GetKeyDown(KeyCode.E))
        {
            if (rag && playerInventory.rag < 13)
            {
                playerInventory.rag += 1;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
            if (alcohol && playerInventory.alcohol < 13)
            {
                playerInventory.alcohol += 1;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
            if (duct_tape && playerInventory.duct_tape < 13)
            {
                playerInventory.duct_tape += 1;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
            if (blade && playerInventory.blade < 13)
            {
                playerInventory.blade += 1;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
            if (box_of_nails && playerInventory.box_of_nails < 13)
            {
                playerInventory.box_of_nails += 1;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {            
            zone = true;
            if (alcohol)
                img = Instantiate(TakeImage, new Vector3(transform.position.x,transform.position.y + 0.7f, transform.position.z), Quaternion.identity).GetComponent<RectTransform>();
            else
                img = Instantiate(TakeImage, new Vector3(transform.position.x,transform.position.y + 0.3f, transform.position.z), Quaternion.identity).GetComponent<RectTransform>();
        } 
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {            
            img.LookAt(playerInventory.gameObject.transform);
            img.rotation=new Quaternion(0,img.rotation.y,0,1);
        } 
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            zone = false;
            Destroy(img.gameObject);
        }
    }
}
