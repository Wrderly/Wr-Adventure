using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAttribute
{
    public float maxHealth;
    public float health;
    public float atk;
    public float def;
    public float speed;
    public bool isDead = false;
}
