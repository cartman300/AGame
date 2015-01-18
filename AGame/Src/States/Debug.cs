using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.Meshes;

using HLLibrary;

using Awesomium;
using Awesomium.Core;

namespace AGame.Src.States {
	class Debug {
		ColoredText Txt;

		public Debug() {
			Flib.Font F = new Flib.Font("C:/Windows/Fonts/lucon.ttf", 10);
			Txt = new ColoredText(F, 30, 3, BufferUsageHint.StreamDraw);
		}

		public void Update(float T) {

		}

		public void Render3D(float T) {
		}

		public void RenderGUI(float T) {
			Txt.Clear();
			Txt.Print("Frametime: " + T.ToString() + "\n");
			Txt.Print("FPS: "  + Math.Round(1f / T, 1).ToString() + "\n");
			Txt.Print("State: " + Engine.Game.ActiveState.GetType().Name);
			Engine.Text2D.Use(Txt.Render);
		}
	}
}