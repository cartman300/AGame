using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
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
	class Engine : GameWindow {
		public static Engine Game;

		public static ShaderProgram ScreenShader;
		public static ShaderProgram Generic2D;
		public static ShaderProgram Text2D;
		public static ShaderProgram Generic3D;

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
			ModelLoaders.Add(new ObjLoader());

			MouseEnabled = true;
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

		public void InitGL() {
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Generic2D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, "Data/Shaders/Generic2D.vertex.glsl"),
				new Shader(ShaderType.FragmentShader, "Data/Shaders/Generic2D.fragment.glsl"),
			});

			ScreenShader = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, "Data/Shaders/Generic2D.vertex.glsl"),
				new Shader(ShaderType.FragmentShader, "Data/Shaders/Screen.fragment.glsl"),
			});

			Generic3D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, "Data/Shaders/Generic3D.vertex.glsl"),
				new Shader(ShaderType.FragmentShader, "Data/Shaders/Generic3D.fragment.glsl"),
			});

			Text2D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader,"Data/Shaders/Text2D.vertex.glsl"),
				new Shader(ShaderType.FragmentShader, "Data/Shaders/Text2D.fragment.glsl"),
			});

			Matrix4 Offset = Matrix4.CreateTranslation(-Resolution.X / 2, -Resolution.Y / 2, 0);

			Generic2D.Cam = new Camera();
			Generic2D.Cam.Projection = Matrix4.CreateOrthographic(Resolution.X, -Resolution.Y, -1, 1);
			Generic2D.Cam.Translation = Offset;
			ScreenShader.Cam = Text2D.Cam = Generic2D.Cam;

			float FOV = 90f * (float)Math.PI / 180f;
			Generic3D.Cam = new Camera();
			Generic3D.Cam.Projection = Matrix4.CreatePerspectiveFieldOfView(FOV, (float)Resolution.X / Resolution.Y, 1, 1000);

			ColorTex = new Texture(TextureTarget.Texture2D);
			ColorTex.Use(() => {
				ColorTex.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Nearest);
				ColorTex.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Nearest);
				ColorTex.TexImage2D(0, PixelInternalFormat.Rgba, (int)Resolution.X, (int)Resolution.Y, PixelFormat.Rgba);
				//ColorTex.TexImage2DMultisample(PixelInternalFormat.Rgba, (int)Resolution.X, (int)Resolution.Y);
			});

			DSTex = new Texture(TextureTarget.Texture2D);
			DSTex.Use(() => {
				DSTex.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Nearest);
				DSTex.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Nearest);
				DSTex.TexImage2D(0, PixelInternalFormat.Depth24Stencil8, (int)Resolution.X, (int)Resolution.Y,
					PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
			});

			Scr = new FBO();
			Scr.Attach(FramebufferAttachment.ColorAttachment0, ColorTex);
			Scr.Attach(FramebufferAttachment.DepthStencilAttachment, DSTex);

			ScrQuad = new Quads2D();
			ScrQuad.SetData(Quads2D.Quad(0, 0, Resolution.X, Resolution.Y), Quads2D.Quad(0, 1, 1, -1));
		}

		public Point GetWindowCenter() {
			return new Point(Location.X + (Size.Width / 2), Location.Y + (Size.Height / 2));
		}

		ShaderProgram CreateShader(Shader[] S) {
			ShaderProgram Prog = new ShaderProgram(S);
			Prog.BindFragDataLocation(0, "Color");
			Prog.Bind();
			return Prog;
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

		FBO Scr;
		Texture ColorTex, DSTex;
		Quads2D ScrQuad;

		protected override void OnRenderFrame(FrameEventArgs E) {
			base.OnRenderFrame(E);
			float Time = (float)E.Time;

			Scr.RenderTo(() => {
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

			GL.Disable(EnableCap.DepthTest);
			ColorTex.Use(() => ScreenShader.Use(ScrQuad.Render));
			ActiveState.PreRenderGUI(Time);
			ActiveState.RenderGUI(Time);
			ActiveState.PostRenderGUI(Time);
			DebugState.RenderGUI(Time);

			SwapBuffers();
		}
	}
}