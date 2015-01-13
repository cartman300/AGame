using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace AGame.Utils {
	static class ArrayExtension {
		public static T[] Resize<T>(this T[] A, int NewSize, bool Copy = false) {
			if (!Copy && A.Length == NewSize)
				return A;

			T[] B = new T[NewSize];
			for (int i = 0; i < NewSize; i++)
				if (i < A.Length)
					B[i] = A[i];
			return B;
		}

		public static void Insert<T>(this T[] A, int Idx, T[] B) {
			for (int i = 0; i < B.Length; i++)
				A[Idx + i] = B[i];
		}
	}

	static class StringExtensions {
		public static string[] SplitByLen(this string Str, int Len) {
			string[] Res = new string[Str.Length / Len + 1];
			for (int n = 0; n < Res.Length; n++)
				if (n + 1 >= Res.Length)
					Res[n] = Str.Substring(n * Len);
				else
					Res[n] = Str.Substring(n * Len, Len);
			return Res;
		}
	}

	static unsafe class PointerExtensions {
		public static void* Fill(this IntPtr IPtr, int Len, byte Val = 0) {
			void* Ptr = IPtr.ToPointer();
			for (int i = 0; i < Len; i++)
				*(byte*)Ptr = Val;
			return Ptr;
		}
	}

	static class VectorExtensions {
		public static Vector3 Transform(this Vector3 Vec, Matrix4 Mat) {
			return Vector3.Transform(Vec, Mat);
		}
	}
}