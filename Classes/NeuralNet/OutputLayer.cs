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

        public override void SetLayerBias(Matrix<float> newBias, int biasDim)
        {
            if (!(newBias.ColumnCount == 1)) throw new InvalidDataException("Method only allows matrix with one column");
            else if (newBias.RowCount != biasDim) throw new InvalidDataException("New bias matrix row count must match the biasDim parameter");
            this.LayerBias = newBias;
        }

        public override Matrix<float> getOutputAfterWeights(Matrix<float> inputMatrix)
        {
            this.InputMatrix = inputMatrix;
            this.InputMatrix = MatrixOperationHelper.TanhMatrix(inputMatrix);
            //return MatrixOperationHelper.TanhMatrix(neuronValues);
            return this.InputMatrix;
        }

    }
}
