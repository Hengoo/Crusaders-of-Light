using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController>
{

    public int Seed = 0;

	// Use this for initialization
    protected override void Awake () {
		base.Awake();
        DontDestroyOnLoad(gameObject);
	}
}
