using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src;

namespace AGame.Src {
	class Game : GameWindow {
		public static Engine Engine;
		public static Game AGame;

		public Game(int W, int H, GraphicsMode GMode, bool Borderless = true)
			: base(W, H, GMode, "A Game", GameWindowFlags.FixedWindow) {
			if (Engine != null)
				throw new Exception("Cannot run 2 instances of game in same application");
			Engine = new Src.Engine();
			AGame = this;

			Context.ErrorChecking = true;

			if (Borderless)
				WindowBorder = OpenTK.WindowBorder.Hidden;
			ClientSize = new Size(W, H);
			if (Borderless)
				Location = new Point(0, 0);
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			Engine.InitGL();
			AConsole.Init();
			Engine.ActiveState = new States.Menu();
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			base.OnKeyDown(e);

			if (e.Key == Key.F4 && e.Modifiers.HasFlag(KeyModifiers.Alt))
				Exit();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			base.OnMouseDown(e);
			Engine.ActiveState.MouseClick(e.X, e.Y, true);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e) {
			base.OnMouseUp(e);
			Engine.ActiveState.MouseClick(e.X, e.Y, false);
		}

		protected override void OnUpdateFrame(FrameEventArgs e) {
			base.OnUpdateFrame(e);
			Engine.ActiveState.Update((float)e.Time);
		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			base.OnRenderFrame(e);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Clear(ClearBufferMask.StencilBufferBit);
			GL.Clear(ClearBufferMask.DepthBufferBit);

			float Time = (float)e.Time;
			Engine.ActiveState.PreRenderOpaque(Time);
			Engine.ActiveState.RenderOpaque(Time);
			Engine.ActiveState.PostRenderOpaque(Time);
			Engine.ActiveState.PreRenderTransparent(Time);
			Engine.ActiveState.RenderTransparent(Time);
			Engine.ActiveState.PostRenderTransparent(Time);
			Engine.ActiveState.PreRenderGUI(Time);
			Engine.ActiveState.RenderGUI(Time);
			Engine.ActiveState.PostRenderGUI(Time);

			SwapBuffers();
		}
	}
}