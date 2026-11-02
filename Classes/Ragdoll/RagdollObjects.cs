using _3D_Engine.Classes.Helper;
using _3D_Engine.Classes.Objects;
using Jitter2.Dynamics;
using OpenTK.Mathematics;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollObjects
    {

        public bool EditFinalized = false;

        private bool _isBeingEdited = false;
        public bool IsBeingEdited
        {
            get
            { return _isBeingEdited; }
            set
            {
                _isBeingEdited = value;
                //if (value && this is Bone)
                //{
                //    // handle when the object is being edited like moved, rotation
                //    RB.MotionType = MotionType.Static; // make the object static while editing
                //}
                //else
                //{
                //    if (this is Bone)
                //    // Logic to handle when the object is not being edited
                //        RB.MotionType = MotionType.Dynamic; // make the object dynamic again

                //}
            }
        }

        public WorldObject Visual { get; set; }
        public RigidBody RB { get; set; }

        public Vector3 Position { get; set; }

        public RagdollObjects()
        {

        }

        public virtual void UpdateVisual()
        {
        }

        public virtual RagdollObjects Clone(Jitter2.World world)
        {
            return null;
        }
    }
}
