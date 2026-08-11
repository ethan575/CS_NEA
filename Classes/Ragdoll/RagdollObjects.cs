using _3D_Engine.Classes.Objects;
using Jitter2.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollObjects
    {
        public WorldObject Visual { get; set; }
        public RigidBody RB { get; set; }
        public virtual void UpdateVisual()
        {
            // This method should be overridden in derived classes to update the visual representation of the object.
        }   
    }
}
