using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnemy : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.ENEMIES;

    protected override void Update()
    {
        base.Update();
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    // ========================================= AI =========================================

    // TODO : AI

    // ======================================== /AI =========================================
}
