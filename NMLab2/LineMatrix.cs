using System.Globalization;
using System.Runtime.InteropServices;

namespace NMLab2
{
	public class LineMatrix
	{
		public int N { get; }
		public int L { get; }

		public float[,] Matrix { get; set; }

		public float[] F { get; set; }

		public LineMatrix(int n, int l)
		{
			N = n;
			L = l;

			Matrix = new float[N, L];
			F = new float[N];
		}

		public LineMatrix(string filePath)
		{
			StreamReader reader = new StreamReader(filePath);
			string? line = reader.ReadLine();
			if (line == null)
			{
				reader.Close();
				throw new Exception("File is empty");
			}

			string[] tokens = line.Split(' ');
			N = int.Parse(tokens[0]);
			L = int.Parse(tokens[1]);
			Matrix = new float[N, L];
			F = new float[N];

			int level = 0;
			//read matrix
			for (int k = 0; k < N; ++k)
			{
				line = reader.ReadLine();
				if (line == null)
				{
					reader.Close();
					throw new Exception("Incorrect file data");
				}

				tokens = line.Split(' ');

				//fill with 0's
				int i;
				for (i = 0; i + level < L - 1; ++i)
				{
					Matrix[level, i] = 0.0f;
				}

				//parse the rest
				for (int j = 0; i < L; ++i, ++j)
				{
					Matrix[level, i] = float.Parse(tokens[j], CultureInfo.InvariantCulture);
				}

				++level;
			}

			//read f vector
			line = reader.ReadLine();
			if (line == null)
			{
				reader.Close();
				throw new Exception("Incorrect F vector data");
			}

			tokens = line.Split(' ');
			for (int i = 0; i < N; ++i)
			{
				F[i] = float.Parse(tokens[i], CultureInfo.InvariantCulture);
			}

			reader.Close();
		}

		public void FillRandom(Random random)
		{
			
		}

		public float[] Multiply(float[] arr)
		{
			if (arr.Length != N)
			{
				throw new Exception("Incorrect array size");
			}

			int lowerBound = L - 1;

			int arrLowerBound = 0;
			int arrUpperBound = L - lowerBound;

			float[] result = new float[N];
			for (int i = 0; i < N; ++i)
			{
				Console.WriteLine($"LINE {i}");
				//this line
				int j2 = arrLowerBound;
				for (int j = lowerBound; j < L; ++j, ++j2)
				{
					Console.Write($"{Matrix[i, j]} mult by {arr[j2]}\t");
					result[i] += Matrix[i, j] * arr[j2];
				}

				//symmetrical line
				for (int j = L - 2, i2 = i + 1; j >= 0 && i2 < N && j2 < N; --j, ++i2, ++j2)
				{
					Console.Write($"{Matrix[i2, j]} mult by {arr[j2]}\t");
					result[i] += Matrix[i2, j] * arr[j2];
				}


				lowerBound = (int)MathF.Max(0, lowerBound - 1);

				++arrUpperBound;
				++arrLowerBound;
				if (i < L - 1)
				{
					arrLowerBound = 0;
				}
			}

			return result;
		}

		public void Print()
		{
			Console.WriteLine("------");
			for (int i = 0; i < N; ++i)
			{
				for (int j = 0; j < L; ++j)
				{
					Console.Write($"{Matrix[i, j],15}");
				}

				Console.WriteLine();
			}

			Console.WriteLine("\nF:");
			for (int i = 0; i < N; ++i)
			{
				Console.Write(F[i] + " ");
			}

			Console.WriteLine();
		}

		public LineMatrix CalculateTmatrix()
		{
			LineMatrix tMatrix = new LineMatrix(N, L);

			//first row 
			float denom = MathF.Sqrt(Matrix[0, L - 1]);
			tMatrix.Matrix[0, L - 1] = denom;

			int i = 1;
			for (int j = L - 2; j >= 0 && i < N; --j, ++i)
			{
				tMatrix.Matrix[i, j] = Matrix[i, j] / denom;
			}

			//other rows
			for (i = 1; i < N; ++i)
			{
				float sum = 0.0f;
				for (int j = 0; j < L - 1; ++j)
				{
					sum += tMatrix.Matrix[i, j] * tMatrix.Matrix[i, j];
				}

				tMatrix.Matrix[i, L - 1] = MathF.Sqrt(Matrix[i, L - 1] - sum);
				float mainDiag = tMatrix.Matrix[i, L - 1];

				int i2 = i + 1;
				for (int j = L - 2; j >= 0 && i2 < N; --j, ++i2)
				{
					sum = 0.0f;
					for (int k = 0; k < j; ++k)
					{
						sum += tMatrix.Matrix[i2, k] * tMatrix.Matrix[i2 - 1, k + 1];
					}

					tMatrix.Matrix[i2, j] = (Matrix[i2, j] - sum) / mainDiag;
				}
			}

			return tMatrix;
		}

		public float[] SolveFull()
		{
			LineMatrix tMatrix = CalculateTmatrix();
			tMatrix.F = F;
			float[] result = tMatrix.Solve();
			tMatrix.F = result;
			return tMatrix.SolveTransposed();
		}

		public float[] Solve()
		{
			float[] result = new float[N];
			result[0] = F[0] / Matrix[0, L - 1];

			for (int i = 1; i < N; ++i)
			{
				float sum = 0.0f;
				Console.WriteLine($"Line {i}");
				int j = 0;
				int j2 = 0;

				if (i < L)
				{
					j = L - i - 1;
					//	j2 = j;
				}
				else
				{
					j2 = i - L + 1;
				}

				for (; j < L - 1; ++j, ++j2)
				{
					sum += Matrix[i, j] * result[j2];
					Console.WriteLine($"{Matrix[i, j]} multiply by {result[j2]}");
				}

				result[i] = (F[i] - sum) / Matrix[i, L - 1];
				Console.WriteLine($"Result: {result[i]}");
			}

			return result;
		}

		public float[] SolveTransposed()
		{
			float[,] tempMat = Matrix;
			Matrix = GetTransposedMatrix();
			float[] tempF = F;
			F = F.Reverse().ToArray();

			Print();

			float[] result = Solve();
			Matrix = tempMat;
			F = tempF;
			return result.Reverse().ToArray();
		}

		public float[,] GetTransposedMatrix()
		{
			float[,] transpMatr = new float[N, L];
			float[] column = new float[N];

			for (int j = L - 1, step = 0; j >= 0; --j, ++step)
			{
				for (int i = 0; i < N - step; ++i)
				{
					column[i] = Matrix[i + step, j];
				}

				var reverseCol = column.Reverse().ToArray();
				for (int i = step; i < N; ++i)
				{
					transpMatr[i, j] = reverseCol[i];
				}

				for (int i = 0; i < N; ++i)
				{
					column[i] = 0.0f;
				}
			}

			return transpMatr;
		}
	}
}