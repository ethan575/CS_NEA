using _3D_Engine.Classes.Objects;
using Jitter2;
using Jitter2.Collision;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.physics
{

    public struct RaycastHitInfo
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Distance;
    }

    public static class Raycast
    {
        public static bool RaycastHit(World world, JVector origin, JVector direction, float maxDistance, out RaycastHitInfo hitInfo,
            out JVector Normal, out JVector hitOffset, out IDynamicTreeProxy proxy, DynamicTree.RayCastFilterPre preFilter = null, DynamicTree.RayCastFilterPost postFilter = null)
        {

            //bool hit = world.DynamicTree.RayCast(origin, direction, preFilter, postFilter, out proxy, out JVector normal, out float distance);
            bool hit = world.DynamicTree.RayCast(origin, direction, maxDistance, preFilter, postFilter,
                                                    out proxy, out JVector normal, out float distance);

            hitInfo = hit ? new RaycastHitInfo()
            {
                Point = FromJVec(origin + (direction * distance)),
                Normal = FromJVec(normal),
                Distance = distance,
            }: default;

            hitInfo = hit ? hitInfo : default;
            Normal = hit ? normal : default;
            hitOffset = hit ? direction * distance : default;
            
            

            return hit;

        }

        static Vector3 FromJVec(JVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        static JVector ToJVec(Vector3 v)
        {
            return new JVector(v.X, v.Y, v.Z);
        }
    }
}
