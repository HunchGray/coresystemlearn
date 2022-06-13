using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    List<IEndGameObserver> endGameObservers=new List<IEndGameObserver>();
    public void RigisterPlayer(CharacterStats player)
    {
        playerStats = player;
    }
    public void AddObsever(IEndGameObserver obsever)
    {
        endGameObservers.Add(obsever);
    }
    public void RemoveObsever(IEndGameObserver obsever)
    {
        endGameObservers.Remove(obsever);
    }
    public void NotifyObservers()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
}
