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

		public Menu() {
			Font TxtFont = Text.LoadFont("Data/Fonts/FreeSans.ttf", "FreeSans", 12);

			Txt = new Text(TxtFont, Game.AGame.Width, Game.AGame.Height);
			Txt.FillBackground = true;

			for (int i = 0; i < 20; i++)
				AConsole.Print("Hello World #{0}!", i);
		}

		public override void RenderGUI(float T) {
			AConsole.Render();

			Txt.Tex.FillRectangle(Txt.BackColor, Vector2.Zero, Txt.MeasureString("FPS ####"));
			Txt.Print("FPS " + Math.Round(1.0f / T, 1).ToString());
			Txt.Render();
		}
	}
}