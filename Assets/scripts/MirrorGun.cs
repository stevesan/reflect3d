using UnityEngine;
using System.Collections;

public class MirrorGun : MonoBehaviour {

    public GameObject paintDot;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate()
    {
        Ray ray = Camera.main.ViewportPointToRay( new Vector3(0.5f,0.5f,0) );
        RaycastHit hit = new RaycastHit();

        if( Physics.Raycast( ray, out hit ) )
        {
            paintDot.SetActive(true);
            paintDot.transform.position = hit.point;
            paintDot.transform.up = hit.normal;
        }
        else
        {
            paintDot.SetActive(false);
        }
	}
}
