using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AcademyBehaviours : MonoBehaviour
{
    private SpawnController sp;

    [SerializeField] private Vector2 boxCastArea;
    [SerializeField] private LayerMask detectionLayers;
    [SerializeField] private LayerMask dropLayer;

    // Start is called before the first frame update
    void Start()
    {
        sp = FindObjectOfType<SpawnController>();

        //Academy.Instance.OnEnvironmentReset += ResetScene;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.N))
            ClearItems();
    }

    public void ResetScene() {
        sp.NewStart();
        ClearItems();
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, boxCastArea, 0, Vector2.zero, 0, detectionLayers);
        foreach(RaycastHit2D hit in hits) {
            Destroy(hit.transform.gameObject);
        }
    }

    public void ClearItems() {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, boxCastArea, 0, Vector2.zero, 0, dropLayer);
        foreach (RaycastHit2D hit in hits) {
            Destroy(hit.transform.gameObject);
        }
    }
}
