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
	class Quads2D {
		bool OwnVAO;
		int Len;

		Vector2[] Pos, UVs;

		public VAO QuadVAO;
		public VBO QuadPos;
		public VBO QuadUV;

		public static Vector2[] Quad(Vector2 Pos, Vector2 Size, bool Triangles = false) {
			if (Triangles) {
				return new Vector2[] {
					new Vector2(Pos.X, Pos.Y + Size.Y),
					new Vector2(Pos.X + Size.X, Pos.Y + Size.Y),
					new Vector2(Pos.X + Size.X, Pos.Y),
					new Vector2(Pos.X, Pos.Y + Size.Y),
					new Vector2(Pos.X + Size.X, Pos.Y),
					new Vector2(Pos.X, Pos.Y)
				};
			}

			return new Vector2[] {
				new Vector2(Pos.X, Pos.Y + Size.Y),
				new Vector2(Pos.X + Size.X, Pos.Y + Size.Y),
				new Vector2(Pos.X + Size.X, Pos.Y),
				new Vector2(Pos.X, Pos.Y)
			};
		}

		public Quads2D(VAO Owner = null, BufferUsageHint Hint = BufferUsageHint.StaticDraw) {
			Pos = new Vector2[] { };
			UVs = new Vector2[] { };

			if (Owner != null) {
				QuadVAO = Owner;
				OwnVAO = false;
			} else {
				QuadVAO = new VAO(PrimitiveType.Quads);
				OwnVAO = true;
			}
			QuadVAO.Bind();

			QuadPos = new VBO(BufferTarget.ArrayBuffer, Hint);
			QuadUV = new VBO(BufferTarget.ArrayBuffer, Hint);
		}

		public Quads2D(VAO Owner, Vector2[] Pos, Vector2[] UVs, BufferUsageHint Hint = BufferUsageHint.StaticDraw)
			: this(Owner, Hint) {
			SetData(Pos, UVs);
		}

		public void GetData(out Vector2[] Pos, out Vector2[] UVs) {
			Pos = this.Pos;
			UVs = this.UVs;
		}

		public void SetData(Vector2[] Pos, Vector2[] UVs) {
			this.Pos = Pos;
			this.UVs = UVs;
			Len = Pos.Length;

			Bind();

			QuadPos.Bind();
			QuadPos.Data(Pos);
			QuadPos.VertexAttribPointer(Engine.Generic2D.PosAttrib);

			QuadUV.Bind();
			QuadUV.Data(UVs);
			QuadUV.VertexAttribPointer(Engine.Generic2D.UVAttrib);

			Unbind();
		}

		public void Delete() {
			if (OwnVAO)
				QuadVAO.Delete();
			QuadPos.Delete();
			QuadUV.Delete();
		}

		public void Bind() {
			QuadVAO.Bind();
		}

		public void Unbind() {
			QuadVAO.Unbind();
		}

		public void Render() {
			Bind();
			QuadVAO.DrawArrays(0, Len);
			Unbind();
		}
	}
}