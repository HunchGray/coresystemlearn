using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 30;
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direaction = attackTarget.transform.position - transform.position;
            direaction.Normalize();
            //targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direaction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
