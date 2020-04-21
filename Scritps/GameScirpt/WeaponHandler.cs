using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour {

    [Header("Reload UI settings")]
    [SerializeField] private Transform worldUICanvas;
    [SerializeField] private GameObject reloadUI;

    [Header("Weapon settings")]
    [SerializeField] private Transform WeaponHolder;
    [SerializeField] private string startWeaponPath;
    [SerializeField] private GameObject ammoCreate;
    [SerializeField] private float ammoCreatSpawnOffset;
    [SerializeField] private float ammoThrowForce;
    [SerializeField] private float pickupDeley;
    [SerializeField] private int maxAmmoDrop;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private AudioClip dropSound;

    [Header("AI settings")]
    [SerializeField] private bool isAI;

    private Camera cam;
    private WeaponWrapper currentWeapon;
    [HideInInspector] public WeaponScriptebleObject startWeapon;
    private WeaponUIController wepUI;
    private HealthController hp;
    private GameObject currentWeaponObj;
    private Transform currentWeapenMuzzle;
    private PlayerController playerController;
    private AudioSource audio;
    private ItemDropScirpt dropper;
    private bool canShoot;
    private bool isReloading;
    private bool canPickup;

    private void Awake() {
        startWeapon = Resources.Load<WeaponScriptebleObject>(startWeaponPath);
        currentWeapon = startWeapon.InisiazlieWeapon();
    }

    void Start() {
        cam = Camera.main;
        hp = GetComponent<HealthController>();
        playerController = GetComponent<PlayerController>();
        dropper = GetComponent<ItemDropScirpt>();

        GameObject instance = Instantiate(reloadUI, worldUICanvas);
        wepUI = instance.GetComponent<WeaponUIController>();
        audio = GetComponent<AudioSource>();


        canShoot = true;
        isReloading = false;
        canPickup = true;
        ChangeWeapon(currentWeapon);
        var ammoInfo = currentWeapon.GetAmmoInfo();
        wepUI.initializeReloadUI(transform, currentWeapon.weaponSprite, ammoInfo.currentClipAmmo, ammoInfo.currenAmmo, ammoInfo.unlimitedAmmo);
    }

    void Update() {
        if (!isAI && !playerController.isDown()) {
            if (Input.GetButton("Fire1") && currentWeapon != null && canShoot && !isReloading)
                StartCoroutine(FireBullet(currentWeapon.GetObjects().bullet, GetCharaterAimDirection(cam.WorldToScreenPoint(currentWeapenMuzzle.position), Input.mousePosition), null));

            if (Input.GetButton("Reload") && currentWeapon != null && currentWeapon.GetAmmoInfo().currenAmmo > 0 && currentWeapon.AmmoMissing() > 0 && !isReloading)
                ReloadWeapon();

            if (Input.GetButtonDown("Drop") && !isReloading && canShoot)
                DropAmmoCreate();

            if (Input.GetButtonDown("DropWep") && !isReloading && canShoot)
                DropWeapon();
        }
    }

    public void ReloadWeapon() {
        if (currentWeapon != null && currentWeapon.GetAmmoInfo().currenAmmo > 0 && currentWeapon.AmmoMissing() > 0 && !isReloading)
            StartCoroutine(ReloadWepaon());
    }

    public WeaponWrapper GetCurrentWeapon() {
        return currentWeapon;
    }

    public void DropAmmoCreate() {
        var ammo = currentWeapon.GetAmmoInfo();

        if (ammo.unlimitedAmmo)
            return;

        int ammoToDrop = maxAmmoDrop;

        if (ammo.currenAmmo < maxAmmoDrop)
            ammoToDrop = ammo.currenAmmo;

        if (ammoToDrop > 0) {
            currentWeapon.RemoveAmmo(ammoToDrop);
            Vector2 dir = -transform.up;
            GameObject instance = Instantiate(ammoCreate, (Vector2)transform.position + dir * ammoCreatSpawnOffset, Quaternion.identity);
            instance.GetComponent<PickupHandler>().ChangeAmmoAmount(ammoToDrop);
            instance.GetComponent<Rigidbody2D>().AddForce(dir * ammoThrowForce, ForceMode2D.Impulse);
        }

        audio.PlayOneShot(dropSound);

        ammo = currentWeapon.GetAmmoInfo();
        wepUI.UpdateClipAmmo(ammo.currentClipAmmo, ammo.currenAmmo, ammo.unlimitedAmmo);

    }

    public List<GameObject> DropWeapon() {
        
        if (currentWeapon == null)
            ChangeWeapon(startWeapon.InisiazlieWeapon());

        if (currentWeapon.weaponType == startWeapon.weaponType)
            return null;

        List<GameObject> droppedItems = playerController.DropCurrentWeapon(currentWeapon);
        ChangeWeapon(startWeapon.InisiazlieWeapon());
        
        return droppedItems;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!isAI && Input.GetButton("Pickup") && canPickup && !playerController.isDown() && ((1 << other.gameObject.layer) & pickupLayer) != 0) {
            PickupItem(other);
        }
    }

    public void PickupItem(Collider2D other) {
        StartCoroutine(PickUpDrop(other));
    }

    private IEnumerator PickUpDrop(Collider2D other) {
        
        canPickup = false;
        var pickup = other.gameObject.GetComponent<PickupHandler>().GetPickup();

        switch (pickup.type) {
            case PickupType.Ammo:
                currentWeapon.AddAmmo(pickup.ammo);
                break;
            case PickupType.Health:
                hp.RecoverHealth(pickup.health);
                break;
            case PickupType.Weapon:
                if(currentWeapon.weaponType != startWeapon.weaponType)
                    playerController.DropCurrentWeapon(currentWeapon);
                ChangeWeapon(pickup.weapon);
                break;
        }

        audio.Play();

        var ammo = currentWeapon.GetAmmoInfo();
        wepUI.UpdateClipAmmo(ammo.currentClipAmmo, ammo.currenAmmo, ammo.unlimitedAmmo);

        Destroy(other.gameObject);
        yield return new WaitForSeconds(pickupDeley);
        canPickup = true;
    }

    private Vector2 GetCharaterAimDirection(Vector2 currentPosition, Vector2 targetPos) {
        return (targetPos - currentPosition).normalized;
    }

    public bool FireWeapon(Vector2 direction, lstmMLAgent agent) {
        if (!playerController.isDown() && currentWeapon != null && canShoot && !isReloading) {
            StartCoroutine(FireBullet(currentWeapon.GetObjects().bullet, direction, agent));
            return true;
        }

        return false;
    }

    private IEnumerator FireBullet(GameObject bullet, Vector2 direction, lstmMLAgent agent) {
        canShoot = false;

        var fireStatus = currentWeapon.FireWeapon(direction);

        if (!fireStatus.didShoot && currentWeapon.GetAmmoInfo().currenAmmo > 0) {
            StartCoroutine(ReloadWepaon());
            canShoot = true;
            yield break;
        }

        if (fireStatus.didShoot) {

            for (int i = 0; i < fireStatus.recoils.Count; i++) {
                GameObject instance = Instantiate(currentWeapon.projectile, currentWeapenMuzzle.position, transform.rotation * Quaternion.Euler(new Vector3(0, 0, fireStatus.recoils[i] * Mathf.Rad2Deg)));
                instance.gameObject.GetComponent<ProjectileScript>().Iniziate(fireStatus.rotDir[i], currentWeapon.GetWeaponInfo().damage, hitLayer, agent);
            }

            audio.PlayOneShot(currentWeapon.GetSounds().fire);
        }

        var ammo = currentWeapon.GetAmmoInfo();
        wepUI.UpdateClipAmmo(ammo.currentClipAmmo, ammo.currenAmmo, ammo.unlimitedAmmo);
        yield return new WaitForSeconds(currentWeapon.GetWeaponInfo().fireSpeed);

        canShoot = true;
    }

    private IEnumerator ReloadWepaon() {
        isReloading = true;
        wepUI.StartReload(currentWeapon.GetWeaponInfo().relaodSpeed);
        StartCoroutine(ReloadSound(currentWeapon.GetSounds().reload));
        yield return new WaitForSeconds(currentWeapon.GetWeaponInfo().relaodSpeed);
        if (currentWeapon != null) {
            currentWeapon.ReloadWeapon();
            var ammoInfo = currentWeapon.GetAmmoInfo();
            wepUI.UpdateClipAmmo(ammoInfo.currentClipAmmo, ammoInfo.currenAmmo, ammoInfo.unlimitedAmmo);
            isReloading = false;
        }
    }

    private IEnumerator ReloadSound(AudioClip[] sound) {
        for (int i = 0; i < sound.Length; i++) {
            audio.PlayOneShot(sound[i]);
            yield return new WaitForSeconds(sound[i].length - 0.1f);
        }
    }

    public void ChangeWeapon(WeaponWrapper newWeapon) {
        Destroy(currentWeaponObj);
        currentWeapon = newWeapon;

        if (currentWeapon == null) {
            wepUI.UpdateWeaponSprite(null);
            wepUI.UpdateClipAmmo(0, 0, false);
            return;
        }

        currentWeaponObj = Instantiate(newWeapon.GetObjects().weapon, WeaponHolder);
        currentWeapenMuzzle = currentWeaponObj.transform.GetChild(0);
        wepUI.UpdateWeaponSprite(currentWeapon.weaponSprite);

        var ammo = currentWeapon.GetAmmoInfo();
        wepUI.UpdateClipAmmo(ammo.currentClipAmmo, ammo.currenAmmo, ammo.unlimitedAmmo);
    }
}
