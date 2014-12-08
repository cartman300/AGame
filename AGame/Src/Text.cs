using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;

namespace AGame.Src {
	static class GraphicsExtensions {
		public static void SetGLQuality(this Graphics Gfx) {
			Gfx.SmoothingMode = SmoothingMode.None;
			Gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
			Gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Gfx.CompositingMode = CompositingMode.SourceOver;
			Gfx.CompositingQuality = CompositingQuality.HighQuality;
		}
	}

	class Text {
		static PrivateFontCollection PFC = new PrivateFontCollection();

		public static Font LoadFont(string Pth, string Name, float Size) {
			PFC.AddFontFile(Pth);
			FontFamily FF = new FontFamily(Path.GetFileNameWithoutExtension(Pth), PFC);
			return new Font(FF, Size);
		}

		bool Dirty;
		Font F;
		Bitmap TBmp;
		Texture T;

		public VAO StrVAO;
		public VBO StrPos;
		public VBO StrUV;

		public Text(Font Fnt, int W, int H) {
			StrVAO = new VAO(PrimitiveType.Quads);
			StrVAO.Bind();

			float Sz = 1.0f;
			StrPos = new VBO(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			StrPos.Data(new Vector2[] {
				new Vector2(-Sz, Sz),
				new Vector2(Sz, Sz),
				new Vector2(Sz, -Sz),
				new Vector2(-Sz, -Sz),
			});

			StrUV = new VBO(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
			StrUV.Data(new Vector2[] {
				new Vector2(0.0f, 0.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(1.0f, 1.0f),
				new Vector2(0.0f, 1.0f),
			});

			F = Fnt;

			TBmp = new Bitmap(W, H);
		}

		void CreateTexture(Bitmap Bmp) {
			if (T != null)
				T.Delete();
			T = new Texture(TextureTarget.Texture2D);
			T.BindTo(TextureUnit.Texture0);
			T.Wrapping(Texture.Wrap.X, Texture.WrapMode.ClampToEdge);
			T.Wrapping(Texture.Wrap.Y, Texture.WrapMode.ClampToEdge);
			T.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Linear);
			T.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Linear);
			T.Load(Bmp);
		}

		public void UpdateGraphics(Action<Graphics> A) {
			using (Graphics Gfx = Graphics.FromImage(TBmp)) {
				A(Gfx);
				Gfx.Flush(FlushIntention.Sync);
			}
			Dirty = true;
		}

		public Vector2 MeasureString(string Str) {
			using (Graphics Gfx = Graphics.FromImage(TBmp)) {
				SizeF Sz = Gfx.MeasureString(Str, F);
				return new Vector2(Sz.Width, Sz.Height);
			}	
		}

		public void Print(string Str, Color Clr, Vector2 Pos) {
			UpdateGraphics((Gfx) => {
				Brush ClrBrush = new SolidBrush(Clr);
				Gfx.SetGLQuality();
				Gfx.DrawString(Str, F, ClrBrush, Pos.X, Pos.Y);
			});
		}

		public void DrawBitmap(Bitmap Bmp) {
			TBmp = Bmp;
			Dirty = true;
		}

		public void Print(string Str, Color Clr) {
			Print(Str, Clr, Vector2.Zero);
		}

		public void PrintBG(string Str, Color Clr, Color BGClr, Vector2 Pos) {
			UpdateGraphics((Gfx) => {

			});
		}

		public void PrintBG(string Str, Color Clr, Color BGClr) {
			PrintBG(Str, Clr, BGClr, Vector2.Zero);
		}

		public void Clear(Color Clr) {
			UpdateGraphics((Gfx) => Gfx.Clear(Clr));
		}

		public void Render() {
			if (Dirty) {
				Dirty = false;
				CreateTexture(TBmp);
			}

			StrVAO.Bind();
			T.BindTo(TextureUnit.Texture0);
			StrVAO.DrawArrays(0, 4);
		}
	}
}