using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.NeuralNet
{

    public class Generation
    {
        public float ActiveTimeLeft = 20f;
        public List<NeuralNetwork> Population = new List<NeuralNetwork>();

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

        private const float DefaultMutationRate = 0.25f;

        public static Generation EvolveGeneration(List<NeuralNetwork> population, int NoEliteNNs = 5)
        {
            if (population == null || population.Count == 0)
            {
                var emptyGen = new Generation(new List<NeuralNetwork>());
                LatestGen = emptyGen;
                return emptyGen;
            }

            List<NeuralNetwork> sorted = population.OrderByDescending(nn => nn.FitnessScore).ToList();
            int eliteCount = Math.Clamp(NoEliteNNs, 1, sorted.Count);
            int offspringCount = sorted.Count - eliteCount;

            EliteNNs = sorted.Take(eliteCount).Select(nn =>
            {
                var copy = nn.Copy();
                copy.ExecuteEndOfGenerationTasks();
                return copy;
            }).ToList();

            NewNNs = new List<NeuralNetwork>(offspringCount);

            if (offspringCount > 0)
            {
                int poolSize = Math.Clamp(Math.Max(eliteCount, sorted.Count / 4), 1, sorted.Count);
                int immigrantCount = offspringCount >= 10 ? 3 : 0;
                int tunableCount = offspringCount - immigrantCount;
                int crossoverCount = poolSize >= 2 ? Math.Min(Math.Max(0, tunableCount / 6), tunableCount) : 0;

                for (int i = 0; i < offspringCount; i++)
                {
                    if (i >= tunableCount)
                    {
                        // Fresh random immigrants preserve global exploration
                        var immigrant = new NeuralNetwork(sorted[0].LayerDims);
                        immigrant.ExecuteEndOfGenerationTasks();
                        NewNNs.Add(immigrant);
                        continue;
                    }

                    NeuralNetwork child;
                    if (i < crossoverCount)
                    {
                        int p1 = SelectTournamentIndex(poolSize, tournamentSize: 3);
                        int p2;
                        do
                        {
                            p2 = SelectTournamentIndex(poolSize, tournamentSize: 3);
                        } while (p2 == p1 && poolSize > 1);

                        child = NeuronCrossover(sorted[p1], sorted[p2]);
                        MutateNetwork(child, DefaultMutationRate, sigma: 0.15f);
                    }
                    else
                    {
                        int parentIdx = SelectTournamentIndex(poolSize, tournamentSize: 3);
                        child = sorted[parentIdx].Copy();

                        int mutIdx = i - crossoverCount;
                        int mutSpan = Math.Max(1, tunableCount - crossoverCount);
                        float frac = (float)mutIdx / mutSpan;

                        if (frac < 0.35f)
                            MutateNetwork(child, rate: 0.20f, sigma: 0.08f);
                        else if (frac < 0.80f)
                            MutateNetwork(child, rate: 0.25f, sigma: 0.18f);
                        else
                            MutateNetwork(child, rate: 0.30f, sigma: 0.35f);
                    }

                    child.ExecuteEndOfGenerationTasks();
                    NewNNs.Add(child);
                }
            }

            List<NeuralNetwork> nextPopulation = new List<NeuralNetwork>(sorted.Count);
            nextPopulation.AddRange(EliteNNs);
            nextPopulation.AddRange(NewNNs);

            Generation newGen = new Generation(nextPopulation);
            population.ForEach(nn => nn.FitnessScore = 0); // reset old fitness scores

            LatestGen = newGen;
            return newGen;
        }

        public static NeuralNetwork CreateMutatedVariant(NeuralNetwork parent, float rate = DefaultMutationRate, float sigma = 0.18f)
        {
            NeuralNetwork variant = parent.Copy();
            MutateNetwork(variant, rate, sigma);
            variant.ExecuteEndOfGenerationTasks();
            return variant;
        }

        // Select an index in [0, poolSize) biased toward earlier (fitter) entries in sorted list
        private static int SelectTournamentIndex(int poolSize, int tournamentSize)
        {
            int best = rng.Next(poolSize);
            for (int t = 1; t < tournamentSize; t++)
            {
                int candidate = rng.Next(poolSize);
                if (candidate < best)
                    best = candidate;
            }
            return best;
        }

        // Crosses entire neuron rows (incoming weights + that neuron's bias) together so hidden units stay coherent
        private static NeuralNetwork NeuronCrossover(NeuralNetwork parent1, NeuralNetwork parent2)
        {
            NeuralNetwork child = parent1.Copy();

            for (int layerWeightIdx = 0; layerWeightIdx < child.NetworkWeights.Count; layerWeightIdx++)
            {
                Matrix<float> childWeights = child.NetworkWeights[layerWeightIdx];
                Matrix<float> p1Weights = parent1.NetworkWeights[layerWeightIdx];
                Matrix<float> p2Weights = parent2.NetworkWeights[layerWeightIdx];

                int targetLayerIdx = layerWeightIdx + 1;
                bool hasTargetBias = targetLayerIdx < child.NetworkLayers.Count;

                for (int row = 0; row < childWeights.RowCount; row++)
                {
                    bool pickFirst = rng.NextSingle() < 0.5f;
                    Matrix<float> srcWeights = pickFirst ? p1Weights : p2Weights;

                    for (int col = 0; col < childWeights.ColumnCount; col++)
                    {
                        childWeights[row, col] = srcWeights[row, col];
                    }

                    if (hasTargetBias && row < child.NetworkLayers[targetLayerIdx].LayerBias.RowCount)
                    {
                        Matrix<float> srcBias = pickFirst
                            ? parent1.NetworkLayers[targetLayerIdx].LayerBias
                            : parent2.NetworkLayers[targetLayerIdx].LayerBias;
                        child.NetworkLayers[targetLayerIdx].LayerBias[row, 0] = srcBias[row, 0];
                    }
                }
            }

            return child;
        }

        private static void MutateNetwork(NeuralNetwork network, float rate, float sigma)
        {
            for (int layerWeightIdx = 0; layerWeightIdx < network.NetworkWeights.Count; layerWeightIdx++)
            {
                Mutate(network.NetworkWeights[layerWeightIdx], rate, sigma);
            }

            // Skip layer 0 (InputLayer bias is unused)
            for (int layerBiasIdx = 1; layerBiasIdx < network.NetworkLayers.Count; layerBiasIdx++)
            {
                Mutate(network.NetworkLayers[layerBiasIdx].LayerBias, rate, sigma);
            }
        }

        private static void Mutate(Matrix<float> matrix, float rate, float sigma)
        {
            for (int row = 0; row < matrix.RowCount; row++)
            {
                for (int column = 0; column < matrix.ColumnCount; column++)
                {
                    if (rng.NextSingle() < rate)
                        matrix[row, column] += (float)gaussian(0.0, sigma);
                }
            }
        }

        // https://github.com/conradshyu/rng/blob/master/random.cs
        static double gaussian(double mean = 0.0, double s = 1.0)
        {
            double u1;
            do u1 = uniform(); while (u1 <= 0.0);   // remove 0.0 to avoid log(0)
            double u2 = uniform();

            return s * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2) + mean;
        }

        static double uniform(double a = 0.0, double b = 1.0)
        {
            return a + (b - a) * rng.NextDouble();
        }

    }
}
