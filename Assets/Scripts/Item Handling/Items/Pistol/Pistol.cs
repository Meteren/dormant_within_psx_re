using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{
    [SerializeField] int damageAmount;
    public override int InflictDamage() => damageAmount;
}
