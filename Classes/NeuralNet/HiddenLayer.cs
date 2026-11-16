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
            InputMatrix = inputMatrix + LayerBias;
            neuronValues = MatrixOperationHelper.ApplyLeakyReluToMatrix(InputMatrix);

            Matrix<float> output = LayerWeightRight * neuronValues;
            return output; // next layer dim x 1

        }

    }
}
