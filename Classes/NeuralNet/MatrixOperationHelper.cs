using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace _3D_Engine.Classes.NeuralNet
{
    public static class MatrixOperationHelper
    {
        public static float ReLU(float a)
        {
            return Math.Max(0, a);
        }
        public static float LeakyReLU(float a)
        {
            return Math.Max(0.01f*a, a);
        }

        public static Matrix<float> ApplyLeakyReluToMatrix(Matrix<float> matrix)
        {
            if (matrix.ColumnCount != 1) throw new InvalidDataException("Method only allows matrix with one column");

            float[] column =  matrix.Column(0).AsArray();
            float[] resultArr = new float[column.Length];
            Matrix<float> Mresult;

            for (int i= 0; i < column.Length; i++)
            {
                resultArr[i] = LeakyReLU((float)column[i]);
            }

            Vector<float> ColumnsResult = Vector<float>.Build.DenseOfArray(resultArr);
            Mresult = Matrix<float>.Build.DenseOfColumnVectors(ColumnsResult);

            return Mresult;
        }

        public static Matrix<float> ApplyReLUToMatrix(Matrix<float> matrix)
        {
            if (matrix.ColumnCount != 1) throw new InvalidDataException();

            float[] column = matrix.Column(0).AsArray();
            float[] resultArr = new float[column.Length];
            Matrix<float> Mresult;
            for (int i = 0; i < column.Length; i++)
            {
                resultArr[i] = ReLU((float)column[i]);
            }
            Vector<float> ColumnsResult = Vector<float>.Build.DenseOfArray(resultArr);
            Mresult = Matrix<float>.Build.DenseOfColumnVectors(ColumnsResult);
            return Mresult;
        }

        public static float NormalizeAngleRad(float angle)
        {
            return angle / (float)Math.PI*2;
        }
        
        public static Matrix<float> NormalizeAngleRadMatrix(Matrix<float> matrix)
        {
            return matrix / (2 * (float)Math.PI);
        }

        public static float Tanh(float x)
        {
            return (float)Math.Tanh(x);
        }

        public static Matrix<float> TanhMatrix(Matrix<float> matrix)
        {
            if (matrix.ColumnCount !=  1)  throw new InvalidDataException("Method only allows matrix with one column");

            float[] column = matrix.Column(0).AsArray();
            float[] resultArr = new float[column.Length];
            Matrix<float> Mresult;
            for (int i = 0; i < column.Length; i++)
            {
                resultArr[i] = Tanh((float)column[i]);
            }
            Vector<float> ColumnsResult = Vector<float>.Build.DenseOfArray(resultArr);
            Mresult = Matrix<float>.Build.DenseOfColumnVectors(ColumnsResult);
            return Mresult;
        }
    }
}
