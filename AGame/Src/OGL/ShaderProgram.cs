using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		bool MatDirty;
		Matrix4 Proj, Trans, Rot, Mat;
		public Matrix4 Projection {
			get {
				return Proj;
			}
			set {
				Proj = value;
				MatDirty = true;
			}
		}
		public Matrix4 Translation {
			get {
				return Trans;
			}
			set {
				Trans = value;
				MatDirty = true;
			}
		}
		public Matrix4 Rotation {
			get {
				return Rot;
			}
			set {
				Rot = value;
				MatDirty = true;
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

		public ShaderProgram(Shader[] Shaders) {
			ID = GL.CreateProgram();
			Projection = Translation = Rotation = Matrix4.Identity;

			for (int i = 0; i < Shaders.Length; i++)
				Shaders[i].Attach(this);
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
			GL.UseProgram(ID);
			if (MatDirty) {
				MatDirty = false;
				Mat = Trans * Rot * Proj;
			}
			SetUniform(UniformMatrix, ref Mat);
		}

		public override void Unbind() {
			GL.UseProgram(0);
		}

		public int GetAttribLocation(string Name) {
			return GL.GetAttribLocation(ID, Name);
		}

		public int GetUniformLocation(string Name) {
			return GL.GetUniformLocation(ID, Name);
		}

		public void SetUniform(string Name, int Val) {
			GL.Uniform1(GetUniformLocation(Name), Val);
		}

		public void SetUniform(string Name, Vector2 Val) {
			GL.Uniform2(GetUniformLocation(Name), Val);
		}

		public void SetUniform(string Name, Vector3 Val) {
			GL.Uniform3(GetUniformLocation(Name), Val);
		}

		public void SetUniform(string Name, Vector4 Val) {
			GL.Uniform4(GetUniformLocation(Name), Val);
		}

		public void SetUniform(string Name, ref Matrix4 Matrix, bool Transpose = false) {
			GL.UniformMatrix4(GetUniformLocation(Name), Transpose, ref Matrix);
		}

		public void SetUniform(string Name, TextureUnit Unit) {
			SetUniform(Name, (int)Unit - (int)TextureUnit.Texture0);
		}
	}
}