using DownBelow.Entity;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tool 
{
    public Deck Deck;
    public EClass Class;
    public PlayerBehavior ActualPlayer;
    public virtual void WorldAction() 
    {
        switch (Class) {
            //Fuck every each one of you, fuck polymorphism, fuck joe biden, fuck macron, fuck putin, embrace monkey
            case EClass.Fisherman:
                break;
            case EClass.Farmer:
                break;
            case EClass.Herbalist:
                break;
            case EClass.Miner:
                break;
        }
    }
}
public enum EClass {
    Fisherman,
    Farmer,
    Herbalist,
    Miner
}
