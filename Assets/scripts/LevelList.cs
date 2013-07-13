using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lobo;

public class LevelList : MonoBehaviour
{
    public static LevelList main;

    private List<LevelRoot> levels = new List<LevelRoot>();

    private bool inited = false;

    void Awake()
    {
        Utils.Assert( main == null );
        main = this;
    }

	// Use this for initialization
	void Initialize()
    {
        if( !inited )
        {
            inited = true;

            foreach( LevelRoot lev in GetComponentsInChildren<LevelRoot>() )
            {
                levels.Add(lev);
                lev.gameObject.SetActive(false);
            }
        }
	}

    public LevelRoot Find(string name)
    {
        Initialize();

        // find the level
        for( int i = 0; i < LevelList.main.levels.Count; i++ )
        {
            if( levels[i].gameObject.name == name )
            {
                return levels[i];
            }
        }
        return null;
    }
}
