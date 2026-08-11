using _3D_Engine.Classes.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.Scenes
{
    public class Scene
    {
        public float gravity { get; private set; } = 9.81f;

        public List<WorldObject> rootObjects;
        public List<PhysicsObject> AllPhysicsObjects;

        protected Game GameInstance;
        public bool IsReady = false;

        public Scene(Game game)
        {
            GameInstance = game;
            rootObjects = new List<WorldObject>();
            AllPhysicsObjects = new List<PhysicsObject>();
        }

        public virtual void Update(float dt)
        {
            // call in the update main loop of the game
            // Override this method to implement scene-specific update logic (e.g., animations, physics)
            
        }

        public virtual void OnFrameRender(float dt) {
            // call in the main OnFrameRender of the game
            // Override this method to implement scene-specific rendering logic (e.g., drawing objects)
            // draw all root objects, which will recursively draw their children

        }

        public virtual void createScene()
        {
            // Override this method to set up the initial state of the scene (e.g., create objects, set positions)
        }

        public virtual void destroyScene()
        {
            // Override this method to clean up resources when the scene is destroyed (e.g., dispose shaders, buffers)

        }
        public void AddObject(WorldObject obj)
        {
            rootObjects.Add(obj);
            obj.SetScene(this); // so the object knows which scene it belongs to
        }
        public void RemoveObject(WorldObject obj)
        {
            rootObjects.Remove(obj);
        }

    }
}
