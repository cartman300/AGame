using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace AGame.Src.OGL {
	class VAO : GLObject {
		public PrimitiveType Primitive;

		public VAO(PrimitiveType Primitive) {
			ID = GL.GenVertexArray();
			this.Primitive = Primitive;
		}

		public override void Delete() {
			GL.DeleteVertexArray(ID);
		}

		public override void Bind() {
			GL.BindVertexArray(ID);
		}

		public override void Unbind() {
			GL.BindVertexArray(0);
		}

		public void DisableAttrib(int Idx) {
			GL.DisableVertexAttribArray(Idx);
		}

		public void DisableAttrib(int Idx, Vector4 Val) {
			DisableAttrib(Idx);
			GL.VertexAttrib4(Idx, ref Val);
		}

		public void DrawArrays(int First, int Count) {
			GL.DrawArrays(Primitive, First, Count);
		}

		public void DrawElements(int Count) {
			GL.DrawElements(Primitive, Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
		}
	}
}