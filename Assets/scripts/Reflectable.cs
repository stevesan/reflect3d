using UnityEngine;
using System.Collections;
using Lobo;

public class Reflectable : MonoBehaviour
{
    public MeshFilter reflectionPreviewPrefab;

    private MeshFilter reflectionPreview;

    private ConvexPolygonMesh currentRealMesh = null;
    private ConvexPolygonMesher converter = new ConvexPolygonMesher();

    void Start()
    {
        if( currentRealMesh == null )
        {
            currentRealMesh = new ConvexPolygonMesh();
            Debug.Log("converting from unity mesh");
            currentRealMesh.polys = ConvexPolygon.FromUnityMesh( GetComponent<MeshFilter>().mesh );
        }
    }

    public void OnReflectingBegin(MirrorGun gun)
    {
        GameObject obj = Utils.ClonePrefab( reflectionPreviewPrefab.gameObject, transform );
        obj.name = gameObject.name + "-preview";
        Utils.IdentifyLocalTransform(obj);
        obj.transform.parent = transform.parent;
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
            Destroy( reflectionPreview.gameObject );

        if( isConfirm )
        {
            // Do the clip
            Plane lsPlane = transform.InverseTransformPlane( gun.GetReflectingPlane() );
            currentRealMesh.Clip( lsPlane );

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
                negMesh.Reflect( lsPlane );
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
        Plane lsPlane = transform.InverseTransformPlane( gun.GetReflectingPlane() );
        posHalf.Clip( lsPlane );
        converter.Push( posHalf.polys, GetComponent<MeshFilter>().mesh );

        // TODO we could just use the same mesh, but transform it to be reflected along the plane..

        ConvexPolygonMesh negHalf = posHalf.Clone();
        negHalf.Reflect( lsPlane );
        converter.Push( negHalf.polys, reflectionPreview.mesh );
    }


    // Important that this is done in LateUpdate, since one-frame off could have players falling through
    public void LateUpdate()
    {
        // Update mesh collider
        if( GetComponent<MeshCollider>() == null )
            gameObject.AddComponent<MeshCollider>();
    }
}
