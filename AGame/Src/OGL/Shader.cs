using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace AGame.Src.OGL {
	class Shader : GLObject {
		internal List<ShaderProgram> Programs;

		public ShaderType SType;

		public Shader(ShaderType SType) {
			Programs = new List<ShaderProgram>();

			ID = GL.CreateShader(SType);

			this.SType = SType;
		}

		public Shader(ShaderType SType, string Src)
			: this(SType) {
			Compile(Src);
		}

		public override void Delete() {
			DetachAll();
			GL.DeleteShader(ID);
		}

		public void Attach(ShaderProgram Prog) {
			if (!Programs.Contains(Prog))
				Programs.Add(Prog);
			GL.AttachShader(Prog.ID, ID);
		}

		public void Detach(ShaderProgram Prog) {
			GL.DetachShader(Prog.ID, ID);
		}

		public void DetachAll() {
			for (int i = 0; i < Programs.Count; i++)
				Detach(Programs[i]);
		}

		public void Compile(string Src) {
			GL.ShaderSource(ID, Src);
			GL.CompileShader(ID);

			int Status = 0;
			GL.GetShader(ID, ShaderParameter.CompileStatus, out Status);

			if (Status != 1)
				throw new Exception(GL.GetShaderInfoLog(ID));
		}
	}
}