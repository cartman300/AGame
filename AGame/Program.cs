using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using BulletSharp;

using AGame.Utils;

namespace AGame {
	class Program {
		static void Main(string[] args) {
			Console.Title = "A Game";

			ToolkitOptions TOpt = new ToolkitOptions();
			TOpt.Backend = PlatformBackend.PreferNative;
			Toolkit.Init(TOpt);

			ColorFormat ClrFormat = new ColorFormat(24, 24, 24, 24);
			GraphicsMode GMode = new GraphicsMode(ClrFormat, 24, 8, 0, ClrFormat, 2, false);

			using (Game G = new Game(Screen.W, Screen.H, GMode)) {
				G.Run(60, 60);
			}

			Console.WriteLine("Completed!");
			//Console.ReadLine();
		}
	}
}
