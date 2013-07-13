using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public static Player main;

    private int numGoalsGot = 0;

    void Awake()
    {
        //Lobo.Assert( main == null );
        main = this;
    }

	// Update is called once per frame
	void Update () {
	
	}

    void OnControllerColliderHit( ControllerColliderHit hit )
    {
        hit.gameObject.SendMessage("OnControllerColliderHit", hit,
                SendMessageOptions.DontRequireReceiver);
    }

    public void OnGetGoal(Goal goal)
    {
        numGoalsGot++;
        Destroy(goal.gameObject);
    }
}
