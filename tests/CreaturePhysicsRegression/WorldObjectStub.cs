// Rendering is deliberately replaced, but all ragdoll/NN/physics code is real.
using OpenTK.Mathematics;

namespace _3D_Engine.Classes.Objects;

public class WorldObject
{
    private Vector3 position;
    private Vector3 scale = Vector3.One;
    private Quaternion rotation = Quaternion.Identity;
    public bool visible = true;
    public WorldObject Clone() => new() { position = position, scale = scale, rotation = rotation };
    public Vector3 GetScale() => scale;
    public Quaternion GetOrientation() => rotation;
    public void setPosition(Vector3 value) => position = value;
    public void setRotation(Quaternion value) => rotation = value;
    public void setScale(Vector3 value) => scale = value;
    public void Draw() { }
    public void Destroy() { }
}
