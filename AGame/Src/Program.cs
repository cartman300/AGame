using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Flib;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using BulletSharp;

using AGame.Utils;
using AGame.Src.ModelFormats;
using AGame.Src.Meshes;

namespace AGame.Src {
	class Program {
		static Mutex ProcessMutex;

		static Program() {
			AppDomain.CurrentDomain.AssemblyResolve += (S, Args) => {
				string Pth = "bin/" + new AssemblyName(Args.Name).Name + ".dll";
				if (!FSys.Exists(Pth))
					return null;
				return Assembly.Load(FSys.ReadAllBytes(Pth));
			};
		}

		static void MountPacks() {
			if (Directory.Exists("Packs")) {
				string[] MountExts = SharpFileSystem.SevenZip.SeamlessSevenZipFileSystem.ArchiveExtensions.ToArray();
				string[] PackFiles = Directory.GetFiles("Packs", "*", SearchOption.AllDirectories);
				for (int i = 0; i < PackFiles.Length; i++) {
					string PackFile = PackFiles[i];
					if (MountExts.Contains(Path.GetExtension(PackFile)))
						FSys.Mount(PackFile);
				}
			}
		}

		static void Main(string[] args) {
			System.Console.Title = "A Game";

			bool NewMuted = false;
			ProcessMutex = new Mutex(true, "AGameMutex", out NewMuted);
			if (!NewMuted) {
				System.Console.WriteLine("Can not run multiple instances of game");
				Environment.Exit(1);
				return;
			}

			ToolkitOptions TOpt = new ToolkitOptions();
			TOpt.Backend = PlatformBackend.PreferNative;
			Toolkit.Init(TOpt);
			GraphicsMode GMode = new GraphicsMode();

			int W = Screen.W;
			int H = Screen.H;
			bool Fullscreen = true;

			//*
			W = 800;
			H = 600;
			Fullscreen = false;
			//*/

			using (Engine G = new Engine(W, H, GMode, Fullscreen)) {
				Console.WriteLine("OpenGL {0}", GL.GetString(StringName.Version));
				Console.WriteLine("GLSL {0}", GL.GetString(StringName.ShadingLanguageVersion));
				Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
				Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));
				File.WriteAllText("GLExt.txt", GL.GetString(StringName.Extensions).Replace(' ', '\n'));
				MountPacks();

				G.VSync = VSyncMode.On;
				G.Run(60, 60);
			}

			ProcessMutex.ReleaseMutex();
			System.Console.WriteLine("Completed!");
			Environment.Exit(0);
			return;
		}
	}
}