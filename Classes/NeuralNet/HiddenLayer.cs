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

        public override Matrix<float> getOutput(Matrix<float> inputMatrix)
        {
            InputMatrix = inputMatrix;

            Matrix<float> output = LayerWeightRight * inputMatrix + NeuronBias;
            return MatrixOperationHelper.ApplyLeakyReluToMatrix(output); // n x 1 dim

        }

    }
}
