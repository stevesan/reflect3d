using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lobo
{
    public class Polygon
    {
        public List<Vector3> points = new List<Vector3>();

        public int GetNumPoints() { return points.Count; }

        // This will wrap the index safely, even if i is negative
        public Vector3 GetPoint(int i)
        {
            return points.GetWrapped(i);
        }

        //----------------------------------------
        //  Keeps the polygon on the positive side of the given plane
        //  TODO: UV coordinates
        //----------------------------------------
        public void Clip( Plane wsPlane, Transform meshTrans )
        {
            List<Vector3> newPoints = new List<Vector3>();

            for( int i = 0; i < points.Count; i++ )
            {
                Vector3 wsCurr = meshTrans.TransformPoint(GetPoint(i));
                Vector3 wsPrev = meshTrans.TransformPoint(GetPoint(i-1));
                bool currSide = wsPlane.GetSide( wsCurr );
                bool prevSide = wsPlane.GetSide( wsPrev );

                if( currSide != prevSide )
                {
                    // intersection
                    newPoints.Add( meshTrans.InverseTransformPoint(
                                Utils.GetIntersection( wsPlane, wsPrev, wsCurr ) ) );
                }

                if( currSide )
                    newPoints.Add( points[i] );
            }

            points = newPoints;
        }

        public void Reflect( Plane wsPlane, Transform meshTrans )
        {
            for( int i = 0; i < points.Count; i++ )
            {
                Vector3 wsPt = meshTrans.TransformPoint( points[i] );
                float dist = wsPlane.GetDistanceToPoint( wsPt );
                Vector3 wsNewPt = wsPt - wsPlane.normal * 2 * dist;
                points[i] = meshTrans.InverseTransformPoint(wsNewPt);
            }

            // We gotta reverse the winding order
            points.Reverse();
        }
        
        public void Translate( Vector3 d )
        {
            for( int i = 0; i < points.Count; i++ )
                points[i] = points[i] + d;
        }

        public static List<ConvexPolygon> FromUnityMesh( Mesh uMesh )
        {
            List<ConvexPolygon> polys = new List<ConvexPolygon>();
            polys.Capacity = uMesh.triangles.Length/3;

            for( int i = 0; i < uMesh.triangles.Length; i++ )
            {
                if( i % 3 == 0 )
                    polys.Add( new ConvexPolygon() );

                ConvexPolygon p = polys[ polys.Count-1 ];
                p.points.Add( uMesh.vertices[ uMesh.triangles[i] ] );
            }

            return polys;
        }
    }

    public class ConvexPolygon : Polygon
    {
        public ConvexPolygon Clone()
        {
            ConvexPolygon clone = new ConvexPolygon();
            clone.points.Capacity = points.Count;
            foreach( Vector3 p in points )
                clone.points.Add(Vector3.zero+p);

            return clone;
        }

        public static ConvexPolygon CreatePentagon()
        {
            ConvexPolygon poly = new ConvexPolygon();
            poly.points.Add( new Vector3(0, 0, 0) );
            poly.points.Add( new Vector3(1, 0, 0) );
            poly.points.Add( new Vector3(1.5f, 1, 0) );
            poly.points.Add( new Vector3(0.5f, 1.5f, 0) );
            poly.points.Add( new Vector3(-0.5f, 1, 0) );

            return poly;
        }

        public static List<ConvexPolygon> CreateTestMesh( int n )
        {
            List<ConvexPolygon> mesh = new List<ConvexPolygon>();

            for( int i = 0; i < n; i++ )
            {
                ConvexPolygon p = CreatePentagon();
                p.Translate( new Vector3( 3f*i, 0, 0 ) );
                mesh.Add( p );
            }

            return mesh;
        }
    }

    public class ConvexPolygonMesh
    {
        public List<ConvexPolygon> polys;

        public void Clip( Plane wsPlane, Transform trans )
        {
            List<int> toRemove = new List<int>();

            for( int i = 0; i < polys.Count; i++ )
            {
                polys[i].Clip(wsPlane, trans);

                if( polys[i].points.Count == 0 )
                    toRemove.Add(i);
            }

            // Remove in reverse
            for( int i = toRemove.Count-1; i >= 0; i-- )
            {
                polys.RemoveAt(toRemove[i]);
            }
        }

        public void Reflect( Plane wsPlane, Transform trans )
        {
            foreach( ConvexPolygon p in polys )
                p.Reflect(wsPlane, trans);
        }

        public ConvexPolygonMesh Clone()
        {
            ConvexPolygonMesh clone = new ConvexPolygonMesh();
            clone.polys = new List<ConvexPolygon>(polys.Count);

            foreach( ConvexPolygon p in polys )
                clone.polys.Add( p.Clone() );

            return clone;
        }

        public void ShallowAppend( ConvexPolygonMesh other )
        {
            foreach( ConvexPolygon p in other.polys )
                polys.Add(p);
        }
    }

    //----------------------------------------
    //  Handles pushing a set of convex polygons to a triangle mesh
    //  This keeps around builtin-arrays and only resizes as needed for efficiency
    //----------------------------------------
    public class ConvexPolygonMesher
    {
        public Vector3[] verts;
        public int[] triVerts;

        //----------------------------------------
        //  'polys' should be the same across calls for best results.
        //----------------------------------------
        public void Push( IList<ConvexPolygon> polys, Mesh mesh )
        {
            //----------------------------------------
            //  Figure out how large our buffers need to be
            //----------------------------------------

            int numVerts = 0;
            foreach( ConvexPolygon poly in polys )
            {
                if( poly.GetNumPoints() >= 3 )
                    numVerts += (poly.GetNumPoints()-2) * 3;
            }

            // Resize buffers
            if( verts == null || verts.Length < numVerts )
                verts = new Vector3[ numVerts ];

            if( triVerts == null || triVerts.Length < numVerts )
                triVerts = new int[numVerts];

            //----------------------------------------
            //  Set values
            //----------------------------------------
            int currVert = 0;
            int currTriVert = 0;

            for( int i = 0; i < polys.Count; i++ )
            {
                ConvexPolygon poly = polys[i];
                if( poly.GetNumPoints() < 3 )
                    continue;

                int firstVert = currVert;

                for( int j = 0; j < poly.GetNumPoints(); j++ )
                {
                    verts[ currVert ] = poly.GetPoint(j);
                    currVert++;
                }

                for( int j = 0; j < poly.GetNumPoints()-2; j++ )
                {
                    triVerts[ currTriVert++ ] = firstVert;
                    triVerts[ currTriVert++ ] = firstVert + j + 1;
                    triVerts[ currTriVert++ ] = firstVert + j + 2;
                }
            }

            //----------------------------------------
            //  Degenerate extra tris
            //----------------------------------------
            for( ; currTriVert < triVerts.Length; currTriVert++ )
                triVerts[ currTriVert ] = 0;

            //----------------------------------------
            //  
            //----------------------------------------
            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = new Vector2[ verts.Length ];
            mesh.triangles = triVerts;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.MarkDynamic();
        }
    }
}
