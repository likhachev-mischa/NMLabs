using System.Globalization;

namespace NMLab2
{
	public class LineMatrix
	{
		public int N { get; private set; }
		public int L { get; private set; }

		public double[,] Matrix { get; set; }

		public double[] F { get; set; }

		public LineMatrix(int n, int l)
		{
			N = n;
			L = l;

			Matrix = new double[N, L];
			F = new double[N];
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
			Matrix = new double[N, L];
			F = new double[N];

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
					Matrix[level, i] = double.Parse(tokens[j], CultureInfo.InvariantCulture);
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
				F[i] = double.Parse(tokens[i], CultureInfo.InvariantCulture);
			}

			reader.Close();
		}

		public void GenerateRandom(Random random)
		{
			for (int i = 0; i < N; ++i)
			{
				int j;
				for (j = 0; j < L - i - 1 && i < L; ++j)
				{
					Matrix[i, j] = 0.0f;
				}

				double sum = 0.0f;
				for (; j < L - 1; ++j)
				{
					while (Math.Abs(Matrix[i, j]) <= 1.0f)
					{
						Matrix[i, j] = random.NextSingle() * random.NextInt64(-10, 10);
					}

					//	sum += Matrix[i, j] * Matrix[i, j];
				}

				Matrix[i, L - 1] = random.NextDouble() * N * 100000;
				//while (Matrix[i, L - 1] <= sum)
				//{
				//	Matrix[i, L - 1] = random.NextSingle() *
				//	                   random.NextInt64((int)(sum * 0.5), (int)Math.Max(sum * 1.5, 10));
				//}
			}

			/*
			bool status = false;
			int failedLine;
			double retSum;
			CalculateTmatrix(out status, out failedLine, out retSum);
			while (status == false)
			{
				while (Matrix[failedLine, L - 1] <= retSum)
				{
					Matrix[failedLine, L - 1] = random.NextSingle() *
					                            random.NextInt64((int)(retSum * 0.5), (int)Math.Max(retSum * 1.5, 10));
				}

				CalculateTmatrix(out status, out failedLine, out retSum);
			}
		*/
		}

		public void GenerateIllConditioned(Random random, int k)
		{
			double[,] Lmat = new double[N, N];
			double[,] Umat = new double[N, N];

			int lowerBound;
			int upperBound = L;
			for (int i = 0; i < N; ++i)
			{
				lowerBound = i;
				for (int j = lowerBound, j2 = 0; j2 < upperBound - lowerBound && j < N; ++j2, ++j)
				{
					Umat[i, j] = random.NextSingle() * random.NextInt64(-10, 10) * Math.Pow(10, -k);
				}

				Umat[i, i] = Math.Abs(random.NextSingle() * 10000 * Math.Pow(10, -k + 1) * N);

				++upperBound;
			}

			for (int i = 0; i < N; ++i)
			{
				lowerBound = Math.Max(0, i - L + 1);
				upperBound = i;
				for (int j = lowerBound, j2 = 0; j2 <= upperBound - lowerBound && j < N; ++j2, ++j)
				{
					Lmat[i, j] = random.NextSingle() * random.NextInt64(-10, 10) * Math.Pow(10, -k);
				}

				Lmat[i, i] =
					Math.Abs(random.NextSingle() * 10000 *
					         Math.Pow(10, -k + 1) * N);
				++lowerBound;
			}

			//	Console.WriteLine("L MATR");
			//	Print(Lmat);
			//	Console.WriteLine("U MATR");
			//	Print(Umat);

			var mult = Multiply(Lmat, Umat);
			//	Console.WriteLine("MULTIPLICATION RES:");
			//Print(mult);

			for (int j = 0; j < L; ++j)
			{
				var diag = ExtractDiagonal(mult, j);

				for (int i = 0; i < N; ++i)
				{
					Matrix[i, L - j - 1] = diag[i];
				}
			}

			//Print();
		}

		public double[] ExtractDiagonal(double[,] matrix, int num)
		{
			if (matrix.GetLength(0) != matrix.GetLength(1) ||
			    matrix.GetLength(0) != N)
			{
				throw new Exception("Incorrect matrix size");
			}

			if (num >= N || num < 0)
			{
				throw new Exception("Incorrect num");
			}

			double[] diag = new double[N];
			for (int i = 0; i < num; ++i)
			{
				diag[i] = 0;
			}

			for (int i = num; i < N; ++i)
			{
				diag[i] = matrix[i, i - num];
			}

			return diag;
		}

		public double[] SolveFull()
		{
			LineMatrix tMatrix = CalculateTmatrix(out bool status, out _, out _);

			if (!status)
			{
				throw new Exception("Incorrect matrix generation");
			}

			tMatrix.F = F;
			double[] result = tMatrix.Solve();
			tMatrix.F = result;
			return tMatrix.SolveTransposed();
		}

		public double[] Multiply(double[] arr)
		{
			if (arr.Length != N)
			{
				throw new Exception("Incorrect array size");
			}

			int lowerBound = L - 1;

			int arrLowerBound = 0;

			double[] result = new double[N];
			for (int i = 0; i < N; ++i)
			{
				//	Console.WriteLine($"LINE {i}");
				//this line
				int j2 = arrLowerBound;
				for (int j = lowerBound; j < L; ++j, ++j2)
				{
					//Console.Write($"{Matrix[i, j]} mult by {arr[j2]}\t");
					result[i] += Matrix[i, j] * arr[j2];
				}

				//symmetrical line
				for (int j = L - 2, i2 = i + 1; j >= 0 && i2 < N && j2 < N; --j, ++i2, ++j2)
				{
					//Console.Write($"{Matrix[i2, j]} mult by {arr[j2]}\t");
					result[i] += Matrix[i2, j] * arr[j2];
				}


				lowerBound = Math.Max(0, lowerBound - 1);

				++arrLowerBound;
				if (i < L - 1)
				{
					arrLowerBound = 0;
				}

				//Console.WriteLine();
			}

			return result;
		}

		public void Print(double[,] matrix)
		{
			int n = matrix.GetLength(0);
			int l = matrix.GetLength(1);

			for (int i = 0; i < n; ++i)
			{
				for (int j = 0; j < l; ++j)
				{
					Console.Write($"{matrix[i, j],30}");
				}

				Console.WriteLine();
			}
		}

		public void Print()
		{
			Console.WriteLine("------");
			for (int i = 0; i < N; ++i)
			{
				for (int j = 0; j < L; ++j)
				{
					Console.Write($"{Matrix[i, j],30}");
				}

				Console.WriteLine();
			}

			Console.WriteLine("\nF:");
			for (int i = 0; i < N; ++i)
			{
				Console.Write($"{F[i] + " ",15}");
			}

			Console.WriteLine();
		}


		private double[,] Multiply(double[,] l, double[,] u)
		{
			if (l.GetLength(0) != u.GetLength(0) ||
			    l.GetLength(1) != u.GetLength(1) ||
			    l.GetLength(1) != N)
			{
				throw new Exception("Incorrect matrix sizes");
			}

			double[,] result = new double[N, N];

			for (int i = 0; i < N; ++i)
			for (int j = 0; j < N; ++j)
			{
				double sum = 0.0f;
				for (int k = 0; k < N; ++k)
				{
					sum += l[i, k] * u[k, j];
				}

				result[i, j] = sum;
			}

			return result;
		}

		private LineMatrix CalculateTmatrix(out bool status, out int failedLine, out double retSum)
		{
			LineMatrix tMatrix = new LineMatrix(N, L);

			//first row 
			double denom = Math.Sqrt(Matrix[0, L - 1]);
			if (double.IsNaN(denom))
			{
				status = false;
				failedLine = 0;
				retSum = 0;
				return new LineMatrix(N, L);
			}

			tMatrix.Matrix[0, L - 1] = denom;

			int i = 1;
			for (int j = L - 2; j >= 0 && i < N; --j, ++i)
			{
				tMatrix.Matrix[i, j] = Matrix[i, j] / denom;
			}

			//other rows
			for (i = 1; i < N; ++i)
			{
				double sum = 0.0f;
				for (int j = 0; j < L - 1; ++j)
				{
					sum += tMatrix.Matrix[i, j] * tMatrix.Matrix[i, j];
				}

				double elem = Math.Sqrt(Matrix[i, L - 1] - sum);
				if (double.IsNaN(elem))
				{
					status = false;
					failedLine = i;
					retSum = sum;
					return new LineMatrix(N, L);
				}

				tMatrix.Matrix[i, L - 1] = elem;

				double mainDiag = tMatrix.Matrix[i, L - 1];

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

			status = true;
			failedLine = -1;
			retSum = 0;
			return tMatrix;
		}


		private double[] Solve()
		{
			double[] result = new double[N];
			result[0] = F[0] / Matrix[0, L - 1];

			for (int i = 1; i < N; ++i)
			{
				double sum = 0.0f;
				//Console.WriteLine($"Line {i}");
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
					//Console.WriteLine($"{Matrix[i, j]} multiply by {result[j2]}");
				}

				result[i] = (F[i] - sum) / Matrix[i, L - 1];
				//Console.WriteLine($"Result: {result[i]}");
			}

			return result;
		}

		private double[] SolveTransposed()
		{
			double[,] tempMat = Matrix;
			Matrix = GetTransposedMatrix();
			double[] tempF = F;
			F = F.Reverse().ToArray();

			//Print();

			double[] result = Solve();
			Matrix = tempMat;
			F = tempF;
			return result.Reverse().ToArray();
		}

		private double[,] GetTransposedMatrix()
		{
			double[,] transpMatr = new double[N, L];
			double[] column = new double[N];

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