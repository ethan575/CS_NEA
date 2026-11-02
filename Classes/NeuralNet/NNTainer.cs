using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.NeuralNet
{

    public class Generation
    {
        public float ActiveTimeLeft = 20f;
        public List<NeuralNetwork> Population;
    }

    // contains all the weights and biases of a neural network
    // this is evolved and mutated to create new data for the next generation of neural networks
    public class Genome
    {
        public List<Matrix<float>> Weights;
        public List<Matrix<float>> Biases;
        public Genome(List<Matrix<float>> weights, List<Matrix<float>> biases)
        {
            this.Weights = weights;
            this.Biases = biases;
        }

        public Genome Copy()
        {
            List<Matrix<float>> newWeights = new List<Matrix<float>>();
            List<Matrix<float>> newBiases = new List<Matrix<float>>();
            foreach (var weight in Weights)
            {
                newWeights.Add(weight.Clone());
            }
            foreach (var bias in Biases)
            {
                newBiases.Add(bias.Clone());
            }
            return new Genome(newWeights, newBiases);
        }
    }

    public static class NNTainer
    {

        static Random rng = new Random();
        public static List<Generation> Generations = new List<Generation>();
        public static Generation? LatestGen;
        
        public Generation EvolveGeneration(List<NeuralNetwork> population, int NoEliteNNs = 5)
        {
            Generation newGen = new Generation();
            newGen.Population = population;

            List<NeuralNetwork> EliteNNs = new List<NeuralNetwork>(newGen.Population.Take(NoEliteNNs)); // keep these the same
            List<NeuralNetwork> NewNNs = new List<NeuralNetwork>(newGen.Population.Skip(NoEliteNNs)); // evolve

            // new network is just a combination of the entire weight matrix of fittest
            // 


            Generations.Add(newGen);
            LatestGen = newGen;
            return newGen;
        }


    }
}
