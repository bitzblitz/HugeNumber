using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugeNumbers
	{
	/// <summary>
	/// This class uses a base(-2) encoding to represent integers of arbitrary size.
	/// The use of a negative base allows for encodings that do not need a "sign bit"
	/// and therefor don't roll over at an upper or lower limit.
	/// The numbers are encoded in powers of -2 starting with the low order bit.
	/// Max/min value is limited by the number of bits that can be stored in a int indexed
	/// List<bool>, or 2^int.MaxValue+2^(int.MaxValue-2)+... which is pretty huge.
	/// .NET uses actual bools in its list but C++ has a vector<bool> specialization
	/// that uses actual bits. So the .NET implementation is not actually practical.
	/// Large number math can be done similarly with lists based on other bases, but
	/// base -2 gives some algorithmic simplifications that make it easier to code 
	/// and understand.
	/// </summary>
	public class HugeNumber:List<bool>, IComparable<long>, IComparable<HugeNumber>
		{
		#region Construction and initialization
		/// <summary>
		/// Conversion constructor. Turns an array of int into an HugeNumber.
		/// </summary>
		/// <param name="Value">An array of 1s and 0s representing bits.</param>
		public HugeNumber(int[] Value)
			{
			foreach(int b in Value)
				Add(b != 0);
			}

		/// <summary>
		/// Default constructor. Necessary because we need the other constructors.
		/// </summary>
		public HugeNumber()
			{ }

		/// <summary>
		/// Conversion constructor. Turns a long into an HugeNumber.
		/// </summary>
		/// <param name="Value">The value to encode in base(-2).</param>
		public HugeNumber(long Value)
			{
			FromLong(Value);
			}

		/// <summary>
		/// Copy constructor makes a copy of the source number to give value like behaviour.
		/// </summary>
		/// <param name="Source">Another number to copy.</param>
		public HugeNumber(HugeNumber Source)
			{
			foreach(bool b in Source)
				Add(b);
			}
		/// <summary>
		/// Converts an HugeNumber to a conventional long.
		/// </summary>
		/// <returns>A long int with the equivalent value, if it will fit.</returns>
		public long ToLong()
			{
			if(Count > 65 && this.Skip(sizeof(long)).Any(b => b)) // long.MaxValue takes 65 bits.
				throw new ArgumentOutOfRangeException();
			long result = 0;
			long bit_value = 1L;
			for(int b = 0;b < Count;++b)
				{
				result += this[b] ? bit_value : 0;
				bit_value *=-2;
				}
			return result;
			}

		/// <summary>
		/// Initialize from a conventional long integer.
		/// </summary>
		/// <param name="Value">The value to encode.</param>
		public void FromLong(long Value)
			{
			Clear();
			while(Value != 0)
				{
				long rem = 0;	// get the remainder as and out parameter.
				Value = Math.DivRem(Value, -2 , out rem);	// do divide and modulo at the same time for efficiency.
				if(rem < 0)
					++Value;
				Add(rem != 0);
				}
			}

		/// <summary>
		/// A way to remove superfluous leading zeros that just take up space.
		/// </summary>
		/// <returns>A representation with the minimum number of terms.</returns>
		public HugeNumber Trim()
			{
			int len = Count-1;
			while(len >= 0 && !this[len])
				RemoveAt(len--);  // remove high order 0's
			return this;
			}

		#endregion  Construction and initialization

		/// <summary>
		/// Create a human readable representation.
		/// </summary>
		/// <returns>A string of 1s and 0s representing the base(-2) encoding, with low order first.</returns>
		public override string ToString()
			{
			StringBuilder text = new StringBuilder();
			ForEach(b => text.Append(b ? "1" : "0"));
			return text.ToString();
			}

		#region Mathematical operations
		/// <summary>
		/// Change the sign of the number.
		/// </summary>
		/// <returns>A new HugeNumber with the opposite sign.</returns>
		public HugeNumber Negate()
			{
			HugeNumber negated = new HugeNumber();
			for(int b = 0;b < Count;++b)
				{
				if(this[b])
					{
					negated.Add(true);
					if(Count <= ++b)
						negated.Add(true);	// insert a bit
					else
						negated.Add(!this[b]); // compliment the next highest bit
					}
				else
					negated.Add(false); // 0 stays 0
				}
			return negated.Trim();
			}

		/// <summary>
		/// Adds a bit at bit position B. Note that this is and iterative approach to
		/// avoid the deep recursion that might occur in long carry scenarios. 
		/// It should also be faster by avoiding function calls for simple operations.
		/// </summary>
		/// <param name="B">The bit position at which to increment.</param>
		/// <returns>true if a carry is required. Caries happen 2 bit positions up.</returns>
		protected bool AddBit(int B)
			{
			int pad = B - Count + 1;
			while(pad-- > 0)
				Add(false);
			if(!this[B])
				{
				this[B] = true; // increment this bit
				return false; // no carry.
				}
			else
				{
				if(Count == B + 1)
					Add(false);
				this[B] = false;  // remove +bit_value
				if(this[B + 1])
					{
					this[B + 1] = false;  // also remove -2*bit_value
					return false; // no carry
					}
				else
					{
					this[B + 1] = true; // add -2*bit_value
					if(Count == B + 2)
						{
						Add(true); // make room for carry
						return false; // no need to carry
						}
					return true;  // need to carry on B+2 position to add +4*bit_value
					}
				}
			}

		/// <summary>
		/// Adds this HugeNumber another to produce a third which is the mathematical sum
		/// of this and the other.
		/// </summary>
		/// <param name="Other">The number to add to our value.</param>
		/// <returns>A new HugeNumber with the value of us plus the other.</returns>
		public HugeNumber Add(HugeNumber Other)
			{
			HugeNumber result = new HugeNumber(this);
			for(int b = 0;b < Other.Count;++b)
				{
				if(Other[b])
					{
					int bit = b;
					while(result.AddBit(bit))
						bit += 2;  // carry required.
					}
				}
			return result.Trim();
			}

		public static HugeNumber operator +(HugeNumber Left, HugeNumber Right)
			{
			return Left.Add(Right);
			}

		/// <summary>
		/// Subtracts a bit at bit position B. Note that this is and iterative approach to
		/// avoid the deep recursion that might occur in long borrow scenarios. 
		/// It should also be faster by avoiding function calls for simple operations.
		/// </summary>
		/// <param name="B">The bit position at which to decrement.</param>
		/// <returns>true if a borrow is required. Borrows happen 2 bit positions up.</returns>
		protected bool SubractBit(int B)
			{
			int pad = B - Count + 1;
			while(pad-- > 0)
				Add(false);
			if(this[B])
				{
				this[B] = false; // decrement this bit
				return false; // no carry.
				}
			else
				{
				if(Count == B + 1)
					Add(false);
				if(!this[B + 1])
					{
					this[B] = true;  // add this bit and
					this[B + 1] = true;  // also add next higher bit_value
					return false; // no carry
					}
				else
					{
					this[B] = true;  // add this bit
					this[B + 1] = false; // add -2*bit_value
					if(Count == B + 2)
						{
						return false;	// no need to no need to borrow
						}
					return true;  // need to borrow on B+2 position to add +4*bit_value
					}
				}
			}

		/// <summary>
		/// Subtracts another HugeNumber from this.
		/// This could be done as a negate and add, but direct subtract is more efficient in time and space.
		/// </summary>
		/// <param name="Other">The number to subtract from me.</param>
		/// <returns>A new HugeNumber representing the difference between me and the Other.</returns>
		public HugeNumber Subtract(HugeNumber Other)
			{
			HugeNumber result = new HugeNumber(this);
			for(int b = 0;b < Other.Count;++b)
				{
				if(Other[b])
					{
					int bit = b;
					while(result.SubractBit(bit))
						bit += 2;  // borrow required.
					}
				}
			return result.Trim();
			}

		public static HugeNumber operator -(HugeNumber Left, HugeNumber Right)
			{
			return Left.Subtract(Right);
			}

		#endregion Mathematical operaitons

		#region Comparisons
		public int CompareTo(long other)
			{
			return CompareTo(new HugeNumber(other));  // up converting avoids OoR exceptions.
			}

		/// <summary>
		/// Compare two HugeNumbers. Starts with high order bits and quits as soon as a difference is found.
		/// </summary>
		/// <param name="other">THe HugeNumber to compare ourselves to.</param>
		/// <returns>-1 if we are less than other, 0 if we are equal, 1 if we are greater.</returns>
		public int CompareTo(HugeNumber other)
			{
			int compare = 0;
			if(other.Count > Count)
				{
				int b = other.Count-1;
				while(b >= Count)
					if(other[b])
						{
						compare = (b & 1) == 1 ? 1 : -1;
						return compare;
						}
				}
			else
				{
				int b = Count - 1;
				while(b >= other.Count)
					if(this[b])
						{
						compare = (b & 1) == 1 ? -1 : 1;
						return compare;
						}
				}
			for(int b = Count-1;b >= 0 && compare == 0;--b)
				{
				if(this[b] == other[b])
					continue; // matching bits don't affect comparison.
				if((b&1) == 0)
					compare = this[b] ? 1 : -1;
				else
					compare = this[b] ? -1 : 1;
				}
			return compare;
			}

		/// <summary>
		/// Value Comparison for "is less than".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is less than RHS.</returns>
		public static bool operator <(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) < 0;
			}

		/// <summary>
		/// Value Comparison for "is greater than".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is greater than RHS.</returns>
		public static bool operator >(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) > 0;
			}

		/// <summary>
		/// Value Comparison for "is equal to".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is equal to RHS.</returns>
		public static bool operator ==(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) == 0;
			}

		/// <summary>
		/// Value Comparison for "is NOT equal to".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is NOT equal to RHS.</returns>
		public static bool operator !=(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) != 0;
			}

		/// <summary>
		/// Value Comparison for "is less than or equal to".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is less than or equal to RHS.</returns>
		public static bool operator <=(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) <= 0;
			}

		/// <summary>
		/// Value Comparison for "is greater than or equal to".
		/// </summary>
		/// <param name="Left">LHS</param>
		/// <param name="Right">RHS</param>
		/// <returns>true if LHS is greater than or equal to RHS.</returns>
		public static bool operator >=(HugeNumber Left, HugeNumber Right)
			{
			return Left.CompareTo(Right) >= 0;
			}
		#endregion Comparisons
		}
	}
