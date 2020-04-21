using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

    [SerializeField] private Image hpBar;
    [SerializeField] private Text hpText;
    [SerializeField] private float hpBarSpeed;
    [SerializeField] private float hpBarDestorySpeed;
    [SerializeField] private Vector2 offsetFromHolder;

    private float currentHpTarget;
    private Transform currentHolder;

    public void initializeHpBar(Transform holder, int currentHp, int maxHp) {
        UpdateHpBar(currentHp, maxHp);
        currentHolder = holder;
    }

    public void UpdateHpBar(int currentHp, int maxHp) {
        currentHpTarget = (float)currentHp / (float)maxHp;
        hpText.text = currentHp + " / " + maxHp;
    }

    public void DestoryHealthbar() {
        Destroy(gameObject, hpBarDestorySpeed);
    }

    private void Update() {
        hpBar.fillAmount = Mathf.MoveTowards(hpBar.fillAmount, currentHpTarget, hpBarSpeed);

        if (currentHolder != null)
            transform.position = currentHolder.position + (Vector3)offsetFromHolder;
        else
            Destroy(gameObject);
    }

}
