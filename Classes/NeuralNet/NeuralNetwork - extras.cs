using MathNet.Numerics.LinearAlgebra;


//https://www.geeksforgeeks.org/deep-learning/spiking-neural-networks-in-deep-learning-/

namespace _3D_Engine.Classes.NeuralNet.Extras
{
    public struct Neuron
    {
        public float value;
        public float bias; // activate even if low activation
    }




    public class NeuralNetwork
    {

        public List<NetworkLayer> NetworkLayers { get; private set; } = new List<NetworkLayer>();
        //𝑊0,2 means the weight from neuron 2 in the previous layer to neuron 0 in the next layer
        // Wij notation check notes
        public List<Matrix<float>> NetworkWeights { get; private set; } = new List<Matrix<float>>();
        public int[] LayerDims { get; private set; }

        public NeuralNetwork(int[] LayerDims)
        {
            this.LayerDims = LayerDims;
            initializeWeights();
            initializeNetwork();

        }

        public NeuralNetwork(List<Matrix<float>> WeightMatrices, int[] LayerDims)
        {
            this.LayerDims = LayerDims;
            ConstructWeightMatrices(WeightMatrices);
        }


        private void initializeNetwork() // gives neurons random biases
        {

            for (int i = 0; i < LayerDims.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        NetworkLayers.Add(new InputLayer(Matrix<float>.Build.Random(LayerDims[i], 1), NetworkWeights[i]));
                        break;

                    default:
                        if (!(i == LayerDims.Length - 1))
                        {
                            NetworkLayers.Add(new HiddenLayer(LayerDims[i], NetworkWeights[i]));
                            break;
                        }
                        NetworkLayers.Add(new OutputLayer(Matrix<float>.Build.Random(LayerDims[i], 1)));
                        break;

                }
            }
        }

        private void initializeWeights()////////////////////implement non random????
        {
            // between each layer
            //for (int i = 0; i < NetworkLayers.Count-2; i++)
            for (int i = 0; i < LayerDims.Length; i++)
            {

                Matrix<float> currentLayerWeight = Matrix<float>.Build.Random(NetworkLayers[i+ 1].neurons.Count, NetworkLayers[i].neurons.Count);
                //Matrix<float> currentLayerWeight = Matrix<float>.Build.Random(LayerDims[i + 1], LayerDims[i]);
                NetworkWeights.Add(currentLayerWeight);

            }
        }

        public Matrix<float> GetNetworkOutput(Matrix<float> Input) // forward pass
        {
            if (Input.ColumnCount != LayerDims[0] || Input.ColumnCount != 1)
                throw new InvalidDataException("Method only allows matrix with one column and must be the same rows as input layer");

            // 0 idx is the input and final idx is the output
            Matrix<float> CurrentOutput = NetworkLayers[0].getOutputAfterWeights(Input); // overidden every layer

            for (int i = 1; i < NetworkLayers.Count; i++) // from second layer to last
            {
                CurrentOutput = NetworkLayers[i].getOutputAfterWeights(CurrentOutput);
            }
            return CurrentOutput;
        }

        private void ConstructWeightMatrices(List<Matrix<float>> NetworkWeights)
        {
            this.NetworkWeights = NetworkWeights;
            initializeNetwork();
        }

    }
}
