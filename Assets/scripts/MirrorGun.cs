using UnityEngine;
using System.Collections;
using Lobo;

public class MirrorGun : MonoBehaviour
{
    public static MirrorGun main;

    //----------------------------------------
    //  Editor variables
    //----------------------------------------

    public GameObject paintDot;
    public GameObject sceneRoot;

    //----------------------------------------
    //  Private state
    //----------------------------------------

    private Plane reflectingPlane;
    private string state = "idle";

    void Awake()
    {
        Utils.Assert( main == null );
        main = this;
    }

    void Update()
    {
        // Check button input
        if( Input.GetMouseButtonDown(0) )
        {
            if( state == "idle" )
            {
                state = "preview";
                paintDot.transform.localScale = new Vector3(2f, 2f, 2f);
                foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
                    target.OnReflectingBegin(this);
            }
            else
            {
                state = "idle";
                paintDot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
                    target.OnReflectingEnd(this, true);
            }
        }
    }
	
	void LateUpdate()
    {
        Ray ray = Camera.main.ViewportPointToRay( new Vector3(0.5f,0.5f,0) );
        RaycastHit hit = new RaycastHit();

        if( Physics.Raycast( ray, out hit ) )
        {
            paintDot.SetActive(true);
            paintDot.transform.position = hit.point;
            paintDot.transform.up = hit.normal;

            if( state == "preview" )
            {
                reflectingPlane = new Plane( hit.normal, hit.point );
                foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
                    target.OnReflectingMotion(this);
            }
        }
        else
        {
            paintDot.SetActive(false);
        }
	}

    //----------------------------------------
    //  Public interface
    //----------------------------------------

    public Plane GetReflectingPlane() { return reflectingPlane; }
}
