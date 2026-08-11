using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Ragdoll;
using _3D_Engine.Classes.UI;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Vector3 = OpenTK.Mathematics.Vector3;


// implement a simulation scene that inherits from the base Scene class
// this can be used to add objects and logic specific to a simulation, such as physics or AI updates

namespace _3D_Engine.Classes.Scenes
{
    public class SimulationScene : Scene
    {
        Game game;

        WorldObject jointMesh;
        WorldObject boneMesh;
        WorldObject boneMesh2;

        PhysicsObject floor;

        PhysicsObject convex1;
        PhysicsObject convex2;

        // jitter physics world
        World world = new World();

        Shader shader;
        PhysicsServer physicsServer;

        RagdollBody r1;
        Bone bone1;
        Joint joint1;


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

            if (game.KeyboardState.IsKeyPressed(Keys.Escape))
            {
                SceneManager.LoadScene(new BuildScene(game));
                return;
                //convex2.Destroy();
                //convex2 = null;

            }

            if (game.KeyboardState.IsKeyDown(Keys.P))
            {
                Console.WriteLine(convex2.Name);
            }


            float u = 0.5f;

            if (!game.KeyboardState.IsKeyDown(Keys.Q) && !game.KeyboardState.IsKeyDown(Keys.E))
            {
                joint1.HindgeJointConstraint.Motor.TargetVelocity = 0f;
                joint1.FreeJoint();
            }

            if (game.KeyboardState.IsKeyDown(Keys.Q))
            {
                //bone1.RB.Torque = JVector.UnitZ*5000;
                joint1.TorqueToTargetAngle(MathF.PI / 2);

            }
            if (game.KeyboardState.IsKeyDown(Keys.E))
            {
                //bone1.RB.Torque = -JVector.UnitZ * 5000;
                joint1.TorqueToTargetAngle(-MathF.PI / 2);

            }

            if (game.keyboardState.IsKeyDown(Keys.P))
            {
                r1.ApplyForce(JVector.UnitY * 200);
            }

            if (game.KeyboardState.IsKeyDown(Keys.W))
            {
                convex2.changeVelociy(Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.S))
            {
                convex2.changeVelociy(-Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.A))
            {
                convex2.changeVelociy(-Vector3.UnitX * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.D))
            {
                convex2.changeVelociy(Vector3.UnitX * u);
            }

            if (game.KeyboardState.IsKeyDown(Keys.Down))
            {
                convex2.changeVelociy(-Vector3.UnitY * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.Up))
            {
                convex2.changeVelociy(Vector3.UnitY * u);
            }

            if (game.KeyboardState.IsKeyDown(Keys.R))
            {
                convex1.setPosition(5f, 3f, 20f);
                convex1.velocity = Vector3.Zero;

                convex2.setPosition(5, 0, 20f);
                convex2.velocity = Vector3.Zero;


            }

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

            r1.Update(dt);
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
            floor.setPosition(new Vector3(0, -10, 0));

            RigidBody RB1 = world.CreateRigidBody();
            RB1.AddShape(new BoxShape(1));
            RB1.MotionType = MotionType.Dynamic;
            convex1 = new PhysicsObject("convex1", shader, cubeVertex, this, game, RB1, color);
            convex1.setMass(29f);
            convex1.setObeyGravity(true);
            convex1.Scale(0.5f);
            convex1.setPosition(5f, 3f, 12f);

            RigidBody RB2 = world.CreateRigidBody();
            RB2.AddShape(new BoxShape(1));
            RB2.MotionType = MotionType.Dynamic;
            convex2 = new PhysicsObject("convex2", shader, cubeVertex, this, game, RB2, new Vector3(0.4f, 1, 0.7f));
            convex2.setMass(2f);
            RB2.SetMassInertia(5);
            convex2.Scale(0.5f);
            convex2.setObeyGravity(true);
            convex2.setPosition(5f, 0f, 5f);
            convex2.setRotation(45, 45, 0);
            //convex2.setRotation(45, 45, 0);


            WorldObject b1 = new WorldObject("b1", shader, boneVertex, this, color);
            b1.setScale(1f, 0.5f, 0.5f);
            b1.setRotation(0, 0, 0);
            bone1 = new Bone(world, b1, new Vector3(0, 5, 10), "b1");

            WorldObject b2 = new WorldObject("b2", shader, boneVertex, this, color);
            b2.setScale(1f, 0.5f, 0.5f);
            b2.setRotation(0, 0, 0);
            Bone bone2 = new Bone(world, b2, new Vector3(b1.GetPosition().X + (b1.GetScale().X * 8) + 1, 5, 10), "b2");

            WorldObject b3 = new WorldObject("b3", shader, boneVertex, this, color);
            b3.setScale(1f, 0.5f, 0.5f);
            b3.setRotation(0, 0, 0);
            Bone bone3 = new Bone(world, b3, new Vector3(b2.GetPosition().X + (b2.GetScale().X * 8) + 10, 5, 10), "b3");


            //WorldObject j1 = new WorldObject("j1", shader, jointVertex, this, color);
            //j1.setScale(0.5f, 0.5f, 0.5f);
            //j1.setPosition(new Vector3(0, 10, 0));

            WorldObject j1 = new WorldObject("j1", shader, jointVertex, this, color);
            WorldObject j2 = new WorldObject("j2", shader, jointVertex, this, color);
            joint1 = new Joint(world, bone1, bone2, j1);
            Joint joint2 = new Joint(world, bone2, bone3, j2);

            r1 = new RagdollBody(new List<Bone> { bone1, bone2, bone3 }, new List<Joint> { joint1, joint2 });


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
