using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] private GameObject downSprite;
    [SerializeField] private GameObject companion;
    [SerializeField] private bool isAi;

    private bool playerDown;
    private ItemDropScirpt dropper;
    private WeaponHandler wepHandler;
    private Rigidbody2D rb2d;
    private HealthController hc;

    private void Start() {
        hc = GetComponent<HealthController>();
        hc.InitinializeHealthController(null);
        dropper = GetComponent<ItemDropScirpt>();
        wepHandler = GetComponent<WeaponHandler>();
        rb2d = GetComponent<Rigidbody2D>();
        downSprite.SetActive(false);
    }

    private void Update() {
        if(!isAi && Input.GetButtonDown("Pickup") && Vector2.Distance(transform.position, companion.transform.position) < 1 && companion.GetComponent<PlayerController>().isDown()) {
            companion.GetComponent<PlayerController>().Restored();
        }
    }

    public void OnDown() {
        if(playerDown)
            return;

        playerDown = true;
        GameObject wep = wepHandler.GetCurrentWeapon().GetObjects().dropWeapon;
       
        if(wep != null)
            DropCurrentWeapon(wepHandler.GetCurrentWeapon());

        wepHandler.ChangeWeapon(null);
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        downSprite.SetActive(true);
    }

    public void Restored() {
        playerDown = false;
        wepHandler.DropWeapon();
        hc.InitinializeHealthController(null);
        downSprite.SetActive(false);
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public List<GameObject> DropCurrentWeapon(WeaponWrapper currenWep) {
        if(currenWep == null)
            return null;

        GameObject[] inventory = { currenWep.GetObjects().dropWeapon };
        inventory[0].GetComponent<PickupHandler>().ChangeWeapon(currenWep);
        return dropper.DropInventory(inventory);
    }

    public bool isDown() {
        return playerDown;
    }

}
