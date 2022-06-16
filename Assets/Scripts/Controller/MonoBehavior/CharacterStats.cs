using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;
    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if(templateData!=null)
            characterData=Instantiate(templateData);
    }
    #region Read from Dato_SO
    public int maxHealth
    {
        get
        {
            if (characterData != null)
            {
                return characterData.maxHealth;
            }
            else return 0;
        }
        set
        {
            characterData.maxHealth = value;
        }
    }
    public int currentHealth
    {
        get
        {
            if (characterData != null)
            {
                return characterData.currentHealth;
            }
            else return 0;
        }
        set
        {
            characterData.currentHealth = value;
        }
    }
    public int baseDefence
    {
        get
        {
            if (characterData != null)
            {
                return characterData.baseDefence;
            }
            else return 0;
        }
        set
        {
            characterData.baseDefence = value;
        }
    }
    public int currentDefence
    {
        get
        {
            if (characterData != null)
            {
                return characterData.currentDefence;
            }
            else return 0;
        }
        set
        {
            characterData.currentDefence = value;
        }
    }
    #endregion
    #region Character Combat
    public void TakeDamage(CharacterStats attacker,CharacterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.currentDefence,0);
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }

    }
    public void TakeDamage(int damage,CharacterStats defener)
    {
        int currentDamage = Mathf.Max(damage - defener.currentDefence, 0);
        currentHealth = Mathf.Max(currentHealth - currentDamage, 0);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamge);
        if(isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("±©»÷" + coreDamage);
        }
        return (int)coreDamage;
    }
    #endregion
}
