using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;

namespace AGame.Src.OGL {
	static class GraphicsExtensions {
		public static void SetGLTextQuality(this Graphics Gfx) {
			Gfx.SmoothingMode = SmoothingMode.None;
			Gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
			Gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Gfx.CompositingMode = CompositingMode.SourceOver;
			Gfx.CompositingQuality = CompositingQuality.HighQuality;
		}
	}

	class GraphicsTexture {
		bool Dirty;

		public void SetDirty() {
			Dirty = true;
		}

		void ResolveDirty() {
			if (Dirty) {
				Dirty = false;
				Gfx.Flush(FlushIntention.Sync);
				CreateTexture();
			}
		}

		Bitmap Bmp;
		public Graphics Gfx;
		public Texture Texture;

		public int W {
			get {
				return Bmp.Width;
			}
		}

		public int H {
			get {
				return Bmp.Height;
			}
		}

		void CreateTexture() {
			if (Texture != null)
				Texture.Delete();
			Texture = new Texture(TextureTarget.Texture2D);
			Texture.BindTo(TextureUnit.Texture0);
			Texture.Wrapping(Texture.Wrap.X, Texture.WrapMode.ClampToEdge);
			Texture.Wrapping(Texture.Wrap.Y, Texture.WrapMode.ClampToEdge);
			Texture.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Linear);
			Texture.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Linear);
			Texture.Load(Bmp);
		}

		public GraphicsTexture(int W, int H) {
			Bmp = new Bitmap(W, H);
			Gfx = Graphics.FromImage(Bmp);
			Dirty = true;
		}

		public void Resize(int H) {
			Resize(Bmp.Width, H);
		}

		public void Resize(int W, int H) {
			Bitmap NewBmp = new Bitmap(W, H);
			using (Graphics Gfx = Graphics.FromImage(NewBmp))
				Gfx.DrawImageUnscaled(Bmp, 0, H - Bmp.Height);
			Bmp = NewBmp;
			Dirty = true;
		}

		public void Clear(Color Clr) {
			Gfx.Clear(Clr);
			Dirty = true;
		}

		public void SetBitmap(Bitmap Bmp) {
			this.Bmp = Bmp;
			Dirty = true;
		}

		public Vector2 MeasureString(string Str, Font Fnt) {
			SizeF Sz = Gfx.MeasureString(Str, Fnt);
			return new Vector2(Sz.Width, Sz.Height);
		}

		public void DrawString(string Str, Font Fnt, Color Clr, Vector2 Pos) {
			Gfx.SetGLTextQuality();
			Gfx.DrawString(Str, Fnt, new SolidBrush(Clr), Pos.X, Pos.Y);
			Dirty = true;
		}

		public void Draw(GraphicsTexture Tex, Vector2 Pos) {
			Gfx.DrawImage(Tex.Bmp, Pos.X, Pos.Y);
		}

		public void Draw(GraphicsTexture Tex) {
			Draw(Tex, Vector2.Zero);
		}

		public void FillRectangle(Color Clr, Vector2 Pos, Vector2 Size) {
			Gfx.FillRectangle(new SolidBrush(Clr), Pos.X, Pos.Y, Size.X, Size.Y);
			Dirty = true;
		}

		public void Bind() {
			ResolveDirty();
			Texture.Bind();
		}

		public void BindTo(TextureUnit Unit) {
			ResolveDirty();
			Texture.BindTo(Unit);
		}
	}
}