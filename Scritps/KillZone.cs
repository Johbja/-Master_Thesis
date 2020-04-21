using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        HealthController hc = collision.GetComponent<HealthController>();
        if (hc != null)
            hc.ReduceHealth(1000);
    }
}
