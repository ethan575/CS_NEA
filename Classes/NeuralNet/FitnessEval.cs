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
        // maximum distance from the target position for reletivity
        public static float MaxDistanceFromPopulation = float.MinValue;

        // 0 = bad, 1 = good
        public static float EvalProgressToTarget(JVector initialPosition, JVector finalPosition, JVector targetPosition)
        {

            float startDistance = (targetPosition - initialPosition).Length();

            float finalDistance = (targetPosition - finalPosition).Length();

            if (startDistance < 0.001f)
                return 0f;

            float progress = (startDistance - finalDistance) / startDistance;

            const float arrivalRadius = 10f;

            float arrivalBonus = finalDistance <= arrivalRadius ? 0.6f : 0f;

            return progress + arrivalBonus;
        }

        public static float EvalEnergyUsed(float EnergyUsed)
        {
            if (EnergyUsed == 0)
                return 0;
            return 1f / (1f + EnergyUsed); // less energy used = higher fitness
        }

        public static float EvalTimeTaken(float TimeTaken, bool reached)
        {
            if (TimeTaken == 0 || !reached)
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
