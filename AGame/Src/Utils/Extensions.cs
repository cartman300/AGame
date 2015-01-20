using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using AGame.Src.OGL;

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

	static class FontExtensions {
		public static Src.OGL.Texture GetAtlas(this Flib.Font Fnt) {
			if (Fnt.Userdata == null) {
				Texture Atlas = new Texture(TextureTarget.Texture2D);
				Atlas.BindTo(TextureUnit.Texture0);
				Atlas.Wrapping(Texture.Wrap.X, Texture.WrapMode.ClampToEdge);
				Atlas.Wrapping(Texture.Wrap.Y, Texture.WrapMode.ClampToEdge);
				Atlas.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.LinearMipmapLinear);
				Atlas.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.LinearMipmapLinear);
				Atlas.Load(Fnt.FontAtlas);
				Fnt.FontAtlas.Save("file.png");
				Atlas.GenerateMipmap();
				Fnt.Userdata = Atlas;
			}
			return (Texture)Fnt.Userdata;
		}
	}

	static class NumberExtensions {
		public static float GridClamp(this float F, float GridSize) {
			return (float)Math.Round(F / GridSize) * GridSize;
		}
	}

	static class VectorExtensions {
		public static Vector4 ToVec4(this Color4 Clr) {
			return new Vector4(Clr.R, Clr.G, Clr.B, Clr.A);
		}

		public static Vector3 Transform(this Vector3 Vec, Matrix4 Mat) {
			return Vector3.Transform(Vec, Mat);
		}

		public static Vector3 GridClamp(this Vector3 Vec, Vector3 GridSize) {
			return new Vector3(Vec.X.GridClamp(GridSize.X), Vec.Y.GridClamp(GridSize.Y), Vec.Z.GridClamp(GridSize.Z));
		}
	}
}