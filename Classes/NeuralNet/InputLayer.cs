using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.NeuralNet
{
    public class InputLayer : NetworkLayer 
    {
        public Matrix<float> LayerWeightRight { get; set; }
        

        public InputLayer(Matrix<float> neuronBiases, Matrix<float> LayerWeightRight) : base(neuronBiases)
        {
            this.LayerWeightRight = LayerWeightRight;
            
        }

        // also need to apply squash func
        public override Matrix<float> getOutput(Matrix<float> inputMatrix)
        {
            InputMatrix = inputMatrix;
            neuronValues = MatrixOperationHelper.NormalizeAngleRadMatrix(InputMatrix);

            Matrix<float> output = LayerWeightRight * neuronValues + NeuronBias;
            return MatrixOperationHelper.ApplyLeakyReluToMatrix(output);

        }

    }
}
