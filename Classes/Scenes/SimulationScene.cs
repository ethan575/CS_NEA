using _3D_Engine.Classes.Data;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using System.Windows.Forms;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Vector3 = OpenTK.Mathematics.Vector3;


// implement a simulation scene that inherits from the base Scene class
// this can be used to add objects and logic specific to a simulation, such as physics or AI updates

namespace _3D_Engine.Classes.Scenes
{
    public class SimulationScene : Scene
    {
        Game game;



        PhysicsObject floor;


        // jitter physics world
        World world = new World();

        Shader shader;
        PhysicsServer physicsServer;

        RagdollBody body;


        public SimulationScene(Game game) : base(game)
        {
            shader = game.shader;
            this.game = game;

            physicsServer = new PhysicsServer();

            //world.SubstepCount = 4;

            createScene();

        }

        public override void Update(float dt)
        {
            // Implement simulation-specific update logic (e.g., physics, AI)
            // This method will be called in the main update loop of the game

            physicsServer.Update(dt, world);
            body?.Update(dt);

            if (game.KeyboardState.IsKeyPressed(Keys.Escape))
            {
                SceneManager.LoadScene(new BuildScene(game));
                return;
                //convex2.Destroy();
                //convex2 = null;

            }

            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.N))
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Creature File (*.creature)|*.creature";
                    dialog.Title = "Select a creature file";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dialog.FileName;
                        
                        body = BuilderSaveReadWrite.BuilderLoadFile(filePath, world, shader, this);
                    }
                }
            }




        }
        public override void OnFrameRender(float dt)
        {
            // Implement simulation-specific rendering logic (e.g., drawing objects)

            foreach (var entity in SceneManager.RenderedEntities)
            {
                entity.Draw();
            }
        }

        public override void createScene()
        {
            base.createScene();

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
            floor.setPosition(new Vector3(0, -10, 0));



            IsReady = true;
        }

        public override void destroyScene()
        {
            // Clean up resources when the simulation scene is destroyed ( dont dispose shaders, dispose object buffers)
            IsReady = false;

            foreach (var entity in new List<WorldObject>(SceneManager.RenderedEntities))
            {
                //entity.mesh.Dispose();
                entity.Destroy();
                world.Clear();

            }

            // also clear the colliders from the static list
            world.Clear();
            world.Dispose();
            SceneManager.CurrentScene?.AllPhysicsObjects.Clear();


        }
    }
}
