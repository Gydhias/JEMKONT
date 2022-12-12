using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Statistics")]
public class EntityStats : SerializedScriptableObject
{
    public int Health;
    public int BaseShield;

    public int Strength;
    public int Dexterity;

    public int Mana;
    public int Movement;

    public int Defense;
    public int Range;
}
