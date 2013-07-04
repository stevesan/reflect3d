using UnityEngine;
using System.Collections;
using Lobo;

public class Reflectable : MonoBehaviour
{
    public MeshFilter reflectionHost;

    private ConvexPolygonMesh currentRealMesh = new ConvexPolygonMesh();
    private ConvexPolygonMesher converter = new ConvexPolygonMesher();

    void Awake()
    {
        currentRealMesh.polys = ConvexPolygon.FromUnityMesh( GetComponent<MeshFilter>().mesh );
    }

    public void OnReflectingBegin(MirrorGun gun)
    {
    }

    public void OnReflectingEnd(MirrorGun gun, bool isConfirm)
    {
        reflectionHost.mesh.Clear();

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
        converter.Push( negHalf.polys, reflectionHost.mesh );
    }

}
