using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Scenes
{
    public  class HeadlessSimulationScene
    {

        public World world;
        public RagdollBody ragdollBody;

        public HeadlessSimulationScene(World world, RagdollBody ragdollBody)
        {
            this.world = world;
            this.ragdollBody = ragdollBody;

            world.AllowDeactivation = false;
        }

    }
}
