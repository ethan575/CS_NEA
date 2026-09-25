using _3D_Engine.Classes.NeuralNet;
using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.Data
{
    public static class NNSaveLoad
    {

        public static void SaveNeuralNetwork(NeuralNetwork nn, string filePath)
        {

            using (var bw = new BinaryWriter(File.OpenWrite(filePath)))
            {

                SaveWeights(nn.NetworkWeights, bw); // save all nn weights

                SaveArray(nn.LayerDims, bw);
            }
        }

        public static NeuralNetwork LoadNeuralNetwork(string filePath)
        {
            List<Matrix<float>> Weights = new List<Matrix<float>>();
            int[] LayerDims;

            using (var br = new BinaryReader(File.OpenRead(filePath)))
            {
                Weights = LoadWeights(filePath, br);

                LoadArray(br, out LayerDims);
            }

            NeuralNetwork LoadedNetwork = new NeuralNetwork(LayerDims, Weights);
            return LoadedNetwork;
        }

        private static void SaveWeights(List<Matrix<float>> weights, BinaryWriter bw)
        {

            bw.Write(weights.Count); // how many weight matrices to read

            foreach (Matrix<float> matrix in weights)
            {
                SaveMatrix(matrix, bw);
            }

        }

        private static List<Matrix<float>> LoadWeights(string filePath, BinaryReader br)
        {
            List<Matrix<float>> weights = new List<Matrix<float>>();

            int numberOfMatrices = br.ReadInt32();

            for (int i = 0; i < numberOfMatrices; i++)
            {
                weights.Add(LoadMatrix(br));
            }

            return weights;
        }

        private static void SaveMatrix(Matrix<float> m, BinaryWriter bw)
        {
            bw.Write(m.ColumnCount);
            bw.Write(m.RowCount); // column x row

            float[] WeightValues = m.AsColumnMajorArray();

            foreach (float value in WeightValues)
            {
                bw.Write(value);
            }
        }

        private static Matrix<float> LoadMatrix(BinaryReader br)
        {
            int ColumnCount = br.ReadInt32();
            int RowCount = br.ReadInt32();

            float[] ColumnMajorWeightValues = new float[RowCount * ColumnCount];

            for (int i = 0; i < (ColumnCount * RowCount); i++)
            {
                ColumnMajorWeightValues[i] = br.ReadSingle();
            }

            return Matrix<float>.Build.DenseOfColumnMajor(RowCount, ColumnCount, ColumnMajorWeightValues);
        }

        private static void SaveArray(float[] floats, BinaryWriter bw)
        {
            bw.Write(floats.Length);

            foreach (float value in floats)
                bw.Write(value);
        }
        private static void SaveArray(int[] ints, BinaryWriter bw)
        {
            bw.Write(ints.Length);

            foreach (int value in ints)
                bw.Write(value);
        }

        private static void LoadArray(BinaryReader br, out float[] finalArray)
        {
            int ValueCount = br.ReadInt32();
            finalArray = new float[ValueCount];

            for (int i = 0; i < ValueCount; i++)
            {
                float value = br.ReadInt32();
                finalArray[i] = value;
            }

        }

        private static void LoadArray(BinaryReader br, out int[] finalArray)
        {
            int ValueCount = br.ReadInt32();
            finalArray = new int[ValueCount];

            for (int i = 0; i < ValueCount; i++)
            {
                int value = br.ReadInt32();
                finalArray[i] = value;

            }


        }
    }
}
