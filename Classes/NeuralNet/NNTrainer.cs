using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;

namespace _3D_Engine.Classes.NeuralNet
{

    public class Generation
    {
        public float ActiveTimeLeft = 20f;
        public List<NeuralNetwork> Population;

        public Generation()
        {
            
        }
        public Generation(List<NeuralNetwork> population)
        {
            Population = population;
        }

        public Generation Copy()
        {
            List<NeuralNetwork> newPopulation = new List<NeuralNetwork>();
            foreach (var nn in Population)
            {
                newPopulation.Add(nn.Copy());
            }
            return new Generation(newPopulation);
        }
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

    public static class NNTrainer
    {

        static Random rng = new Random();
        public static List<Generation> Generations = new List<Generation>();
        public static Generation? LatestGen;

        private static List<NeuralNetwork> EliteNNs = new List<NeuralNetwork>();
        private static List<NeuralNetwork> NewNNs = new List<NeuralNetwork>();

        private static float PNetworkMutates = 0.8f; // chance of mutation
        private static float mutationRate = 0.2f; // chance of  element mutation

        public static Generation EvolveGeneration(List<NeuralNetwork> population, int NoEliteNNs = 5)
        {
            Generation newGen = new Generation(population);

            EliteNNs = new List<NeuralNetwork>(newGen.Population.Take(NoEliteNNs)); // keep these the same
            NewNNs = new List<NeuralNetwork>(newGen.Population.Skip(NoEliteNNs)); // evolve

            EliteNNs = EliteNNs.Select(nn => nn.Copy()).ToList();
            NewNNs = NewNNs.Select(nn => nn.Copy()).ToList();

            //!!!!!!!!!! new network is just a combination of the entire weight matrix of fittest
            // !! make sure to mutate weight as well as bias

            //crossover
            for (int networkIdx = 0; networkIdx < NewNNs.Count - 3; networkIdx++) // 3 last networks are only mutation
            {
                int parent1Idx = rng.Next(0, EliteNNs.Count);
                int parent2Idx;
                do parent2Idx = rng.Next(0, EliteNNs.Count);
                while (parent2Idx == parent1Idx && EliteNNs.Count > 1);

                // weights crossover
                for (int LayerWeightIdx = 0; LayerWeightIdx < EliteNNs[0].NetworkWeights.Count; LayerWeightIdx++) // what layer matrix are we accessing
                {
                    Matrix<float> CurrentChangingMatrix = NewNNs[networkIdx].NetworkWeights[LayerWeightIdx];
                    //Console.WriteLine(CurrentChangingMatrix);

                    for (int row = 0; row < CurrentChangingMatrix.RowCount; row++)
                    {
                        for (int column = 0; column < CurrentChangingMatrix.ColumnCount; column++)
                        {

                            CurrentChangingMatrix[row, column] = rng.NextSingle() < 0.5f ? EliteNNs[parent1Idx].genome.Weights[LayerWeightIdx][row, column] : EliteNNs[parent2Idx].genome.Weights[LayerWeightIdx][row, column];


                        }
                    }
                    //Console.WriteLine(CurrentChangingMatrix);
                }

                // biases crossover
                for (int LayerBiasIdx = 0; LayerBiasIdx < EliteNNs[0].NetworkLayers.Count; LayerBiasIdx++) // what layer matrix are we accessing
                {
                    Matrix<float> CurrentChangingMatrix = NewNNs[networkIdx].NetworkLayers[LayerBiasIdx].LayerBias;
                    //Console.WriteLine(CurrentChangingMatrix);

                    for (int row = 0; row < CurrentChangingMatrix.RowCount; row++)
                    {
                        for (int column = 0; column < CurrentChangingMatrix.ColumnCount; column++)
                        {


                            CurrentChangingMatrix[row, column] = rng.NextSingle() < 0.5f ? EliteNNs[parent1Idx].genome.Biases[LayerBiasIdx][row, column] : EliteNNs[parent2Idx].genome.Biases[LayerBiasIdx][row, column];

                        }
                    }
                    //Console.WriteLine(CurrentChangingMatrix);
                }

                // mutationRate is the chance the network mutates
                if (rng.NextSingle() < PNetworkMutates)
                {
                    // weight mutation
                    for (int LayerWeightIdx = 0; LayerWeightIdx < EliteNNs[0].NetworkWeights.Count; LayerWeightIdx++)
                    {
                        Mutate(NewNNs[networkIdx].NetworkWeights[LayerWeightIdx]);
                    }

                    // bias mutation
                    for (int LayerBiasIdx = 0; LayerBiasIdx < EliteNNs[0].NetworkLayers.Count; LayerBiasIdx++)
                    {
                        Mutate(NewNNs[networkIdx].NetworkLayers[LayerBiasIdx].LayerBias);
                    }
                }



            }

            for (int i = NewNNs.Count - 3; i < NewNNs.Count; i++)
            {
                NewNNs[i] = EliteNNs[rng.Next(EliteNNs.Count)].Copy();
            }
            for (int networkIdx = Math.Max(0, NewNNs.Count - 3); networkIdx < NewNNs.Count; networkIdx++) // 3 last networks are only mutation
            {
                // weight mutation
                for (int LayerWeightIdx = 0; LayerWeightIdx < EliteNNs[0].NetworkWeights.Count; LayerWeightIdx++)
                {
                    Mutate(NewNNs[networkIdx].NetworkWeights[LayerWeightIdx]);
                }

                // bias mutation
                for (int LayerBiasIdx = 0; LayerBiasIdx < EliteNNs[0].NetworkLayers.Count; LayerBiasIdx++)
                {
                    Mutate(NewNNs[networkIdx].NetworkLayers[LayerBiasIdx].LayerBias);
                }
            }

            newGen.Population = new List<NeuralNetwork>(EliteNNs);
            newGen.Population.AddRange(NewNNs);

            population.ForEach(nn=> nn.FitnessScore = 0); // reset fitness score for next generation

            //Generations.Add(newGen);
            LatestGen = newGen;
            return newGen;
        }

        private static void Mutate(Matrix<float> matrix)
        {

            for (int row = 0; row < matrix.RowCount; row++)
            {
                for (int column = 0; column < matrix.ColumnCount; column++)
                {
                    if (rng.NextSingle() < mutationRate)
                        matrix[row, column] += (float)gaussian(0, 0.2); // add a small random value from a normal distribution
                }
            }
        }

        private static System.Random r = new System.Random();

        // https://github.com/conradshyu/rng/blob/master/random.cs
        static double gaussian(double mean = 0.0, double s = 1.0)
        {
            double u1;
            do u1 = uniform(); while (u1 <= 0.0);   // remove 0.0 to avoid log(0)
            double u2 = uniform();

            return s * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2) + mean;
        }   // normal distribution with mean 0 stadnard deviation 1

        static double uniform(double a = 0.0, double b = 1.0)
        {
            return (a + (b - a) * r.NextDouble());
        }   // uniform distribution, default [0, 1]

    }
}
