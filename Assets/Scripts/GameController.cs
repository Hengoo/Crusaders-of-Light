using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController>
{

    public int Seed = 0;

	// Use this for initialization
	void Awake () {
		base.Awake();
        DontDestroyOnLoad(gameObject);
	}
}
