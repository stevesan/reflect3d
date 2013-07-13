using UnityEngine;
using System.Collections;

public class Exit : MonoBehaviour
{
    public LevelRoot targetLevel;

    void OnControllerColliderHit( ControllerColliderHit hit )
    {
        Player player = hit.controller.GetComponent<Player>();
        if( player != null )
        {
            GameController.main.ActivateLevel(
                    targetLevel == null ? ""
                    : targetLevel.gameObject.name);
        }
    }
}
