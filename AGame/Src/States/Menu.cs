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
	unsafe class Menu : State {
		TextLines Txt;

		Model Missile;
		Model Cube;

		public Menu() {
			Flib.Font F = new Flib.Font("C:/Windows/Fonts/consola.ttf", 14);
			Txt = new TextLines(F);

			Missile = Engine.Game.CreateModel("models/missile/missile3.mdl")[0];
			Missile.GLInit();

			Cube = new Model();
			Mesh Cuboid = Mesh.CreateCuboid(10, 10, 10);
			Cuboid.Position = new Vector3(0, 100, 0);
			Cuboid.Color = Color.White;
			Cuboid.MultiplyColor = false;

			Cube.Add(Cuboid, Engine.Generic3D);
			Cube.GLInit();
		}

		public override void RenderOpaque(float T) {
			Missile.RenderOpaque();
			Cube.RenderOpaque();
		}

		public override void RenderGUI(float T) {
			Txt[0] = "Frametime: " + T;
			Txt[1] = "FPS: " + Math.Round(1f / T, 1);
			Txt.Render();
		}
	}
}