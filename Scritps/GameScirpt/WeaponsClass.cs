using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class WeaponsClass {
    [SerializeField] protected float fireSpeed;
    [SerializeField] protected float reloadSpeed;
    [SerializeField] protected float bulletVelocity;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected int currentTotalAmmo;
    [SerializeField] protected int clipSize;
    [SerializeField] protected int currentClipAmmo;
    [SerializeField] protected bool unlimitedAmmo;
    [SerializeField] protected AudioClip fire;
    [SerializeField] protected AudioClip[] reload;


    public WeaponsClass(WeaponsClass weapon) {
        fireSpeed = weapon.fireSpeed;
        reloadSpeed = weapon.reloadSpeed;
        bulletVelocity = weapon.bulletVelocity;
        attackDamage = weapon.attackDamage;
        maxAmmo = weapon.maxAmmo;
        clipSize = weapon.clipSize;
        currentClipAmmo = weapon.currentClipAmmo;
        currentTotalAmmo = weapon.currentTotalAmmo;
        unlimitedAmmo = weapon.unlimitedAmmo;
        fire = weapon.fire;
        reload = weapon.reload;
    }

    public virtual Vector2[] FireWeapon(Vector2 direction) { return null; }

    public void Reload(int ammoMissing)
    {
        if(unlimitedAmmo) {
            currentClipAmmo = clipSize;
            return;
        }

        if (currentTotalAmmo >= ammoMissing)
        {
            currentTotalAmmo -= ammoMissing;
            currentClipAmmo = clipSize;
        }
        else{
            currentClipAmmo += currentTotalAmmo;
            currentTotalAmmo = 0;
        }
    }

    public void AddAmmo(int ammo) {
        currentTotalAmmo = Mathf.Clamp(currentTotalAmmo + ammo, 0, maxAmmo);
    }

    public void RemoveAmmo(int ammoToRemove) {
        currentTotalAmmo = Mathf.Clamp(currentTotalAmmo - ammoToRemove, 0, maxAmmo);
    }

    public int AmmoMissing() {
        return clipSize - currentClipAmmo;
    }

    public (int maxAmmo, int currenAmmo, int currentClipAmmo, bool unlimitedAmoo) GetAmmoInfo() {
        return (maxAmmo, currentTotalAmmo, currentClipAmmo, this.unlimitedAmmo);
    }

    public (float fireSpeed, float relaodSpeed, int damage) GetWeaponInfo() {
        var tuple = (fireSpeed, reloadSpeed, attackDamage);
        return tuple;
    }

    public (AudioClip fire, AudioClip[] reload) GetSounds() {
        return (fire, reload);
    }

}

public class SingleFireGun : WeaponsClass {

    public SingleFireGun(WeaponsClass weapon) : base(weapon) {
        // call main
    }

    public override Vector2[] FireWeapon(Vector2 direction) {
        if(currentClipAmmo > 0) {
            Vector2[] forceDirections = { direction * bulletVelocity };
            currentClipAmmo--;
            return forceDirections;
        }

        currentClipAmmo = 0;
        return null;
    }
}
