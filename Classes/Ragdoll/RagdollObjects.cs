using _3D_Engine.Classes.Objects;
using Jitter2.Dynamics;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollObjects
    {
        private bool _isBeingEdited = false;
        public bool IsBeingEdited
        {
            get
            { return _isBeingEdited; }
            set
            {
                _isBeingEdited = value;
                if (value)
                {
                    // handle when the object is being edited like moved, rotation
                    RB.MotionType = MotionType.Static; // make the object static while editing
                }
                else
                {
                    // Logic to handle when the object is not being edited
                    RB.MotionType = MotionType.Dynamic; // make the object dynamic again
                }
            }
        }
        public WorldObject Visual { get; set; }
        public RigidBody RB { get; set; }
        public virtual void UpdateVisual()
        {
            // This method should be overridden in derived classes to update the visual representation of the object.
        }
    }
}
