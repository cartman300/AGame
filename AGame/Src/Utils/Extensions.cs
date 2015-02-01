using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using PF = System.Drawing.Imaging.PixelFormat;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using AGame.Src.OGL;

namespace AGame.Utils {
	unsafe static class AssimpExtensions {
		public static Vector3 ToVector3(this Assimp.Vector3D V) {
			return new Vector3(V.X, V.Y, V.Z);
		}

		public static Vector2 ToVector2(this Assimp.Vector2D V) {
			return new Vector2(V.X, V.Y);
		}

		public static Texture.WrapMode ToWrap(this Assimp.TextureWrapMode M) {
			switch (M) {
				case Assimp.TextureWrapMode.Clamp:
					return Texture.WrapMode.ClampToEdge;
				case Assimp.TextureWrapMode.Decal:
					return Texture.WrapMode.MirroredRepeat;
				case Assimp.TextureWrapMode.Mirror:
					return Texture.WrapMode.MirroredRepeat;
				case Assimp.TextureWrapMode.Wrap:
					return Texture.WrapMode.Repeat;
				default:
					throw new Exception("Unsupported: " + M);
			}
		}

		public static Color4 ToColor(this Assimp.Color4D C) {
			return new Color4(C.R, C.B, C.G, C.A);
		}

		public static Matrix4 ToMatrix(this Assimp.Matrix4x4 Mat) {
			Matrix4 M = new Matrix4();
			M.M11 = Mat.A1;
			M.M12 = Mat.A2;
			M.M13 = Mat.A3;
			M.M14 = Mat.A4;
			M.M21 = Mat.B1;
			M.M22 = Mat.B2;
			M.M23 = Mat.B3;
			M.M24 = Mat.B4;
			M.M31 = Mat.C1;
			M.M32 = Mat.C2;
			M.M33 = Mat.C3;
			M.M34 = Mat.C4;
			M.M41 = Mat.D1;
			M.M42 = Mat.D2;
			M.M43 = Mat.D3;
			M.M44 = Mat.D4;
			return M;
		}

		public static void DecomposeToMesh(this Assimp.Matrix4x4 Trans, Src.Meshes.Mesh Msh) {
			Assimp.Vector3D Scaling, Translation;
			Assimp.Quaternion Rotation;
			Trans.Decompose(out Scaling, out Rotation, out Translation);

			Msh.Scale = Scaling.ToVector3();
			Msh.Position = Translation.ToVector3();
			Msh.Rotation = Rotation.ToQuaternion();
		}

		public static Quaternion ToQuaternion(this Assimp.Quaternion Q) {
			return new Quaternion(Q.X, Q.Y, Q.Z, Q.W);
		}

		static bool ExistsWith(string Pth, string Ex) {
			return Src.FSys.Exists(Pth + Ex);
		}

		public static Texture ToTexture(this Assimp.TextureSlot TexSlot) {
			string FName = TexSlot.FilePath;
			if (!FName.Contains('.')) {
				if (ExistsWith(FName, ".jpg"))
					FName += ".jpg";
				else if (ExistsWith(FName, ".png"))
					FName += ".png";
				else if (ExistsWith(FName, ".tga"))
					FName += ".tga";
			}
			Texture T = Texture.FromFile(FName);
			T.Use(() => {
				T.Filtering(Texture.Filter.MinFilter, Texture.FilterMode.LinearMipmapLinear);
				T.Filtering(Texture.Filter.MagFilter, Texture.FilterMode.LinearMipmapLinear);
				T.Wrapping(Texture.Wrap.X, TexSlot.WrapModeU.ToWrap());
				T.Wrapping(Texture.Wrap.Y, TexSlot.WrapModeV.ToWrap());
				T.GenerateMipmap();
			});
			return T;
		}

		public static Bitmap ToBitmap(this Assimp.EmbeddedTexture Tex) {
			if (Tex.CompressedData != null)
				return (Bitmap)Bitmap.FromStream(new MemoryStream(Tex.CompressedData));
			else {
				Bitmap Bmp = new Bitmap(Tex.Width, Tex.Height);
				/*BitmapData BDta = Bmp.LockBits(new Rectangle(new Point(0, 0), new Size(Bmp.Width, Bmp.Height)),
					ImageLockMode.WriteOnly, PF.Format32bppArgb);*/
				int i = 0;
				for (int x = 0; x < Bmp.Width; x++)
					for (int y = 0; y < Bmp.Height; y++) {
						Assimp.Texel T = Tex.NonCompressedData[i++];
						Bmp.SetPixel(x, y, Color.FromArgb(T.A, T.R, T.G, T.B));
					}
				return Bmp;
			}
		}

		public static Texture ToTexture(this Assimp.EmbeddedTexture Tex, Assimp.TextureSlot TexSlot) {
			Texture T = new Texture(TextureTarget.Texture2D);
			T.BindTo(TextureUnit.Texture0);
			T.Filtering(Texture.Filter.MinFilter, Texture.FilterMode.LinearMipmapLinear);
			T.Filtering(Texture.Filter.MagFilter, Texture.FilterMode.LinearMipmapLinear);
			T.Wrapping(Texture.Wrap.X, TexSlot.WrapModeU.ToWrap());
			T.Wrapping(Texture.Wrap.Y, TexSlot.WrapModeV.ToWrap());

			if (Tex.CompressedData != null)
				T.Load(Bitmap.FromStream(new MemoryStream(Tex.CompressedData)));
			else {
				byte[] RGBA888 = new byte[4 * Tex.Width * Tex.Height];
				for (int i = 0, j = 0; i < Tex.NonCompressedDataSize * 4; i += 4, j++) {
					Assimp.Texel Texel = Tex.NonCompressedData[j];
					RGBA888[i] = Texel.R;
					RGBA888[i + 1] = Texel.G;
					RGBA888[i + 2] = Texel.B;
					RGBA888[i + 3] = Texel.A;
				}

				fixed (byte* RGBA888Ptr = RGBA888)
					T.TexImage2D(0, PixelInternalFormat.Rgba, Tex.Width, Tex.Height,
						PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(RGBA888Ptr));
			}

			T.GenerateMipmap();
			T.Unbind();
			return T;
		}
	}

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

	static class ColorExtensions {
		public static Color4 Mult(this Color4 A, Color4 B) {
			return new Color4(A.R * B.R, A.G * B.G, A.B * B.B, A.A * B.A);
		}
	}

	static class BinaryExtensions {
		public static void Write(this BinaryWriter BW, Vector3 Vec) {
			BW.Write(Vec.X);
			BW.Write(Vec.Y);
			BW.Write(Vec.Z);
		}

		public static void Write(this BinaryWriter BW, Vector2 Vec) {
			BW.Write(Vec.X);
			BW.Write(Vec.Y);
		}

		public static void Write(this BinaryWriter BW, Color4 Clr) {
			BW.Write(Clr.R);
			BW.Write(Clr.G);
			BW.Write(Clr.B);
			BW.Write(Clr.A);
		}

		public static void Write(this BinaryWriter BW, Quaternion Q) {
			BW.Write(Q.X);
			BW.Write(Q.Y);
			BW.Write(Q.Z);
			BW.Write(Q.W);
		}

		public static Vector3 ReadVector3(this BinaryReader BR) {
			return new Vector3(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
		}

		public static Vector2 ReadVector2(this BinaryReader BR) {
			return new Vector2(BR.ReadSingle(), BR.ReadSingle());
		}

		public static Color4 ReadColor4(this BinaryReader BR) {
			return new Color4(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
		}

		public static Quaternion ReadQuaternion(this BinaryReader BR) {
			return new Quaternion(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
		}
	}
}