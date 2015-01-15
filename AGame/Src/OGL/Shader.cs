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
		public bool Dirty;
		public FileSystemWatcher FileWatcher;

		string Dir, Fil, Pth;

		public Shader(ShaderType SType, string Pth) {
			Pth = Pth.Replace('\\', '/');

			this.SType = SType;
			ID = GL.CreateShader(SType);
			Programs = new List<ShaderProgram>();
			Dirty = true;

			Dir = Path.GetFullPath(Pth.Substring(0, Pth.LastIndexOf('/')));
			Fil = Pth.Substring(Pth.LastIndexOf('/') + 1);
			this.Pth = Path.Combine(Dir, Fil);

			FileWatcher = new FileSystemWatcher(Dir, Fil);
			FileWatcher.Changed += (S, E) => Dirty = true;
			FileWatcher.EnableRaisingEvents = true;
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

		public void Recompile() {
			Compile(File.ReadAllText(Pth));
		}

		public void Compile(string Src) {
			GL.ShaderSource(ID, Src);
			GL.CompileShader(ID);

			int Status = 0;
			GL.GetShader(ID, ShaderParameter.CompileStatus, out Status);

			if (Status != 1)
				throw new Exception(string.Format("Exception in shader {0}\n{1}", Fil, GL.GetShaderInfoLog(ID)).Trim());
		}
	}
}