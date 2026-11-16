using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;

namespace _3D_Engine.Classes.Scenes
{
    public sealed class RagdollSelfCollisionFilter : IBroadPhaseFilter
    {
        private readonly HashSet<RigidBody> ragdollBodies;

        public RagdollSelfCollisionFilter(IEnumerable<Bone> bones)
        {
            ragdollBodies = bones.Select(b => b.RB).ToHashSet();
        }

        public bool Filter(IDynamicTreeProxy proxyA, IDynamicTreeProxy proxyB)
        {
            if (proxyA is RigidBodyShape shapeA && proxyB is RigidBodyShape shapeB)
            {
                if (ragdollBodies.Contains(shapeA.RigidBody) && ragdollBodies.Contains(shapeB.RigidBody))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class HeadlessSimulationScene
    {

        public World world;
        public RagdollBody ragdollBody;

        public HeadlessSimulationScene(World world, RagdollBody ragdollBody)
        {
            this.world = world;
            this.ragdollBody = ragdollBody;

            world.AllowDeactivation = false;
            if (ragdollBody != null && ragdollBody.Bones.Count > 1)
            {
                world.BroadPhaseFilter = new RagdollSelfCollisionFilter(ragdollBody.Bones);
            }
        }

    }
}
