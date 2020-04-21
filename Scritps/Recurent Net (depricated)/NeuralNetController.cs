//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class NeuralNetController : MonoBehaviour {

//    [Header("Cell block settings")]
//    [SerializeField] private int blocks;
//    [SerializeField] private int extraInputSize;
//    [SerializeField] private int outputSize;
//    [SerializeField] private float wieghtRange;
//    [SerializeField] private float biasRange;

//    [Header("Input settings")]
//    [SerializeField] private Vector2Int gridSize;
//    [SerializeField] private Vector2 cellSize;
//    [SerializeField] private Vector2 offset;
//    [SerializeField] private float defualtGridValue;
//    [SerializeField] private LayerMask[] layers;
//    [SerializeField] private Color gridColor;

//    [Header("Pickup settings")]
//    [SerializeField] private LayerMask hpLayer;
//    [SerializeField] private LayerMask wepLayer;
//    [SerializeField] private LayerMask ammoLayer;

//    [Header("Movement settings")]
//    [SerializeField] private float speed;
//    [SerializeField] private float maxSpeed;
//    [SerializeField] private float movementSlowTolorance;
//    [SerializeField] private float drag;
//    [SerializeField, Range(0, 1)] private float standDeadZone;
//    [SerializeField] private float rotationSpeed;

//    [Header("traning settings")]
//    [SerializeField] private int timeSteps;

//    [SerializeField] private float learningRate;
//    [SerializeField] private float recodingTime;

//    [Header("references")]
//    [SerializeField] private Transform otherPlayer;

//    private HealthController hc;
//    private HealthController otherHc;
//    private PlayerController pc;
//    private PlayerController otherPc;
//    private WeaponHandler wh;
//    private WeaponHandler otherWh;
//    private NeuralNetwork lstm;
//    private Rigidbody2D rb2d;
//    private LayerMask combinedLayerMask;
//    private Dictionary<LayerMask, int> layerMaskTable;
//    private float[] input;
//    private int traningCyckels;
//    private float lastRotation;
//    private bool shouldCompute;

//    private void Start() {
//        //input = new float[gridSize.x * gridSize.y + extraInputSize];

//        //traningCyckels = (int)((recodingTime/Time.fixedDeltaTime)/timeSteps);
//        //lstm = new NeuralNetwork(blocks, (gridSize.x * gridSize.y) + extraInputSize, outputSize, wieghtRange, biasRange, learningRate, timeSteps, traningCyckels);

//        hc = GetComponent<HealthController>();
//        pc = GetComponent<PlayerController>();
//        wh = GetComponent<WeaponHandler>();
//        rb2d = GetComponent<Rigidbody2D>();
//        otherHc = otherPlayer.GetComponent<HealthController>();
//        otherPc = otherPlayer.GetComponent<PlayerController>();
//        otherWh = otherPlayer.GetComponent<WeaponHandler>();

//        string[] layerNames = new string[layers.Length];
//        for(int i = 0; i < layerNames.Length; i++) {
//            int counter = 0;
//            int currentLayer = layers[i];

//            while(currentLayer > 0) {
//                counter++;
//                currentLayer = currentLayer >> 1;
//            }

//            layerNames[i] = LayerMask.LayerToName(counter - 1);
//        }

//        combinedLayerMask = LayerMask.GetMask(layerNames);

//        layerMaskTable = new Dictionary<LayerMask, int>();
//        for(int i = 0; i < layers.Length; i++)
//            layerMaskTable.Add(layers[i], i);

//        lastRotation = transform.rotation.eulerAngles.z;
//        //shouldCompute = true;
//        //StartCoroutine(record());
//    }

//    private IEnumerator record() {

//        while (true) {

//            float timeLeft = recodingTime;

//            float[][][] inputs = new float[traningCyckels][][];
//            float[][][] outputs = new float[traningCyckels][][];

//            for (int i = 0; i < inputs.Length; i++) {
//                inputs[i] = new float[timeSteps][];
//                outputs[i] = new float[timeSteps][];
//            }

//            int cycel = 0;

//            while (timeLeft > 0) {

//                for (int i = 0; i < timeSteps; i++) {
//                    float[] view = GetView();

//                    inputs[cycel][i] = new float[gridSize.x * gridSize.y + extraInputSize];

//                    for (int j = 0; j < view.Length; j++) {
//                        inputs[cycel][i][j] = view[j];
//                    }

//                    inputs[cycel][i][gridSize.x * gridSize.y] = hc.CurrentHealthNormalized;
//                    inputs[cycel][i][gridSize.x * gridSize.y + 1] = wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / wh.GetCurrentWeapon().GetAmmoInfo().maxAmmo;
//                    inputs[cycel][i][gridSize.x * gridSize.y + 2] = (float)wh.GetCurrentWeapon().weaponType / 3f; //noramlizes weapon type
//                    inputs[cycel][i][gridSize.x * gridSize.y + 3] = transform.rotation.eulerAngles.z / 360;

//                    outputs[cycel][i] = GetPlayerAction();
//                    timeLeft -= Time.fixedDeltaTime;
//                    yield return new WaitForFixedUpdate();
//                }
//                Debug.Log("cycle" + cycel);
//                cycel++;
//            }

//            shouldCompute = false;
//            foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
//                if (!UnityEditor.EditorUtility.IsPersistent(obj.transform.root.gameObject) && !(obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)) {
//                    if (obj != gameObject && obj.transform != otherPlayer) {
//                        obj.SetActive(false);
//                    }
//                }
//            }

//            lstm.BackProp(inputs, outputs);

//            foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
//                if (!UnityEditor.EditorUtility.IsPersistent(obj.transform.root.gameObject) && !(obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)) {
//                    if (obj != gameObject && obj.transform != otherPlayer) {
//                        obj.SetActive(true);
//                    }
//                }
//            }

//            shouldCompute = true;

//        }
//    }

//    private float[] GetPlayerAction() {
//        float[] result = new float[outputSize];

//        for(int i = 0; i < result.Length; i++) {
//            result[i] = 0;
//        }

//        if (Input.GetButton("Fire1"))
//            result[0] = 1;

//        if (Input.GetButton("Reload"))
//            result[1] = 1;

//        if (Input.GetButtonDown("Drop"))
//            result[2] = 1;

//        if (Input.GetButtonDown("DropWep"))
//            result[3] = 1;

//        if (Input.GetButtonDown("Pickup")) {
//            result[4] = 1;
//            result[5] = 1;
//            result[6] = 1;
//            result[7] = 1;
//        }

//        result[8] = Input.GetAxis("Horizontal");
//        result[9] = Input.GetAxis("Vertical");

//        result[10] = (lastRotation/360) - (transform.rotation.eulerAngles.z/360);
//        lastRotation = transform.rotation.eulerAngles.z;

//        return result;
//    }

//    private void FixedUpdate() {
//        //if(shouldCompute)
//        //    if (!pc.isDown())
//        //        UpdateNeuralNet();
//    }

//    private void UpdateNeuralNet() {
//        float[] view = GetView();

//        for(int i = 0; i < view.Length; i++) {
//            input[i] = view[i];
//        }

//        input[gridSize.x * gridSize.y] = hc.CurrentHealthNormalized;
//        input[gridSize.x * gridSize.y + 1] = wh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / wh.GetCurrentWeapon().GetAmmoInfo().maxAmmo;
//        input[gridSize.x * gridSize.y + 2] = (float)wh.GetCurrentWeapon().weaponType / 3f; //noramlizes weapon type
//        input[gridSize.x * gridSize.y + 3] = transform.rotation.eulerAngles.z / 360;


//        if(!otherPc.isDown()) {
//            input[gridSize.x * gridSize.y + 4] = 0;
//            input[gridSize.x * gridSize.y + 5] = otherHc.CurrentHealthNormalized;
//            input[gridSize.x * gridSize.y + 6] = otherWh.GetCurrentWeapon().GetAmmoInfo().currenAmmo / otherWh.GetCurrentWeapon().GetAmmoInfo().maxAmmo;
//        } else {
//            input[gridSize.x * gridSize.y + 4] = 1;
//            input[gridSize.x * gridSize.y + 5] = 0;
//            input[gridSize.x * gridSize.y + 6] = 1;
//        }

//        float[] output = lstm.CalculateOutput(input);

//        if(output[0] > output[1] && output[0] > 0.8f)
//            wh.FireWeapon(-transform.up, null);
//        else if(output[1] > 0.8f)
//            wh.ReloadWeapon();
            
//        if(output[2] >= 0.8f)
//            wh.DropAmmoCreate();

//        if(output[3] >= 0.8f)
//            wh.DropWeapon();
        
//        if(output[4] >= 0.8f) {
//            RaycastHit2D hpHit = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, hpLayer);
            
//            if(hpHit.collider != null) 
//                wh.PickupItem(hpHit.collider);
//        }

//        if(output[5] >= 0.8f) {
//            RaycastHit2D ammoHiy = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, ammoLayer);

//            if(ammoHiy.collider != null)
//                wh.PickupItem(ammoHiy.collider);
//        }

//        if(output[6] >= 0.8f) {
//            RaycastHit2D wepHit = Physics2D.CircleCast(transform.position, 1, Vector2.zero, 0, wepLayer);

//            if(wepHit.collider != null)
//                wh.PickupItem(wepHit.collider);
//        }

//        if(output[7] >= 0.8f)
//            if(Vector2.Distance(transform.position, otherPlayer.position) <= 1) 
//                otherPc.Restored();

//        MoveCharacter(output[8], output[9]);

//        if(output[8] > -standDeadZone && output[8] < standDeadZone) {
//            rb2d.AddForce(Vector2.right * -rb2d.velocity.normalized.x * drag);
//        }

//        if(output[9] > -standDeadZone && output[9] < standDeadZone) {
//            rb2d.AddForce(Vector2.up * -rb2d.velocity.normalized.y * drag);
//        }

//        if(rb2d.velocity.magnitude > 0 && rb2d.velocity.magnitude < movementSlowTolorance) {
//            rb2d.velocity = Vector2.zero;
//        }

//        transform.rotation *= Quaternion.Euler(new Vector3(0, 0, output[10] * -rotationSpeed));
//    }

//    private void MoveCharacter(float horizontal, float vertical) {
//        Vector2 direction = new Vector2(horizontal, vertical);
//        rb2d.AddForce(direction.normalized * speed, ForceMode2D.Force);

//        if(rb2d.velocity.magnitude > maxSpeed) {
//            rb2d.velocity = rb2d.velocity.normalized * maxSpeed;
//        }
//    }

//    public float[] GetView() {
//        float[] result = new float[gridSize.x * gridSize.y + extraInputSize];

//        for(int i = 0; i < input.Length; i++)
//            input[i] = defualtGridValue;

//        Vector2 bounds = gridSize * cellSize;

//        for(int i = 0; i < gridSize.y; i++) {
//            float currentY = i * cellSize.y - bounds.y / 2;

//            for(int j = 0; j < gridSize.x; j++) {
//                float currentX = j * cellSize.x - bounds.x / 2;
//                Vector2 currenPos = new Vector2(currentX + cellSize.x / 2, currentY + cellSize.y / 2) + (Vector2)transform.position + offset;

//                Collider2D[] collisions = Physics2D.OverlapBoxAll(currenPos, cellSize, 0, combinedLayerMask);

//                if(collisions.Length > 0) {
//                    if(collisions.Length > 1) {
//                        int counter = 0;

//                        foreach(var collision in collisions) {
//                            input[i * j] += layerMaskTable[1 << collision.gameObject.layer];
//                            counter++;
//                        }

//                        input[i * j] /= counter;
//                        input[i * j] /= layers.Length + 1;

//                    } else
//                        input[i * j] = layerMaskTable[1 << collisions[0].gameObject.layer] + 1;
//                }
//            }
//        }

//        return result;
//    }


//    private void OnDrawGizmosSelected() {

//        Gizmos.color = gridColor;
//        Vector2 bounds = gridSize * cellSize;
//        Gizmos.DrawWireCube((Vector2)transform.position + offset, bounds);

//        for(int i = 0; i < gridSize.y; i++) {
//            float currentY = i * cellSize.y - bounds.y / 2;

//            for(int j = 0; j < gridSize.x; j++) {
//                float currentX = j * cellSize.x - bounds.x / 2;
//                Vector2 currenPos = new Vector2(currentX + cellSize.x / 2, currentY + cellSize.y / 2) + (Vector2)transform.position + offset;

//                Gizmos.DrawWireCube(currenPos, cellSize);
//            }
//        }
//    }

//}
