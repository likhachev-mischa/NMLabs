static void JacobiRotationMethod(int N, double[,] A, double epsilon, int M, out int K, out double[] eigenvalues)
{
	K = 0;
	eigenvalues = new double[N];

	// Initialize eigenvalues with diagonal elements
	for (int i = 0; i < N; i++)
	{
		eigenvalues[i] = A[i, i];
	}

	// Perform rotations
	for (K = 0; K < M; K++)
	{
		// Find the largest off-diagonal element in upper triangle
		double maxOffDiagonal = 0.0;
		int p = 0, q = 0;

		for (int i = 0; i < N - 1; i++)
		{
			for (int j = i + 1; j < N; j++)
			{
				double absAij = Math.Abs(A[i, j]);
				if (absAij > maxOffDiagonal)
				{
					maxOffDiagonal = absAij;
					p = i;
					q = j;
				}
			}
		}

		// Check for convergence
		if (maxOffDiagonal < epsilon)
		{
			return;
		}

		// Compute rotation parameters
		double app = eigenvalues[p];
		double aqq = eigenvalues[q];
		double apq = A[p, q];

		double phi = 0.5 * Math.Atan2(2 * apq, aqq - app);
		double sinPhi = Math.Sin(phi);
		double cosPhi = Math.Cos(phi);

		// Update eigenvalues
		double appNew = app * cosPhi * cosPhi + aqq * sinPhi * sinPhi + 2 * apq * sinPhi * cosPhi;
		double aqqNew = app * sinPhi * sinPhi + aqq * cosPhi * cosPhi - 2 * apq * sinPhi * cosPhi;
		eigenvalues[p] = appNew;
		eigenvalues[q] = aqqNew;

		// Set the largest off-diagonal element to zero
		A[p, q] = 0.0;

		// Update the remaining elements
		for (int i = 0; i < N; i++)
		{
			if (i != p && i != q)
			{
				double aip = A[Math.Min(i, p), Math.Max(i, p)];
				double aiq = A[Math.Min(i, q), Math.Max(i, q)];

				double aipNew = aip * cosPhi + aiq * sinPhi;
				double aiqNew = -aip * sinPhi + aiq * cosPhi;

				A[Math.Min(i, p), Math.Max(i, p)] = aipNew;
				A[Math.Min(i, q), Math.Max(i, q)] = aiqNew;
			}
		}
	}
}


double[,] A = new double[,]
{
	{ 4.0, -2.0, 2.0 },
	{ -2.0, 5.0, 0.0 },
	{ 2.0, 0.0, 6.0 }
};

JacobiRotationMethod(3, A, 1e-15, 3, out int K, out double[] eigenvalues);
foreach (var eigenvalue in eigenvalues)
{
	Console.Write(eigenvalue + " ");
}