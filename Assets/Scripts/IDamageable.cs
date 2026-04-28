using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int());
}
