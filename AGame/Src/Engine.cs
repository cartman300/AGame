using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using Stopwatch = System.Diagnostics.Stopwatch;
using FCursor = System.Windows.Forms.Cursor;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Src;
using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.States;
using AGame.Src.Meshes;

namespace AGame.Src {
	unsafe class Engine : GameWindow {
		public static Engine Game;

		public static ShaderProgram ScreenShader3D;
		public static ShaderProgram ScreenShaderFull;
		public static ShaderProgram Generic2D;
		public static ShaderProgram Text2D;
		public static ShaderProgram Generic3D;

		Stopwatch SWatch;
		public float Time {
			get {
				return (float)SWatch.ElapsedMilliseconds / 1000f;
			}
		}

		public Vector2 Resolution;
		States.Debug DebugState;

		State _ActiveState;
		List<ModelLoader> ModelLoaders;

		public Engine(int W, int H, GraphicsMode GMode, bool Borderless = true)
			: base(W, H, GMode, "A Game", GameWindowFlags.FixedWindow) {
			Context.ErrorChecking = true;
			Resolution = new Vector2(W, H);

			if (Game != null)
				throw new Exception("Can not run multiple instances of engine");
			Game = this;

			if (Borderless)
				WindowBorder = OpenTK.WindowBorder.Hidden;
			ClientSize = new Size(W, H);
			if (Borderless)
				Location = new Point(0, 0);

			ModelLoaders = new List<ModelLoader>();
			ModelLoaders.Add(new MdlLoader());
			ModelLoaders.Add(new AssimpLoader());

			MouseEnabled = true;

			SWatch = new Stopwatch();
			SWatch.Start();
			ToggleMouse();
		}

		public Model[] CreateModel(string Path) {
			for (int i = 0; i < ModelLoaders.Count; i++)
				if (ModelLoaders[i].CanLoad(Path))
					return ModelLoaders[i].CreateModel(Path);
			return null;
		}

		public State ActiveState {
			get {
				return _ActiveState;
			}
			set {
				if (_ActiveState != null)
					_ActiveState.Deactivate(value);
				value.Activate(_ActiveState);
				_ActiveState = value;
			}
		}

		public Point GetWindowCenter() {
			return new Point(Location.X + (Size.Width / 2), Location.Y + (Size.Height / 2));
		}

		ShaderAssembler CreateShader() {
			ShaderAssembler ProgAsm = new ShaderAssembler();
			return ProgAsm.AddUniform("mat4", "Matrix", "ModelMatrix", "NormMatrix")
				.AddUniform("sampler2D", "Texture"/*, "NormalTexture"*/).AddUniform("vec4", "ObjColor")
				.AddUniform("vec2", "Resolution").AddUniform("float", "Time");
		}

		ShaderProgram Finalize(ShaderAssembler SAsm) {
			ShaderProgram P = SAsm.Assemble();
			P.BindFragDataLocation(0, "Color");
			//P.BindFragDataLocation(1, "NormalColor");
			P.Bind();
			return P;
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			InitGL();

			DebugState = new Debug();
			Menu.SwitchTo();
		}

		Point OldMousePos;
		public bool MouseEnabled;
		public void ToggleMouse() {
			if (!(CursorVisible = !(MouseEnabled = !MouseEnabled))) {
				OldMousePos = FCursor.Position;
				FCursor.Position = GetWindowCenter();
			} else
				FCursor.Position = OldMousePos;
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			base.OnKeyDown(e);
			if (e.Key == Key.F4 && e.Modifiers.HasFlag(KeyModifiers.Alt))
				Exit();

			if (e.Key == Key.Enter || e.Key == Key.KeypadEnter)
				ActiveState.TextEntered("\n");
			else if (e.Key == Key.BackSpace)
				ActiveState.TextEntered("\b");

			ActiveState.OnKey(e, true);
		}

		protected override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e);
			ActiveState.OnKey(e, false);
		}

		protected override void OnKeyPress(KeyPressEventArgs e) {
			base.OnKeyPress(e);
			ActiveState.TextEntered(e.KeyChar.ToString());
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			base.OnMouseDown(e);
			ActiveState.MouseClick(e.X, e.Y, true);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e) {
			base.OnMouseUp(e);
			ActiveState.MouseClick(e.X, e.Y, false);
		}

		protected override void OnUpdateFrame(FrameEventArgs E) {
			base.OnUpdateFrame(E);
			ActiveState.Update((float)E.Time);
			DebugState.Update((float)E.Time);
		}

		public void InitGL() {
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			/*GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);*/

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);

			Generic2D = Finalize(CreateShader().AddInput("vec2", "pos", "uv")
				.AddShader(ShaderType.VertexShader, ShaderType.FragmentShader, "Generic.vertex")
				.AddShader(ShaderType.FragmentShader, null, "Generic.fragment")
				.AddShader(ShaderType.VertexShader, null, "Parts/utils", true));

			Text2D = Finalize(CreateShader().AddInput("vec2", "pos", "uv").AddInput("vec4", "clr")
				.AddShader(ShaderType.VertexShader, ShaderType.FragmentShader, "Generic.vertex")
				.AddShader(ShaderType.FragmentShader, null, "Text2D.fragment")
				.AddShader(ShaderType.VertexShader, null, "Parts/utils", true));

			Generic3D = Finalize(CreateShader().AddInput("vec3", "pos", "norm").AddInput("vec2", "uv")
				.AddShader(ShaderType.VertexShader, ShaderType.GeometryShader, "Generic.vertex")
				.AddShader(ShaderType.GeometryShader, ShaderType.FragmentShader, "Empty.geometry")
				.AddShader(ShaderType.FragmentShader, null, "Generic3D.fragment")
				.AddShader(ShaderType.VertexShader, null, "Parts/utils", true));

			ScreenShader3D = Finalize(CreateShader().AddInput("vec2", "pos", "uv")
				.AddShader(ShaderType.VertexShader, ShaderType.FragmentShader, "Generic.vertex")
				.AddShader(ShaderType.FragmentShader, null, "Screen.fragment")
				.AddShader(ShaderType.VertexShader, null, "Parts/utils", true)
				.AddShader(ShaderType.FragmentShader, null, "Parts/fxaa", true));

			ScreenShaderFull = Finalize(CreateShader().AddInput("vec2", "pos", "uv")
				.AddShader(ShaderType.VertexShader, ShaderType.FragmentShader, "Generic.vertex")
				.AddShader(ShaderType.FragmentShader, null, "Generic.fragment")
				.AddShader(ShaderType.VertexShader, null, "Parts/utils", true));

			Matrix4 Offset = Matrix4.CreateTranslation(-Resolution.X / 2, -Resolution.Y / 2, 0);

			Generic2D.Cam = new Camera();
			Generic2D.Cam.Projection = Matrix4.CreateOrthographic(Resolution.X, -Resolution.Y, -1, 1);
			Generic2D.Cam.Translation = Offset;
			ScreenShaderFull.Cam = ScreenShader3D.Cam = Text2D.Cam = Generic2D.Cam;

			float FOV = 90f * (float)Math.PI / 180f;
			Generic3D.Cam = new Camera();
			Generic3D.Cam.Projection = Matrix4.CreatePerspectiveFieldOfView(FOV, (float)Resolution.X / Resolution.Y, 1, 10000);

			Scr3D = new RenderTarget((int)Resolution.X, (int)Resolution.Y, true);
			ScrFull = new RenderTarget((int)Resolution.X, (int)Resolution.Y);
			ScrQuad = new Quads2D();
			ScrQuad.SetData(Quads2D.Quad(0, 0, Resolution.X, Resolution.Y), Quads2D.Quad(0, 1, 1, -1));
		}

		RenderTarget Scr3D, ScrFull;
		Quads2D ScrQuad;

		protected override void OnRenderFrame(FrameEventArgs E) {
			base.OnRenderFrame(E);
			float Time = (float)E.Time;

			Scr3D.RenderTo(() => {
				GL.ClearColor(new Color4(42, 42, 42, 255));
				GL.Clear(ClearBufferMask.ColorBufferBit);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				GL.Clear(ClearBufferMask.StencilBufferBit);
				GL.Enable(EnableCap.DepthTest);

				ActiveState.PreRenderOpaque(Time);
				ActiveState.RenderOpaque(Time);
				ActiveState.PostRenderOpaque(Time);
				ActiveState.PreRenderTransparent(Time);
				ActiveState.RenderTransparent(Time);
				ActiveState.PostRenderTransparent(Time);
				DebugState.Render3D(Time);
			});

			ScrFull.RenderTo(() => {
				GL.Disable(EnableCap.DepthTest);
				Scr3D.UseColor(() => ScreenShader3D.Use(ScrQuad.Render));
				ActiveState.PreRenderGUI(Time);
				ActiveState.RenderGUI(Time);
				ActiveState.PostRenderGUI(Time);
				DebugState.RenderGUI(Time);
			});

			ScrFull.UseColor(() => ScreenShaderFull.Use(ScrQuad.Render));

			SwapBuffers();
		}
	}
}