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
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace _3D_Engine.Classes.Scenes
{
    public class SimulationScene : Scene
    {
        private const float GenerationDuration = 20f;
        private const int TargetPhasesPerGeneration = 4;
        private const float TargetFloorY = -9f;

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
        bool brainLoadedFromFile = false;

        // simulation stuff
        bool isSimulationRunning = false;
        int GenNumber = 0;
        int populationSize = 50;
        RagdollBody LastLoadedCreature;
        private bool backgroundPopulationHidden = false;
        Generation currentGeneration = new Generation();
        string logFileName;
        private Random rng = new Random();

        private int currentPhaseIndex = 0;
        private readonly JVector[] phaseTargets = new JVector[TargetPhasesPerGeneration];

        public SimulationScene(Game game) : base(game)
        {
            shader = game.shader;
            this.game = game;

            physicsServer = new PhysicsServer();

            logFileName = $"{DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace(" ", "_")}_training.log";

            createScene();
        }

        public override void Update(float dt)
        {
            physicsServer.Update(dt, AllSimWorlds, ForegroundWorld);

            // dt is in seconds
            if (isSimulationRunning && PhysicsServer.AllowedWorldStep)
            {
                float simulationDt = MathF.Min(dt, 1 / 30f); // cant have larger than 1/30 of a sec
                currentGeneration.ActiveTimeLeft -= simulationDt;
                UiManager.UiTexts["Generation Time left"] = Convert.ToString(MathF.Truncate(MathF.Max(0f, currentGeneration.ActiveTimeLeft)));

                if (currentGeneration.ActiveTimeLeft <= 0)
                {
                    PrepareEndGeneration();
                }
                else
                {
                    float elapsed = GenerationDuration - currentGeneration.ActiveTimeLeft;
                    float phaseDuration = GenerationDuration / TargetPhasesPerGeneration;
                    int desiredPhase = Math.Clamp((int)(elapsed / phaseDuration), 0, TargetPhasesPerGeneration - 1);

                    if (desiredPhase != currentPhaseIndex)
                    {
                        currentPhaseIndex = desiredPhase;
                        JVector nextTarget = phaseTargets[currentPhaseIndex];
                        SetTargetCubePosition(nextTarget);
                        foreach (var scene in AllSimWorlds)
                        {
                            scene.ragdollBody.BeginTargetPhase(nextTarget);
                        }
                    }

                    for (int i = 0; i < AllSimWorlds.Count; i++)
                    {
                        var scene = AllSimWorlds[i];
                        scene.ragdollBody.AdvanceSimulationClock(simulationDt);
                        scene.ragdollBody.TargetPosition = new JVector(TargetCube.GetPosition().X, TargetCube.GetPosition().Y, TargetCube.GetPosition().Z);
                        var inputM = scene.ragdollBody.GetBrainInputMatrix();
                        scene.ragdollBody.BrainNNFF(inputM);
                    }
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
            }

            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.N))
            {
                if (!isFirstSimulationClick)
                {
                    MessageBox.Show("Simulation is already running. Return to BuildScene (Esc) and re-enter to load a different creature.", "Info", MessageBoxButtons.OK);
                }
                else
                {
                    using (var dialog = new OpenFileDialog())
                    {
                        dialog.Filter = "Creature File (*.creature)|*.creature";
                        dialog.Title = "Select a creature file";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string filePath = dialog.FileName;

                            if (ForegroundBody != null)
                            {
                                foreach (var b in ForegroundBody.Bones)
                                {
                                    b.Visual?.Destroy();
                                    ForegroundWorld.Remove(b.RB);
                                }
                                foreach (var j in ForegroundBody.Joints)
                                {
                                    j.destroy();
                                }
                                AllSimWorlds.Clear();
                            }

                            LastLoadedCreature = BuilderSaveReadWrite.BuilderLoadFile(filePath, ForegroundWorld, shader, this);
                            ForegroundBody = LastLoadedCreature;
                            ForegroundBody.initialPos = ForegroundBody.initialConfig.COMPosition;
                            brainLoadedFromFile = false;

                            AllSimWorlds.Add(new HeadlessSimulationScene(ForegroundWorld, ForegroundBody));
                        }
                    }
                }
            }

            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.S))
            {
                if (currentGeneration.Population != null && currentGeneration.Population.Count > 0)
                {
                    NeuralNetwork NN = currentGeneration.Population.OrderByDescending(nn => nn.FitnessScore).First();

                    bool wasAllowed = PhysicsServer.AllowedWorldStep;
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
                            Console.WriteLine("Neural Network saved successfully.");
                        }
                    }
                    PhysicsServer.AllowedWorldStep = wasAllowed;
                }
            }

            // opening NN file
            if (game.KeyboardState.IsKeyDown(Keys.LeftControl) && game.keyboardState.IsKeyPressed(Keys.O))
            {
                bool wasAllowed = PhysicsServer.AllowedWorldStep;
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

                        try
                        {
                            NeuralNetwork loaded = NNSaveLoad.LoadNeuralNetwork(SavePath);

                            if (ForegroundBody != null)
                            {
                                ForegroundBody.SetBrain(loaded);
                                brainLoadedFromFile = true;

                                if (AllSimWorlds.Count > 1)
                                {
                                    currentGeneration.Population = new List<NeuralNetwork>(AllSimWorlds.Count) { loaded };
                                    for (int i = 1; i < AllSimWorlds.Count; i++)
                                    {
                                        NeuralNetwork variant = NNTrainer.CreateMutatedVariant(loaded);
                                        AllSimWorlds[i].ragdollBody.SetBrain(variant);
                                        currentGeneration.Population.Add(variant);
                                    }
                                }
                                else
                                {
                                    currentGeneration.Population = new List<NeuralNetwork> { loaded };
                                }

                                Console.WriteLine("Neural Network loaded successfully.");
                            }
                            else
                            {
                                MessageBox.Show("There is no body to attach a brain to.", "Error", MessageBoxButtons.OK);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to load neural network: {ex.Message}", "Error", MessageBoxButtons.OK);
                        }
                    }
                }
                PhysicsServer.AllowedWorldStep = wasAllowed;
            }

            if (game.KeyboardState.IsKeyPressed(Keys.Tab))
            {
                if (LastLoadedCreature == null)
                {
                    MessageBox.Show("No creature loaded. Please load a creature first.", "Error", MessageBoxButtons.OK);
                    return;
                }
                else if (ForegroundBody.Brain == null)
                {
                    int inputCount = ForegroundBody.RequiredInputCount;
                    int hiddenSize = Math.Max(16, inputCount);
                    NeuralNetwork newNN = new NeuralNetwork(new int[] { inputCount, hiddenSize, 16, ForegroundBody.Joints.Count });
                    ForegroundBody.SetBrain(newNN);
                }

                if (isFirstSimulationClick && LastLoadedCreature != null)
                {
                    isFirstSimulationClick = false;
                    PhysicsServer.AllowedWorldStep = true;
                    ForegroundBody.initialPos = ForegroundBody.initialConfig.COMPosition;

                    currentGeneration = new Generation(new List<NeuralNetwork>());
                    currentGeneration.ActiveTimeLeft = GenerationDuration;
                    currentGeneration.Population.Add(ForegroundBody.Brain);

                    for (int i = 0; i < populationSize - 1; i++) // '-1' because the first one is foreground
                    {
                        var newWorld = new World();

                        RigidBody floorRB = newWorld.CreateRigidBody();
                        floorRB.AddShape(new BoxShape(new JVector(120, 1f, 120)));
                        floorRB.MotionType = MotionType.Static;
                        floorRB.Position = new JVector(0, -10, 0);

                        RagdollBody newBody = LastLoadedCreature.Clone(newWorld);
                        NeuralNetwork bgBrain = brainLoadedFromFile
                            ? NNTrainer.CreateMutatedVariant(ForegroundBody.Brain)
                            : new NeuralNetwork(ForegroundBody.Brain.LayerDims);
                        newBody.SetBrain(bgBrain);
                        newBody.initialPos = newBody.initialConfig.COMPosition;

                        currentGeneration.Population.Add(bgBrain);
                        AllSimWorlds.Add(new HeadlessSimulationScene(newWorld, newBody));
                    }

                    AllSimWorlds.ForEach(scene => scene.ragdollBody.Joints.ForEach(joint => joint.Visual.visible = false));
                    AllSimWorlds.ForEach(scene => scene.ragdollBody.Bones.ForEach(bone => bone.Visual.visible = true));

                    InitializeGenerationTargetSchedule();
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
            if (isFirstSimulationClick)
            {
                ForegroundBody?.Update(dt);
            }
            else
            {
                foreach (var scene in AllSimWorlds)
                {
                    scene.ragdollBody.Update(dt);
                }
            }

            foreach (var entity in SceneManager.RenderedEntities)
            {
                entity.Draw();
            }
        }

        public override void createScene()
        {
            base.createScene();

            float[] floorVertex = ModelImporter.LoadModel("models/Raw/floor.stl");
            float[] cubeVertex = ModelImporter.LoadModel("models/Raw/cube.stl");

            RigidBody floorRB = ForegroundWorld.CreateRigidBody();
            floorRB.AddShape(new BoxShape(new JVector(120, 1f, 120)));
            floorRB.MotionType = MotionType.Static;
            floor = new PhysicsObject("floor", shader, floorVertex, this, game, floorRB, new Vector3(1, 1, 1));
            floor.Scale(6, 0.5f, 6);
            floor.setPosition(new Vector3(0, -10, 0));

            RigidBody RB1 = ForegroundWorld.CreateRigidBody();
            RB1.SetMassInertia(20);
            RB1.AddShape(new BoxShape(1));
            RB1.MotionType = MotionType.Dynamic;
            TargetCube = new PhysicsObject("convex1", shader, cubeVertex, this, game, RB1, new Vector3(0, 1, 0));
            TargetCube.setMass(20f);
            TargetCube.setObeyGravity(true);
            TargetCube.Scale(0.5f);
            TargetCube.setPosition(5f, TargetFloorY, 35f);

            IsReady = true;
            PhysicsServer.AllowedWorldStep = false;
            UiManager.UiMode = UiTypes.Simulation;
        }

        public override void destroyScene()
        {
            IsReady = false;

            foreach (var entity in new List<WorldObject>(SceneManager.RenderedEntities))
            {
                entity.Destroy();
            }

            foreach (var sim in AllSimWorlds)
            {
                if (sim.world != ForegroundWorld)
                {
                    sim.world.Clear();
                    sim.world.Dispose();
                }
            }
            AllSimWorlds.Clear();

            ForegroundWorld.Clear();
            ForegroundWorld.Dispose();
            SceneManager.CurrentScene?.AllPhysicsObjects.Clear();
        }

        public void PrepareEndGeneration()
        {
            foreach (HeadlessSimulationScene scene in AllSimWorlds)
            {
                scene.ragdollBody.EndOfGenerationTasks();
            }

            for (int i = 0; i < AllSimWorlds.Count; i++)
            {
                var scene = AllSimWorlds[i];
                float fitnessScore = FitnessEval.EvaluateFitness(scene.ragdollBody);
                scene.ragdollBody.Brain.FitnessScore = fitnessScore;
            }

            currentGeneration.Population = currentGeneration.Population.OrderByDescending(nn => nn.FitnessScore).ToList();
            UiManager.UiTexts["BestFitness"] = $"{currentGeneration.Population[0].FitnessScore:F2}";

            UiManager.FitnessGraphValues.Add(currentGeneration.Population[0].FitnessScore);

            string log = $"Generation {GenNumber} - Best Fitness: {currentGeneration.Population[0].FitnessScore:F2} \n\n " +
                $"Weights: \n{string.Join("\n", currentGeneration.Population[0].NetworkWeights.Select(WeightM => WeightM.ToString()))}" +
                $"Bias: \n{string.Join("\n", currentGeneration.Population[0].ConstructBiasList().Select(BiasM => BiasM.ToString()))}";

            TextLogSave.AppendToTextLog(log, $"SAVES/logs/{logFileName}");

            currentGeneration = NNTrainer.EvolveGeneration(currentGeneration.Population);
            currentGeneration.ActiveTimeLeft = GenerationDuration;

            for (int i = 0; i < AllSimWorlds.Count; i++)
            {
                var scene = AllSimWorlds[i];
                scene.ragdollBody.SetBrain(currentGeneration.Population[i]);
            }

            StartNextGeneration();
        }

        public void StartNextGeneration()
        {
            GenNumber++;
            UiManager.UiTexts["Generation"] = $"{GenNumber}";

            AllSimWorlds.ForEach(scene =>
            {
                scene.ragdollBody.reset();
            });

            InitializeGenerationTargetSchedule();
        }

        private void InitializeGenerationTargetSchedule()
        {
            JVector centre = ForegroundBody != null
                ? ForegroundBody.initialConfig.COMPosition
                : JVector.Zero;

            float baseAngle = rng.NextSingle() * MathF.PI * 2f;
            float[] angles = new float[TargetPhasesPerGeneration];
            for (int i = 0; i < TargetPhasesPerGeneration; i++)
            {
                angles[i] = baseAngle + i * (MathF.PI * 2f / TargetPhasesPerGeneration);
            }

            // Shuffle phase directions so the network cannot memorize a fixed turn order from clock alone
            for (int i = angles.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (angles[i], angles[j]) = (angles[j], angles[i]);
            }

            for (int i = 0; i < TargetPhasesPerGeneration; i++)
            {
                float radius = 32f + rng.NextSingle() * 14f; // 32..46 units away
                float x = Math.Clamp(centre.X + MathF.Cos(angles[i]) * radius, -52f, 52f);
                float z = Math.Clamp(centre.Z + MathF.Sin(angles[i]) * radius, -52f, 52f);
                phaseTargets[i] = new JVector(x, TargetFloorY, z);
            }

            currentPhaseIndex = 0;
            SetTargetCubePosition(phaseTargets[0]);

            foreach (var scene in AllSimWorlds)
            {
                scene.ragdollBody.BeginTargetPhase(phaseTargets[0]);
            }
        }

        private void SetTargetCubePosition(JVector pos)
        {
            TargetCube.setPosition(pos.X, pos.Y, pos.Z);
            TargetCube.setVelocity(Vector3.Zero);
            TargetCube.Rbody.AngularVelocity = JVector.Zero;
        }
    }
}
