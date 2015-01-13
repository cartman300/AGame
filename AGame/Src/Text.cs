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

using Font = Flib.Font;

namespace AGame.Src {
	class Text {
		Font F;
		public Texture Atlas;

		public Color ForeColor;
		public Color BackColor;
		public bool FillBackground;

		bool Dirty;

		string _Str;
		public string Str {
			get {
				return _Str;
			}
			set {
				if (_Str == value)
					return;
				_Str = value;
				Dirty = true;
			}
		}

		float S;
		public float Scale {
			get {
				return S;
			}
			set {
				S = value;
				Dirty = true;
			}
		}

		VAO TextVAO;
		Quads2D Chars;

		public Text(Font Fnt) {
			F = Fnt;
			ForeColor = Color.White;
			BackColor = Color.Black;
			FillBackground = false;

			if (Fnt.Userdata == null) {
				Texture Atlas = new Texture(TextureTarget.Texture2D);
				Atlas.BindTo(TextureUnit.Texture0);
				Atlas.Wrapping(Texture.Wrap.X, Texture.WrapMode.ClampToEdge);
				Atlas.Wrapping(Texture.Wrap.Y, Texture.WrapMode.ClampToEdge);
				Atlas.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Linear);
				Atlas.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Linear);
				Atlas.Load(Fnt.FontAtlas);
				Fnt.Userdata = Atlas;
			}
			this.Atlas = (Texture)Fnt.Userdata;

			Str = "";
			Scale = 1.0f;
			TextVAO = new VAO(PrimitiveType.Quads);
			Chars = new Quads2D(TextVAO, BufferUsageHint.DynamicDraw);
		}

		public void Render() {
			if (_Str == null || _Str.Length == 0)
				return;
			if (Dirty) {
				Dirty = false;

				int ChrSz = 4;
				Vector2[] Qds = new Vector2[_Str.Length * ChrSz];
				Vector2[] UVs = new Vector2[Qds.Length];

				F.Iterate(_Str, (M, X, Y) => {
					float U, V, W, H;
					if (!F.GetPackUV(M.Glyph, out U, out V, out W, out H))
						return;

					Qds.Insert(M.StringIdx * ChrSz, Quads2D.Quad(new Vector2(X * S, -Y * S),
						new Vector2(M.Width * S, -M.Height * S)));
					UVs.Insert(M.StringIdx * ChrSz, Quads2D.Quad(new Vector2(U, V), new Vector2(W, H)));
				});

				Chars.SetData(Qds, UVs);
			}

			Atlas.Bind();
			Chars.Render();
			Atlas.Unbind();
		}
	}

	class TextLines {
		Text[] Texts;
		Font Fnt;

		public string Str {
			get {
				string Ret = "";
				for (int i = 0; i < Texts.Length; i++) {
					if (i > 0)
						Ret += "\n";
					if (Texts[i] != null)
						Ret += Texts[i].Str;
				}
				return Ret;
			}
			set {
				string[] Lines = value.Split('\n');
				for (int i = 0; i < Lines.Length; i++)
					this[i] = Lines[i];
			}
		}

		public TextLines(Font Fnt, int Capacity = 0) {
			this.Fnt = Fnt;
			Texts = new Text[Capacity];
		}

		void Verify(int Line) {
			if (Line >= Texts.Length)
				Texts = Texts.Resize(Line + 1);
			if (Texts[Line] == null)
				Texts[Line] = new Text(Fnt);
		}

		public string this[int Line] {
			get {
				Verify(Line);
				return Texts[Line].Str.TrimStart('\n');
			}
			set {
				Verify(Line);
				Texts[Line].Str = new string('\n', Line) + value;
			}
		}

		public void Render() {
			for (int i = 0; i < Texts.Length; i++)
				if (Texts[i] != null)
					Texts[i].Render();
		}
	}
}