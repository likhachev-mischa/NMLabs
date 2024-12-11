using System.Reflection;
using NMLab2;

//LineMatrix matrix = new LineMatrix(@"C:\Programming\labs\NM\NMLab2\test.txt");
//matrix.Print();

//float[] a = new float[] { 1, 2, 3, 4 };

//float[] mul = matrix.Multiply(a);

//Console.WriteLine("MULTIPLICATION RES:");
//foreach (var f in mul)
//{
//	Console.Write(f + " ");
//}


//float[] result = matrix.SolveFull();
//Console.WriteLine("-----------------");
//Console.WriteLine("RESULT:");
//foreach (var f in result)
//{
//	Console.Write(f + " ");
//}
Random random = new Random(42);

for (int i = 1; i <= 2; ++i)
{
	int N = (int)MathF.Pow(10, i) * (int)random.NextInt64(1, 10);
	int L = N / 10;

	LineMatrix matrix = new LineMatrix(N, L);

}