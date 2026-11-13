using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.NeuralNet
{
    public class HiddenLayer : NetworkLayer
    {
        public Matrix<float> LayerWeightRight { get; set; }

        public HiddenLayer(int numberOfNeurons, Matrix<float> LayerWeightRight):base(numberOfNeurons)
        {
            this.LayerWeightRight = LayerWeightRight;
        }

        public override Matrix<float> getOutputAfterWeights(Matrix<float> inputMatrix)
        {
            InputMatrix = inputMatrix;
            InputMatrix = MatrixOperationHelper.ApplyLeakyReluToMatrix(InputMatrix);
            //Matrix<float> output = LayerWeightRight * inputMatrix + LayerBias;
            //return MatrixOperationHelper.ApplyLeakyReluToMatrix(output); // n x 1 dim

            Matrix<float> output = LayerWeightRight * InputMatrix + LayerBias;
            return output; // n x 1 dim

        }

    }
}
