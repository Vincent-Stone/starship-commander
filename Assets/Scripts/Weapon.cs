using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionType = Player.ActionType;
public class Weapon {

    public List<ActionType> actionMode = new List<ActionType>();
    public Weapon(ActionType[] actionMode)
    {
        this.actionMode.AddRange(actionMode);
    }
}
