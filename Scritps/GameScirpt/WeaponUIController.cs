using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour {

    [SerializeField] private Image reloadTimerBar;
    [SerializeField] private GameObject reloadBar; 
    [SerializeField] private Image weaponImage;
    [SerializeField] private Text ammoText;
    [SerializeField] private float barTicks;
    [SerializeField] private float UIDestorySpeed;
    [SerializeField] private int barframeOffset;
    [SerializeField] private Vector2 offsetFromHolder;

    private Transform currentHolder;

    public void initializeReloadUI(Transform holder, Sprite weaponImage, int currenAmmo, int maxAmmo, bool unlimitedAmmo) {
        currentHolder = holder;
        this.weaponImage.sprite = weaponImage;
        UpdateClipAmmo(currenAmmo, maxAmmo, unlimitedAmmo);
    }

    public void UpdateWeaponSprite(Sprite wepImage) {
        if(wepImage != null) {
            weaponImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(wepImage.texture.width, wepImage.texture.height);
            Color c = weaponImage.color;
            c.a = 1;
            weaponImage.color = c;
        } else {
            Color c = weaponImage.color;
            c.a = 0;
            weaponImage.color = c;
        }
        
        weaponImage.sprite = wepImage;
    }

    public void DestoryAmmoUI() {
        Destroy(gameObject, UIDestorySpeed);
    }

    public void UpdateClipAmmo(int currentClipAmmom, int curretMaxAmmo, bool unlimtedAmmo) {

        if(unlimtedAmmo) {
            ammoText.text = currentClipAmmom + "/ ∞";
            return;
        }
        
        ammoText.text = currentClipAmmom + "/" + curretMaxAmmo;
    }

    public void StartReload(float reloadTime) {
        StartCoroutine(StartReloadUI(reloadTime));
    }

    private IEnumerator StartReloadUI(float reloadTime) {
        float currentTime = 0;
        float tickAmount = reloadTime / barTicks;
        reloadBar.SetActive(true);

        while(currentTime + tickAmount * barframeOffset < reloadTime) {
            currentTime += tickAmount;
            UpdateBarUi(currentTime + tickAmount * barframeOffset, reloadTime);
            yield return new WaitForSeconds(tickAmount);
        }

        reloadBar.SetActive(false);

    }

    private void UpdateBarUi(float current, float reloadTime) {
        reloadTimerBar.fillAmount = current / reloadTime;
    }

    private void Update() {
        if(currentHolder != null)
            transform.position = currentHolder.position + (Vector3)offsetFromHolder;
    }


}
