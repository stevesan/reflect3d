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

    private bool hadValidTarget = false;

    void Awake()
    {
        Utils.Assert( main == null );
        main = this;
    }

    void Start()
    {
        state = "idle";
    }

    void EnterPreviewMode()
    {
        paintDot.transform.localScale = new Vector3(2f, 2f, 2f);
        foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
            target.OnReflectingBegin(this);
    }

    void ExitPreviewMode(bool commit)
    {
        paintDot.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
            target.OnReflectingEnd(this, commit);
    }

    void Update()
    {
        // Check button input
        if( Screen.lockCursor && Input.GetMouseButtonDown(0) )
        {
            if( state == "idle" )
            {
                EnterPreviewMode();
                state = "preview";
            }
            else
            {
                ExitPreviewMode(true);
                state = "idle";
            }
        }
        else if( Screen.lockCursor && Input.GetMouseButtonDown(1) )
        {
            if( state == "preview" )
            {
                ExitPreviewMode(false);
                state = "idle";
            }
        }
    }
	
	void LateUpdate()
    {
        if( state == "preview" )
        {
            Ray ray = Camera.main.ViewportPointToRay( new Vector3(0.5f,0.5f,0) );
            RaycastHit hit = new RaycastHit();

            if( Physics.Raycast( ray, out hit ) )
            {
                paintDot.SetActive(true);
                paintDot.transform.position = hit.point;
                paintDot.transform.up = hit.normal;

                if( !hadValidTarget )
                    EnterPreviewMode();

                hadValidTarget = true;

                reflectingPlane = new Plane( hit.normal, hit.point );
                foreach( Reflectable target in sceneRoot.GetComponentsInChildren<Reflectable>() )
                    target.OnReflectingMotion(this);
            }
            else
            {
                if( hadValidTarget )
                    ExitPreviewMode(false);
                hadValidTarget = false;
            }
        }
	}

    //----------------------------------------
    //  Public interface
    //----------------------------------------

    public Plane GetReflectingPlane() { return reflectingPlane; }
}
