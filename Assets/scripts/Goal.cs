using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour
{

    void OnControllerColliderHit( ControllerColliderHit hit )
    {
        Player player = hit.controller.GetComponent<Player>();
        if( player != null )
        {
            player.OnGetGoal(this);
        }
    }
}
