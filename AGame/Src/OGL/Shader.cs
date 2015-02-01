using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace AGame.Src.OGL {
	class Shader : GLObject {
		internal List<ShaderProgram> Programs;
		public ShaderType SType;
		public string ShaderFile;

		public static string GetShaderPath(string ShaderName) {
			return "Shaders/" + ShaderName + ".glsl";
		}

		void Load(ShaderType SType, string Source, string Path) {
			Path = Path.Replace('\\', '/');

			this.SType = SType;
			ID = GL.CreateShader(SType);
			Programs = new List<ShaderProgram>();

			ShaderFile = Path.Substring(Path.LastIndexOf('/') + 1);
			Compile(Source);
		}

		void Load(ShaderType SType, string Path) {
			Load(SType, FSys.ReadAllText(Path), Path);
		}

		public Shader(ShaderType SType, string Source, string Path) {
			Load(SType, Source, Path);
		}

		public Shader(string ShaderName) {
			ShaderType T = ShaderType.FragmentShader;
			if (ShaderName.EndsWith("vertex"))
				T = ShaderType.VertexShader;
			else if (ShaderName.EndsWith("fragment"))
				T = ShaderType.FragmentShader;
			else if (ShaderName.EndsWith("geometry"))
				T = ShaderType.GeometryShader;

			Load(T, GetShaderPath(ShaderName));
		}

		public Shader(ShaderType SType, string ShaderName) {
			Load(SType, GetShaderPath(ShaderName));
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
			if (Programs.Contains(Prog))
				Programs.Remove(Prog);
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
				throw new Exception(string.Format("Exception in shader {0}\n{1}", ShaderFile, GL.GetShaderInfoLog(ID)).Trim());
		}
	}
}