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

		public static ShaderProgram Generic2D;
		public static ShaderProgram Text2D;
		public static ShaderProgram Generic3D;

		State _ActiveState;
		List<ModelLoader> ModelLoaders;

		public Engine(int W, int H, GraphicsMode GMode, bool Borderless = true)
			: base(W, H, GMode, "A Game", GameWindowFlags.FixedWindow) {
			Context.ErrorChecking = true;

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
			GL.ClearColor(new Color4(42, 0, 0, 255));

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Generic2D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, File.ReadAllText("Data/Shaders/Generic2D.vertex.glsl")),
				new Shader(ShaderType.FragmentShader, File.ReadAllText("Data/Shaders/Generic2D.fragment.glsl")),
			});

			Generic3D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, File.ReadAllText("Data/Shaders/Generic3D.vertex.glsl")),
				new Shader(ShaderType.FragmentShader, File.ReadAllText("Data/Shaders/Generic3D.fragment.glsl")),
			});

			Text2D = CreateShader(new Shader[] {
				new Shader(ShaderType.VertexShader, File.ReadAllText("Data/Shaders/Text2D.vertex.glsl")),
				new Shader(ShaderType.FragmentShader, File.ReadAllText("Data/Shaders/Text2D.fragment.glsl")),
			});

			Size Res = ClientSize;
			Matrix4 Offset = Matrix4.CreateTranslation(-Res.Width / 2, Res.Height / 2, 0);
			Matrix4 Ortho = Matrix4.CreateOrthographic(Res.Width, Res.Height, -1, 1);

			Generic2D.Projection = Text2D.Projection = Ortho;
			Generic2D.Translation = Text2D.Translation = Offset;

			float FOV = 90f * (float)Math.PI / 180f;
			Generic3D.Projection = Matrix4.CreatePerspectiveFieldOfView(FOV, (float)Res.Width / Res.Height, 1, 1000);

			Matrix4 LookAt = Matrix4.LookAt(new Vector3(50, 50, 10), Vector3.Zero, Vector3.UnitY);
			Generic3D.Translation = Matrix4.CreateTranslation(LookAt.ExtractTranslation());
			Generic3D.Rotation = Matrix4.CreateFromQuaternion(LookAt.ExtractRotation());

			//Generic3D.Translation = Matrix4.CreateTranslation(50, 0, 0);
		}

		ShaderProgram CreateShader(Shader[] S) {
			ShaderProgram Prog = new ShaderProgram(S);
			Prog.BindFragDataLocation(0, "Color");
			Prog.Link();
			Prog.Bind();
			return Prog;
		}

		public void CenterMouse() {
			FCursor.Position = new Point(Location.X + Size.Width / 2, Location.Y + Size.Height / 2);
			CursorVisible = false;
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			InitGL();
			ActiveState = new States.Menu();
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e) {
			base.OnKeyDown(e);
			if (e.Key == Key.F4 && e.Modifiers.HasFlag(KeyModifiers.Alt))
				Exit();

			if (e.Key == Key.Enter || e.Key == Key.KeypadEnter)
				ActiveState.TextEntered("\n");
			else if (e.Key == Key.BackSpace)
				ActiveState.TextEntered("\b");

			ActiveState.Key(e, true);
		}

		protected override void OnKeyUp(KeyboardKeyEventArgs e) {
			base.OnKeyUp(e);
			ActiveState.Key(e, false);
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

		int MX, MY;
		float RotX;
		float RotY;
		bool FirstTimeMouse = true;

		protected override void OnUpdateFrame(FrameEventArgs E) {
			const float Sens = 1.0f;
			const float MoveSpeed = 42.0f;
			float T = (float)E.Time;

			if (Keyboard[Key.W]) {
				Generic3D.Translation *= Matrix4.CreateTranslation(new Vector3((float)Math.Sin(RotX), 0,
					(float)Math.Cos(RotX)) * T * MoveSpeed);
			}
			if (Keyboard[Key.S]) {
				Generic3D.Translation *= Matrix4.CreateTranslation(-new Vector3((float)Math.Sin(RotX), 0,
					(float)Math.Cos(RotX)) * T * MoveSpeed);
			}
			if (Keyboard[Key.A]) {
				Generic3D.Translation *= Matrix4.CreateTranslation(new Vector3((float)Math.Sin(RotX + MathHelper.PiOver2), 0,
					(float)Math.Cos(RotX + MathHelper.PiOver2)) * T * MoveSpeed);
			}
			if (Keyboard[Key.D]) {
				Generic3D.Translation *= Matrix4.CreateTranslation(-new Vector3((float)Math.Sin(RotX + MathHelper.PiOver2), 0,
					(float)Math.Cos(RotX + MathHelper.PiOver2)) * T * MoveSpeed);
			}

			if (Keyboard[Key.Space])
				Generic3D.Translation *= Matrix4.CreateTranslation(-new Vector3(0, MoveSpeed * T, 0));
			if (Keyboard[Key.LControl])
				Generic3D.Translation *= Matrix4.CreateTranslation(new Vector3(0, MoveSpeed *T, 0));

			if (!FirstTimeMouse) {
				float DX = (MX - Mouse.X) * Sens * T;
				float DY = (MY - Mouse.Y) * Sens * T;
				if (DX != 0 || DY != 0) {
					RotX += DX;
					RotY += DY;
					RotY = (float)MathHelper.Clamp(RotY, -MathHelper.PiOver2, MathHelper.PiOver2);

					Generic3D.Rotation = Matrix4.CreateRotationY(-RotX) * Matrix4.CreateRotationX(-RotY);
				}
				MX = Mouse.X;
				MY = Mouse.Y;
			} else
				FirstTimeMouse = false;

			base.OnUpdateFrame(E);
			ActiveState.Update((float)E.Time);
		}

		protected override void OnRenderFrame(FrameEventArgs E) {
			base.OnRenderFrame(E);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Clear(ClearBufferMask.StencilBufferBit);
			GL.Clear(ClearBufferMask.DepthBufferBit);

			float Time = (float)E.Time;
			Generic3D.Bind();
			ActiveState.PreRenderOpaque(Time);
			ActiveState.RenderOpaque(Time);
			ActiveState.PostRenderOpaque(Time);
			ActiveState.PreRenderTransparent(Time);
			ActiveState.RenderTransparent(Time);
			ActiveState.PostRenderTransparent(Time);
			Generic2D.Bind();
			ActiveState.PreRenderGUI(Time);
			ActiveState.RenderGUI(Time);
			ActiveState.PostRenderGUI(Time);

			SwapBuffers();
		}
	}
}