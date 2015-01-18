using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;
using Flib;
using Font = Flib.Font;

namespace AGame.Src {
	class ColoredText {
		const int CharSize = 4;

		Font F;
		Texture FntAtlas;
		ShaderProgram Shader;

		VAO TxtVAO;
		VBO TxtPos, TxtUVs, TxtClr;

		Vector2[] Pos, UVs;
		Vector4[] Clr;

		bool Dirty;
		char[,] Chars;
		Color4[,] Colors;
		int Width, Height;

		public int X, Y;

		public ColoredText(Font Fnt, int W, int H, BufferUsageHint Hint = BufferUsageHint.DynamicDraw) {
			F = Fnt;
			FntAtlas = F.GetAtlas();

			this.Width = W;
			this.Height = H;
			Chars = new char[W, H];
			Colors = new Color4[W, H];

			Pos = new Vector2[] { };
			UVs = new Vector2[] { };
			Clr = new Vector4[] { };

			Shader = Engine.Text2D;
			TxtVAO = new VAO(PrimitiveType.Quads);
			TxtVAO.Bind();

			TxtPos = new VBO(BufferTarget.ArrayBuffer, Hint);
			TxtUVs = new VBO(BufferTarget.ArrayBuffer, Hint);
			TxtClr = new VBO(BufferTarget.ArrayBuffer, Hint);

			Dirty = true;
		}

		public void Clear() {
			Chars = new char[Width, Height];
			Colors = new Color4[Width, Height];
			X = Y = 0;
		}

		void ClearBuffers() {
			Pos = new Vector2[Width * Height * CharSize];
			UVs = new Vector2[Pos.Length];
			Clr = new Vector4[Pos.Length];
		}

		void Update() {
			ClearBuffers();
			GlyphMetrics M = new GlyphMetrics();
			Vector4 CharClr;
			int X = 0, Y = 0, Idx = 0;
			float U, V, W, H;
			char C;

			for (int cy = 0; cy < Height; cy++) {
				for (int cx = 0; cx < Width; cx++) {
					C = Chars[cx, cy];

					if (F.GetPackUV(C, out U, out V, out W, out H)) {
						M = F.GlyphMetrics(C, cx > 0 ? (char?)Chars[cx - 1, cy] : null);
						CharClr = Colors[cx, cy].ToVec4();

						Pos.Insert(Idx, Quads2D.Quad(new Vector2(X + F.GetRelativeX(M), Y + F.GetRelativeY(M)),
							new Vector2(M.Width, M.Height)));
						UVs.Insert(Idx, Quads2D.Quad(new Vector2(U, V), new Vector2(W, H)));
						Clr.Insert(Idx, new Vector4[] { CharClr, CharClr, CharClr, CharClr });
						Idx += CharSize;
					}

					X += M.AdvanceX;
					Y += M.AdvanceY;
				}

				X = 0;
				Y += (int)(F.LineSpacing * F.Size);
			}

			TxtVAO.Bind();
			TxtPos.Bind();
			TxtPos.Data(Pos);
			TxtPos.VertexAttribPointer(Shader.PosAttrib);
			TxtUVs.Bind();
			TxtUVs.Data(UVs);
			TxtUVs.VertexAttribPointer(Shader.UVAttrib);
			TxtClr.Bind();
			TxtClr.Data(Clr);
			TxtClr.VertexAttribPointer(Shader.ClrAttrib);
			TxtVAO.Unbind();
		}

		public void Put(int X, int Y, char C, Color4 Clr) {
			Chars[X, Y] = C;
			Colors[X, Y] = Clr;
			Dirty = true;
		}

		public void Print(ref int X, ref int Y, string Str, Color4 Clr) {
			for (int i = 0; i < Str.Length; i++) {
				char C = Str[i];
				if (C == '\r' || C == '\b')
					continue;

				if (C == '\t') {
					X += F.TabSize;
					continue;
				}

				if (C == '\n') {
					X = 0;
					Y++;
					continue;
				}

				Put(X++, Y, C, Clr);

				if (X >= Width) {
					X = 0;
					Y++;
				}
			}
		}

		public void Print(int X, int Y, string Str, Color4 Clr) {
			Print(ref X, ref Y, Str, Clr);
		}

		public void Print(int X, int Y, string Str) {
			Print(ref X, ref Y, Str, Color4.White);
		}

		public void Print(string Str, Color4 Clr) {
			Print(ref X, ref Y, Str, Clr);
		}

		public void Print(string Str) {
			Print(Str, Color4.White);
		}

		public void Render() {
			if (Dirty) {
				Dirty = false;
				Update();
			}

			FntAtlas.Bind();
			TxtVAO.Bind();
			TxtVAO.DrawArrays(0, Pos.Length);
			TxtVAO.Unbind();
			FntAtlas.Unbind();
		}
	}
}