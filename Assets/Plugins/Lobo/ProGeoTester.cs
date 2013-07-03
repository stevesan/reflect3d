
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Lobo;

public class ProGeoTester : MonoBehaviour
{
    IEnumerator Start()
    {
        List<ConvexPolygon> mesh = ConvexPolygon.CreateTestMesh(1000);
        ConvexPolygonMesher mesher = new ConvexPolygonMesher();

        mesher.Push( mesh, GetComponent<MeshFilter>().mesh );

        float planeY = 0.1f;

        while( true )
        {
            yield return null;

            Plane plane = new Plane( Vector3.up, new Vector3( 0f, planeY, 0f ) );
            foreach( ConvexPolygon p in mesh )
                p.Clip(plane);

            mesher.Push( mesh, GetComponent<MeshFilter>().mesh );

            planeY += 0.1f * Time.deltaTime;
        }
    }
}
