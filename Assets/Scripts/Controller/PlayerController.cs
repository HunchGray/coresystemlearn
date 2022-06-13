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
    private GameObject attackTarget;//����
    private float lastAttackTime;
    private bool isDead;

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
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject enemy)
    {
        if (enemy != null)
        {
            attackTarget = enemy;
            characterStats.isCritical = UnityEngine.Random.value<characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

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
            
            lastAttackTime =characterStats.attackData.coolDown;//���ù���CD
        }
    }
    //Animation Event
    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats, targetStats);
    }
}
