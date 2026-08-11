using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.Scenes;
using Jitter2;
using Jitter2.LinearMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.physics
{
    public class PhysicsServer
    {

        public void Update(float dt, World jitterWorld)
        {
            
            jitterWorld.Step(dt);

            foreach (PhysicsObject obj in  SceneManager.CurrentScene.AllPhysicsObjects)
            {
       
                obj.Update(dt);
            }
        }

    }
}
