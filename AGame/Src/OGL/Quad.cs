using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;

namespace AGame.Src.OGL {
	class Quad2D {
		public VAO QuadVAO;
		public VBO QuadPos;
		public VBO QuadUV;

		public Quad2D(Vector2 Pos, Vector2 Size, BufferUsageHint Hint = BufferUsageHint.StaticDraw) {
			QuadVAO = new VAO(PrimitiveType.Quads);
			QuadVAO.Bind();

			float HSzX = Size.X / 2; // Half size X
			float HSzY = Size.Y / 2; // Half size Y

			QuadPos = new VBO(BufferTarget.ArrayBuffer, Hint);
			QuadPos.Data(new Vector2[] {
				new Vector2(Pos.X, Pos.Y + Size.Y),
				new Vector2(Pos.X + Size.X, Pos.Y + Size.Y),
				new Vector2(Pos.X + Size.X, Pos.Y),
				new Vector2(Pos.X, Pos.Y),
			});
			QuadPos.VertexAttribPointer(Game.Engine.Generic2D.PosAttrib, 2);

			QuadUV = new VBO(BufferTarget.ArrayBuffer, Hint);
			QuadUV.Data(new Vector2[] {
				new Vector2(0.0f, 0.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(1.0f, 1.0f),
				new Vector2(0.0f, 1.0f),
			});
			QuadUV.VertexAttribPointer(Game.Engine.Generic2D.UVAttrib, 2);
		}

		public void Bind() {
			QuadVAO.Bind();
		}

		public void Render() {
			QuadVAO.DrawArrays(0, 4);
		}
	}
}