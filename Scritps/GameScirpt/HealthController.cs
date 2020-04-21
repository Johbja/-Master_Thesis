using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{

    public int maxHp;
    [SerializeField] private bool isPlayer;
    [SerializeField] private Transform hpBarHolder;
    [SerializeField] private GameObject hpBar;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip[] deathSounds;

    public int CurrenHealth { get { return currentHealth; } }
    public float CurrentHealthNormalized { get { return currentHealth / maxHp; } }

    private int currentHealth;
    private HealthBarController hpController;
    private PlayerController pc;
    private ItemDropScirpt dropper;
    private AudioSource audio;

    public void InitinializeHealthController(Transform hpBarHolder)
    {
        currentHealth = maxHp;
        pc = GetComponent<PlayerController>();
        dropper = GetComponent<ItemDropScirpt>();
        audio = GetComponent<AudioSource>();

        GameObject instance;
        if (hpBarHolder == null)
            instance = Instantiate(hpBar, this.hpBarHolder);
        else
            instance = Instantiate(hpBar, hpBarHolder);

        hpController = instance.GetComponent<HealthBarController>();
        hpController.initializeHpBar(transform, currentHealth, maxHp);
        instance.SetActive(false);
        Invoke("ActivateHpBar", 1f);
    }

    private void ActivateHpBar() {
        hpController.gameObject.SetActive(true);
    }

    public bool ReduceHealth(int amount)
    {
        if(isPlayer && pc.isDown()) {
            currentHealth = 0;
            hpController.UpdateHpBar(currentHealth, maxHp);
            return false;
        }

        currentHealth -= amount;
        hpController.UpdateHpBar(currentHealth, maxHp);

        audio.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length)]);

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            AudioClip a = deathSounds[Random.Range(0, deathSounds.Length)];
            GameObject soundObj = new GameObject();
            soundObj.AddComponent<AudioSource>();
            soundObj.transform.position = transform.position;
            soundObj.GetComponent<AudioSource>().PlayOneShot(a);
            Destroy(soundObj, a.length);

            if(!isPlayer) {
                hpController.DestoryHealthbar();
                dropper.DropItem();
                Destroy(gameObject);
                return true;
                
            } else if(!pc.isDown()){
                pc.OnDown();
                
            } 
        }
        return false;
    }

    public void RecoverHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHp)
            currentHealth = maxHp;

        hpController.UpdateHpBar(currentHealth, maxHp);
    }
}
