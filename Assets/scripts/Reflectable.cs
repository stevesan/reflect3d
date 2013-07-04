using UnityEngine;
using System.Collections;
using Lobo;

public class Reflectable : MonoBehaviour
{
    public MeshFilter reflectionPreviewPrefab;

    private MeshFilter reflectionPreview;

    private ConvexPolygonMesh currentRealMesh = new ConvexPolygonMesh();
    private ConvexPolygonMesher converter = new ConvexPolygonMesher();

    void Awake()
    {
        currentRealMesh.polys = ConvexPolygon.FromUnityMesh( GetComponent<MeshFilter>().mesh );
    }

    public void OnReflectingBegin(MirrorGun gun)
    {
        GameObject obj = Utils.ClonePrefab( reflectionPreviewPrefab.gameObject, this.transform );
        Utils.IdentifyLocalTransform(obj);
        reflectionPreview = obj.GetComponent<MeshFilter>();
    }

    public void OnReflectingEnd(MirrorGun gun, bool isConfirm)
    {
        if( reflectionPreview != null )
            Destroy( reflectionPreview.gameObject );
        Destroy( GetComponent<MeshCollider>() );    // force recreate, so the collider updates with the new mesh

        if( isConfirm )
        {
            // Append negative half to positive half
            ConvexPolygonMesh posHalf = currentRealMesh.Clone();
            Plane lsPlane = transform.InverseTransformPlane( gun.GetReflectingPlane() );
            posHalf.Clip( lsPlane );

            // TODO we could just use the same mesh, but transform it to be reflected along the plane..

            ConvexPolygonMesh negHalf = posHalf.Clone();
            negHalf.Reflect( lsPlane );

            posHalf.ShallowAppend(negHalf);
            currentRealMesh = posHalf;
        }

        converter.Push( currentRealMesh.polys, GetComponent<MeshFilter>().mesh );

        Debug.Log("current real mesh has "+currentRealMesh.polys.Count+ " polys");
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


    public void Update()
    {
        // Update mesh collider
        if( GetComponent<MeshCollider>() == null )
            gameObject.AddComponent<MeshCollider>();
    }
}
