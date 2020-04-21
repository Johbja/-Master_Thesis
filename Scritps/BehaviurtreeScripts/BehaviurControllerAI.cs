using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviurControllerAI : MonoBehaviour
{

    [Header("Movement settings")]
    [SerializeField] private float speed;
    [SerializeField] private float drag;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float movementSlowTolorance;
    [SerializeField] private float movementDetectionRadius;
    [SerializeField] private float minWait;
    [SerializeField] private float maxWait;
    [SerializeField] private float spinSpeed;
    [SerializeField, Range(0, 1)] private float procrastinationValue;
    [SerializeField, Range(0, 1)] private float shootProbability;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector2 strafeDetectionSize;

    [Header("Force settings")]
    [SerializeField] private float followPlayerForce;
    [SerializeField] private float stayCloseForce;
    [SerializeField] private float avoidForce;
    [SerializeField] private float strafeForce;
    [SerializeField] private float centerForce;
    [SerializeField] private float moveToPickupForce;
    [SerializeField] private float wanderForce;
    [SerializeField] private float moveToPlayerFroce;
    [SerializeField] private float MoveToPlayerDownForce;

    [Header("AimSettings")]
    //[SerializeField] private float rotationSpeed;
    [SerializeField] private float inaccurateRating;
    [SerializeField] private float leadAmount;
    [SerializeField] private float minLerp;
    [SerializeField] private float maxLerp;

    [Header("Collision settings")]
    [SerializeField] private LayerMask enemy;
    [SerializeField] private LayerMask wall;
    [SerializeField] private Vector2 detectionRange;

    [Header("Targeting settings")]
    [SerializeField] private float updatesPerSecond;
    [SerializeField] private float updateInnaccuracyDeley;

    [Header("Range targeting settings")]
    [SerializeField] private float rangedBiasLimit;
    [SerializeField] private float rangedDistanceBias;
    [SerializeField] private float rangedHealthBias;
    [SerializeField] private int maxCloseEnemies;

    [Header("Melee targeting settings")]
    [SerializeField] private float meleeBiasLimit;
    [SerializeField] private float meleeDistanceBias;
    [SerializeField] private float meleeHealthBias;

    [Header("Pickup settings")]
    [SerializeField] private float ammoTriggerPickup;
    [SerializeField] private float dropAmmoTrigger;
    [SerializeField] private float dropAmmoTriggerPlayer;
    [SerializeField] private float pickupRange;
    [SerializeField] private LayerMask ammo;
    [SerializeField] private LayerMask hp;
    [SerializeField] private LayerMask sub;
    [SerializeField] private LayerMask assult;

    [Header("UI settings")]
    [SerializeField] private Color rangeColor;
    [SerializeField] private Color movementDetection;

    private float currentBias;
    private bool willDropWep;

    private Timer timer;
    private Timer timerAim;
    private Timer timerSpin;
    private Timer shootTimer;
    private Rigidbody2D rb2d;
    private Vector2 startPos;
    private Vector2 direction;
    private Timer dropAmmoTimer;
    private PlayerController pc;
    private Sequence aimingTree;
    private Vector2 innaccuracy;
    private Vector2 resultVector;
    private Timer doNotShootTimer;
    private Timer timerAimAtPoint;
    private Sequence movementTree;
    private Selector targetingTree;
    private Vector2 randomLookPoint;
    private Vector2 currenWonderPos;
    private LayerMask currentPickup;
    private GameObject currentTarget;
    private WeaponHandler weaponHandler;
    private RaycastHit2D[] enemysOnScreen;
    private WeaponHandler playerWepHandler;
    private Collider2D currentPickupTarget;
    private List<GameObject> weaponsDropped;
    private HealthController healthController;
    private PlayerController playerController;

    private void Start()
    {
        startPos = transform.position;
        direction = transform.right;
        currentBias = 2;
        rb2d = GetComponent<Rigidbody2D>();
        weaponHandler = GetComponent<WeaponHandler>();
        healthController = GetComponent<HealthController>();
        pc = GetComponent<PlayerController>();
        playerWepHandler = playerTransform.gameObject.GetComponent<WeaponHandler>();
        playerController = playerTransform.gameObject.GetComponent<PlayerController>();
        weaponsDropped = new List<GameObject>();
        timer = gameObject.AddComponent<Timer>();
        timerAim = gameObject.AddComponent<Timer>();
        timerSpin = gameObject.AddComponent<Timer>();
        shootTimer = gameObject.AddComponent<Timer>();
        doNotShootTimer = gameObject.AddComponent<Timer>();
        timerAimAtPoint = gameObject.AddComponent<Timer>();
        dropAmmoTimer = gameObject.AddComponent<Timer>();
        timer.InitializeTimer();
        timerAim.InitializeTimer();
        timerSpin.InitializeTimer();
        shootTimer.InitializeTimer();
        doNotShootTimer.InitializeTimer();
        timerAimAtPoint.InitializeTimer();
        dropAmmoTimer.InitializeTimer();
        CreateMovementTree();
        CreateTargetingTree();
        CreateAmingTree();
        currenWonderPos = transform.position;
        currentPickup = -1;
        willDropWep = false;

        StartCoroutine(Targeting());
        StartCoroutine(UpdateInnaccuracy());
    }

    private IEnumerator Targeting()
    {
        while (true)
        {
            if(!pc.isDown())
                targetingTree.Evaluate();

            yield return new WaitForSeconds(1 / updatesPerSecond);
        }
    }

    private IEnumerator UpdateInnaccuracy()
    {
        while (true)
        {
            if(!pc.isDown())
                innaccuracy = new Vector2(Random.Range(-inaccurateRating, inaccurateRating), Random.Range(-inaccurateRating, inaccurateRating));
            
            yield return new WaitForSeconds(updateInnaccuracyDeley);
        }
    }

    private void FixedUpdate()
    {
        if(!pc.isDown()) {
            aimingTree.Evaluate();
            movementTree.Evaluate();
        }
    }

    private void CreateAmingTree()
    {
        //create first layer
        List<BaseNode> childs = new List<BaseNode>() {
            new ActionNode(Aim),
            new ActionNode(Shoot)
        };

        //add all child to root
        aimingTree = new Sequence(childs);
    }

    private void CreateTargetingTree()
    {

        //create second layer
        List<BaseNode> childs = new List<BaseNode>() {
            new ActionNode(FindRanged),
            new ActionNode(BestClosest)
        };

        targetingTree = new Selector(childs);
    }

    private void CreateMovementTree()
    {

        List<BaseNode> pickupItemChilds = new List<BaseNode>() {
            new ActionNode(MoveToPickup),
            new ActionNode(Pickup)
        };
        Sequence moveTo = new Sequence(pickupItemChilds);

        List<BaseNode> getHpChilds = new List<BaseNode>() {
            new ActionNode(GetHealth),
            moveTo
        };
        Sequence getHp = new Sequence(getHpChilds);

        List<BaseNode> getGunChilds = new List<BaseNode>() {
            new ActionNode(GetOtherWeapon),
            moveTo
        };
        Sequence getGunSequence = new Sequence(getGunChilds);

        List<BaseNode> getAmmoChilds = new List<BaseNode>() {
            new ActionNode(GetAmmo),
            moveTo
        };
        Sequence getAmmoSequence = new Sequence(getAmmoChilds);

        List<BaseNode> selectPickupChilds = new List<BaseNode>() {
            getHp,
            getGunSequence,
            getAmmoSequence
        };
        Selector selectPickup = new Selector(selectPickupChilds);

        List<BaseNode> dropAmmoChilds = new List<BaseNode>() {
            new ActionNode(MoveToPlayer),
            new ActionNode(DropAmmo)
        };
        Sequence dropAmmoSequence = new Sequence(dropAmmoChilds);

        List<BaseNode> checkIfDropChilds = new List<BaseNode>() {
            new ActionNode(CheckIfDropAmmo),
            dropAmmoSequence
        };
        Selector checkIfDropSelect = new Selector(checkIfDropChilds);

        List<BaseNode> getSuffChild = new List<BaseNode>() {
            selectPickup,
            new ActionNode(DropWeapon),
            checkIfDropSelect,
            new ActionNode(ReturnDefualt)
        };
        Selector selectGetStuff = new Selector(getSuffChild);

        List<BaseNode> childRanged = new List<BaseNode>() {
            new ActionNode(CheckRanged),
            new ActionNode(Strafe),
            new ActionNode(StayClose),
        };
        Sequence moveAroundRanged = new Sequence(childRanged);

        List<BaseNode> kidtGropChilds = new List<BaseNode>() {
            new ActionNode(Strafe),
            new ActionNode(StayClose),
        };
        Sequence kiteGroup = new Sequence(kidtGropChilds);

        List<BaseNode> movementChilds = new List<BaseNode>() {
            moveAroundRanged,
            kiteGroup,
            new ActionNode(StayClose)
        };
        Selector selectMovement = new Selector(movementChilds);

        List<BaseNode> aimOrSpinChilds = new List<BaseNode>() {
            new ActionNode(AimPlayer),
            new ActionNode(Spin),
            new ActionNode(AimAtPoint)
        };
        Selector selectAimOrSpin = new Selector(aimOrSpinChilds);

        List<BaseNode> StandOrWalkChilds = new List<BaseNode>() {
            new ActionNode(Wander),
            new ActionNode(StandStill)
        };
        Selector selectStandOrWalk = new Selector(StandOrWalkChilds);

        //Don't shoot the children
        List<BaseNode> shootOrNotChilds = new List<BaseNode>() {
            new ActionNode(DoNotShoot),
            new ActionNode(Shoot)
        };
        Selector shootOrNotSelect = new Selector(shootOrNotChilds);

        List<BaseNode> idleChilds = new List<BaseNode>() {
            selectStandOrWalk,
            selectAimOrSpin,
            shootOrNotSelect
        };
        Sequence idleSequence = new Sequence(idleChilds);

        List<BaseNode> idleCheckChilds = new List<BaseNode>() {
            new ActionNode(CheckIdle),
            idleSequence
        };
        Selector idleSelect = new Selector(idleCheckChilds);

        List<BaseNode> helpPlayerChilds = new List<BaseNode>() {
            new ActionNode(MoveToPlayerDown),
            new ActionNode(HelpPlayerDown)
        };
        Sequence sequenceHelpPlayer = new Sequence(helpPlayerChilds);

        //Gotta help, children are a lot of work
        List<BaseNode> checkIfHelpPlayerChilds = new List<BaseNode>() {
            new ActionNode(CheckIfHelpPlayer),
            sequenceHelpPlayer
        };
        Selector checkIfHelpPlayer = new Selector(checkIfHelpPlayerChilds);

        List<BaseNode> childs = new List<BaseNode>() {
            new ActionNode(ResetMovementVector),
            new ActionNode(AvoidEnemeys),
            new ActionNode(StayCentered),
            checkIfHelpPlayer,
            new ActionNode(StayCloseToPlayer),
            selectGetStuff,
            selectMovement,
            idleSelect,
            new ActionNode(LimitSpeed)
        };
        movementTree = new Sequence(childs);
    }

    private NodeState FindRanged()
    {
        enemysOnScreen = Physics2D.BoxCastAll(startPos, detectionRange, 0, Vector2.zero, 0, enemy);

        if (enemysOnScreen.Length < 1)
            return NodeState.Failure;

        GameObject tempTarget = null;
        float minWeight = 10000;

        foreach (RaycastHit2D hit in enemysOnScreen)
        {
            EnemyController enemyController = hit.collider.gameObject.GetComponent<EnemyController>();
            float distance = Vector2.Distance(hit.transform.position, transform.position);
            float hp = hit.transform.gameObject.GetComponent<HealthController>().CurrenHealth;

            if (hit.transform.gameObject == currentTarget)
            {
                currentBias = distance / rangedDistanceBias;
                RaycastHit2D[] blockers = Physics2D.RaycastAll(transform.position, hit.transform.position - transform.position, distance, enemy);
                currentBias += blockers.Length - 1;
                currentBias += hp / rangedHealthBias;
            }

            if (enemyController.IsRanged)
            {
                RaycastHit2D[] enemysClose = Physics2D.CircleCastAll(hit.collider.gameObject.transform.position, 1, Vector2.zero, 0, enemy);

                if (hit.transform.gameObject == currentTarget)
                {
                    currentBias += enemysClose.Length;

                    if (currentBias < rangedBiasLimit)
                    {
                        return NodeState.Sucesess;
                    }
                }

                float currentWeight = distance * hp;
                if (currentWeight < minWeight && hit.collider.gameObject != currentTarget)
                {
                    minWeight = currentWeight;
                    tempTarget = hit.collider.gameObject;
                }
            }
        }

        if (tempTarget != null)
        {
            currentTarget = tempTarget;
            return NodeState.Sucesess;
        }
        else
        {
            return NodeState.Failure;
        }

    }

    private NodeState BestClosest()
    {

        if (enemysOnScreen.Length < 1)
            return NodeState.Failure;


        GameObject tempTarget = null;
        float minWeight = 10000000;
        foreach (RaycastHit2D hit in enemysOnScreen)
        {
            float distance = Vector2.Distance(hit.transform.position, transform.position);
            float hp = hit.transform.gameObject.GetComponent<HealthController>().CurrenHealth;
            float currentWeight = distance * hp;

            if (hit.transform.gameObject == currentTarget)
            {
                currentBias = distance / meleeDistanceBias;
                currentBias += hp / meleeHealthBias;

                if (currentBias < meleeBiasLimit)
                {
                    currentTarget = hit.transform.gameObject;
                    return NodeState.Sucesess;
                }
            }

            if (currentWeight < minWeight)
            {
                minWeight = currentWeight;
                tempTarget = hit.collider.gameObject;
            }
        }

        if (tempTarget != null)
        {
            currentTarget = tempTarget;
            return NodeState.Sucesess;
        }
        else
        {
            return NodeState.Failure;
        }
    }

    private NodeState Aim()
    {
        if (currentTarget == null)
            return NodeState.Failure;

        Vector2 pos = (Vector2)currentTarget.transform.position + innaccuracy - (Vector2)currentTarget.transform.up * leadAmount;
        RotateTowardsTarget(pos);

        return NodeState.Sucesess;
    }

    private void RotateTowardsTarget(Vector2 target)
    {
        Vector2 pos = rb2d.position;
        float angle = Mathf.Atan2(pos.x - target.x, pos.y - target.y) * Mathf.Rad2Deg;

        float angleb = Vector2.Angle(-transform.up, target);
        float procentSpeed = Mathf.InverseLerp(0, 180, Mathf.Abs(angleb));
        float rotationSpeed = Mathf.Lerp(minLerp, maxLerp, procentSpeed);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0, 0, -angle)), rotationSpeed);
    }

    private NodeState Shoot()
    {
        weaponHandler.FireWeapon(-transform.up, null);
        return NodeState.Sucesess;
    }

    private NodeState AvoidEnemeys()
    {
        RaycastHit2D[] enemysClose = Physics2D.CircleCastAll(transform.position, movementDetectionRadius, Vector2.zero, 0, enemy + wall);

        foreach (var hit in enemysClose)
        {
            Vector2 temp = ((Vector2)transform.position - hit.point);
            resultVector += temp.normalized / Vector2.Distance(transform.position, hit.collider.gameObject.transform.position) * avoidForce;
        }

        return NodeState.Sucesess;
    }

    private NodeState LimitSpeed()
    {
        if (resultVector.magnitude > movementSlowTolorance)
        {
            rb2d.AddForce(resultVector.normalized * speed);
        }
        else
        {
            if (rb2d.velocity.magnitude > movementSlowTolorance)
            {
                rb2d.AddForce(-rb2d.velocity.normalized * drag);
            }
            else if (rb2d.velocity.magnitude > 0 && rb2d.velocity.magnitude < movementSlowTolorance)
            {
                rb2d.velocity = Vector2.zero;
            }
        }

        if (rb2d.velocity.magnitude > maxSpeed)
        {
            rb2d.velocity = rb2d.velocity.normalized * maxSpeed;
        }

        return NodeState.Sucesess;
    }

    private NodeState ResetMovementVector()
    {
        resultVector = Vector2.zero;
        return NodeState.Sucesess;
    }

    private NodeState GetHealth()
    {
        if (currentPickup == hp)
            return NodeState.Sucesess;

        float randLimit = Random.Range(healthController.CurrentHealthNormalized, 1f);

        if (randLimit > 0.3f)
            return NodeState.Failure;

        currentPickup = hp;
        return NodeState.Sucesess;
    }

    private NodeState GetOtherWeapon()
    {

        WeaponWrapper currentWep = weaponHandler.GetCurrentWeapon();

        if (currentWep == null)
            return NodeState.Failure;

        if (currentWep.weaponType == WeaponType.AssultRifle)
            return NodeState.Failure;

        if (currentWep.weaponType == WeaponType.SubmachineGun)
        {
            currentPickup = assult;
            return NodeState.Sucesess;
        }

        currentPickup = assult + sub;
        return NodeState.Sucesess;
    }

    private NodeState GetAmmo()
    {

        WeaponWrapper currentWep = weaponHandler.GetCurrentWeapon();
        if (currentWep.weaponType == WeaponType.HandGun)
            return NodeState.Failure;

        if (currentWep.GetAmmoInfo().currenAmmo <= ammoTriggerPickup)
        {
            currentPickup = ammo;
            return NodeState.Sucesess;
        }

        return NodeState.Failure;
    }

    private NodeState MoveToPickup()
    {
        List<GameObject> removeList = new List<GameObject>();
        foreach (var item in weaponsDropped) {

            if (item == null) {
                removeList.Add(item);
                continue;
            }
        }

        foreach (var item in removeList)
            weaponsDropped.Remove(item);

        float dist = 0;

        //check for empty weapons and pick them up if there alos is ammo avaible
        if ((currentPickup == assult || currentPickup == assult + sub))
        {
            RaycastHit2D ammoOnMap = Physics2D.BoxCast(startPos, detectionRange, 0, Vector2.zero, 0, ammo);

            if (ammoOnMap.collider != null && weaponsDropped.Count > 0)
            {

                GameObject currentBest = null;
                

                foreach (var item in weaponsDropped)
                {
                    WeaponWrapper wep = item.GetComponent<PickupHandler>().GetWeapon();
                    if (wep.weaponType == WeaponType.AssultRifle)
                    {
                        currentBest = item;
                        break;
                    }
                    else if (wep.weaponType == WeaponType.SubmachineGun)
                    {
                        currentBest = item;
                    }
                }
                
                if (currentTarget == null)
                    RotateTowardsTarget(currentBest.transform.position);

                dist = Vector2.Distance(transform.position, currentBest.transform.position);

                if (dist < 2)
                    resultVector += (Vector2)(currentBest.transform.position - transform.position) * moveToPickupForce * 6;
                else
                    resultVector += (Vector2)(currentBest.transform.position - transform.position) * moveToPickupForce;

                currentPickupTarget = currentBest.GetComponent<Collider2D>();

                return NodeState.Sucesess;
            }
        }

        RaycastHit2D itemOnMap = Physics2D.BoxCast(startPos, detectionRange, 0, Vector2.zero, 0, currentPickup);

        //return found empty weapon
        if ((currentPickup == assult || currentPickup == assult + sub))
            foreach (var item in weaponsDropped)
                if (item == itemOnMap.collider.gameObject)
                    return NodeState.Failure;

        //return found nothing
        if (itemOnMap.collider == null)
            return NodeState.Failure;

        if (currentTarget == null)
            RotateTowardsTarget(itemOnMap.collider.gameObject.transform.position);

        dist = Vector2.Distance(transform.position, itemOnMap.transform.position);

        if (dist < 2)
            resultVector += (Vector2)(itemOnMap.transform.position - transform.position) * moveToPickupForce * 6;
        else
            resultVector += (Vector2)(itemOnMap.transform.position - transform.position) * moveToPickupForce;

        currentPickupTarget = itemOnMap.collider;

        return NodeState.Sucesess;
    }

    private NodeState Pickup()
    {

        if (Vector2.Distance(currentPickupTarget.transform.position, transform.position) < pickupRange)
        {
            weaponHandler.PickupItem(currentPickupTarget);

            if (weaponsDropped.Contains(currentPickupTarget.gameObject))
                weaponsDropped.Remove(currentPickupTarget.gameObject);

            currentPickupTarget = null;

        }

        return NodeState.Sucesess;
    }

    private NodeState DropWeapon()
    {
        WeaponWrapper currentWep = weaponHandler.GetCurrentWeapon();

        if (currentWep.weaponType != WeaponType.HandGun && currentWep.GetAmmoInfo().currenAmmo <= 0 && currentWep.GetAmmoInfo().currentClipAmmo <= 0)
        {
            if (!willDropWep) {
                Invoke("WaitDropWep", 1.5f);
                willDropWep = true;
            }

            return NodeState.Sucesess;
        }

        return NodeState.Failure;
    }

    private void WaitDropWep() {

        Debug.Log("Drop Weapon");
        List<GameObject> droppedWeps = weaponHandler.DropWeapon();
        foreach (var item in droppedWeps)
            weaponsDropped.Add(item);

        willDropWep = false;
    }

    private NodeState ReturnDefualt()
    {
        return NodeState.Sucesess;
    }

    private NodeState Strafe()
    {

        if (currentTarget == null)
            return NodeState.Sucesess;

        RaycastHit2D[] left = Physics2D.BoxCastAll(transform.position - (Vector3)direction * ((strafeDetectionSize.x / 2) + 0.1f), strafeDetectionSize, 0, -direction, 0, enemy + wall);
        RaycastHit2D[] right = Physics2D.BoxCastAll(transform.position + (Vector3)direction * ((strafeDetectionSize.x / 2) + 0.1f), strafeDetectionSize, 0, direction, 0, enemy + wall);

        if (right.Length > left.Length)
            direction = transform.right;
        else
            direction = -transform.right;

        resultVector += direction * strafeForce;

        return NodeState.Sucesess;
    }

    private NodeState StayClose()
    {

        if (currentTarget == null)
            return NodeState.Sucesess;

        RaycastHit2D[] enemiesCloseToTarget = Physics2D.CircleCastAll(currentTarget.transform.position, 1, Vector2.zero);

        foreach (var hit in enemiesCloseToTarget)
        {
            if (hit.collider == null)
                continue;

            Vector2 temp = (hit.collider.gameObject.transform.position - transform.position);
            resultVector += temp.normalized * Vector2.Distance(transform.position, hit.collider.gameObject.transform.position) * stayCloseForce;
        }

        return NodeState.Sucesess;
    }


    private NodeState CheckRanged()
    {
        if (currentTarget == null)
            return NodeState.Failure;

        RaycastHit2D[] enemiesCloseToTarget = Physics2D.CircleCastAll(currentTarget.transform.position, 1, Vector2.zero);
        if (!currentTarget.GetComponent<EnemyController>().IsRanged || enemiesCloseToTarget.Length >= maxCloseEnemies)
            return NodeState.Failure;

        return NodeState.Sucesess;
    }

    private NodeState StayCentered()
    {
        resultVector += -(Vector2)transform.position * centerForce;
        return NodeState.Sucesess;
    }

    private NodeState StayCloseToPlayer()
    {
        resultVector += (Vector2)(playerTransform.position - transform.position) * followPlayerForce;
        return NodeState.Sucesess;
    }

    private NodeState CheckIfDropAmmo() {
        if(playerWepHandler.GetCurrentWeapon() == null)
            return NodeState.Sucesess;

        if(playerWepHandler.GetCurrentWeapon().weaponType == WeaponType.HandGun)
            return NodeState.Sucesess;

        if(dropAmmoTimer.IsCompleted() && weaponHandler.GetCurrentWeapon().GetAmmoInfo().currenAmmo >= dropAmmoTrigger && playerWepHandler.GetCurrentWeapon().GetAmmoInfo().currenAmmo <= dropAmmoTriggerPlayer) {
            dropAmmoTimer.StarTimer(0.25f);
            return NodeState.Failure;
        }

        return NodeState.Sucesess;
    }

    private NodeState MoveToPlayer() {
        resultVector += (Vector2)(playerTransform.position - transform.position) * moveToPlayerFroce;
        RotateTowardsTarget(playerTransform.position);
        return NodeState.Sucesess;
    }

    private NodeState DropAmmo() {

        if(Vector2.Distance(transform.position, playerTransform.position) < 1) 
            weaponHandler.DropAmmoCreate();
        
        return NodeState.Failure;
    }

    private NodeState CheckIdle()
    {

        if (currentTarget == null && currentPickupTarget == null)
            return NodeState.Failure;

        return NodeState.Sucesess;
    }

    private NodeState Wander()
    {

        if (!timer.IsCompleted())
            return NodeState.Failure;

        if (Vector2.Distance(transform.position, currenWonderPos) < 1)
        {

            float rand = Random.Range(0f, 1f);
            if (rand > procrastinationValue)
                return NodeState.Failure;

            currenWonderPos = new Vector2(Random.Range(-detectionRange.x / 2, detectionRange.x / 2), Random.Range(-detectionRange.y / 2, detectionRange.y / 2));

        }
        else
        {
            resultVector += (currenWonderPos - (Vector2)transform.position) * wanderForce;
        }

        return NodeState.Sucesess;
    }

    private NodeState AimPlayer()
    {

        if (!timerAimAtPoint.IsCompleted() || !timerSpin.IsCompleted())
            return NodeState.Failure;

        if (timerAim.IsCompleted())
        {
            float rand = Random.Range(0f, 1f);
            if (rand > procrastinationValue)
                return NodeState.Failure;
        }

        timerAim.StarTimer(Random.Range(minWait, maxWait));
        RotateTowardsTarget(playerTransform.position);
        return NodeState.Sucesess;
    }

    private NodeState Spin()
    {

        if (!timerAimAtPoint.IsCompleted())
            return NodeState.Failure;

        if(timerSpin.IsCompleted()) {
            float rand = Random.Range(0f, 1f);
            if (rand > procrastinationValue)
                return NodeState.Failure;
        }

        timerSpin.StarTimer(Random.Range(minWait, maxWait));
        transform.rotation *= Quaternion.Euler(new Vector3(0, 0, -spinSpeed));

        return NodeState.Sucesess;
    }

    private NodeState AimAtPoint()
    {
        if (timerAimAtPoint.IsCompleted())
            randomLookPoint = new Vector2(Random.Range(-detectionRange.x / 2, detectionRange.x / 2), Random.Range(-detectionRange.y / 2, detectionRange.y / 2));

        timerAimAtPoint.StarTimer(Random.Range(minWait, maxWait));
        RotateTowardsTarget(randomLookPoint);
        return NodeState.Sucesess;
    }

    private NodeState StandStill()
    {
        resultVector = Vector2.zero;
        timer.StarTimer(Random.Range(minWait, maxWait));
        return NodeState.Sucesess;
    }

    private NodeState DoNotShoot()
    {
        if (weaponHandler.GetCurrentWeapon().weaponType != WeaponType.HandGun)
            return NodeState.Sucesess;

        if (!shootTimer.IsCompleted())
            return NodeState.Failure;

        if (!doNotShootTimer.IsCompleted())
            return NodeState.Sucesess;

        if (Random.Range(0f, 1f) < shootProbability)
        {
            shootTimer.StarTimer(Random.Range(minWait, maxWait));
            return NodeState.Failure;
        }

        if (Random.Range(0f, 1f) < shootProbability)
        {
            doNotShootTimer.StarTimer(Random.Range(minWait, maxWait));
            return NodeState.Sucesess;
        }

        return NodeState.Failure;
    }

    private NodeState CheckIfHelpPlayer() {
        if(playerController.isDown())
            return NodeState.Failure;

        return NodeState.Sucesess;
    }

    private NodeState MoveToPlayerDown() {
        resultVector += (Vector2)(playerTransform.position - transform.position) * MoveToPlayerDownForce;
        return NodeState.Sucesess;
    }

    private NodeState HelpPlayerDown() {
        
        if(Vector2.Distance(transform.position, playerTransform.position) < 1) 
            playerController.Restored();

        return NodeState.Sucesess;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = rangeColor;
        Gizmos.DrawWireCube(Vector2.zero, detectionRange);

        Gizmos.color = movementDetection;
        Gizmos.DrawWireSphere(transform.position, movementDetectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - transform.right * ((strafeDetectionSize.x / 2) + 0.1f), strafeDetectionSize);
        Gizmos.DrawWireCube(transform.position + transform.right * ((strafeDetectionSize.x / 2) + 0.1f), strafeDetectionSize);
    }
}
