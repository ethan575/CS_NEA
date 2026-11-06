using _3D_Engine.Classes.Data;
using _3D_Engine.Classes.NeuralNet;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
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
        List<HeadlessSimulationScene> AllSimWorlds = new List<HeadlessSimulationScene>();
        World ForegroundWorld = new World();

        Shader shader;
        PhysicsServer physicsServer;

        RagdollBody body;

        PhysicsObject convex1;

        bool isFirstSimulationClick = true;

        // simulation stuff
        bool isSimulationRunning = false;
        int populationSize = 20;
        RagdollBody LastLoadedCreature;
        NeuralNetwork LastSavedNN;
        private bool backgroundPopulationHidden = false;
        Generation currentGeneration = new Generation();


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

            physicsServer.Update(dt, AllSimWorlds, ForegroundWorld);

            //dt is in seconds
            if (isSimulationRunning)
            {
                currentGeneration.ActiveTimeLeft -= dt;
                if (currentGeneration.ActiveTimeLeft <= 0)
                {
                    PrepareEndGeneration();
                }
            }


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

                        LastLoadedCreature = BuilderSaveReadWrite.BuilderLoadFile(filePath, ForegroundWorld, shader, this);
                        body = LastLoadedCreature;

                        AllSimWorlds.Add(new HeadlessSimulationScene(ForegroundWorld, body));
                    }
                }
            }

            //ctrl + s must dave the trained NN
            //// test for savng a neural network and loading it back in, this is just a test and some will be removed later/ changes
            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.S))
            {
                var NN = new NeuralNetwork(new int[] { 10, 7, 8, 8 }); //{ 12, 7, 8, 8 } { 8, 7, 8, 8 }

                PhysicsServer.AllowedWorldStep = false;
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save NN as";
                    dialog.Filter = "NN File (*.nn)|*.nn";
                    dialog.DefaultExt = "nn";
                    dialog.AddExtension = true;
                    dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string SavePath = dialog.FileName;


                        NNSaveLoad.SaveNeuralNetwork(NN, SavePath);

                        Console.WriteLine("Neural Network saved  successfully.");
                    }
                }
            }

            // opening NN file
            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.O))
            {

                PhysicsServer.AllowedWorldStep = false;
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Open NN file";
                    dialog.Filter = "NN File (*.nn)|*.nn";
                    dialog.DefaultExt = "nn";
                    dialog.AddExtension = true;
                    dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string SavePath = dialog.FileName;

                        LastSavedNN = NNSaveLoad.LoadNeuralNetwork(SavePath);
                        NeuralNetwork loaded = LastSavedNN;

                        currentGeneration.Population = new List<NeuralNetwork>();

                        if (body != null)
                        {
                            body.SetBrain(loaded);
                            currentGeneration.Population.Add(loaded);
                        }

                        else
                            MessageBox.Show("There is no body to attatch a brain to.", "Error", MessageBoxButtons.OK);

                        Console.WriteLine("Neural Network saved and loaded successfully.");
                    }



                }
            }

            //!!!! Implement saving the fittest


            if (game.IsKeyDown(Keys.Space))
            {
                ////var testM = Matrix<float>.Build.Random((body.Bones.Count * 6) + 2, 1);
                //var inputM = body.GetBrainInputMatrix();
                //body.BrainNNFF(inputM);
                ////body.BrainNNFF(body.GetBrainInputMatrix());
                //Console.Write(inputM);
                for (int i = 0; i < AllSimWorlds.Count; i++)
                {
                    var scene = AllSimWorlds[i];
                    var inputM = scene.ragdollBody.GetBrainInputMatrix();
                    scene.ragdollBody.BrainNNFF(inputM);
                }
                Console.WriteLine(AllSimWorlds[0].ragdollBody.Joints[0].AngularVelocityRad());

            }

            if (game.IsKeyReleased(Keys.Space))
            {
                AllSimWorlds.ForEach(s => s.ragdollBody.ResetAngularVelocities());
            }


            if (game.KeyboardState.IsKeyPressed(Keys.Tab))
            {
                if (isFirstSimulationClick && LastLoadedCreature != null && LastSavedNN != null)
                {
                    isFirstSimulationClick = false;
                    PhysicsServer.AllowedWorldStep = true;

                    

                    //AllSimWorlds.Add(new HeadlessSimulationScene(ForegroundWorld, body));

                    for (int i = 0; i < populationSize - 1; i++) // '-1' because the first one is foreground
                    {
                        var newWorld = new World();

                        RigidBody floorRB = newWorld.CreateRigidBody();
                        floorRB.AddShape(new BoxShape(new JVector(40, 1f, 40)));
                        floorRB.MotionType = MotionType.Static;
                        floorRB.Position = new JVector(0, -10, 0);

                        //!!!!!!!!!!!!!!! create a method for bnes and joints so theu can be clone to be added to new worlds - done
                        RagdollBody newBody = LastLoadedCreature.Clone(newWorld);
                        currentGeneration.Population.Add(newBody.Brain);
                        // clone NN and set it to the new body
                        //newBody.SetBrain(LastSavedNN);
                        AllSimWorlds.Add(new HeadlessSimulationScene(newWorld, newBody));
                    }
                    AllSimWorlds.ForEach(scene => scene.ragdollBody.Joints.ForEach(joint => joint.Visual.visible = false));
                    AllSimWorlds.ForEach(scene => scene.ragdollBody.Bones.ForEach(bone => bone.Visual.visible = true));

                    isSimulationRunning = true;
                }

            }

            if (game.KeyboardState.IsKeyPressed(Keys.P))
            {

                for (int i = 0; i < AllSimWorlds.Count; i++)
                {
                    var scene = AllSimWorlds[i];
                    var com = scene.ragdollBody.COMPosition;
                    Console.WriteLine(
                        $"World {i}: bodies={scene.world.RigidBodies.Count} " +
                        $"COM=({com.X:F2}, {com.Y:F2}, {com.Z:F2}) " +
                        $"vel={scene.ragdollBody.BodyVelocity.Length():F2}");
                }
            }

            if (game.KeyboardState.IsKeyPressed(Keys.H))
            {
                for (int i = 1; i < AllSimWorlds.Count; i++)
                {
                    var scene = AllSimWorlds[i];
                    foreach (Bone bone in scene.ragdollBody.Bones)
                    {
                        bone.Visual.visible = backgroundPopulationHidden;
                        Console.WriteLine($"Bone {bone.name} visibility: {bone.Visual.visible}");
                    }
                }
                backgroundPopulationHidden = !backgroundPopulationHidden;
            }

            float u = 0.5f;

            if (game.KeyboardState.IsKeyDown(Keys.W))
            {
                convex1.changeVelociy(Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.S))
            {
                convex1.changeVelociy(-Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.A))
            {
                convex1.changeVelociy(-Vector3.UnitX * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.D))
            {
                convex1.changeVelociy(Vector3.UnitX * u);
            }



        }


        public override void OnFrameRender(float dt)
        {
            // Implement simulation-specific rendering logic (e.g., drawing objects)
            //body?.Update(dt);

            // ragdoll updates but it falls thorugh?
            // somethings not running update, in physics serevr?
            if (isFirstSimulationClick)
            {
                body?.Update(dt);
            }
            else
                foreach (var scene in AllSimWorlds)
                {

                    scene.ragdollBody.Update(dt);

                }


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



            RigidBody floorRB = ForegroundWorld.CreateRigidBody();
            floorRB.AddShape(new BoxShape(new JVector(40, 1f, 40)));
            floorRB.MotionType = MotionType.Static;
            floor = new PhysicsObject("floor", shader, floorVertex, this, game, floorRB, new Vector3(1, 1, 1));
            floor.Scale(2, 0.5f, 2);
            floor.setPosition(new Vector3(0, -10, 0));

            RigidBody RB1 = ForegroundWorld.CreateRigidBody();
            RB1.AddShape(new BoxShape(1));
            RB1.MotionType = MotionType.Dynamic;
            convex1 = new PhysicsObject("convex1", shader, cubeVertex, this, game, RB1, color);
            convex1.setMass(29f);
            convex1.setObeyGravity(true);
            convex1.Scale(0.5f);
            convex1.setPosition(5f, 3f, 12f);



            IsReady = true;
            PhysicsServer.AllowedWorldStep = false;
        }

        public override void destroyScene()
        {
            // Clean up resources when the simulation scene is destroyed ( dont dispose shaders, dispose object buffers)
            IsReady = false;

            foreach (var entity in new List<WorldObject>(SceneManager.RenderedEntities))
            {
                //entity.mesh.Dispose();
                entity.Destroy();
                ForegroundWorld.Clear();

            }

            // also clear the colliders from the static list
            ForegroundWorld.Clear();
            ForegroundWorld.Dispose();
            SceneManager.CurrentScene?.AllPhysicsObjects.Clear();


        }

        // start of evolve for next gen
        // fitness values as ragdoll data  required
        public void PrepareEndGeneration()
        {

            foreach (HeadlessSimulationScene scene in AllSimWorlds) // includes foreground world
            {
                scene.ragdollBody.EndOfGenerationTasks();
            }
            // Objectives.EvalProximityToTarget
            

            // give each network a fitness score based on the ragdoll performance
            for (int i = 0; i < AllSimWorlds.Count; i++)
            {
                var scene = AllSimWorlds[i];
                // calculate fitness score based on ragdoll performance
                float fitnessScore = FitnessEval.EvaluateFitness(scene.ragdollBody);
                scene.ragdollBody.Brain.FitnessScore = fitnessScore;
            }
            // best fitness score first so the fittest neural networks are at the top 
            currentGeneration.Population = currentGeneration.Population.OrderByDescending(nn => nn.FitnessScore).ToList();
            NNTrainer.Generations.Add(currentGeneration.Copy());

            currentGeneration = NNTrainer.EvolveGeneration(currentGeneration.Population);

            // new generation is the evolved NN
            // give evolved NNs a ragdoll
            // the ragdolls are still the same instance, but their neural networks are updated with the evolved ones
            for (int i = 0; i < AllSimWorlds.Count; i++)
            {
                var scene = AllSimWorlds[i];
                scene.ragdollBody.SetBrain(currentGeneration.Population[i]);
            }

            //List<NeuralNetwork> newNNs = NNTainer.EvolveGeneration(currentGeneration);
            //currentGeneration = new Generation();

            // positions and velocities are reset
            StartNextGeneration();
        }

        public void StartNextGeneration()
        {
            // reset
            AllSimWorlds.ForEach(scene =>
            {
                scene.ragdollBody.ResetAngularVelocities();
                scene.ragdollBody.setPostion(JVector.Zero);
                scene.ragdollBody.EnergyUsed = 0;
            });
        }
    }
}
