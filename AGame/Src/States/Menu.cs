using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;

namespace AGame.Src.States {
	class Menu : State {
		Text Txt;

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

		public Menu() {
			Font TxtFont = Text.LoadFont("Data/Fonts/FreeSans.ttf", "FreeSans", 12);

			Txt = new Text(TxtFont, Game.AGame.Width, Game.AGame.Height);
			int posLoc = Game.Engine.Generic2D.GetAttribLocation("pos");
			GL.EnableVertexAttribArray(posLoc);
			Txt.StrPos.VertexAttribPointer(posLoc, 2);

			int uvLoc = Game.Engine.Generic2D.GetAttribLocation("uv");
			GL.EnableVertexAttribArray(uvLoc);
			Txt.StrUV.VertexAttribPointer(uvLoc, 2);
		}

		public override void RenderGUI(float T) {
			Txt.Clear(Color.Transparent);
			Txt.Print("FPS " + Math.Round(1.0f / T, 1).ToString(), Color.White);
			Txt.Render();
		}
	}
}