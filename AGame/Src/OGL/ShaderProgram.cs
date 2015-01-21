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
	class ShaderAssembler {
		string Version;
		List<Tuple<string, string>> Inputs, Uniforms;
		List<Shader> Shaders;

		public ShaderAssembler(string Version = "150") {
			this.Version = Version;
			Inputs = new List<Tuple<string, string>>();
			Uniforms = new List<Tuple<string, string>>();
			Shaders = new List<Shader>();
		}

		public ShaderAssembler AddUniform(string UniType, params string[] UniName) {
			for (int i = 0; i < UniName.Length; i++)
				Uniforms.Add(new Tuple<string, string>(UniType, UniName[i]));
			return this;
		}

		public ShaderAssembler AddInput(string InType, params string[] InName) {
			for (int i = 0; i < InName.Length; i++)
				Inputs.Add(new Tuple<string, string>(InType, InName[i]));
			return this;
		}

		static string GetPrefix(ShaderType T) {
			switch (T) {
				case ShaderType.ComputeShader:
					return "compute_";
				case ShaderType.FragmentShader:
					return "frag_";
				case ShaderType.GeometryShader:
					return "geom_";
				case ShaderType.TessControlShader:
					return "tessctrl_";
				case ShaderType.TessEvaluationShader:
					return "tesseval_";
				case ShaderType.VertexShader:
					return "vert_";
				default:
					throw new Exception("Unknown shader type " + T.ToString());
			}
		}

		static string CreateInput(ShaderType T, ShaderType? Next, string Type, string Name) {
			string I = "in " + Type + " " + GetPrefix(T) + Name;
			if (T == ShaderType.GeometryShader)
				I += "[]";
			I += ";\n";
			I += "#define __" + Name + " " + GetPrefix(T) + Name + "\n";
			I += "#define typeof__" + Name + " " + Type + "\n";
			if (Next.HasValue) {
				I += "out " + Type + " " + GetPrefix(Next.Value) + Name + ";\n";
				I += "#define " + Name + "__ " + GetPrefix(Next.Value) + Name + "\n";
			}
			return I;
		}

		public string GetMappings(ShaderType T, ShaderType Next) {
			string Mappings = "";
			for (int i = 0; i < Inputs.Count; i++) {
				Mappings += GetPrefix(Next) + Inputs[i].Item2 + " = " + GetPrefix(T) + Inputs[i].Item2;
				if (T == ShaderType.GeometryShader)
					Mappings += "[i]";
				Mappings += ";\n";
			}
			return Mappings;
		}

		public ShaderAssembler AddShader(ShaderType T, ShaderType? Next, string ShaderName, bool IsPart = false) {
			StringBuilder Src = new StringBuilder();
			Src.AppendFormat("#version {0}\n", Version);

			if (!IsPart) {
				for (int i = 0; i < Uniforms.Count; i++)
					Src.AppendFormat("uniform {0} {1};\n", Uniforms[i].Item1, Uniforms[i].Item2);

				for (int i = 0; i < Inputs.Count; i++)
					Src.Append(CreateInput(T, Next, Inputs[i].Item1, Inputs[i].Item2));
			}

			Src.Append(File.ReadAllText(Shader.GetShaderPath(ShaderName)));
			if (!IsPart && Next.HasValue)
				Src.Replace("///MAPPINGS", GetMappings(T, Next.Value));

			//Console.WriteLine("{0}\n\n", Src.ToString());
			Shaders.Add(new Shader(T, Src.ToString(), ShaderName));
			return this;
		}

		public ShaderProgram Assemble() {
			return new ShaderProgram(Shaders.ToArray());
		}
	}

	class ShaderProgram : GLObject {
		const string VertShader_Pos = "vert_pos";
		const string VertShader_Norm = "vert_norm";
		const string VertShader_UV = "vert_uv";
		const string VertShader_Clr = "vert_clr";

		const string UniformMatrix = "Matrix";
		const string UniformModelMatrix = "ModelMatrix";
		const string UniformNormMatrix = "NormMatrix";
		const string UniformMultColor = "MultColor";
		const string UniformColor = "ObjColor";
		const string UniformResolution = "Resolution";

		public static ShaderProgram ActiveShader, LastActive;

		public Matrix4 Matrix {
			set {
				Matrix4 M = value;
				SetUniform(UniformMatrix, ref M);
			}
		}

		public Vector2 Resolution {
			set {
				SetUniform(UniformResolution, value);
			}
		}

		public Matrix4 ModelMatrix {
			set {
				Matrix4 M = value;
				SetUniform(UniformModelMatrix, ref M);
			}
		}

		public Matrix4 NormMatrix {
			set {
				Matrix4 M = value;
				SetUniform(UniformNormMatrix, ref M);
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

		public int ClrAttrib {
			get {
				return GetAttribLocation(VertShader_Clr);
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
			Link();
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
			LastActive = ActiveShader = this;
			GL.UseProgram(ID);	
			SetUniform("Time", Engine.Game.Time);
			Resolution = Engine.Game.Resolution;
			if (Cam != null) {
				ModelMatrix = Modelview;
				Matrix = Modelview * Cam.Collapse();
				Matrix4 M = Modelview;
				M.Transpose();
				M.Invert();
				NormMatrix = M;
			}
		}

		public void Use(Vector3 Pos, Quaternion Rot, float Scale, Color4 Clr, bool MultClr, Action A) {
			Modelview = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rot) * Matrix4.CreateTranslation(Pos);
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
			ActiveShader = null;
		}

		public int GetAttribLocation(string Name) {
			return GL.GetAttribLocation(ID, Name);
		}

		int GetUniformLocation(string Name) {
			return GL.GetUniformLocation(ID, Name);
		}

		void SetUniform(string Name, int Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform1(Loc, Val);
		}

		void SetUniform(string Name, Vector2 Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform2(Loc, Val);
		}

		void SetUniform(string Name, Vector3 Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform3(Loc, Val);
		}

		void SetUniform(string Name, float Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform1(Loc, Val);
		}

		void SetUniform(string Name, Vector4 Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform4(Loc, Val);
		}

		void SetUniform(string Name, Color4 Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform4(Loc, Val);
		}

		void SetUniform(string Name, bool Val) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.Uniform1(Loc, Val ? 1 : 0);
		}

		void SetUniform(string Name, ref Matrix4 Matrix, bool Transpose = false) {
			int Loc = GetUniformLocation(Name);
			if (Loc >= 0)
				GL.UniformMatrix4(Loc, Transpose, ref Matrix);
		}

		void SetUniform(string Name, TextureUnit Unit) {
			SetUniform(Name, (int)Unit - (int)TextureUnit.Texture0);
		}
	}
}