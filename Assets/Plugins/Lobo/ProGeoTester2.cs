
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Lobo;

public class ProGeoTester2 : MonoBehaviour
{
    public MeshFilter reflectionHost;

    IEnumerator Start()
    {
        ConvexPolygonMesh mesh = new ConvexPolygonMesh();
        mesh.polys = ConvexPolygon.FromUnityMesh( GetComponent<MeshFilter>().mesh );

        ConvexPolygonMesher converter = new ConvexPolygonMesher();
        converter.Push( mesh.polys, GetComponent<MeshFilter>().mesh );

        Vector3 wsMin = Utils.GetWorldMins( GetComponent<MeshFilter>() );
        float planeY = wsMin.y;

        while( true )
        {
            yield return new WaitForSeconds(0.01f);
            Debug.Log("clipping mesh");

            Plane wsPlane = new Plane( new Vector3(0.5f, 0.5f, 0f).normalized, new Vector3(0f, planeY, 0f) );
            mesh.Clip( wsPlane, transform );
            converter.Push( mesh.polys, GetComponent<MeshFilter>().mesh );
            
            ConvexPolygonMesh relfection = mesh.Clone();
            relfection.Reflect( wsPlane, transform );
            converter.Push( relfection.polys, reflectionHost.mesh );

            planeY += 0.05f * Time.deltaTime;
        }

        yield return null;
    }
}
