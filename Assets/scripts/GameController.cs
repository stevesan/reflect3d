using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lobo;

public class GameController : MonoBehaviour
{
    public static GameController main;

    public string hubName = "0-hub";

    private LevelRoot activeLevel;

    void Awake()
    {
        Utils.Assert( main == null );
        main = this;
    }

    public void ActivateLevel(string levelName)
    {
        LevelRoot lev = LevelList.main.Find(levelName);
        if( lev == null )
            lev = LevelList.main.Find(hubName);
        ActivateLevel(lev);
    }

    void ActivateLevel(LevelRoot level)
    {
        if( activeLevel != null )
            Destroy(activeLevel.gameObject);

        activeLevel = Utils.ClonePrefab(level.gameObject).GetComponent<LevelRoot>();
        Player.main.transform.position = activeLevel.playerStart.position;
        Player.main.transform.rotation = activeLevel.playerStart.rotation;
    }

    public LevelRoot GetActiveLevel()
    {
        return activeLevel;
    }

	// Use this for initialization
	IEnumerator Start()
    {
        ActivateLevel(hubName);
        return null;
	}
}
