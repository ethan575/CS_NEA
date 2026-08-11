using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace _3D_Engine.Classes.Scenes
{
    public class BuildScene : Scene
    {

        List<RagdollObjects> SelectableObjects = new List<RagdollObjects>();

        Game game;
        Shader shader;
        PhysicsServer physicsServer;
        // jitter physics world
        World world = new World();

        PhysicsObject floor;

        RagdollBuilder builder;

        public BuildScene(Game game) : base(game)
        {
            this.game = game;
            shader = game.shader;
            this.builder = new RagdollBuilder(game, world);

            physicsServer = new PhysicsServer();

        }

        public override void Update(float dt)
        {
            physicsServer.Update(dt, world);

            if (game.KeyboardState.IsKeyPressed(Keys.Escape))
            {
                SceneManager.LoadScene(new SimulationScene(game));
                return;
            }

            //if (game.mouseState.IsButtonPressed(MouseButton.Left))
            //{
            //    Console.WriteLine(game.mouseState.Position);
            //}

            builder.Update();

        }

        public override void OnFrameRender(float dt)
        {
            // Implement simulation-specific rendering logic (e.g., drawing objects)
            // This method will be called in the main OnFrameRender of the game
            // this is temporary, just to test the collision
            // it phases the objects through the collision eventually
            // ********implement forces and velocity to make it more realistic and not phase through 
            //ballMesh1.moveBy(0f, -0.01f, 0f);
            //ballMesh1.DrawChildrenRecursive();
            //ballMesh2.DrawChildrenRecursive();

            //floor.DrawChildrenRecursive();

            //convex1.DrawChildrenRecursive();
            //convex2.DrawChildrenRecursive();
            foreach (var entity in SceneManager.RenderedEntities)
            {
                entity.Draw();
            }

            //r1.Update(dt);
            //Console.WriteLine($"ball2 position: {ballMesh2.GetWorldPosition()}");

        }

        public override void createScene()
        {
            Vector3 color = new Vector3(0.5f, 0.5f, 0.5f);

            // Set up the initial state of the simulation scene (e.g., create objects, set positions)
            float[] jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
            float[] boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");
            float[] floorVertex = ModelImporter.LoadModel("models/Raw/floor.stl");
            float[] cubeVertex = ModelImporter.LoadModel("models/Raw/cube.stl");



            RigidBody floorRB = world.CreateRigidBody();
            floorRB.AddShape(new BoxShape(new JVector(40, 1f, 40)));
            floorRB.MotionType = MotionType.Static;
            floor = new PhysicsObject("floor", shader, floorVertex, this, game, floorRB, new Vector3(1, 1, 1));
            floor.Scale(2, 0.5f, 2);
            floor.setPosition(new Vector3(0, -10, 10));
            //floor.setPosition(new Vector3(0, -10, 0));

            IsReady = true;
        }

        public override void destroyScene()
        {
            // Clean up resources when the simulation scene is destroyed ( dont dispose shaders, dispose object buffers)
            IsReady = false;

            foreach (WorldObject entity in new List<WorldObject>(SceneManager.RenderedEntities))
            {
                //entity.mesh.Dispose();
                entity.Destroy();
                world.Clear();

            }

            // also clear the colliders from the static list
            SceneManager.CurrentScene?.AllPhysicsObjects.Clear();


        }
    }

}

