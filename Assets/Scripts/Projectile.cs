using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Projectile
{
    internal Vector2 MaxBorder
    {
        get
        {
            return new Vector2(4.5f, 5 - 9 + ChessManager.instance.highestRow);
        }
    }
    internal Vector2 MinBorder
    {
        get
        {
            return new Vector2(-4.5f, -5 - 9 + ChessManager.instance.highestRow);
        }
    }
    public void Shoot(Vector2 shootDirection);

    public IEnumerator Flying();
}
