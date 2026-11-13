using _3D_Engine.Classes.Data;
using _3D_Engine.Classes.NeuralNet;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Ragdoll;
using _3D_Engine.Classes.UI;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using System.Security.Cryptography;
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

        RagdollBody ForegroundBody;

        PhysicsObject TargetCube;

        bool isFirstSimulationClick = true;

        // simulation stuff
        bool isSimulationRunning = false;
        int GenNumber = 0;
        int populationSize = 50;
        RagdollBody LastLoadedCreature;
        private bool backgroundPopulationHidden = false;
        Generation currentGeneration = new Generation();
        string logFileName;
        private Random rng = new Random();

        public SimulationScene(Game game) : base(game)
        {
            shader = game.shader;
            this.game = game;

            physicsServer = new PhysicsServer();

            //world.SubstepCount = 4;
            logFileName = $"{DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace(" ", "_")}_training.log";

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

                for (int i = 0; i < AllSimWorlds.Count; i++)
                {
                    var scene = AllSimWorlds[i];
                    scene.ragdollBody.TargetPosition = new JVector(TargetCube.GetPosition().X, TargetCube.GetPosition().Y, TargetCube.GetPosition().Z);
                    var inputM = scene.ragdollBody.GetBrainInputMatrix();
                    scene.ragdollBody.BrainNNFF(inputM);
                    //if (i < 2)
                    //{
                    //    foreach (var j in scene.ragdollBody.Joints)
                    //        Console.WriteLine($"{i}: {j.HindgeJointConstraint.Motor.TargetVelocity}");
                    //}
                }

            }

            else
            {
                AllSimWorlds.ForEach(s => s.ragdollBody.ResetAngularVelocities());
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
                        ForegroundBody = LastLoadedCreature;

                        AllSimWorlds.Add(new HeadlessSimulationScene(ForegroundWorld, ForegroundBody));
                    }
                }
            }

            //ctrl + s must dave the best NN
            //// test for savng a neural network and loading it back in, this is just a test and some will be removed later/ changes
            //if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.S))
            //{
            //    var NN = new NeuralNetwork(new int[] { 10, 7, 8, 8 }); //{ 12, 7, 8, 8 } { 8, 7, 8, 8 }

            //    PhysicsServer.AllowedWorldStep = false;
            //    using (var dialog = new SaveFileDialog())
            //    {
            //        dialog.Title = "Save NN as";
            //        dialog.Filter = "NN File (*.nn)|*.nn";
            //        dialog.DefaultExt = "nn";
            //        dialog.AddExtension = true;
            //        dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();

            //        if (dialog.ShowDialog() == DialogResult.OK)
            //        {
            //            string SavePath = dialog.FileName;


            //            NNSaveLoad.SaveNeuralNetwork(NN, SavePath);

            //            Console.WriteLine("Neural Network saved  successfully.");
            //        }
            //    }
            //}

            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.S) )
            {
                
                NeuralNetwork NN = currentGeneration.Population.OrderByDescending(nn => nn.FitnessScore).ToList()[0];

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
                PhysicsServer.AllowedWorldStep = true;
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

                        NeuralNetwork loaded = NNSaveLoad.LoadNeuralNetwork(SavePath);

                        currentGeneration.Population = new List<NeuralNetwork>();

                        if (ForegroundBody != null)
                        {
                            ForegroundBody.SetBrain(loaded);
                            currentGeneration.Population.Add(loaded);
                        }

                        else
                            MessageBox.Show("There is no body to attatch a brain to.", "Error", MessageBoxButtons.OK);

                        Console.WriteLine("Neural Network saved and loaded successfully.");
                    }



                }
            }

            // Implement saving the fittest


            //if (game.IsKeyDown(Keys.Space))
            //{
            //    ////var testM = Matrix<float>.Build.Random((body.Bones.Count * 6) + 2, 1);
            //    //var inputM = body.GetBrainInputMatrix();
            //    //body.BrainNNFF(inputM);
            //    ////body.BrainNNFF(body.GetBrainInputMatrix());
            //    //Console.Write(inputM);
            //    for (int i = 0; i < AllSimWorlds.Count; i++)
            //    {
            //        var scene = AllSimWorlds[i];
            //        scene.ragdollBody.TargetPosition = new JVector(TargetCube.GetPosition().X, TargetCube.GetPosition().Y, TargetCube.GetPosition().Z);
            //        var inputM = scene.ragdollBody.GetBrainInputMatrix();
            //        scene.ragdollBody.BrainNNFF(inputM);
            //    }

            //}

            //if (game.IsKeyReleased(Keys.Space))
            //{
            //    AllSimWorlds.ForEach(s => s.ragdollBody.ResetAngularVelocities());
            //}


            if (game.KeyboardState.IsKeyPressed(Keys.Tab))
            {
                if (LastLoadedCreature == null)
                {
                    MessageBox.Show("No creature loaded. Please load a creature first.", "Error", MessageBoxButtons.OK);
                    return;
                }
                else if (ForegroundBody.Brain == null)
                {
                    NeuralNetwork newNN = new NeuralNetwork(new int[] { (ForegroundBody.Joints.Count * 2) + 8, 8, 8, ForegroundBody.Joints.Count });
                    ForegroundBody.SetBrain(newNN);
                    
                }

                if (isFirstSimulationClick && LastLoadedCreature != null)
                {
                    isFirstSimulationClick = false;
                    PhysicsServer.AllowedWorldStep = true;
                    currentGeneration.Population = new List<NeuralNetwork>();
                    currentGeneration.Population.Add(ForegroundBody.Brain);


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
                        //Console.WriteLine($"Bone {bone.name} visibility: {bone.Visual.visible}");
                    }
                }
                backgroundPopulationHidden = !backgroundPopulationHidden;
            }

            float u = 0.5f;

            if (game.KeyboardState.IsKeyDown(Keys.W))
            {
                TargetCube.changeVelociy(Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.S))
            {
                TargetCube.changeVelociy(-Vector3.UnitZ * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.A))
            {
                TargetCube.changeVelociy(-Vector3.UnitX * u);
            }
            if (game.KeyboardState.IsKeyDown(Keys.D))
            {
                TargetCube.changeVelociy(Vector3.UnitX * u);
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
                ForegroundBody?.Update(dt);
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
            
            floorRB.AddShape(new BoxShape(new JVector(120, 1f, 120)));
            floorRB.MotionType = MotionType.Static;
            floor = new PhysicsObject("floor", shader, floorVertex, this, game, floorRB, new Vector3(1, 1, 1));
            floor.Scale(6, 0.5f, 6); 
            // so scale factor = (20, 2, 20)
            floor.setPosition(new Vector3(0, -10, 0));

            RigidBody RB1 = ForegroundWorld.CreateRigidBody();
            RB1.SetMassInertia(20);
            RB1.AddShape(new BoxShape(1));
            RB1.MotionType = MotionType.Dynamic;
            TargetCube = new PhysicsObject("convex1", shader, cubeVertex, this, game, RB1, color);
            TargetCube.setMass(29f);
            TargetCube.setObeyGravity(true);
            TargetCube.Scale(0.5f);
            TargetCube.setPosition(5f, 3f, 12f);

            

            IsReady = true;
            PhysicsServer.AllowedWorldStep = false;
            UiManager.UiMode = UiTypes.Simulation;
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
            UiManager.UiTexts["BestFitness"] = $"{currentGeneration.Population[0].FitnessScore}";
            UiManager.UiTexts["Generation Time left"] = Convert.ToString(currentGeneration.ActiveTimeLeft);
            UiManager.FitnessGraphValues.Add(currentGeneration.Population[0].FitnessScore);
            NNTrainer.Generations.Add(currentGeneration.Copy());

            //string log = $"Best Fitness: {currentGeneration.Population[0].FitnessScore} \n\n " +
            //    $"Weights: \n{string.Join("\n", currentGeneration.Population[0].NetworkWeights.Select(WeightM => WeightM.ToString()))}" +
            //    $"Bias: \n{string.Join("\n", currentGeneration.Population[0].ConstructBiasList().Select(BiasM => BiasM.ToString()))}";
            string log = $"Generation {GenNumber} - Best Fitness (not the shown NN): {currentGeneration.Population[0].FitnessScore} \n\n " +
                $"Weights: \n{string.Join("\n", AllSimWorlds[0].ragdollBody.Brain.NetworkWeights.Select(WeightM => WeightM.ToString()))}" +
                $"Bias: \n{string.Join("\n", AllSimWorlds[0].ragdollBody.Brain.ConstructBiasList().Select(BiasM => BiasM.ToString()))}";


            TextLogSave.AppendToTextLog(log, $"SAVES/logs/{logFileName}");

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
            GenNumber++;
             UiManager.UiTexts["Generation"] = $"{GenNumber}";
            // reset
            AllSimWorlds.ForEach(scene =>
            {
                scene.ragdollBody.ResetAngularVelocities();
                //scene.ragdollBody.reset();
                scene.ragdollBody.setPostion(scene.ragdollBody.initialConfig.COMPosition);
                for (int i = 0; i < scene.ragdollBody.Bones.Count; i++)
                {
                    scene.ragdollBody.Bones[i].RB.Orientation = scene.ragdollBody.initialConfig.BoneRotations[i];
                    scene.ragdollBody.Bones[i].RB.Velocity = JVector.Zero;
                }
                scene.ragdollBody.EnergyUsed = 0;
            });


            float numX = (float)((rng.NextSingle() * 2) - 1.0); // between 0 and -1
            float numY = (float)((rng.NextSingle() * 2) - 1.0); // between 0 and -1
            numX = numX * 50;
            numY = numY * 50;
            TargetCube.setPosition(numX, 0, numY);
        }
    }
}
