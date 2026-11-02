using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;


//https://www.geeksforgeeks.org/deep-learning/spiking-neural-networks-in-deep-learning-/

namespace _3D_Engine.Classes.NeuralNet
{
    public struct Neuron
    {
        public float value;
        public float bias; // activate even if low activation
    }

    public struct SaveableModel // can be serialized and deserialized when needed
    {
        public List<NetworkLayer> NetworkLayers;
        public List<Matrix<float>> NetworkWeights;
        public int[] LayerDims;
    }



    public class NeuralNetwork
    {

        public List<NetworkLayer> NetworkLayers = new List<NetworkLayer>();
        //𝑊0,2 means the weight from neuron 2 in the previous layer to neuron 0 in the next layer
        // Wij notation check notes
        public List<Matrix<float>> NetworkWeights = new List<Matrix<float>>();
        public int[] LayerDims;
        public float FitnessScore = 0;
        public Genome genome;

        public NeuralNetwork(int[] LayerDims)
        {
            this.LayerDims = LayerDims;
            initializeWeights();
            initializeNetwork();

            genome = new Genome(NetworkWeights, ConstructBiasList());
        }

        // for loading a model
        public NeuralNetwork(int[] LayerDims, List<Matrix<float>> weights, List<Matrix<float>> biases)
        {
            this.LayerDims = LayerDims;
            this.NetworkWeights = weights;
            initializeNetwork(biases);

            genome = new Genome(NetworkWeights, ConstructBiasList());
        }


        private void initializeNetwork() // gives neurons random biases
        {

            for (int i = 0; i < LayerDims.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        NetworkLayer inputLayer = new InputLayer(Matrix<float>.Build.Random(LayerDims[i], 1), NetworkWeights[i]);
                        inputLayer.SetLayerBias(Matrix<float>.Build.Random(LayerDims[i], 1));
                        NetworkLayers.Add(inputLayer);
                        break;

                    default:
                        if (i != LayerDims.Length - 1)
                        {
                            NetworkLayer hiddenLayer = new HiddenLayer(LayerDims[i], NetworkWeights[i]);
                            //hiddenLayer.SetLayerBias(Matrix<float>.Build.Random(LayerDims[i], 1), NetworkWeights[i]);
                            hiddenLayer.SetLayerBias(Matrix<float>.Build.Random(LayerDims[i + 1], 1));
                            NetworkLayers.Add(hiddenLayer);
                            break;
                        }

                        NetworkLayer outputLayer = new OutputLayer(Matrix<float>.Build.Random(LayerDims[i], 1));
                        outputLayer.SetLayerBias(Matrix<float>.Build.Random(LayerDims[i], 1), LayerDims[i]);
                        NetworkLayers.Add(outputLayer);
                        break;

                }
            }
        }

        private void initializeNetwork(List<Matrix<float>> biases) // gives neurons random biases
        {

            for (int i = 0; i < LayerDims.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        NetworkLayer inputLayer = new InputLayer(biases[i], NetworkWeights[i]);
                        inputLayer.SetLayerBias(biases[i]);
                        NetworkLayers.Add(inputLayer);
                        break;

                    default:
                        if (i != LayerDims.Length - 1)
                        {
                            NetworkLayer hiddenLayer = new HiddenLayer(LayerDims[i], NetworkWeights[i]);
                            //hiddenLayer.SetLayerBias(Matrix<float>.Build.Random(LayerDims[i], 1), NetworkWeights[i]);
                            hiddenLayer.SetLayerBias(biases[i]);
                            NetworkLayers.Add(hiddenLayer);
                            break;
                        }

                        NetworkLayer outputLayer = new OutputLayer(biases[i]);
                        outputLayer.SetLayerBias(biases[i]);
                        NetworkLayers.Add(outputLayer);
                        break;

                }
            }
        }

        private void initializeWeights()////////////////////implement non random????
        {
            // between each layer
            for (int i = 0; i < LayerDims.Count() - 1; i++)
            {
                Random rng = new Random();
                // why mostly negative weight values
                Matrix<float> currentLayerWeight = Matrix<float>.Build.Random(LayerDims[i + 1], LayerDims[i], new Random().Next(-1000000, 1000000));
                //Matrix<float> currentLayerWeight = Matrix<float>.Build.Dense(LayerDims[i+1], LayerDims[i], (i, j) => -(i % 2) * (float)rng.NextDouble() * 2 );
                NetworkWeights.Add(currentLayerWeight);
            }
        }

        public Matrix<float> GetNetworkOutput(Matrix<float> Input) // forward pass
        {
            //if (Input.ColumnCount != LayerDims[0] || Input.ColumnCount != 1)
            if (Input.RowCount != LayerDims[0] || Input.ColumnCount != 1)
                throw new InvalidDataException("Method only allows matrix with one column and must be the same rows as input layer");

            // 0 idx is the input and final idx is the output
            Matrix<float> CurrentOutput = NetworkLayers[0].getOutputAfterWeights(Input); // overidden every layer

            for (int i = 1; i < NetworkLayers.Count; i++) // from second layer to last
            {
                CurrentOutput = NetworkLayers[i].getOutputAfterWeights(CurrentOutput);
            }
            return CurrentOutput;
        }

        public List<Matrix<float>> ConstructBiasList()
        {
            List<Matrix<float>> result = new List<Matrix<float>>();

            foreach (NetworkLayer l in this.NetworkLayers)
            {
                result.Add(l.LayerBias);
            }

            return result;
        }

        public void ExecuteEndOfGenerationTasks()
        {
            genome = new Genome(NetworkWeights, ConstructBiasList());
        }

        public void ExecuteStartOfGenerationTasks()
        {
            // Reset fitness score for the new generation
            FitnessScore = 0;
        }

        public NeuralNetwork Copy()
        {
            List<Matrix<float>> newWeights = new List<Matrix<float>>();
            foreach (var weight in NetworkWeights)
            {
                newWeights.Add(weight.Clone());
            }
            List<Matrix<float>> newBiases = new List<Matrix<float>>();
            foreach (var bias in NetworkLayers)
            {
                newBiases.Add(bias.LayerBias.Clone());
            }
            NeuralNetwork copy = new NeuralNetwork(LayerDims, newWeights, newBiases);
            return copy;
        }
    }
}
