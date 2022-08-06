using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class gunSwitcher : MonoBehaviourPunCallbacks
{
    
    public GameObject[] Primgun;
    
   
    public GameObject[] Secgun;
    
    public GameObject[] melee;
    
    public GameObject gunPos;
    

    private GameObject SessionPrimaryGun;
    private GameObject SessionSecondaryGun;
    private GameObject SessionMelee;
    
    // private GameObject PrimaryWeaponIcon;
    // private GameObject SecondWeaponIcon;
    // private GameObject MeleeWeaponIcon;


    void Awake(){
       // PrimaryWeaponIcon = GameObject.Find("/PenisUI/penisPrimary");
      //  SecondWeaponIcon = GameObject.Find("/PenisUI/penisSecondary");
       // MeleeWeaponIcon = GameObject.Find("/PenisUI/penisMelee");
        
        // if(PrimaryWeaponIcon = null){
        //     Debug.LogError("primary weapon icon not found!");
        // }else{
        //     Debug.Log("icon found");
        // }
        
        
        //randomizes gun from array for session;
        int PrimGunRandomIndex = Random.Range(0, Primgun.Length);
        int SecGunRandomIndex = Random.Range(0, Secgun.Length);
        int MeleeRandomIndex = Random.Range(0, melee.Length);

        SessionPrimaryGun = Instantiate(Primgun[PrimGunRandomIndex], gunPos.transform.position, gunPos.transform.rotation);
        SessionSecondaryGun = Instantiate(Secgun[SecGunRandomIndex], gunPos.transform.position, gunPos.transform.rotation);
        SessionMelee = Instantiate(melee[MeleeRandomIndex], gunPos.transform.position, gunPos.transform.rotation);


        SessionPrimaryGun.transform.parent = gunPos.transform;
        SessionSecondaryGun.transform.parent = gunPos.transform;
        SessionMelee.transform.parent = gunPos.transform;

        SessionPrimaryGun.SetActive(true);
        SessionSecondaryGun.SetActive(false);
        SessionMelee.SetActive(false);

        // PrimaryWeaponIcon.SetActive(true);
        // SecondWeaponIcon.SetActive(false);
        // MeleeWeaponIcon.SetActive(false);
        
    
    }

    void LateUpdate(){
       
        if(photonView.IsMine){
            SessionPrimaryGun.transform.rotation = Camera.main.transform.rotation;
            SessionSecondaryGun.transform.rotation = Camera.main.transform.rotation;
            SessionMelee.transform.rotation = Camera.main.transform.rotation;
        }

    }
    void Update(){

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            Debug.Log("1 pressed");
            StartCoroutine(SwitchWeaponActive(SessionPrimaryGun));
            StartCoroutine(SwitchWeaponInActive(SessionSecondaryGun));
            StartCoroutine(SwitchWeaponInActive(SessionMelee));
            
            // PrimaryWeaponIcon.SetActive(true);
            // SecondWeaponIcon.SetActive(false);
            // MeleeWeaponIcon.SetActive(false);



        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            Debug.Log("2 pressed");
            
            StartCoroutine(SwitchWeaponActive(SessionSecondaryGun));
            StartCoroutine(SwitchWeaponInActive(SessionPrimaryGun));
            StartCoroutine(SwitchWeaponInActive(SessionMelee));
            
            // PrimaryWeaponIcon.SetActive(false);
            // SecondWeaponIcon.SetActive(true);
            // MeleeWeaponIcon.SetActive(false);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            Debug.Log("3 pressed");
            
            StartCoroutine(SwitchWeaponActive(SessionMelee));
            StartCoroutine(SwitchWeaponInActive(SessionSecondaryGun));
            StartCoroutine(SwitchWeaponInActive(SessionPrimaryGun));
            
            // PrimaryWeaponIcon.SetActive(false);
            // SecondWeaponIcon.SetActive(false);
            // MeleeWeaponIcon.SetActive(true);
        }


    }

    IEnumerator SwitchWeaponActive(GameObject Weapon){
        Weapon.SetActive(true);
        yield return null;
    }
    IEnumerator SwitchWeaponInActive(GameObject Weapon){
        Weapon.SetActive(false);
        yield return null;
    }

    
}
