using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTester : MonoBehaviour {

    [SerializeField] private int bulletDamage;
    [SerializeField] private float relaodSpeed;
    [SerializeField] private float bulletStartForce;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private LayerMask hitLayer;

    private Camera cam;
    private bool canShoot;

    private void Start() {
        cam = Camera.main;
        canShoot = true;
    }

    void Update() {
        if(Input.GetButton("Fire1") && canShoot)
            StartCoroutine(FireBullet(bullet));            
    }

    private IEnumerator FireBullet(GameObject bullet) {
        canShoot = false;
        Vector2 pos = Input.mousePosition;
        Vector2 transPos = cam.WorldToScreenPoint(transform.position);
        Vector2 dir = (pos - transPos).normalized;

        GameObject instance = Instantiate(bullet, (Vector2)transform.position + dir, transform.rotation, bulletParent);
        instance.GetComponent<ProjectileScript>().Iniziate(dir * bulletStartForce, bulletDamage, hitLayer, null);
        yield return new WaitForSeconds(relaodSpeed);
        canShoot = true;
    }

}
