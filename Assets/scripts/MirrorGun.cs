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

    GameObject GetSceneRoot()
    {
        return GameController.main.GetActiveLevel().gameObject;
    }

    void EnterPreviewMode()
    {
        foreach( Reflectable target in GetSceneRoot().GetComponentsInChildren<Reflectable>() )
        {
            Debug.Log("fdsfd "+target.gameObject.name);
            target.OnReflectingBegin(this);
        }
    }

    void ExitPreviewMode(bool commit)
    {
        foreach( Reflectable target in GetSceneRoot().GetComponentsInChildren<Reflectable>() )
            target.OnReflectingEnd(this, commit);
    }

    void Update()
    {
        if( !Screen.lockCursor )
            return;

        // These events can actually occur in the same frame, since the user
        // can press and release really quick.
        bool leftDown = Input.GetMouseButtonDown(0);
        bool leftUp = Input.GetMouseButtonUp(0);

        if( state == "idle" )
        {
            // Check button input
            if( leftDown && !leftUp )
            {
                // Don't enter preview mode yet, cuz the aim may be over invalid target
                hadValidTarget = false;
                state = "preview";
            }
        }
        else if( state == "preview" )
        {
            if( !leftDown && leftUp )
            {
                if( hadValidTarget )
                    ExitPreviewMode(false);
                state = "idle";
            }
            else if( Input.GetButtonDown("Commit") )
            {
                if( hadValidTarget )
                    ExitPreviewMode(true);
                state = "idle";
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
            paintDot.transform.position = hit.point + hit.normal*0.1f;
            paintDot.transform.up = hit.normal;

            if( state == "preview" )
            {
                if( !hadValidTarget )
                    EnterPreviewMode();

                hadValidTarget = true;

                reflectingPlane = new Plane( hit.normal, hit.point );
                foreach( Reflectable target in GetSceneRoot().GetComponentsInChildren<Reflectable>() )
                    target.OnReflectingMotion(this);
            }
        }
        else
        {
            paintDot.SetActive(false);

            if( state == "preview" )
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
