using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.physics;
using _3D_Engine.Classes.Scenes;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace _3D_Engine.Classes.Ragdoll
{
    public class RagdollBuilder
    {
        Game game;
        Scene scene;
        RagdollObjects? selectedObject;
        World world;
        Shader shader;

        List<RagdollObjects> SelectableObjects = new List<RagdollObjects>();
        float[] jointVertex;
        float[] boneVertex;

        // single reusable marker for the hit point (spawning a new WorldObject per
        // click would add a permanent object to the scene every time)
        WorldObject? hitMarker = null;

        public RagdollBuilder(Game game, World world, List<RagdollObjects> ragdollObjects)
        {
            this.game = game;
            this.shader = game.shader;
            this.world = world;
            this.SelectableObjects = ragdollObjects;
            this.scene = SceneManager.CurrentScene;

            jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
            boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");
        }

        public RagdollBuilder(Game game, World world)
        {
            this.game = game;
            this.shader = game.shader;
            this.world = world;
            this.SelectableObjects = new List<RagdollObjects>();
            this.scene = SceneManager.CurrentScene;

            jointVertex = ModelImporter.LoadModel("models/Raw/joint.stl");
            boneVertex = ModelImporter.LoadModel("models/Raw/bone.stl");
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

            // only on the frame the button goes down, so we don't spawn a marker every frame while held
            if (game.mouseState.IsButtonPressed(MouseButton.Left) && !game.mouseState.WasButtonDown(MouseButton.Left))
            {
                //Console.WriteLine($"Mouse Position: {game.mouseState.X}, {game.mouseState.Y}");
                // make a raycast from the camera position to the mouse position and check if it hits any of the objects in the scene
                // add relative position of the omouse to the camera position and use that as the origin of the raycast
                // top left corner of the screen is (0, 0) and bottom right corner is (1280, 720) pretty certain
                //Console.WriteLine(game.GetCamera().Position);
                //bool hit = Raycast.RaycastHit(world, ToJVec(game.GetCamera().Position), ToJVec(ScreenToWorldDirection(game.mouseState.Position)), 100f, out RaycastHitInfo hitInfo, out JVector normal, out JVector hitOffset, out IDynamicTreeProxy proxy);
                (Vector3 origin, Vector3 direction) = ScreenToWorldPointer(game.mouseState.Position);

                bool hit = Raycast.RaycastHit(world, ToJVec(origin), ToJVec(direction), 100f, out RaycastHitInfo hitInfo, out JVector normal, out JVector hitOffset, out IDynamicTreeProxy proxy);

                if (hit)
                {
                    // [PICK] marker line so the console shows when a pick registers
                    Console.WriteLine($"[PICK] hit {proxy?.GetType().Name ?? "null"} @ {hitInfo.Point} (distance {hitInfo.Distance:F2})");

                    // move the marker to the hit point (the scene renders all
                    // WorldObjects in SceneManager.RenderedEntities every frame)
                    if (hitMarker == null)
                    {
                        hitMarker = new WorldObject("hitPoint", shader, boneVertex, scene, new Vector3(0.1f, 0.1f, 0.1f));
                    }
                    hitMarker.setPosition(hitInfo.Point);


                    SelectableObjects.ForEach(obj => obj.Visual.color = obj.Visual.defaultColor); // Reset all objects to default color

                    if (proxy is RigidBodyShape shape)
                    {
                        selectedObject = SelectableObjects.FirstOrDefault(o => o.RB == shape.RigidBody);

                        if (selectedObject != null)
                        {
                            Console.WriteLine($"[PICK] selected {selectedObject.Visual.Name}");
                            selectedObject.Visual.color = new Vector3(
                                selectedObject.Visual.defaultColor.X,
                                selectedObject.Visual.defaultColor.Y * 1.5f,
                                selectedObject.Visual.defaultColor.Z * 1.5f);
                        }
                    }
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
                else
                {
                    Console.WriteLine("[PICK] miss");
                }
            }

            if (game.keyboardState.IsKeyPressed(Keys.A))
            {
                // add
                WorldObject newObj = new WorldObject("1", shader, boneVertex, scene, new Vector3(0.5f, 0.5f, 0.5f));
                AddObject(new Bone(world, newObj, new Vector3(0, 10, 10), ""));

            }
        }

        private (Vector3 origin, Vector3 direction) ScreenToWorldPointer(Vector2 screenPos)
        {
            // The viewport and the projection matrix used for rendering are in framebuffer
            // pixels (Camera.Size, kept in sync in Game.OnFramebufferResize), but the mouse
            // position is in client pixels. Convert the mouse position to framebuffer pixels
            // first, otherwise the ray is computed against a different projection than the
            // one the image was drawn with and the hit drifts away from the cursor (the
            // drift grows with distance from the camera).
            int width = (int)game.GetCamera().Size.X;
            int height = (int)game.GetCamera().Size.Y;

            float scaleX = game.ClientSize.X > 0 ? (float)width / game.ClientSize.X : 1f;
            float scaleY = game.ClientSize.Y > 0 ? (float)height / game.ClientSize.Y : 1f;

            // Screen -> NDC. Add 0.5 to sample pixel centres. The +0.5 must be added in
            // client pixels BEFORE the framebuffer scaling: the framebuffer pixel that
            // maps to client pixel i is centred at (i + 0.5) * scale, not i * scale + 0.5,
            // so applying it after scaling drifts the ray by 0.5 * (scale - 1) pixels on
            // displays where the client size differs from the framebuffer size (HiDPI).
            float x = (screenPos.X + 0.5f) * scaleX;
            float y = (screenPos.Y + 0.5f) * scaleY;

            float ndcX = (2.0f * x) / width - 1.0f;
            float ndcY = 1.0f - (2.0f * y) / height;

            Matrix4 projection = game.GetCamera().GetProjectionMatrix(width, height);
            Matrix4 view = game.GetCamera().GetViewMatrix();

            Matrix4 invProjection = Matrix4.Invert(projection);
            Matrix4 invView = Matrix4.Invert(view);

            // Clip space points on the near (-1) and far (+1) planes.
            Vector4 clipNear = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
            Vector4 clipFar = new Vector4(ndcX, ndcY, 1.0f, 1.0f);

            // Clip -> Eye (view) space.
            Vector4 eyeNear = clipNear * invProjection;
            Vector4 eyeFar = clipFar * invProjection;
            eyeNear /= eyeNear.W;
            eyeFar /= eyeFar.W;

            // Eye -> World space.
            Vector4 worldNear = eyeNear * invView;
            Vector4 worldFar = eyeFar * invView;

            Vector3 origin = worldNear.Xyz;
            Vector3 direction = Vector3.Normalize(worldFar.Xyz - worldNear.Xyz);

            return (origin, direction);
        }




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

