using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    [Header("Generall Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private int damage;
    [SerializeField] private float damageRange;
    [SerializeField] private float attackDeley;

    [Header("Range settings")]
    [SerializeField] private bool isRanged;
    [SerializeField] private float bulletStartForce;
    [SerializeField] private GameObject projectile;
    [SerializeField] private LayerMask hitLayer;

    [Header("Change target settings")]
    [SerializeField] private float changeIntervall;
    [SerializeField, Range(0, 1)] private float changeChance;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] attackSounds;

    public bool IsRanged { get { return isRanged; } }

    private Transform[] targets;
    private int currentTarget;
    private Rigidbody2D rb2d;
    private AudioSource audio;
    private bool canAttack;

    public void InizialiseEnemey(Transform[] targets, Transform hpBarHolder) {
        this.targets = targets;

        rb2d = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
        canAttack = true;

        GetComponent<HealthController>().InitinializeHealthController(hpBarHolder);

        if (targets.Length > 1 && Vector2.Distance(targets[0].position, transform.position) < Vector2.Distance(targets[1].position, transform.position)) {
            currentTarget = 1;
        } else {
            currentTarget = 0;
        }
    }

    private IEnumerator ChangeTarget() {
        WaitForSeconds time = new WaitForSeconds(changeIntervall);

        while (true) {
            if(Random.Range(0f, 1f) <= changeChance) {
                ChangeTargetValue();
            }
            yield return time;
        }
    }

    private void ChangeTargetValue() {
        if(currentTarget == 1) 
            currentTarget = 0;
         else 
            currentTarget = 1;
    }

    void FixedUpdate() {

        //move
        rb2d.position = Vector2.MoveTowards(rb2d.position, targets[currentTarget].position, movementSpeed);

        //rotate towards target
        float angle = Mathf.Atan2(rb2d.position.x - targets[currentTarget].position.x, rb2d.position.y - targets[currentTarget].position.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));

        //Attack target
        if(canAttack && Vector2.Distance(rb2d.position, targets[currentTarget].position) <= damageRange) {
            StartCoroutine(Attack());
        }
    }

    public IEnumerator Attack() {
        canAttack = false;

        float volume = audio.volume;
        audio.volume = 0.25f;
        audio.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);
        audio.volume = volume;

        if(isRanged) {
            Vector2 dir = (targets[currentTarget].position - transform.position).normalized;
            GameObject instance = Instantiate(projectile, (Vector2)transform.position + dir, transform.rotation);
            instance.GetComponent<ProjectileScript>().Iniziate(dir * bulletStartForce, damage, hitLayer, null);
        } else {
            targets[currentTarget].gameObject.GetComponent<HealthController>().ReduceHealth(damage);
        }

        yield return new WaitForSeconds(attackDeley);
        canAttack = true;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }

}
