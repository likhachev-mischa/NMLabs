using NMLab2;

double[] GenerateRandomArray(Random random, int n)
{
	double[] result = new double[n];

	for (int i = 0; i < n; i++)
	{
		result[i] = random.NextDouble() * random.NextInt64(-10, 10);
	}

	return result;
}

double GetRelativeError(double[] result, double[] f, double eps = double.Epsilon)
{
	if (result.Length != f.Length)
	{
		throw new Exception("Incorrect result size");
	}

	int n = result.Length;

	double max = double.MinValue;
	for (int i = 0; i < n; ++i)
	{
		double diff = Math.Abs(f[i] - result[i]);
		if (f[i] > eps)
		{
			diff /= f[i];
		}

		if (diff > max)
		{
			max = diff;
		}
	}

	return max;
}

Random random = new Random(22);

{
	//LineMatrix matrix = new LineMatrix(@"C:\Programming\labs\NM\NMLab2\test.txt");
	//double[] randomF = GenerateRandomArray(random, 4);
	//Console.WriteLine("TEST MATRIX:");
	//matrix.Print();
	//matrix.F = matrix.Multiply(randomF);
	//var res = matrix.SolveFull();
	//Console.WriteLine("---------------");
	//Console.WriteLine(GetRelativeError(res, randomF));
}

Console.WriteLine("RANDOM: ");
for (int i = 1; i <= 2; ++i)
{
	int n = (int)Math.Pow(10, i) * (int)random.NextInt64(1, 11);
	int l = n / 10;
	LineMatrix matrix = new LineMatrix(n, l);
	matrix.GenerateRandom(random);
	double[] randomF = GenerateRandomArray(random, n);
	matrix.F = matrix.Multiply(randomF);
	var res = matrix.SolveFull();
	Console.WriteLine("---------------");
	Console.WriteLine($"N: {n} L: {l}");
	Console.WriteLine(GetRelativeError(res, randomF));
}

Console.WriteLine("---------------");
Console.WriteLine("---------------");
Console.WriteLine("WELL CONDITIONED: ");
for (int i = 1; i <= 2; ++i)
{
	for (int j = 0; j < 2; ++j)
	{
		int n = (int)Math.Pow(10, i) * (int)random.NextInt64(1, 11);
		int l = n;
		LineMatrix matrix = new LineMatrix(n, l);
		matrix.GenerateRandom(random);
		double[] randomF = GenerateRandomArray(random, n);
		matrix.F = matrix.Multiply(randomF);
		var res = matrix.SolveFull();
		Console.WriteLine("---------------");
		Console.WriteLine($"N: {n}");
		Console.WriteLine(GetRelativeError(res, randomF));
	}
}

Console.WriteLine("---------------");
Console.WriteLine("---------------");
Console.WriteLine("ILL CONDITIONED: ");
for (int i = 1; i <= 2; ++i)
{
	int n = (int)Math.Pow(10, 1) * (int)random.NextInt64(1, 11);
	int l = n;
	for (int j = 2; j <= 6; j += 2)
	{
		LineMatrix matrix = new LineMatrix(n, l);
		matrix.GenerateIllConditioned(random, j);
		double[] randomF = GenerateRandomArray(random, n);
		matrix.F = matrix.Multiply(randomF);
		var res = matrix.SolveFull();
		Console.WriteLine("---------------");
		Console.WriteLine($"N: {n} k: {j}");
		Console.WriteLine(GetRelativeError(res, randomF));
	}
}