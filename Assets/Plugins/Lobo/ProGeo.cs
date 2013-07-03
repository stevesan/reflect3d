using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lobo
{
    public class Polygon
    {
        public List<Vector3> points = new List<Vector3>();

        public Vector3 cachedNormal;

        public int GetNumPoints() { return points.Count; }

        public void UpdateCachedNormal()
        {
            if( points.Count >= 3 )
            {
                Vector3 e0 = points[1] - points[0];
                Vector3 e1 = points[2] - points[0];
                cachedNormal = Vector3.Cross( e0, e1 ).normalized;
            }
        }

        // This will wrap the index safely, even if i is negative
        public Vector3 GetPoint(int i)
        {
            return points.GetWrapped(i);
        }

        //----------------------------------------
        //  Keeps the polygon on the positive side of the given plane
        //  TODO: UV coordinates
        //----------------------------------------
        public void Clip( Plane plane )
        {
            List<Vector3> newPoints = new List<Vector3>();

            for( int i = 0; i < points.Count; i++ )
            {
                bool currSide = plane.GetSide( GetPoint(i) );
                bool prevSide = plane.GetSide( GetPoint(i-1) );

                if( currSide != prevSide )
                {
                    // intersection
                    newPoints.Add( Utils.GetIntersection( plane, GetPoint(i-1), GetPoint(i) ) );
                }

                if( currSide )
                    newPoints.Add( points[i] );
            }

            points = newPoints;
        }

        public void Reflect( Plane plane )
        {
            for( int i = 0; i < points.Count; i++ )
            {
                float dist = plane.GetDistanceToPoint( points[i] );
                points[i] -= plane.normal * 2 * dist;
            }
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

            // Update all normals
            foreach( ConvexPolygon p in polys )
                p.UpdateCachedNormal();

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

            clone.cachedNormal = cachedNormal;

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
            poly.UpdateCachedNormal();

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

        public void Clip( Plane lsPlane )
        {
            foreach( ConvexPolygon p in polys )
                p.Clip(lsPlane);
        }

        public void Reflect( Plane lsPlane )
        {
            foreach( ConvexPolygon p in polys )
                p.Reflect(lsPlane);
        }

        public ConvexPolygonMesh Clone()
        {
            ConvexPolygonMesh clone = new ConvexPolygonMesh();
            clone.polys = new List<ConvexPolygon>();
            clone.polys.Capacity = polys.Count;

            foreach( ConvexPolygon p in polys )
                clone.polys.Add( p.Clone() );

            return clone;
        }
    }

    //----------------------------------------
    //  Handles pushing a set of convex polygons to a triangle mesh
    //  This keeps around builtin-arrays and only resizes as needed for efficiency
    //----------------------------------------
    public class ConvexPolygonMesher
    {
        public Vector3[] verts;
        public Vector3[] normals;
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

            if( normals == null || normals.Length < numVerts )
                normals = new Vector3[ numVerts ];

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
                    normals[ currVert ] = poly.cachedNormal;
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
            mesh.normals = normals;
            mesh.triangles = triVerts;

            mesh.RecalculateBounds();
            mesh.MarkDynamic();
        }
    }
}
