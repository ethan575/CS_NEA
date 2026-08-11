using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine.Classes.NeuralNet
{
    public class OutputLayer : NetworkLayer
    {


        public OutputLayer(Matrix<float> neuronBiases) : base (neuronBiases)
        {
            
        }

        public override Matrix<float> getOutput(Matrix<float> inputMatrix)
        {
            return MatrixOperationHelper.TanhMatrix(neuronValues);
        }

    }
}
