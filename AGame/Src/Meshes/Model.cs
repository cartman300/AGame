using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AGame.Src.OGL;
using OpenTK;
using OpenTK.Graphics;

namespace AGame.Src.Meshes {
	class Model {
		public List<Mesh> Meshes;

		public bool MultiplyColor;
		public Color4 Color;
		public Vector3 Position;
		public float Scale;
		public Quaternion Rotation;

		public Model() {
			Meshes = new List<Mesh>();

			Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, 0);
			MultiplyColor = true;
			Color = Color4.White;
			Scale = 1.0f;
		}

		public void Add(Mesh M) {
			M.ModelParent = this;
			Meshes.Add(M);
		}

		public void GLInit(ShaderProgram S) {
			S.Bind();
			for (int i = 0; i < Meshes.Count; i++)
				Meshes[i].GLInit();
		}

		public void RenderOpaque() {
			Engine.Generic3D.Use(() => {
				for (int i = 0; i < Meshes.Count; i++)
					if (!Meshes[i].IsTransparent)
						Meshes[i].Render(ShaderProgram.LastActive);
			});
		}

		public void RenderTransparent() {
			Engine.Generic3D.Use(() => {
				for (int i = 0; i < Meshes.Count; i++)
					if (!Meshes[i].IsTransparent)
						Meshes[i].Render(ShaderProgram.LastActive);
			});
		}
	}
}