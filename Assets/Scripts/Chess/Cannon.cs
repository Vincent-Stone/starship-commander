using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : Chess
{
    [SerializeField] int fullEnergy = 2;
    int energy = 0;
    public override void Act()
    {

        throw new System.NotImplementedException();
    }

    public override List<Vector2Int> GetAttackRange()
    {
        throw new System.NotImplementedException();
    }

    public override List<Vector2Int> GetMoveRange()
    {
        throw new System.NotImplementedException();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
