using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoverShooter;
public class PickUpAmmo : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private Gun pistol_gun;
    [SerializeField] private Gun rifle_gun;

    RectTransform img;
    [SerializeField] private GameObject TakeImage;
    bool zone;

    [Header("Type`s ammo")]
    [SerializeField] private bool pistol_ammo;
    [SerializeField] private bool rifle_ammo;

    // Update is called once per frame
    void Update()
    {
         if (zone && Input.GetKeyDown(KeyCode.E))
        {
            if (pistol_ammo && pistol_gun.pistol_clip <= 24)
            {
                pistol_gun.pistol_clip += 6;
                Destroy(gameObject);
                Destroy(img.gameObject);
            }
            if (rifle_ammo && pistol_gun.rifle_clip <= 45)
            {
                rifle_gun.rifle_clip += 10;
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
            img = Instantiate(TakeImage, new Vector3(transform.position.x,transform.position.y + 0.3f, transform.position.z), Quaternion.identity).GetComponent<RectTransform>();
        } 
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {            
            img.LookAt(player);
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
