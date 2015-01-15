using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VAPT = OpenTK.Graphics.OpenGL4.VertexAttribPointerType;

namespace AGame.Src.OGL {
	class ShaderProgram : GLObject {
		const string VertShader_Pos = "vert_pos";
		const string VertShader_Norm = "vert_norm";
		const string VertShader_UV = "vert_uv";

		const string UniformMatrix = "Matrix";
		const string UniformMultColor = "MultColor";
		const string UniformColor = "ObjColor";

		public Matrix4 Matrix {
			set {
				Matrix4 M = value;
				SetUniform(UniformMatrix, ref M);
			}
		}

		public bool MultiplyColor {
			set {
				SetUniform(UniformMultColor, value);
			}
		}

		public Color4 Color {
			set {
				SetUniform(UniformColor, value);
			}
		}

		public int PosAttrib {
			get {
				return GetAttribLocation(VertShader_Pos);
			}
		}

		public int UVAttrib {
			get {
				return GetAttribLocation(VertShader_UV);
			}
		}

		public int NormAttrib {
			get {
				return GetAttribLocation(VertShader_Norm);
			}
		}

		public Matrix4 Modelview;
		public Camera Cam;
		public Shader[] Shaders;

		public ShaderProgram(Shader[] Shaders) {
			Create();
			Modelview = Matrix4.Identity;
			this.Shaders = Shaders;

			for (int i = 0; i < Shaders.Length; i++)
				Shaders[i].Attach(this);
		}

		void Create() {
			ID = GL.CreateProgram();
		}

		public override void Delete() {
			GL.DeleteProgram(ID);
		}

		public void BindFragDataLocation(int Color, string Name) {
			GL.BindFragDataLocation(ID, Color, Name);
		}

		public void Link() {
			GL.LinkProgram(ID);

			int Status = 0;
			GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out Status);

			if (Status != 1)
				throw new Exception(GL.GetProgramInfoLog(ID));
		}

		public override void Bind() {
			if (Shaders.Any((S) => S.Dirty)) {
				for (int i = 0; i < Shaders.Length; i++)
					if (Shaders[i].Dirty) {
						bool DoRecompile = true;
						while (DoRecompile) {
							try {
								Console.WriteLine("Reloading {0}", Shaders[i].FileWatcher.Filter);
								Shaders[i].Recompile();
								Shaders[i].Dirty = false;
								DoRecompile = false;
							} catch (Exception E) {
								Console.WriteLine(E.Message);
								DoRecompile = true;
								Shaders[i].FileWatcher.WaitForChanged(WatcherChangeTypes.All);
							}
						}
					}
				Link();
			}

			GL.UseProgram(ID);
			if (Cam != null)
				Matrix = Modelview * Cam.Collapse();
		}

		public void BindUse(Vector3 Pos, Color4 Clr, bool MultClr, Action A) {
			Modelview = Matrix4.CreateTranslation(Pos);	
			Bind();
			Color = Clr;
			MultiplyColor = MultClr;
			A();
			Modelview = Matrix4.Identity;
			Color = Color4.White;
			MultiplyColor = true;
			Unbind();
		}

		public override void Unbind() {
			GL.UseProgram(0);
		}

		public int GetAttribLocation(string Name) {
			return GL.GetAttribLocation(ID, Name);
		}

		int GetUniformLocation(string Name) {
			return GL.GetUniformLocation(ID, Name);
		}

		void SetUniform(string Name, int Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform1(Loc, Val);
		}

		void SetUniform(string Name, Vector2 Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform2(Loc, Val);
		}

		void SetUniform(string Name, Vector3 Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform3(Loc, Val);
		}

		void SetUniform(string Name, Vector4 Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform4(Loc, Val);
		}

		void SetUniform(string Name, Color4 Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform4(Loc, Val);
		}

		void SetUniform(string Name, bool Val) {
			int Loc = GetUniformLocation(Name);
			GL.Uniform1(Loc, Val ? 1 : 0);
		}

		void SetUniform(string Name, ref Matrix4 Matrix, bool Transpose = false) {
			int Loc = GetUniformLocation(Name);
			GL.UniformMatrix4(Loc, Transpose, ref Matrix);
		}

		void SetUniform(string Name, TextureUnit Unit) {
			SetUniform(Name, (int)Unit - (int)TextureUnit.Texture0);
		}
	}
}