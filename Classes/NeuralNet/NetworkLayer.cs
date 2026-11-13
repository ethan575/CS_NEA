using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.NeuralNet
{
    public class NetworkLayer
    {
        public List<Neuron> neurons;
        public Matrix<float> neuronValues;
        public Matrix<float> LayerBias { get; protected set; }

        public Matrix<float> InputMatrix { get; set; }

        public NetworkLayer(int numberofNeurons)
        {
            neurons = new List<Neuron>();

            for (int i = 0; i < numberofNeurons; i++)
            {
                neurons.Add(new Neuron
                {
                    value = 0,
                });
            }

            //!! number of neurons must be same as the column count like 7x8 matrix need 7x1 bias
            // default bias is 0
            LayerBias = Matrix<float>.Build.Dense(numberofNeurons, 1, 0f);

            neuronValues = Matrix<float>.Build.Dense(numberofNeurons, 1, 0.0f);
        }

        public NetworkLayer(Matrix<float> NeuronBiases)
        {
            neurons = new List<Neuron>();

            if (!(NeuronBiases.ColumnCount == 1)) throw new InvalidDataException("Method only allows matrix with one column");
            this.LayerBias = NeuronBiases;

            foreach (float _bias in NeuronBiases.Column(0).AsArray())
            {
                neurons.Add(new Neuron
                {
                    value = 0,
                    bias = _bias
                });
            }

            neuronValues = Matrix<float>.Build.Dense(NeuronBiases.Column(0).AsArray().Length, 1, 0.0f);
        }

        public virtual void SetLayerBias(Matrix<float> newBias)
        {
            if (!(newBias.ColumnCount == 1)) throw new InvalidDataException("Method only allows matrix with one column");
            //else if (newBias.RowCount != this.neurons.Count) throw new InvalidDataException("New bias matrix row count must match the row count of the right weight matrix");

            this.LayerBias = newBias;

        }
        public virtual void SetLayerBias(Matrix<float> newBias, int biasDim)
        {
        }

        public virtual Matrix<float> getOutputAfterWeights(Matrix<float> inputMatrix)
        {
            return Matrix<float>.Build.SparseIdentity(0);
        }

    }
}
