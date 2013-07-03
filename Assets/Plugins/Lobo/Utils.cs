using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lobo
{
    public static class Utils
    {
        public static T GetWrapped<T>( this IList<T> list, int i )
        {
            while( i < 0 )
                i += list.Count;

            return list[ i % list.Count ];
        }

        //----------------------------------------
        //  This assumes a and b are known to be opposite sided
        //----------------------------------------
        public static Vector3 GetIntersection( Plane p, Vector3 a, Vector3 b )
        {
            Vector3 dir = (b-a).normalized;
            Ray ray = new Ray( a, dir );
            float intxDist = 0;
            p.Raycast( ray, out intxDist );
            return a + dir*intxDist;
        }

        public static Vector3 GetWorldMins( MeshFilter filter )
        {
            return filter.gameObject.transform.TransformPoint(
                    filter.mesh.bounds.min );
        }

        public static Plane InverseTransformPlane( this Transform xform, Plane wsPlane )
        {
            Vector3 wsPoint = -1f * wsPlane.normal * wsPlane.distance;

            Plane lsPlane = new Plane(
                    xform.InverseTransformDirection( wsPlane.normal ),
                    xform.InverseTransformPoint( wsPoint ));

            return lsPlane;
        }
    }

}
