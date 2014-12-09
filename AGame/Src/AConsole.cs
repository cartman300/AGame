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
	static class AConsole {
		static bool Dirty;
		static Text Txt;
		static Vector2 CharSize;

		public static GraphicsTexture Texture {
			get {
				ResolveDirty();
				return Txt.Tex;
			}
		}

		public static void Init() {
			Txt = new Text(new Font(FontFamily.GenericMonospace, 12), Game.AGame.Width, Game.AGame.Height);
			CharSize = Txt.MeasureString("#");
		}

		// TODO: Fix
		static void ResolveDirty() {
			if (Dirty) {
				Dirty = false;
			}
		}

		// TODO: Fix
		public static void Print(object Obj) {
			string Str = (Obj ?? "NULL").ToString();
			Txt.Print(Str, Text.Alignment.Bottom | Text.Alignment.Left);
		}

		public static void Print(object Format, params object[] Params) {
			Print(string.Format((Format ?? "").ToString(), Params));
		}

		public static void Render() {
			ResolveDirty();
			Txt.Render();
		}
	}
}