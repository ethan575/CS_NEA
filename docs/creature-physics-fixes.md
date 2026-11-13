# Foreground/background creature physics fixes

Source baseline: `NNBGCreaturesFix`, commit
`70419e22d8bf1dba5dd9461eddd4becd15b49445`.
The Arena working branch started with only `.gitignore`, so the project source,
shaders and model assets were imported here. Build products, save files under
`bin/`, `obj/` and Visual Studio caches were not imported.

## Changes

- **Bone.cs:** apply the default mass after attaching the collision shape; clone
  the actual physics orientation, mass/inverse inertia, motion type, material,
  damping, gravity flag, velocities and queued forces. Do not rely on a visual
  transform having been rendered first. Jitter can overwrite an earlier mass
  assignment when adding a shape, so the old `0.1f` line was not reliable evidence
  of the final mass.
- **Joint.cs:** preserve the hinge limits, axis, motor force, target velocity and
  enabled state. Clone joint visuals instead of sharing them, and calculate the
  local hinge axis from the destination bone's orientation. Ragdoll clones still
  reuse their own already-cloned bones, rather than creating extra bodies.
- **BuilderSaveReadWrite.cs / RagdollBody.cs:** remove the loader's upward impulse
  and the upward force previously injected whenever a brain was assigned. Assigning
  a controller no longer gives a creature an artificial kick.
- **RagdollBody.cs:** record the real starting centre of mass and each bone's
  starting position, orientation and velocities. Deep-copy snapshots, and restore
  the full pose, clear queued forces, and reset the controller clock and energy at
  generation boundaries. Previously translating the end pose and resetting only
  rotations could leave deformed bone positions and residual angular velocities.
- **SimulationScene.cs:** give the background floors the same dimensions as the
  foreground floor. Advance the controller clock in simulation updates, not render
  callbacks. Refresh velocity inputs directly from physics. Generation time and
  the controller clock now use the same capped timestep as the physics server;
  paused physics does not consume generation time or run the controllers.

This does not change the neural-network architecture or evolutionary selection.
After evolution, different genomes should still move differently. The target cube
remains a physical object in the foreground world only, so contact with that cube
can still make trajectories differ; this patch does not duplicate target colliders
into every training world.

## Regression checks

The headless console harness links the actual ragdoll/NN/model-importer source and
uses Jitter2 2.8.8; only the rendering object is stubbed. It checks copied physical
properties, independent ownership, inputs before rendering, controller-clock
independence from rendering, motion in two worlds driven into hinge limits, and
full-pose reset.

With the .NET 10 SDK and NuGet access:

```sh
dotnet run --project tests/CreaturePhysicsRegression/CreaturePhysicsRegression.csproj
```

The main Windows project excludes the harness's C# files from its compilation.

Manual check on Windows: load a creature, press Tab, compare foreground/background
movement during the first generation, press P to inspect COM/velocity data, and let
several generations finish. Check that full poses reset without accumulated spin
or sudden stretching. H continues to toggle the background population.

## Validation in Arena

All 44 C# files were syntax-parsed, both project XML files parsed successfully,
and the new Jitter member names/signature were checked against the branch's
Jitter2 DLL metadata. Patch whitespace was checked against the source baseline.
The .NET SDK is absent in this sandbox, so neither the regression harness nor the
Windows/OpenGL application was compiled or executed here.
