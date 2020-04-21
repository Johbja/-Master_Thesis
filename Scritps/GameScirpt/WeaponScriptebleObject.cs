using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { HandGun, SubmachineGun, AssultRifle }

[CreateAssetMenu(fileName = "WeaponType", menuName = "Custom/WeaponType")]
public class WeaponScriptebleObject : ScriptableObject {
    [SerializeField] private WeaponsClass weapon;
    [SerializeField] private GameObject weaponObj;
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject dropItem;
    public WeaponType weaponType;
    public Sprite WeaponSprite;
    [SerializeField] private float recoil;

    public WeaponWrapper InisiazlieWeapon() {
        WeaponsClass tempWeapon = new SingleFireGun(weapon);

        //if(weaponType == WeaponType.HandGun || weaponType == WeaponType.SubmachineGun || weaponType == WeaponType.AssultRifle)
        //        tempWeapon = new SingleFireGun(weapon);

        return new WeaponWrapper(tempWeapon, weaponObj, projectile, dropItem, weaponType, WeaponSprite, recoil);
    }

    //public bool FireWeapon(Transform muzzle, Vector2 direction, Quaternion characterRotation, int layerMask) {

    //    Vector2[] bullets = currentWeapon.FireWeapon(direction);
        
    //    if(bullets != null) {
    //        foreach(Vector2 v2 in bullets) {
    //            float rand = Random.Range(-recoil, recoil) * Mathf.Deg2Rad;

    //            Vector2 rotateddir = new Vector2(v2.x * Mathf.Cos(rand) - v2.y * Mathf.Sin(rand),
    //                                             v2.x * Mathf.Sin(rand) + v2.y * Mathf.Cos(rand));

    //            GameObject instance = Instantiate(projectile, muzzle.position, characterRotation * Quaternion.Euler(new Vector3(0,0,rand * Mathf.Rad2Deg)));
    //            instance.gameObject.GetComponent<ProjectileScript>().Iniziate(rotateddir, currentWeapon.GetWeaponInfo().damage, layerMask);
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    //public void ReloadWeapon() {
    //    int ammoMissing = currentWeapon.AmmoMissing();

    //    if (ammoMissing > 0)
    //        currentWeapon.Reload(ammoMissing);
    //}

    //public (GameObject weapon, GameObject bullet, GameObject dropWeapon) GetObjects() {
    //    return (weaponObj, projectile, dropItem);
    //}

    //public (float fireSpeed, float relaodSpeed, int damage) GetWeaponInfo() {
    //    return currentWeapon.GetWeaponInfo();
    //}

    //public (int maxAmmo, int currenAmmo, int currentClipAmmo, bool unlimitedAmmo) GetAmmoInfo() {
    //    return currentWeapon.GetAmmoInfo();
    //}

    //public int AmmoMissing() {
    //    return currentWeapon.AmmoMissing();
    //}

    //public void AddAmmo(int ammo) {
    //    currentWeapon.AddAmmo(ammo);
    //}

    //public void RemoveAmmo(int ammoToRemvoe) {
    //    currentWeapon.RemoveAmmo(ammoToRemvoe);
    //}

    //public (AudioClip fire, AudioClip[] reload) GetSounds() {
    //    return currentWeapon.GetSounds();
    //}
}
