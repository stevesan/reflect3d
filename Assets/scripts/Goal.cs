using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour
{
    public AudioClip getClip;

    void OnControllerColliderHit( ControllerColliderHit hit )
    {
        Player player = hit.controller.GetComponent<Player>();
        if( player != null )
        {
            player.OnGetGoal(this);

            AudioSource.PlayClipAtPoint( getClip, transform.position );
        }
    }
}
