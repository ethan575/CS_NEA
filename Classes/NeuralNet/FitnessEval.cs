using _3D_Engine.Classes.Ragdoll;
using Jitter2.LinearMath;

namespace _3D_Engine.Classes.NeuralNet
{

    public class FitnessObjective
    {
        public float weight;
        public Func<RagdollBody, float> fitnessFunction;

        public FitnessObjective(float weight, Func<RagdollBody, float> fitnessFunction)
        {
            this.weight = weight;
            this.fitnessFunction = fitnessFunction;
        }
    }

    public static class Objectives
    {
        // maximum distance from the target position for relativity
        public static float MaxDistanceFromPopulation = float.MinValue;

        public static float EvalProgressToTarget(JVector initialPosition, JVector finalPosition, JVector targetPosition)
        {
            float startDx = targetPosition.X - initialPosition.X;
            float startDz = targetPosition.Z - initialPosition.Z;
            float startDistance = MathF.Sqrt(startDx * startDx + startDz * startDz);

            float finalDx = targetPosition.X - finalPosition.X;
            float finalDz = targetPosition.Z - finalPosition.Z;
            float finalDistance = MathF.Sqrt(finalDx * finalDx + finalDz * finalDz);

            if (startDistance < 0.001f)
                return 0f;

            float progress = (startDistance - finalDistance) / startDistance;

            const float arrivalRadius = 10f;

            float arrivalBonus = finalDistance <= arrivalRadius ? 0.6f : 0f;

            return progress + arrivalBonus;
        }

        public static float EvalMultiPhaseTargetFollowing(RagdollBody ragdoll)
        {
            if (ragdoll.PhaseProgressMeters.Count == 0)
            {
                return 20f * EvalProgressToTarget(ragdoll.initialPos, ragdoll.COMPosition, ragdoll.TargetPosition);
            }

            float posSum = 0f;
            float negSum = 0f;
            float minPhase = float.MaxValue;

            foreach (float delta in ragdoll.PhaseProgressMeters)
            {
                if (delta >= 0f)
                    posSum += delta;
                else
                    negSum += delta;

                if (delta < minPhase)
                    minPhase = delta;
            }

            // Penalize wrong-way drift heavily (0.85x) so blind single-direction movers gain
            // almost nothing regardless of angle, while rewarding steering across all 4 directions.
            float minPosBonus = MathF.Max(0f, minPhase);
            float arrivalBonus = ragdoll.PhasesReachedCount * 8f;

            return posSum + (0.85f * negSum) + (2.0f * minPosBonus) + arrivalBonus;
        }

        public static float EvalEnergyUsed(float EnergyUsed)
        {
            if (EnergyUsed == 0)
                return 0;
            return 1f / (1f + EnergyUsed); // less energy used = higher fitness
        }

        public static float EvalTimeTaken(float TimeTaken, bool reached)
        {
            if (TimeTaken <= 0 || !reached)
                return 0;
            return 1f / (1f + TimeTaken); // less time taken = higher fitness
        }

    }

    public static class FitnessEval
    {
        public static float EvaluateFitness(RagdollBody ragdoll)
        {
            float totalFitness = 0f;

            foreach (FitnessObjective objective in ragdoll.fitnessObjectives)
            {
                totalFitness += objective.weight * objective.fitnessFunction(ragdoll);
            }
            return totalFitness;
        }
    }
}
