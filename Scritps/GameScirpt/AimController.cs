using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour {

    private Rigidbody2D rb2d;
    private Camera cam;
    private PlayerController pc;

    void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        pc = GetComponent<PlayerController>();
    }

    void Update() {
        if(!pc.isDown())
            RotatePlayer();
    }

    private void RotatePlayer() {
        Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 transPos = rb2d.position;
        float angle = Mathf.Atan2(transPos.x - pos.x, transPos.y - pos.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0,0,-angle));
    }

}
