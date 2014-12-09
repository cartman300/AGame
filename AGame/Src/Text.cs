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
	class Text {
		[Flags]
		public enum Alignment {
			Top = 1 << 1,
			Bottom = 1 << 2,
			Left = 1 << 3,
			Right = 1 << 4,

			CenterX = Left | Right,
			CenterY = Top | Bottom
		}

		static PrivateFontCollection PFC = new PrivateFontCollection();

		public static Font LoadFont(string Pth, string Name, float Size) {
			PFC.AddFontFile(Pth);
			FontFamily FF = new FontFamily(Path.GetFileNameWithoutExtension(Pth), PFC);
			return new Font(FF, Size);
		}

		Font F;
		public GraphicsTexture Tex;

		public Color ForeColor;
		public Color BackColor;
		public bool FillBackground;

		Quad2D TextQuad;

		public Text(Font Fnt, int W, int H) {
			F = Fnt;
			ForeColor = Color.White;
			BackColor = Color.Black;
			FillBackground = false;
			Tex = new GraphicsTexture(W, H);
			TextQuad = new Quad2D(new Vector2(-1, -1), new Vector2(2, 2));
		}

		public Vector2 MeasureString(string Str) {
			return Tex.MeasureString(Str, F);
		}

		public void Print(string Str, Vector2 Pos) {
			if (FillBackground)
				Tex.FillRectangle(BackColor, Pos, Tex.MeasureString(Str, F));
			Tex.DrawString(Str, F, ForeColor, Pos);
		}

		public void Print(string Str) {
			Print(Str, Vector2.Zero);
		}

		public void Print(string Str, Alignment A) {
			float W = Tex.W;
			float H = Tex.H;
			Vector2 Sz = MeasureString(Str);
			Vector2 Pos = new Vector2(W / 2, H / 2);

			if (A.HasFlag(Alignment.Top) && !A.HasFlag(Alignment.Bottom))
				Pos.Y = 0;
			if (A.HasFlag(Alignment.Bottom) && !A.HasFlag(Alignment.Top))
				Pos.Y = H - Sz.Y;
			if (A.HasFlag(Alignment.Left) && !A.HasFlag(Alignment.Right))
				Pos.X = 0;
			if (A.HasFlag(Alignment.Right) && !A.HasFlag(Alignment.Left))
				Pos.X = W - Sz.X;

			Print(Str, Pos);
		}

		public void Clear(Color Clr) {
			Tex.Clear(Clr);
		}

		public void Render() {
			Tex.Bind();
			TextQuad.Render();
		}
	}
}