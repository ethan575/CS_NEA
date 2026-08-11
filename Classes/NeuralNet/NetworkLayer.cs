using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix = MathNet.Numerics.LinearAlgebra.Complex.Matrix;

namespace _3D_Engine.Classes.NeuralNet
{
    public class NetworkLayer
    {
        public List<Neuron> neurons;
        public Matrix<float> neuronValues;
        public Matrix<float> NeuronBias;

        public Matrix<float> InputMatrix { get; set; }

        public NetworkLayer(int numberofNeurons)
        {
            neurons = new List<Neuron>();

            for (int i = 0;  i < numberofNeurons; i++)
            {
                neurons.Add(new Neuron
                {
                    /////!!!!!!!!!!!!
                    value = 0,
                    bias = 0
                });
            }

            NeuronBias = Matrix<float>.Build.Dense(numberofNeurons, 1, 0f);

            //////// implement bias matrix initialisation

            neuronValues = Matrix<float>.Build.Dense(numberofNeurons, 1, 0.0f);
        }

        public NetworkLayer(Matrix<float> NeuronBiases)
        {
            neurons = new List<Neuron>();

            if (!(NeuronBiases.ColumnCount == 1)) throw new InvalidDataException("Method only allows matrix with one column");
            this.NeuronBias = NeuronBiases;

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

        public virtual Matrix<float> getOutput(Matrix<float> inputMatrix)
        {
            return Matrix<float>.Build.SparseIdentity(0);
        }

    }
}
