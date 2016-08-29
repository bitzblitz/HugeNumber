using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugeNumbers
	{
	class Program
		{
		static void Main(string[] args)
			{
			Console.WriteLine("Using a list of bits representing powers of -2 one can represent any number in base(-2):");
			for(long number = -16;number <= 16;++number)
				{
				HugeNumber n = new HugeNumber(number);
				Console.WriteLine($"Number: {number} in base(-2): {n}");
				}
			Console.WriteLine("Bits are ordered from low to high.\n This allows the representation of large numbers with NO ROLL OVER or sign bit.");
			HugeNumber max_long = new HugeNumber(long.MaxValue);
			Console.WriteLine($"Max long:\n Original: {int.MaxValue} ({max_long.ToString()})\n Negated: {max_long.Negate()} ({max_long.Negate().ToLong()}) Length {max_long.Count}");
			HugeNumber min_long = new HugeNumber(long.MinValue);
			Console.WriteLine($"Min long\n: Original: {int.MinValue} ({min_long.ToString()})\n Negated: {min_long.Negate()} ({min_long.Negate().ToLong()}) Length {min_long.Count}");
			Console.WriteLine("Math can be done on the bit strings:");
			Console.WriteLine($"long.MaxValue + long.MaxValue = {max_long+max_long} base(-2)");
			HugeNumber diff = max_long + min_long;
			Console.WriteLine($"long.MaxValue + long.MonValue = {diff} base(-2) ({diff.ToLong()} in base 10)");
			Console.ReadLine();
			}
		}
	}

