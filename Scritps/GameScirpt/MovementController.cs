using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {

    [SerializeField] private float speed;
    [SerializeField] private float drag;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float movementSlowTolorance;
    [SerializeField] private float standDeadZone;

    private Rigidbody2D rb2d;
    private PlayerController pc;

    void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();
    }

    void Update() {
        if(!pc.isDown()) {
            if(Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
                MoveCharacter(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));  

            //} else if(rb2d.velocity.magnitude > movementSlowTolorance) {            
            //    rb2d.AddForce(-rb2d.velocity.normalized * drag);
            //} else if (rb2d.velocity.magnitude > 0 && rb2d.velocity.magnitude < movementSlowTolorance) {
            //    rb2d.velocity = Vector2.zero;
            //}

            if (Input.GetAxis("Horizontal") > -standDeadZone && Input.GetAxis("Horizontal") < standDeadZone) {
                rb2d.AddForce(Vector2.right * -rb2d.velocity.normalized.x * drag);
            }

            if (Input.GetAxis("Vertical") > -standDeadZone && Input.GetAxis("Vertical") < standDeadZone) {
                rb2d.AddForce(Vector2.up * -rb2d.velocity.normalized.y * drag);
            }

            if (rb2d.velocity.magnitude > 0 && rb2d.velocity.magnitude < movementSlowTolorance) {
                rb2d.velocity = Vector2.zero;
            }

        }
    }

    private void MoveCharacter(float horizontal, float vertical) {
        Vector2 direction = new Vector2(horizontal, vertical);
        rb2d.AddForce(direction.normalized * speed, ForceMode2D.Force);

        if (rb2d.velocity.magnitude > maxSpeed) {
            rb2d.velocity = rb2d.velocity.normalized * maxSpeed;
        }
    }

}
