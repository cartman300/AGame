using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGame.Src {
	static class Rndom {
		static Random R = new Random();

		public static int Int {
			get {
				return R.Next(int.MinValue, int.MaxValue);
			}
		}

		public static byte Byte {
			get {
				return (byte)R.Next(byte.MinValue, byte.MaxValue);
			}
		}

		public static sbyte SByte {
			get {
				return (sbyte)R.Next(sbyte.MinValue, sbyte.MaxValue);
			}
		}

		public static char Char {
			get {
				return (char)R.Next(char.MinValue, char.MaxValue);
			}
		}
	}
}