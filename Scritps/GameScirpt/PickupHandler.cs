using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupType { Health, Ammo, Weapon, Empty}

public class PickupHandler : MonoBehaviour {

    [SerializeField] private GameObject buttonToPress;
    [SerializeField] private PickupType pickupType;
    [SerializeField] private string weaponPath;
    [SerializeField] private int ammoAmount;
    [SerializeField] private int healthAmount;
    [SerializeField] private LayerMask player;
    [SerializeField] private bool timedDestroy;

    private WeaponWrapper weapon;
    private bool loadedWep = false;

    private void Awake() {
        WeaponScriptebleObject loadedWep = Resources.Load<WeaponScriptebleObject>(weaponPath);
        weapon = loadedWep.InisiazlieWeapon();

        if (timedDestroy) {
            Destroy(gameObject, 30f);
        }
    }

    public (PickupType type, int ammo, int health, WeaponWrapper weapon) GetPickup() {
        switch(pickupType) {
            case PickupType.Ammo:
                return (pickupType, ammoAmount, 0, null);
            case PickupType.Health:
                return (pickupType, 0, healthAmount, null);
            case PickupType.Weapon:
                return (pickupType, 0, 0, weapon);
        }
        return (PickupType.Empty, 0, 0, null);
    }

    public void ChangeButtonStatus(bool show) {
        buttonToPress.SetActive(show);
    }

    public void ChangeAmmoAmount(int ammo) {
        ammoAmount = ammo;
    }

    public void ChangeWeapon(WeaponWrapper wep) {
        weapon = wep;
        loadedWep = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if( ((1 << other.gameObject.layer) & player) != 0) 
            ChangeButtonStatus(true);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (((1 << other.gameObject.layer) & player) != 0)
            ChangeButtonStatus(false);
    }

    public WeaponWrapper GetWeapon() {
        return weapon;
    }
}
