using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class lstmMLAgent : Agent {

    [Header("Input settings")]
    [SerializeField] private bool useGridInput;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2 cellSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float defualtGridValue;
    [SerializeField] private LayerMask[] layers;
    [SerializeField] private Color gridColor;

    [SerializeField] private int enemies;
    [SerializeField] private Vector2 viewSize;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask smgLayer;
    [SerializeField] private LayerMask rifleLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float detectionRadius;

    [Header("Movement settings")]
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float movementSlowTolorance;
    [SerializeField] private float drag;
    [SerializeField, Range(0, 1)] private float standDeadZone;
    [SerializeField] private float rotationSpeed;
    [SerializeField, Range(0, 1)] private float deadZoneRotation;

    [Header("Pickup settings")]
    [SerializeField] private LayerMask hpLayer;
    [SerializeField] private LayerMask wepLayer;
    [SerializeField] private LayerMask ammoLayer;

    [Header("References")]
    [SerializeField] private Transform otherPlayer;
    [SerializeField] private AcademyBehaviours ac;

    private LayerMask combinedLayerMask;
    private Dictionary<LayerMask, int> layerMaskTable = new Dictionary<LayerMask, int>();

    private HealthController hc;
    private HealthController otherHc;
    private PlayerController pc;
    private PlayerController otherPc;
    private WeaponHandler wh;
    private WeaponHandler otherWh;
    private Rigidbody2D rb2d;
    private float lastHpDistance;
    private float lastAmmoDistance;
    private float lastSmgDistance;
    private float lastAssultDistance;
    private float lastPlayerDistance;
    private float lastRotation;
    private float lastLimitedDistance = 0;
    private float enemyViewCounter;

    private float lastAngle = 1000;
    private Vector2 lastPos;
    private Vector2 startPos;

    public GameObject enemyFocus;

    [SerializeField]
    private bool useObs;

    //public override void OnEpisodeBegin() {
    //    transform.position = startPos;
    //    lastPos = startPos;
    //    wh.ChangeWeapon(wh.startWeapon.InisiazlieWeapon());
    //    hc.RecoverHealth(hc.maxHp);
    //    pc.Restored();
    //    otherPc.Restored();
    //    ac.ResetScene();
    //}

    public override void Initialize() {
        startPos = transform.position;
        lastPos = startPos;
        hc = GetComponent<HealthController>();
        pc = GetComponent<PlayerController>();
        wh = GetComponent<WeaponHandler>();
        rb2d = GetComponent<Rigidbody2D>();
        otherHc = otherPlayer.GetComponent<HealthController>();
        otherPc = otherPlayer.GetComponent<PlayerController>();
        otherWh = otherPlayer.GetComponent<WeaponHandler>();
        //enemyViewCounter = new float[enemies];

        string[] layerNames = new string[layers.Length];
        for (int i = 0; i < layerNames.Length; i++) {
            int counter = 0;
            int currentLayer = layers[i];

            while (currentLayer > 0) {
                counter++;
                currentLayer = currentLayer >> 1;
            }

            layerNames[i] = LayerMask.LayerToName(counter - 1);
        }

        combinedLayerMask = LayerMask.GetMask(layerNames);

        layerMaskTable = new Dictionary<LayerMask, int>();
        for (int i = 0; i < layers.Length; i++)
            layerMaskTable.Add(layers[i], i);

        lastRotation = transform.rotation.eulerAngles.z;
    }

    private float GetRot(Vector2 a, Vector2 b) {
        Vector2 dir = b - a;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public override void CollectObservations(VectorSensor sensor) {

        Collider2D[] detectedEnemies = Physics2D.OverlapBoxAll(startPos, viewSize, 0, enemyLayer);

        if (useObs) {
            if (useGridInput) {
                float[] view = GetView();
                foreach (var value in view)
                    sensor.AddObservation(value);
            } else {

                
                RaycastHit2D[] limitedEnemies = Physics2D.CircleCastAll(transform.position, detectionRadius, Vector2.up, 1, enemyLayer + wallLayer);
                Vector2 pos = rb2d.position;

                Collider2D hpPickup = Physics2D.OverlapBox(startPos, viewSize, 0, hpLayer);
                if (hpPickup != null) {
                    Vector2 hpPos = hpPickup.transform.position;
                    float hpDistance = Vector2.Distance(hpPos, pos);
                    float hpAngle = GetRot(pos, hpPos);
                    //float hpAngle = Vector2.SignedAngle(hpPos, transform.position);

                    float currentDistance = Vector2.Distance(hpPos, transform.position);
                    if (currentDistance < lastHpDistance) {
                        AddReward((lastHpDistance - currentDistance) * 0.01f);
                    }
                    lastHpDistance = currentDistance;

                    sensor.AddObservation(1 - (hpDistance / 21));
                    sensor.AddObservation((hpAngle / 180));
                } else {
                    sensor.AddObservation(-1);
                    sensor.AddObservation(0);
                }

                Collider2D ammoPickup = Physics2D.OverlapBox(startPos, viewSize, 0, ammoLayer);
                if (ammoPickup != null) {
                    Vector2 ammoPos = ammoPickup.transform.position;
                    float ammoDistance = Vector2.Distance(ammoPos, pos);
                    float ammoAngle = GetRot(pos, ammoPos);

                    float currentDistance = Vector2.Distance(ammoPos, transform.position);
                    if (currentDistance < lastAmmoDistance) {
                        AddReward((lastAmmoDistance - currentDistance) * 0.00001f);
                    }
                    lastAmmoDistance = currentDistance;

                    sensor.AddObservation(1 - (ammoDistance / 21));
                    sensor.AddObservation((ammoAngle / 180));
                } else {
                    sensor.AddObservation(-1);
                    sensor.AddObservation(0);
                }

                Collider2D smgPickup = Physics2D.OverlapBox(startPos, viewSize, 0, smgLayer);
                if (smgPickup != null) {
                    Vector2 smgPos = smgPickup.transform.position;
                    float smgDistance = Vector2.Distance(smgPos, pos);
                    float smgAngle = GetRot(pos, smgPos);

                    float currentDistance = Vector2.Distance(smgPos, transform.position);
                    if (currentDistance < lastSmgDistance) {
                        AddReward((lastSmgDistance - currentDistance) * 0.00001f);
                    }
                    lastSmgDistance = currentDistance;

                    sensor.AddObservation(1 - (smgDistance / 21));
                    sensor.AddObservation((smgAngle / 180));
                } else {
                    sensor.AddObservation(-1);
                    sensor.AddObservation(0);
                }

                Collider2D riflePickup = Physics2D.OverlapBox(startPos, viewSize, 0, rifleLayer);
                if (riflePickup != null) {
                    Vector2 riflePos = riflePickup.transform.position;
                    float rifleDistance = Vector2.Distance(riflePos, pos);
                    float rifleAngle = GetRot(pos, riflePos);

                    float currentDistance = Vector2.Distance(riflePos, transform.position);
                    if (currentDistance < lastAssultDistance) {
                        AddReward((lastAssultDistance - currentDistance) * 0.00001f);
                    }
                    lastAssultDistance = currentDistance;

                    sensor.AddObservation(1 - (rifleDistance / 21));
                    sensor.AddObservation((rifleAngle / 180));
                } else {
                    sensor.AddObservation(-1);
                    sensor.AddObservation(0);
                }

                //for (int i = 0; i < enemies; i++) {
                //    if (detectedEnemies.Length > i) {
                //        if (detectedEnemies[i] != null) {
                //            Vector2 targetPos = detectedEnemies[i].transform.position - transform.position;
                //            float distance = Vector2.Distance(detectedEnemies[i].transform.position, pos);
                //            //float angle = Mathf.Atan2(pos.x - targetPos.x, pos.y - targetPos.y) * Mathf.Rad2Deg;
                //            float angle = Vector2.SignedAngle(-transform.up, targetPos);
                //            bool ranged = detectedEnemies[i].GetComponent<EnemyController>().IsRanged;

                //            sensor.AddObservation(1 - (distance / 21));
                //            sensor.AddObservation(1 - (angle / 180));
                //            sensor.AddObservation(ranged);

                //            if (angle > -1f && angle < 1f) {

                //                AddReward(enemyViewCounter[i]);
                //                enemyViewCounter[i] += 0.00001f;
                //                Debug.Log("looking");
                //            } else {
                //                enemyViewCounter[i] = 0;
                //            }
                //        }
                //    } else {
                //        enemyViewCounter[i] = 0;
                //        sensor.AddObservation(-1);
                //        sensor.AddObservation(2);
                //        sensor.AddObservation(-1);
                // }
                //}

                int closestEnemy = 0;
                float minimumDistance = 10000;
                for (int i = 0; i < detectedEnemies.Length; i++) {
                    float distance = Vector3.Distance(detectedEnemies[i].transform.position, transform.position);
                    if (distance < minimumDistance) {
                        minimumDistance = distance;
                        closestEnemy = i;
                    }
                }

                if (detectedEnemies.Length > 0) {
                    GameObject newEnemy = detectedEnemies[closestEnemy].gameObject;
                    if (newEnemy != enemyFocus) {
                        if (enemyFocus != null)
                            //enemyFocus.GetComponent<SpriteRenderer>().color = Color.white;
                        enemyFocus = newEnemy;
                        //enemyFocus.GetComponent<SpriteRenderer>().color = Color.black;
                    }

                    Vector2 targetPos = detectedEnemies[closestEnemy].transform.position - transform.position;
                    float distance = Vector2.Distance(detectedEnemies[closestEnemy].transform.position, pos);
                    float angle = Vector2.SignedAngle(-transform.up, targetPos);

                    sensor.AddObservation(1 - (minimumDistance / 21));
                    sensor.AddObservation((angle / 180));

                    if (minimumDistance > 2 && minimumDistance < 3) {
                        AddReward(0.001f);
                    }

                    if (Mathf.Abs(angle) > Mathf.Abs(lastAngle)) {
                        AddReward(-0.01f);
                    } else {
                        AddReward(0.0001f);
                    }

                    lastAngle = angle;

                    if (angle > -5f && angle < 5f) {

                        //enemyViewCounter += 0.001f;
                        AddReward(0.001f);
                    } else {
                        enemyViewCounter = 0;
                        //AddReward(-Mathf.Abs((angle / 180) * 0.00001f));
                    }

                } else {
                    sensor.AddObservation(-1);
                    sensor.AddObservation(0);
                }


                float origoDistance = Vector2.Distance(startPos, pos);
                float origoAngle = GetRot(pos, startPos);
                sensor.AddObservation(1 - (origoDistance / 21));
                sensor.AddObservation((origoAngle / 180));


                Vector2 result = Vector2.zero;
                for (int i = 0; i < limitedEnemies.Length; i++) {
                    result += (Vector2)limitedEnemies[i].point;
                }
                if (limitedEnemies.Length == 0) {
                    //AddReward(0.01f);
                } else {
                    AddReward(-0.01f);
                }

                if (limitedEnemies.Length > 0) {
                    result = new Vector2(result.x / limitedEnemies.Length, result.y / limitedEnemies.Length);
                    float distance = Vector2.Distance(result, pos);
                    if (distance > lastLimitedDistance) {
                        AddReward(0.01f);
                    }
                    lastLimitedDistance = distance;
                    float resultAngle = GetRot(pos, result);
                    sensor.AddObservation((resultAngle / 180));
                } else {
                    sensor.AddObservation(0);
                }

                //if (Vector2.Distance(transform.position, startPos) < Vector2.Distance(lastPos, startPos)) {
                //    AddReward(0.001f);
                //} else {
                //    AddReward(-0.01f);
                //}

                lastPos = transform.position;


                //sensor.AddObservation(hc.CurrentHealthNormalized);

                //if (wh.GetCurrentWeapon() != null) {
                //    sensor.AddObservation(wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / wh.GetCurrentWeapon().GetAmmoInfo().maxAmmo);
                //    sensor.AddObservation((float)wh.GetCurrentWeapon().weaponType / 3f);
                //} else {
                //    sensor.AddObservation(30);
                //    sensor.AddObservation(0);
                //}

                //if (useGridInput) {
                //    sensor.AddObservation((transform.rotation.eulerAngles.z / 180f) - 1);
                //}

                //sensor.AddObservation(otherHc.CurrentHealthNormalized);
                //if (otherWh.GetCurrentWeapon() != null) {
                //    sensor.AddObservation(otherWh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / otherWh.GetCurrentWeapon().GetAmmoInfo().maxAmmo);
                //} else {
                //    sensor.AddObservation(30);
                //}
            }
        } else {
            int closestEnemy = 0;
            float minimumDistance = 10000;
            for (int i = 0; i < detectedEnemies.Length; i++) {
                float distance = Vector3.Distance(detectedEnemies[i].transform.position, transform.position);
                if (distance < minimumDistance) {
                    minimumDistance = distance;
                    closestEnemy = i;
                }
            }

            if (detectedEnemies.Length > 0) {
                GameObject newEnemy = detectedEnemies[closestEnemy].gameObject;
                if (newEnemy != enemyFocus) {
                    if (enemyFocus != null)
                    enemyFocus = newEnemy;
                }

                Vector2 targetPos = detectedEnemies[closestEnemy].transform.position - transform.position;
                lastAngle = Vector2.SignedAngle(-transform.up, targetPos);
            }

            RaycastHit2D hit2D = Physics2D.Raycast(transform.position, -transform.up, 21f, enemyLayer);

            if (hit2D.collider != null) {
                AddReward(0.0001f);
            }
        }

        //if (Vector2.Distance(transform.position, startPos) < 3) {
        //    AddReward(0.001f);
        //}
    }

    public void GiveReward(float value) {
        AddReward(value);
    }

    public override float[] Heuristic() {

        float[] result = new float[10];

        for (int i = 0; i < result.Length; i++) {
            result[i] = 0;
        }

        if (Input.GetKey(KeyCode.Space))
            result[0] = 1;

        if (Input.GetKey(KeyCode.Q))
            result[1] = 1;

        if (Input.GetKey(KeyCode.F)) {
            result[2] = 1;
        }

        result[3] = Input.GetAxis("Horizontal");
        result[4] = Input.GetAxis("Vertical");

        if (lastAngle < -5 && lastAngle > -180) {
            result[9] = 1f;
        } else if (lastAngle > 5 && lastAngle < 180) {
            result[9] = -1f;
        }

        if (Input.GetKey(KeyCode.E)){
            result[5] = 1;
            result[5] = 1;
            result[5] = 1;
            result[6] = 1;
        }
        //result[4] = ((lastRotation / 180f) - 1) - ((transform.rotation.eulerAngles.z / 180f) - 1);
        //lastRotation = transform.rotation.eulerAngles.z;


        return result;
    }

    public override void OnActionReceived(float[] vectorAction) {
        //string values = "";

        //for (int i = 0; i < vectorAction.Length; i++) {
        //    values += vectorAction[i].ToString() + ", ";
        //}

        //Debug.Log(values);

        if (pc.isDown()) {
            AddReward(-1f);
            //EndEpisode();
        }

        if (vectorAction[0] > 0.1f) {
            wh.FireWeapon(-transform.up, this);
            //AddReward(0.0001f);
        }
        /*else if (vectorAction[1] > 0.5f)
            wh.ReloadWeapon();*/

        if (vectorAction[1] >= 0.5f) {
            wh.DropAmmoCreate();
            if (wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo > 100 && otherWh.GetCurrentWeapon().GetAmmoInfo().currenAmmo < 30)
                GiveReward(0.0005f);
        }

        //if (vectorAction[2] >= 0.5f) {
        //    wh.DropWeapon();
        //    if (wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo <= 0)
        //        GiveReward(0.0005f);
        //}

        if (vectorAction[2] >= 0.5f) {
            RaycastHit2D hpHit = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, hpLayer);

            if (hpHit.collider != null) {
                wh.PickupItem(hpHit.collider);
                GiveReward(1 - hc.CurrentHealthNormalized);
            }
        }

        //if (vectorAction[4] >= 0.5f) {

        //    RaycastHit2D ammoHiy = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, ammoLayer);

        //    if (ammoHiy.collider != null) {
        //        wh.PickupItem(ammoHiy.collider);
        //        float ammoNorm = wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / wh.GetCurrentWeapon().GetAmmoInfo().maxAmmo;
        //        GiveReward(1 - ammoNorm);
        //    }

        //}

        //if (vectorAction[5] >= 0.5f) {
        //    RaycastHit2D wepHit = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, wepLayer);

        //    if (wepHit.collider != null) {
        //        wh.PickupItem(wepHit.collider);
        //        if (wepHit.collider.gameObject.GetComponent<PickupHandler>().GetWeapon().weaponType != wh.GetCurrentWeapon().weaponType) {
        //            GiveReward(0.0005f);
        //        }
        //    }
        //}

        //if (vectorAction[6] >= 0.5f) {
        //    if (Vector2.Distance(transform.position, otherPlayer.position) <= 1) {
        //        otherPc.Restored();
        //        GiveReward(0.05f);
        //    }
        //}

        MoveCharacter(vectorAction[3], vectorAction[4]);

        if (vectorAction[3] > -standDeadZone && vectorAction[3] < standDeadZone) {
            rb2d.AddForce(Vector2.right * -rb2d.velocity.normalized.x * drag);
        }

        if (vectorAction[4] > -standDeadZone && vectorAction[4] < standDeadZone) {
            rb2d.AddForce(Vector2.up * -rb2d.velocity.normalized.y * drag);
        }

        if (rb2d.velocity.magnitude > 0 && rb2d.velocity.magnitude < movementSlowTolorance) {
            rb2d.velocity = Vector2.zero;
        }

        if(vectorAction[5] < -deadZoneRotation || vectorAction[5] > deadZoneRotation) {
            transform.rotation *= Quaternion.Euler(new Vector3(0, 0, vectorAction[5] * -rotationSpeed));
            lastRotation = transform.rotation.eulerAngles.z;
        }
    }

    private void MoveCharacter(float horizontal, float vertical) {
        Vector2 direction = new Vector2(horizontal, vertical);
        rb2d.AddForce(direction.normalized * speed, ForceMode2D.Force);

        if (rb2d.velocity.magnitude > maxSpeed) {
            rb2d.velocity = rb2d.velocity.normalized * maxSpeed;
        }
    }

    public float[] GetView() {
        float[] result = new float[gridSize.x * gridSize.y];

        for (int i = 0; i < result.Length; i++)
            result[i] = defualtGridValue;

        Vector2 bounds = gridSize * cellSize;

        for (int i = 0; i < gridSize.y; i++) {
            float currentY = i * cellSize.y - bounds.y / 2;

            for (int j = 0; j < gridSize.x; j++) {
                float currentX = j * cellSize.x - bounds.x / 2;
                Vector2 currenPos = new Vector2(currentX + cellSize.x / 2, currentY + cellSize.y / 2) + offset;

                Collider2D[] collisions = Physics2D.OverlapBoxAll(currenPos, cellSize, 0, combinedLayerMask);

                if (collisions.Length > 0) {
                    if (collisions.Length > 1) {
                        int counter = 0;

                        foreach (var collision in collisions) {
                            result[i * j] += layerMaskTable[1 << collision.gameObject.layer];
                            counter++;
                        }

                        result[i * j] /= counter;
                        result[i * j] /= layers.Length + 1;

                    } else
                        result[i * j] = layerMaskTable[1 << collisions[0].gameObject.layer] + 1;
                }
            }
        }

        return result;
    }

    private void OnDrawGizmosSelected() {

        Gizmos.color = gridColor;
        Vector2 bounds = gridSize * cellSize;
        Gizmos.DrawWireCube(offset, bounds);
        if (useGridInput) {
            for (int i = 0; i < gridSize.y; i++) {
                float currentY = i * cellSize.y - bounds.y / 2;

                for (int j = 0; j < gridSize.x; j++) {
                    float currentX = j * cellSize.x - bounds.x / 2;
                    Vector2 currenPos = new Vector2(currentX + cellSize.x / 2, currentY + cellSize.y / 2) + offset;

                    Gizmos.DrawWireCube(currenPos, cellSize);
                }
            }
        } else {
            Gizmos.DrawWireCube(transform.position, viewSize);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

    }

}
