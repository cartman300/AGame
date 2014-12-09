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
	class VBO : GLObject {
		public BufferTarget Target;
		public BufferUsageHint Hint;

		public VBO(BufferTarget Target, BufferUsageHint Hint) {
			ID = GL.GenBuffer();

			this.Target = Target;
			this.Hint = Hint;
		}

		public override void Delete() {
			GL.DeleteBuffer(ID);
		}

		public override void Bind() {
			GL.BindBuffer(Target, ID);
		}

		public override void Unbind() {
			GL.BindBuffer(Target, 0);
		}

		public void VertexAttribPointer(int Idx, int Size, int Stride = 0, int Offset = 0, VAPT PtrType = VAPT.Float) {
			Bind();
			GL.EnableVertexAttribArray(Idx);
			GL.VertexAttribPointer(Idx, Size, PtrType, false, Stride, Offset);
		}

		public void Data<T>(T[] Data) where T : struct {
			Bind();
			int Size = Data.Length;

			if (typeof(T) == typeof(Vector2))
				Size *= Vector2.SizeInBytes;
			else if (typeof(T) == typeof(Vector3))
				Size *= Vector3.SizeInBytes;
			else if (typeof(T) == typeof(Vector4))
				Size *= Vector4.SizeInBytes;
			else if (typeof(T) == typeof(uint))
				Size *= sizeof(uint);
			else
				throw new Exception("Type not supported: " + typeof(T).ToString());


			GL.BufferData<T>(Target, new IntPtr(Size), Data, Hint);
		}
	}
}