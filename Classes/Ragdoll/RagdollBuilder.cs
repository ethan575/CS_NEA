using _3D_Engine.Classes.CameraPresets;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.Scenes;
using Jitter2;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel.DataAnnotations;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollBuilder
    {
        Game game;
        Scene scene;
        RagdollObjects? _selectedObject;
        RagdollObjects? selectedObject
        {
            get
            {
                return _selectedObject;
            }
            set
            {
                if (_selectedObject != null)
                    _selectedObject.IsBeingEdited = false; // Reset the previous selected object's editing state
                _selectedObject = value;
                _selectedObject.IsBeingEdited = true;
                SelectionChanged(value);
            }
        }
        int PtrSelectedObject = -1;
        World world;
        Shader shader;

        Random rng = new Random();

        List<RagdollObjects> SelectableObjects = new List<RagdollObjects>();
        float[] jointVertex;
        float[] boneVertex;
        float[] HindgeAngleDisplayVertex;

        public RagdollBuilder(Game game, World world, List<RagdollObjects> ragdollObjects)
        {
            this.game = game;
            this.shader = game.shader;
            this.world = world;
            this.SelectableObjects = ragdollObjects;

            jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
            boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");
            HindgeAngleDisplayVertex = ModelImporter.LoadModel("models/Raw/HingeAxisDisplay.stl");
        }

        public RagdollBuilder(Game game, World world)
        {
            this.game = game;
            this.shader = game.shader;
            this.world = world;
            this.SelectableObjects = new List<RagdollObjects>();

            jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
            boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");
            HindgeAngleDisplayVertex = ModelImporter.LoadModel("models/Raw/HingeAxisDisplay.stl");
        }

        private void SelectionChanged(RagdollObjects? newSelection)
        {
            SelectableObjects.ForEach(obj => obj.Visual.color = obj.Visual.defaultColor); // Reset all objects to default color

            if (selectedObject != null)
            {
                selectedObject.Visual.color = new Vector3(
                    selectedObject.Visual.defaultColor.X,
                    selectedObject.Visual.defaultColor.Y * 1.5f,
                    selectedObject.Visual.defaultColor.Z * 1.5f);
            }
        }

        public void AddObject(Bone bone)
        {
            SelectableObjects.Add(bone);
        }

        public void AddObject(Joint joint, Vector3 position)
        {
            joint.position = position;
            SelectableObjects.Add(joint);
        }

        private void PrepareSceneForBuilder(Scene scene)
        {

        }



        public void Update()
        {

            foreach (RagdollObjects obj in SelectableObjects)
            {
                obj.UpdateVisual();
            }

            if (game.keyboardState.IsKeyPressed(Keys.Tab))
            {
                //Console.WriteLine($"Mouse Position: {game.mouseState.X}, {game.mouseState.Y}");
                // make a raycast from the camera position to the mouse position and check if it hits any of the objects in the scene
                // add relative position of the omouse to the camera position and use that as the origin of the raycast
                // too many bugs just use tab 
                // top left corner of the screen is (0, 0) and bottom right corner is (1280, 720) pretty certain
                //Console.WriteLine(game.GetCamera().Position);
                //bool hit = Raycast.RaycastHit(world, ToJVec(game.GetCamera().Position), ToJVec(ScreenToWorldDirection(game.mouseState.Position)), 100f, out RaycastHitInfo hitInfo, out JVector normal, out JVector hitOffset, out IDynamicTreeProxy proxy);



                // inaccuracies? just move camra close rto the cliked regin
                //Vector3 targetCamPos = (game.GetCamera().Position - hitInfo.Point) * 0.2f;
                //Vector3 Front = (hitInfo.Point * new Vector3(-1, 1, 1) - game.GetCamera().Position).Normalized();
                //Vector3 Right = Vector3.Cross(Front, Vector3.UnitY).Normalized();
                //Vector3 Up = Vector3.Cross(Right, Front);


                //SelectableObjects.ForEach(obj => obj.Visual.color = obj.Visual.defaultColor); // Reset all objects to default color
                if (SelectableObjects.Count == 0)
                {
                    return;
                }
                PtrSelectedObject = rng.Next(SelectableObjects.Count);
                selectedObject = SelectableObjects[PtrSelectedObject]; // Select a random object

                //if (selectedObject != null)
                //{
                //    selectedObject.Visual.color = new Vector3(
                //        selectedObject.Visual.defaultColor.X,
                //        selectedObject.Visual.defaultColor.Y * 1.5f,
                //        selectedObject.Visual.defaultColor.Z * 1.5f);
                //}

                //float smallestDistance = float.MaxValue;
                //SelectableObjects.ForEach(obj => obj.Visual.color = obj.Visual.defaultColor); // Reset all objects to default color
                //Console.WriteLine($"Hit at distance: {hitInfo.Distance}, Point: {hitInfo.Point}, Normal: {normal}");

                //foreach (RagdollObjects obj in SelectableObjects)
                //{
                //    float dist = (obj.Visual.GetPosition() - hitInfo.Point).Length;

                //    if (smallestDistance > dist)
                //    {
                //        smallestDistance = dist;
                //        selectedObject = obj;

                //        selectedObject.Visual.color = new Vector3(selectedObject.Visual.defaultColor.X, selectedObject.Visual.defaultColor.Y * 1.5f, selectedObject.Visual.defaultColor.Z * 1.5f);

                //    }
                //}


            }

                  

            if (game.keyboardState.IsKeyPressed(Keys.Delete))
            {
                if (SelectableObjects.Count == 0)
                {
                    return;
                }
                if (selectedObject != null)
                {
                    selectedObject.Visual.Destroy();
                    world.Remove(selectedObject.RB);
                    SelectableObjects.Remove(selectedObject);
                    selectedObject = null;
                }
            }

            if (game.keyboardState.IsKeyPressed(Keys.Right))
            {
                if (SelectableObjects.Count == 0)
                {
                    return;
                }
                if (selectedObject != null)
                {
                    PtrSelectedObject++;
                    if (PtrSelectedObject >= SelectableObjects.Count)
                    {
                        PtrSelectedObject = 0;
                    }
                    selectedObject = SelectableObjects[PtrSelectedObject];
                }
            }

            if (game.keyboardState.IsKeyPressed(Keys.Left))
            {
                if (SelectableObjects.Count == 0)
                {
                    return;
                }
                if (selectedObject != null)
                {
                    PtrSelectedObject--;
                    if (PtrSelectedObject < 0)
                    {
                        PtrSelectedObject = SelectableObjects.Count - 1;
                    }
                    selectedObject = SelectableObjects[PtrSelectedObject];
                }
            }

            if (game.keyboardState.IsKeyDown(Keys.M))
            {
                if (SelectableObjects.Count == 0 || selectedObject == null)
                {
                    return;
                }

                // move selected object by mouse position delta
                // z axis scroll delta
                Vector2 MouseMotion = game.mouseState.Delta * 0.1f;
                //Vector3 CamRight = Vector3.Cross(game.GetCamera().Front, game.GetCamera().Up).Normalized();
                Vector3 CamRight = -Vector3.Cross(game.GetCamera().Front, game.GetCamera().Up).Normalized();

                Console.WriteLine("Up" + game.GetCamera().Up);
                Console.WriteLine("Front" + game.GetCamera().Front);

                SelectableObjects[PtrSelectedObject].RB.Position += ToJVec((MouseMotion.X * CamRight) + (-MouseMotion.Y * game.GetCamera().Up) + (game.mouseState.ScrollDelta.Y * game.GetCamera().Front));
                //SelectableObjects[PtrSelectedObject].RB.Position += new JVector(MouseMotion.X * 0.1f, MouseMotion.Y * -0.1f, game.mouseState.ScrollDelta.Y * 0.5f);
            }

            if (game.keyboardState.IsKeyDown(Keys.R))
            {
                if (SelectableObjects.Count == 0 || selectedObject == null)
                {
                    return;
                }

                if (selectedObject is Joint)
                {
                    // rotate and somehow visualize the axis of rotation aka hindge joint
                    WorldObject AxisVisualizer = new WorldObject("axis", shader, HindgeAngleDisplayVertex, scene, new Vector3(0f, 0.5f, 1f));
                    Vector3 Result = Vector3.UnitX - FromJVec(((Joint)selectedObject).jointAxis);
                    Vector3 Rotation = new Vector3(90, 90, 90);
                    for (int i = 0; i < 3; i++)
                    {
                        if (Result[i] != -Result[i])
                        {
                            Rotation[i] = 1;
                        }
                        else
                        {
                            Result[i] = 0;
                        }
                    }
                    Rotation *= Result;

                    AxisVisualizer.rotate(Rotation.X, Rotation.Y, Rotation.Z);
                }
            }


            if (game.keyboardState.IsKeyPressed(Keys.A))
            {
                // add
                WorldObject newObj = new WorldObject("1", shader, boneVertex, scene, new Vector3(0.5f, 0.5f, 0.5f));
                AddObject(new Bone(world, newObj, new Vector3(0, 10, 10), ""));

            }
        }

        // https://www.reddit.com/r/gamemaker/comments/c6684w/3d_converting_a_screenspace_mouse_position_into_a/
        //private Vector3 ScreenToWorldDirection(Vector2 ScreenPos)
        //{
        //    float x = (2.0f * (ScreenPos.X + 0.5f)) / game.ClientSize.X - 1.0f;
        //    float y = 1.0f - (2.0f * (ScreenPos.Y + 0.5f)) / game.ClientSize.Y;

        //    float m11 = game.projection.M11;
        //    float m22 = game.projection.M22;
        //    Vector3 eyeDir = new Vector3(x / m11, y / m22, -1.0f);

        //    Matrix4 invView = Matrix4.Invert(game.view);

        //    // Transform view-space direction to world space (column-vector convention)
        //    Vector4 worldDir4 = invView * new Vector4(eyeDir, 0f);

        //    // CRITICAL FIX: Correct the mirrored X-axis caused by the camera's Right = (-1, 0, 0)
        //    worldDir4.X = -worldDir4.X;

        //    Vector3 rayDir = Vector3.Normalize(worldDir4.Xyz);
        //    return rayDir;
        //}

        //using tab instead
        //private (Vector3 origin, Vector3 direction) ScreenToWorldPointer(Vector2 screenPos)
        //{
        //    int width = game.ClientSize.X;
        //    int height = game.ClientSize.Y;

        //    // Screen -> NDC. Add 0.5 to sample pixel centers.
        //    float ndcX = -((2.0f * (screenPos.X + 0.5f)) / width - 1.0f);
        //    float ndcY = 1.0f - (2.0f * (screenPos.Y + 0.5f)) / height;

        //    Matrix4 projection = game.GetCamera().GetProjectionMatrix(width, height);
        //    Matrix4 view = game.GetCamera().GetViewMatrix();

        //    Matrix4 invProjection = Matrix4.Invert(projection);
        //    Matrix4 invView = Matrix4.Invert(view);

        //    // Clip space points on the near (-1) and far (+1) planes.
        //    Vector4 clipNear = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
        //    Vector4 clipFar = new Vector4(ndcX, ndcY, 1.0f, 1.0f);

        //    // Clip -> Eye (view) space.
        //    Vector4 eyeNear = clipNear * invProjection;
        //    Vector4 eyeFar = clipFar * invProjection;
        //    eyeNear /= eyeNear.W;
        //    eyeFar /= eyeFar.W;

        //    // Eye -> World space.
        //    Vector4 worldNear = eyeNear * invView;
        //    Vector4 worldFar = eyeFar * invView;

        //    Vector3 origin = worldNear.Xyz;
        //    Vector3 direction = Vector3.Normalize(worldFar.Xyz - worldNear.Xyz);

        //    return (origin, direction);
        //}




        private Vector3 FromJVec(JVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        private JVector ToJVec(Vector3 v)
        {
            return new JVector(v.X, v.Y, v.Z);
        }
    }
}

