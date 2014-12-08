using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using BulletSharp;

using AGame.Utils;

namespace AGame.Src {
	class Program {
		static Program() {
			AppDomain.CurrentDomain.AssemblyResolve += (S, Args) => {
				string Pth = Path.GetFullPath("bin/" + new AssemblyName(Args.Name).Name + ".dll");
				if (!File.Exists(Pth))
					return null;
				return Assembly.LoadFile(Pth);
			};
		}

		static void Main(string[] args) {
			Console.Title = "A Game";

			ToolkitOptions TOpt = new ToolkitOptions();
			TOpt.Backend = PlatformBackend.PreferNative;
			Toolkit.Init(TOpt);

			ColorFormat ClrFormat = new ColorFormat(24, 24, 24, 24);
			GraphicsMode GMode = new GraphicsMode(ClrFormat, 24, 8, 0, ClrFormat, 2, false);

			int W = 800; // Screen.W;
			int H = 600; // Screen.H;

			using (Game G = new Game(W, H, GMode, false)) {
				G.VSync = VSyncMode.On;
				G.Run(60, 60);
			}

			Console.WriteLine("Completed!");
			//Console.ReadLine();
		}
	}
}