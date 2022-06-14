using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates {GRAUD,PATROL,CHASE,DEAD }
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;

    private EnemyStates enemyStates;


    public Animator anim;

    private CharacterStats characterStats;

    private  Collider collider;

    [Header("Basic Setting")]
    public float sightRadius;

    public bool isGuard;

    private float speed;

    private GameObject attackTarget;

    public float loookAtTime;

    private float remainLoookAtTime;

    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float PatrolRange;

    private Vector3 wayPoint;

    private Vector3 guardPos;


    //动画转换条件
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        collider = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
    }
    private void Start()
    {
        remainLoookAtTime = loookAtTime;
        if (isGuard)
        {
            enemyStates = EnemyStates.GRAUD;
        }
        else
        {
            GetNewWayPoint();
            enemyStates = EnemyStates.PATROL;

        }
        //TODO:场景切换时删去
        GameManager.Instance.AddObsever(this);
    }
    //TODO:场景切换时启用
    /*void OnEnable()
    {
        GameManager.Instance.AddObsever(this);
    }*/
    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObsever(this);
    }
    private void Update()
    {
        if(characterStats.currentHealth==0)
        {
            isDead = true;
        }
        if (!playerDead)
        {
        SwitchAnimation();
        SwitchStates();
        lastAttackTime -= Time.deltaTime;
        }
    }
    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Die", isDead);
    }
    void SwitchStates()
    {
        if (isDead)
            enemyStates = EnemyStates.DEAD;
        //如果发现player，切换到chase
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }
        switch (enemyStates)
        {
            case EnemyStates.GRAUD:
                isChase = false;
                if(transform.position!=guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    { 
                    isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation,guardRotation,0.5f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;
                //是否走到巡逻目的地
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLoookAtTime > 0)
                    {
                        remainLoookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                    
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:
                //追击、攻击和回到之前的状态，并执行相应动画
                isWalk = false;
                isChase = true;

                agent.speed = speed;

                if (!FoundPlayer())
                {
                    isFollow = false;
                    if (remainLoookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLoookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GRAUD;
                    }
                    else
                        enemyStates = EnemyStates.PATROL;

                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //暴击判断
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                collider.enabled = false;
                agent.enabled = false;
                Destroy(gameObject, 2f);
                break;
        }
    }
    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近战攻击
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //技能攻击
            anim.SetTrigger("Skill");
        }
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }
    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else return false;
    }
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else return false;
    }
    void GetNewWayPoint()
    {
        remainLoookAtTime = loookAtTime;
        float randomX = Random.Range(-PatrolRange, PatrolRange);
        float randomZ = Random.Range(-PatrolRange, PatrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, PatrolRange, 1) ? hit.position : transform.position;

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.DrawWireSphere(transform.position, PatrolRange);

    }
    //Animation Event
    void Hit()
    {
        if (attackTarget!=null)
        { 
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止移动
        //停止agent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk=false;
        attackTarget = null;
    }
}
