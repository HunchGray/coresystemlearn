using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private CharacterStats characterStats;    
    private Animator anim;
    private GameObject attackTarget;//µ–»À
    private float lastAttackTime;
    private bool isDead;
    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }
    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;    
        
        GameManager.Instance.RigisterPlayer(characterStats);
        stopDistance = agent.stoppingDistance;
    }



    private void Update()
    {
        if (characterStats.currentHealth == 0)
        { 
            isDead = true; 
        }
        if (isDead)
            GameManager.Instance.NotifyObservers();

        SetAnimation();

        lastAttackTime -= Time.deltaTime;
    }
    private void SetAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }
    private void MoveToTarget(Vector3 target)
    {    
        StopAllCoroutines();
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject enemy)
    {
        if (enemy != null)
        {
            if (isDead) return;
            attackTarget = enemy;
            characterStats.isCritical = UnityEngine.Random.value<characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        
        while(Vector3.Distance(attackTarget.transform.position,transform.position)>characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;
        if(lastAttackTime<0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            
            lastAttackTime =characterStats.attackData.coolDown;//÷ÿ÷√π•ª˜CD
        }
    }
    //Animation Event
    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>()&& attackTarget.GetComponent<Rock>().rockStates==Rock.RockStates.HitNothing)
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
            attackTarget.GetComponent<Rigidbody>().velocity=Vector3.one;
            attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
        }
        else
        {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats, targetStats);
        }

    }
}
