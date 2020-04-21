using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour {

    [SerializeField] private float autoDestroyTime;

    private int damage;
    private int layerMask;
    private lstmMLAgent sender;

    public void Iniziate(Vector2 force, int damage, int layerMask, lstmMLAgent sender) {
        this.damage = damage;
        this.layerMask = layerMask;
        this.sender = sender;
        Destroy(gameObject, autoDestroyTime);
        GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (((1 << other.gameObject.layer) & layerMask) != 0) {
            bool result = other.gameObject.GetComponent<HealthController>().ReduceHealth(damage);

            if (result && sender != null && sender.enemyFocus == other.gameObject)
                sender.GiveReward(1f);

            if (sender != null && sender.enemyFocus == other.gameObject)
                sender.GiveReward(0.1f);

            Destroy(gameObject);
        }
    }

}
