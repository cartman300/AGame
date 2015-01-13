using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGame.Src.OGL;

namespace AGame.Src.Meshes {
	class Model {
		public List<Mesh> Meshes;
		public List<ShaderProgram> Shaders;

		public Model() {
			Meshes = new List<Mesh>();
			Shaders = new List<ShaderProgram>();
		}

		public void Add(Mesh M, ShaderProgram S) {
			Meshes.Add(M);
			Shaders.Add(S);
		}

		public void GLInit() {
			for (int i = 0; i < Meshes.Count; i++)
				Meshes[i].GLInit(Shaders[i]);
		}

		public void RenderOpaque() {
			for (int i = 0; i < Meshes.Count; i++)
				if (!Meshes[i].IsTransparent)
					Meshes[i].Render();
		}

		public void RenderTransparent() {
			for (int i = 0; i < Meshes.Count; i++)
				if (!Meshes[i].IsTransparent)
					Meshes[i].Render();
		}
	}
}