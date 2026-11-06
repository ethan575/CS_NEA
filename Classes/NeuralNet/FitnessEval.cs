using _3D_Engine.Classes.Ragdoll;
using Jitter2.LinearMath;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static float EvalProximityToTarget(JVector BodyPosition, JVector TargetPos)
        {
            if (MaxDistanceFromPopulation <= 1e-6f) return 1f;
            float distance = (TargetPos - BodyPosition).Length();
            float normalizedDist = distance / MaxDistanceFromPopulation;
            return 1 - normalizedDist; // closer to target = higher fitness
        }

        public static float EvalEnergyUsed(float EnergyUsed)
        {
            if (EnergyUsed == 0)
                return 0;
            return 1f/(1f/EnergyUsed); // less energy used = higher fitness
        }

    }

    public static class FitnessEval
    {
        public static float EvaluateFitness(RagdollBody ragdoll)
        {
            float totalFitness = 0f;
            foreach (var objective in ragdoll.fitnessObjectives)
            {
                totalFitness += objective.weight * objective.fitnessFunction(ragdoll);
            }
            return totalFitness;
        }
    }
}
