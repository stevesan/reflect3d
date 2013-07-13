using UnityEngine;
using System.Collections;
using Lobo;

public class Reflectable : MonoBehaviour
{
    private MeshFilter reflectionPreview;

    private ConvexPolygonMesh currentRealMesh = null;
    private ConvexPolygonMesher converter = new ConvexPolygonMesher();

    void Start()
    {
        if( currentRealMesh == null )
        {
            currentRealMesh = new ConvexPolygonMesh();
            currentRealMesh.polys = ConvexPolygon.FromUnityMesh( GetComponent<MeshFilter>().mesh );
        }
    }

    public void OnReflectingBegin(MirrorGun gun)
    {
        // Parent it to us first, ID it, then unparent it. So it inherits out transform.
        GameObject obj = Utils.ClonePrefab( PreviewPrefab.main.gameObject, transform );
        Utils.IdentifyLocalTransform(obj);
        //obj.transform.parent = transform.parent;
        obj.name = gameObject.name + "-preview";
        reflectionPreview = obj.GetComponent<MeshFilter>();
    }

    public void RecreateCollider()
    {
        Destroy( GetComponent<MeshCollider>() );
        // Recreate it during next update - for some reason, this is necessary..not sure why.
    }

    public void OnReflectingEnd(MirrorGun gun, bool isConfirm)
    {
        if( reflectionPreview != null )
        {
            reflectionPreview.transform.parent = null;  // so when we clone ourselves to commit, we don't get the preview child
            Destroy( reflectionPreview.gameObject );
        }

        if( isConfirm )
        {
            // Do the clip
            Plane wsPlane = gun.GetReflectingPlane();
            //Plane lsPlane = transform.InverseTransformPlane( gun.GetReflectingPlane() );
            currentRealMesh.Clip( wsPlane, transform );

            if( currentRealMesh.polys.Count <= 0 )
            {
                Destroy(gameObject);
            }
            else
            {
                converter.Push( currentRealMesh.polys, GetComponent<MeshFilter>().mesh );
                RecreateCollider();

                // Create negative half clone
                GameObject negObj = Utils.ClonePrefab( gameObject, transform.parent );
                ConvexPolygonMesh negMesh = currentRealMesh.Clone();
                negMesh.Reflect( wsPlane, transform );
                Reflectable re = negObj.GetComponent<Reflectable>();
                re.currentRealMesh = negMesh;
                re.converter.Push( negMesh.polys, negObj.GetComponent<MeshFilter>().mesh );
                re.RecreateCollider();

                gameObject.SetActive(true);
            }

        }
        else
        {
            // restore original mesh
            converter.Push( currentRealMesh.polys, GetComponent<MeshFilter>().mesh );
        }
    }

    public void OnReflectingMotion(MirrorGun gun)
    {
        ConvexPolygonMesh posHalf = currentRealMesh.Clone();
        posHalf.Clip( gun.GetReflectingPlane(), transform );
        converter.Push( posHalf.polys, GetComponent<MeshFilter>().mesh );   // ~1ms

        // TODO we could just use the same mesh, but transform it to be reflected along the plane..
        ConvexPolygonMesh negHalf = posHalf.Clone();
        negHalf.Reflect( gun.GetReflectingPlane(), transform );
        converter.Push( negHalf.polys, reflectionPreview.mesh );
    }

    // Important that this is done in LateUpdate, since one-frame late could have players falling through
    public void LateUpdate()
    {
        // Update mesh collider
        if( GetComponent<MeshCollider>() == null )
            gameObject.AddComponent<MeshCollider>();
    }
}
