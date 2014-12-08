using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.States;

namespace AGame.Src {
	class Engine {
		public ShaderProgram Generic2D;
		public ShaderProgram Generic3D;

		State _ActiveState;
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

		public Engine() {
		}

		public void InitGL() {
			GL.ClearColor(Color4.CornflowerBlue);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			// Generic 2D shader
			Generic2D = new ShaderProgram(new Shader[] {
				new Shader(ShaderType.VertexShader, File.ReadAllText("Data/Shaders/Generic2D.vertex.glsl")),
				new Shader(ShaderType.FragmentShader, File.ReadAllText("Data/Shaders/Generic2D.fragment.glsl")),
			});
			Generic2D.BindFragDataLocation(0, "Color");
			Generic2D.Link();
			Generic2D.Bind();
		}
	}
}