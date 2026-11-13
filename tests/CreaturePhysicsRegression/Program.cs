using _3D_Engine.Classes.NeuralNet;
using _3D_Engine.Classes.Objects;
using _3D_Engine.Classes.Ragdoll;
using Jitter2;
using Jitter2.LinearMath;
using Vector3 = OpenTK.Mathematics.Vector3;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
using var foregroundWorld = new World();
using var backgroundWorld = new World();
foregroundWorld.AllowDeactivation = backgroundWorld.AllowDeactivation = false;
foregroundWorld.Gravity = backgroundWorld.Gravity = JVector.Zero;

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
void Near(float a, float b, string message, float tolerance = 1e-4f) =>
    Check(MathF.Abs(a - b) <= tolerance, $"{message}: {a} != {b}");
void Vec(JVector a, JVector b, string message, float tolerance = 1e-4f) =>
    Check((a - b).Length() <= tolerance, message);

Bone MakeBone(JVector position)
{
    var visual = new WorldObject();
    visual.setScale(new Vector3(1f, 0.5f, 0.5f));
    return new Bone(foregroundWorld, visual, new Vector3(position.X, position.Y, position.Z), "test");
}

var a = MakeBone(new JVector(-4, 3, 0));
var b = MakeBone(new JVector(4, 3, 0));
// Deliberately do not update visuals: cloning must use the real orientation.
a.RB.Orientation = b.RB.Orientation = new JQuaternion(0, 0, MathF.Sin(0.15f), MathF.Cos(0.15f));
a.RB.SetMassInertia(2f);
b.RB.SetMassInertia(3f);
a.RB.Damping = (0.02f, 0.03f);
a.RB.Friction = 0.7f;
a.RB.Restitution = 0.15f;
a.RB.AffectedByGravity = false;
a.RB.Velocity = new JVector(0.1f, 0.2f, 0.3f);
b.RB.Velocity = a.RB.Velocity;
a.RB.AngularVelocity = b.RB.AngularVelocity = new JVector(0, 0, 0.1f);
a.RB.Force = new JVector(0, 0.2f, 0);
a.RB.Torque = new JVector(0, 0, 0.1f);
var joint = new Joint(foregroundWorld, a, b, new WorldObject(), new JVector(0, 3, 0), JVector.UnitZ, 80, 5);
joint.HindgeJointConstraint.Motor.MaximumForce = 250f;
joint.HindgeJointConstraint.Motor.TargetVelocity = 1.5f;
var body = new RagdollBody(new() { a, b }, new() { joint });
var forceBeforeBrain = a.RB.Force;
body.SetBrain(new NeuralNetwork(new[] { 10, 8, 8, 1 }));
Vec(a.RB.Force, forceBeforeBrain, "SetBrain must not inject a force");
body.TargetPosition = new JVector(20, 3, 20);
body.AdvanceSimulationClock(0.2f);
var clone = body.Clone(backgroundWorld);

Check(backgroundWorld.RigidBodies.Count == foregroundWorld.RigidBodies.Count, "No extra bones from joint cloning");
Check(!ReferenceEquals(body.Brain, clone.Brain), "Brains must be independent");
Check(!ReferenceEquals(joint.Visual, clone.Joints[0].Visual), "Joint visuals must be independent");
Check(ReferenceEquals(clone.Joints[0].BoneA, clone.Bones[0]), "Joint must connect cloned bone A");
Check(ReferenceEquals(clone.Joints[0].BoneB, clone.Bones[1]), "Joint must connect cloned bone B");
Check(!ReferenceEquals(body.initialConfig.BonePositions, clone.initialConfig.BonePositions), "Independent snapshots");

for (int i = 0; i < body.Bones.Count; i++)
{
    var source = body.Bones[i].RB;
    var copy = clone.Bones[i].RB;
    Near(source.Mass, copy.Mass, "Mass");
    Check(source.InverseInertia.Equals(copy.InverseInertia), "Inertia");
    Check(source.Orientation.Equals(copy.Orientation), "Physics orientation, not visual orientation");
    Vec(source.Velocity, copy.Velocity, "Velocity");
    Vec(source.AngularVelocity, copy.AngularVelocity, "Angular velocity");
    Vec(source.Force, copy.Force, "Pending force");
    Vec(source.Torque, copy.Torque, "Pending torque");
    Near(source.Friction, copy.Friction, "Friction");
    Near(source.Restitution, copy.Restitution, "Restitution");
    Check(source.Damping == copy.Damping, "Damping");
    Check(source.AffectedByGravity == copy.AffectedByGravity, "Gravity flag");
}
Near(joint.HindgeJointConstraint.Motor.MaximumForce, clone.Joints[0].HindgeJointConstraint.Motor.MaximumForce, "Motor force");
Near(joint.HindgeJointConstraint.Motor.TargetVelocity, clone.Joints[0].HindgeJointConstraint.Motor.TargetVelocity, "Motor velocity");

var originalInputs = body.GetBrainInputMatrix();
var copiedInputs = clone.GetBrainInputMatrix();
Check((originalInputs - copiedInputs).L2Norm() < 1e-5, "Inputs match before rendering (including clock)");
body.Update(10f); // Rendering must not advance the controller clock.
Check((originalInputs - body.GetBrainInputMatrix()).L2Norm() < 1e-5, "Render does not advance clock");

// Identical controller commands in separate worlds should produce equal motion.
// This also catches missing hinge limits once a motor drives into a stop.
for (int step = 0; step < 180; step++)
{
    joint.HindgeJointConstraint.Motor.TargetVelocity = 5f;
    clone.Joints[0].HindgeJointConstraint.Motor.TargetVelocity = 5f;
    foregroundWorld.Step(1f / 120f);
    backgroundWorld.Step(1f / 120f);
    for (int i = 0; i < body.Bones.Count; i++)
    {
        Vec(body.Bones[i].RB.Position, clone.Bones[i].RB.Position, "Matching simulated positions", 0.01f);
        Vec(body.Bones[i].RB.AngularVelocity, clone.Bones[i].RB.AngularVelocity, "Matching angular velocities", 0.01f);
    }
}

// A reset must reconstruct the actual original pose, not translate a deformed one.
clone.Bones[0].RB.Position += new JVector(10, -10, 5);
clone.Bones[1].RB.AngularVelocity = new JVector(2, 3, 4);
clone.EnergyUsed = 100f;
clone.AdvanceSimulationClock(0.5f);
clone.reset();
for (int i = 0; i < clone.Bones.Count; i++)
{
    Vec(clone.Bones[i].RB.Position, body.initialConfig.BonePositions[i], "Reset position");
    Vec(clone.Bones[i].RB.Velocity, body.initialConfig.BoneVelocities[i], "Reset velocity");
    Vec(clone.Bones[i].RB.AngularVelocity, body.initialConfig.BoneAngularVelocities[i], "Reset angular velocity");
    Check(clone.Bones[i].RB.Orientation.Equals(body.initialConfig.BoneRotations[i]), "Reset rotation");
    Vec(clone.Bones[i].RB.Force, JVector.Zero, "Reset force");
}
Near(clone.EnergyUsed, 0, "Reset energy");
var resetInputs = clone.GetBrainInputMatrix();
Near(resetInputs[8, 0], 0, "Reset sine clock");
Near(resetInputs[9, 0], 1, "Reset cosine clock");
Console.WriteLine("PASS: clone physics, independent ownership, render-independent inputs, matching motion and full pose reset.");
