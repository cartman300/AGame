using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ILM = System.Drawing.Imaging.ImageLockMode;
using IPixelFormat = System.Drawing.Imaging.PixelFormat;
using BData = System.Drawing.Imaging.BitmapData;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace AGame.Src.OGL {
	class Texture : GLObject {
		public enum Wrap {
			S = TextureParameterName.TextureWrapS,
			T = TextureParameterName.TextureWrapT,
			R = TextureParameterName.TextureWrapR,
			X = S,
			Y = T,
			Z = R
		}

		public enum WrapMode {
			Repeat = All.Repeat,
			MirroredRepeat = All.MirroredRepeat,
			ClampToEdge = All.ClampToEdge,
			ClampToBorder = All.ClampToBorder
		}

		public enum Filter {
			MinFilter = All.TextureMinFilter,
			MagFilter = All.TextureMagFilter,
			DownScaled = MinFilter,
			UpScaled = MagFilter
		}

		public enum FilterMode {
			Nearest = All.Nearest,
			Linear = All.Linear,
			NearestMipmapNearest = All.NearestMipmapNearest,
			LinearMipmapNearest = All.LinearMipmapNearest,
			LinearMipmapLinear = All.LinearMipmapLinear,
			NearestMipmapLinear = All.NearestMipmapLinear
		}

		public int TexParameter(int Name, int Param) {
			int P = Param;
			GL.TexParameterI(Target, (TextureParameterName)Name, ref P);
			return P;
		}

		public void TexParameter(int Name, float[] Param) {
			GL.TexParameter(Target, (TextureParameterName)Name, Param);
		}

		public int Wrapping(Wrap W, WrapMode M) {
			return TexParameter((int)W, (int)M);
		}

		public int Filtering(Filter F, FilterMode M) {
			return TexParameter((int)F, (int)M);
		}

		public void BorderColor(Color4 BorderColor) {
			TexParameter((int)TextureParameterName.TextureBorderColor,
				new float[] { BorderColor.R, BorderColor.G, BorderColor.B, BorderColor.A });
		}

		public TextureTarget Target;
		public int Width;
		public int Height;

		public Texture(TextureTarget Target) {
			ID = GL.GenTexture();
			this.Target = Target;
		}

		public static Texture FromFile(string Path, Texture.FilterMode FMode = FilterMode.LinearMipmapLinear) {
			Texture Atlas = new Texture(TextureTarget.Texture2D);
			Atlas.BindTo(TextureUnit.Texture0);
			Atlas.Wrapping(Texture.Wrap.X, Texture.WrapMode.Repeat);
			Atlas.Wrapping(Texture.Wrap.Y, Texture.WrapMode.Repeat);
			Atlas.Filtering(Texture.Filter.DownScaled, FMode);
			Atlas.Filtering(Texture.Filter.UpScaled, FMode);
			Atlas.Load(Bitmap.FromFile(Path));
			Atlas.GenerateMipmap();
			Atlas.Unbind();
			return Atlas;
		}

		public override void Delete() {
			GL.DeleteTexture(ID);
		}

		public override void Bind() {
			GL.BindTexture(Target, ID);
		}

		public void BindTo(TextureUnit Unit) {
			GL.ActiveTexture(Unit);
			Bind();
		}

		public override void Unbind() {
			GL.BindTexture(Target, 0);
			GL.ActiveTexture(TextureUnit.Texture0);
		}

		public void GenerateMipmap() {
			GL.GenerateMipmap((GenerateMipmapTarget)Target);
		}

		public void TexImage2D(int Level, PixelInternalFormat PIF, int W, int H, PixelFormat PF, PixelType PT, IntPtr Data) {
			Width = W;
			Height = H;
			GL.TexImage2D(Target, Level, PIF, W, H, 0, PF, PT, Data);
		}

		public void TexImage2D(int Level, PixelInternalFormat PIF, int W, int H, PixelFormat PF) {
			TexImage2D(Level, PIF, W, H, PF, PixelType.UnsignedByte, IntPtr.Zero);
		}

		public void TexSubImage2D(int Level, int X, int Y, int W, int H, PixelFormat PF, PixelType PT, IntPtr Data) {
			GL.TexSubImage2D(Target, Level, X, Y, W, H, PF, PT, Data);
		}

		public void TexImage3D(int Level, PixelInternalFormat PIF, int W, int H, int D, PixelFormat PF, PixelType PT, IntPtr Data) {
			Width = W;
			Height = H;
			GL.TexImage3D(Target, Level, PIF, W, H, D, 0, PF, PT, Data);
		}

		public void TexImage3D(int Level, PixelInternalFormat PIF, int W, int H, int D, PixelFormat PF) {
			TexImage3D(Level, PIF, W, H, D, PF, PixelType.UnsignedByte, IntPtr.Zero);
		}

		public void Load(Bitmap Bmp) {
			BData BDta = Bmp.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ILM.ReadOnly, IPixelFormat.Format32bppArgb);
			TexImage2D(0, PixelInternalFormat.Rgba, Bmp.Width, Bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, BDta.Scan0);
			Bmp.UnlockBits(BDta);
		}

		public void Load(Image Img) {
			Load(new Bitmap(Img));
		}

		public void Load(string Path) {
			Load(new Bitmap(Path));
		}
	}
}