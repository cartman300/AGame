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
		int Size, DataSize;
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

		public void VertexAttribPointer(int Idx, int Size = 0, int Stride = 0, int Offset = 0, VAPT PtrType = VAPT.Float) {
			Bind();
			if (Size <= 0)
				Size = DataSize;
			GL.EnableVertexAttribArray(Idx);
			GL.VertexAttribPointer(Idx, Size, PtrType, false, Stride, Offset);
		}

		public void Rellocate() {
			GL.BufferData(Target, new IntPtr(Size), IntPtr.Zero, Hint);
		}

		public void Data<T>(T[] Data) where T : struct {
			Bind();
			Size = Data.Length;

			if (typeof(T) == typeof(Vector2)) {
				Size *= Vector2.SizeInBytes;
				DataSize = 2;
			} else if (typeof(T) == typeof(Vector3)) {
				Size *= Vector3.SizeInBytes;
				DataSize = 3;
			} else if (typeof(T) == typeof(Vector4)) {
				Size *= Vector4.SizeInBytes;
				DataSize = 4;
			} else if (typeof(T) == typeof(uint)) {
				Size *= sizeof(uint);
				DataSize = 1;
			} else
				throw new Exception("Type not supported: " + typeof(T).ToString());

			GL.BufferData<T>(Target, new IntPtr(Size), Data, Hint);
		}
	}
}