using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using BulletSharp;

namespace AGame {
	class Game : GameWindow {

		public Game(int W, int H, GraphicsMode GMode)
			: base(640, 420, GMode, "A Game", GameWindowFlags.FixedWindow) {

			WindowBorder = OpenTK.WindowBorder.Hidden;
			ClientSize = new Size(W, H);
			Location = new Point(0, 0);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			GL.ClearColor(Color4.CornflowerBlue);
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			base.OnKeyDown(e);

			if (e.Key == Key.F4 && e.Modifiers.HasFlag(KeyModifiers.Alt))
				Exit();
		}

		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);

		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Clear(ClearBufferMask.StencilBufferBit);
			GL.Clear(ClearBufferMask.DepthBufferBit);

			SwapBuffers();
		}
	}
}