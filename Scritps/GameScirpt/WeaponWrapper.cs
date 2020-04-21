using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWrapper {

    private WeaponsClass currentWeapon;
    private GameObject weaponObj;
    public GameObject projectile;
    private GameObject dropItem;
    public WeaponType weaponType;
    public Sprite weaponSprite;
    private float recoil;


    public WeaponWrapper(WeaponsClass weapon, GameObject wepObj, GameObject projectile, GameObject dropItem, WeaponType type, Sprite sprite, float recoil) {
        currentWeapon = weapon;
        weaponObj = wepObj;
        this.projectile = projectile;
        this.dropItem = dropItem;
        weaponType = type;
        weaponSprite = sprite;
        this.recoil = recoil;
    }

    public (bool didShoot, List<Vector2> rotDir, List<float> recoils) FireWeapon(Vector2 direction) {

        Vector2[] bullets = currentWeapon.FireWeapon(direction);
        List<Vector2> rotatedBullets = new List<Vector2>();
        List<float> recoilList = new List<float>();

        if(bullets != null) {
            foreach(Vector2 v2 in bullets) {
                float rand = Random.Range(-recoil, recoil) * Mathf.Deg2Rad;

                Vector2 rotateddir = new Vector2(v2.x * Mathf.Cos(rand) - v2.y * Mathf.Sin(rand),
                                                 v2.x * Mathf.Sin(rand) + v2.y * Mathf.Cos(rand));

                rotatedBullets.Add(rotateddir);
                recoilList.Add(rand);
            }
            return (true, rotatedBullets, recoilList);
        }
        return (false, null, null);
    }

    public void ReloadWeapon() {
        int ammoMissing = currentWeapon.AmmoMissing();

        if(ammoMissing > 0)
            currentWeapon.Reload(ammoMissing);
    }

    public (GameObject weapon, GameObject bullet, GameObject dropWeapon) GetObjects() {
        return (weaponObj, projectile, dropItem);
    }

    public (float fireSpeed, float relaodSpeed, int damage) GetWeaponInfo() {
        return currentWeapon.GetWeaponInfo();
    }

    public (int maxAmmo, int currenAmmo, int currentClipAmmo, bool unlimitedAmmo) GetAmmoInfo() {
        return currentWeapon.GetAmmoInfo();
    }

    public int AmmoMissing() {
        return currentWeapon.AmmoMissing();
    }

    public void AddAmmo(int ammo) {
        currentWeapon.AddAmmo(ammo);
    }

    public void RemoveAmmo(int ammoToRemvoe) {
        currentWeapon.RemoveAmmo(ammoToRemvoe);
    }

    public (AudioClip fire, AudioClip[] reload) GetSounds() {
        return currentWeapon.GetSounds();
    }

}
