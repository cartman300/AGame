using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using AGame.Src.ModelFormats;
using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.Meshes;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace AGame.Src.Meshes {
	struct Vertex {
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 UV;

		public Vertex(Vector3 Position, Vector3 Normal, Vector2 UV) {
			this.Position = Position;
			this.Normal = Normal;
			this.UV = UV;
		}

		public Vertex(Vector3 Position) {
			this.Position = Position;
			this.Normal = Vector3.Zero;
			this.UV = Vector2.Zero;
		}
	}

	class Mesh {
		public static uint[] Triangulate(uint A, uint B, uint C, uint D, bool CW = true) {
			uint[] Ret = new uint[] { A, B, C, C, D, A };
			if (!CW)
				Ret = Ret.Reverse().ToArray();
			return Ret;
		}

		public static Mesh CreateCuboid(float W, float H, float D) {
			Mesh Cuboid = new Mesh(8);

			Cuboid[0] = new Vertex(new Vector3(W / -2f, H / -2f, D / -2f));
			Cuboid[1] = new Vertex(new Vector3(W / -2f, H / 2f, D / -2f));
			Cuboid[2] = new Vertex(new Vector3(W / 2f, H / 2f, D / -2f));
			Cuboid[3] = new Vertex(new Vector3(W / 2f, H / -2f, D / -2f));
			Cuboid[4] = new Vertex(new Vector3(W / -2f, H / -2f, D / 2f));
			Cuboid[5] = new Vertex(new Vector3(W / -2f, H / 2f, D / 2f));
			Cuboid[6] = new Vertex(new Vector3(W / 2f, H / 2f, D / 2f));
			Cuboid[7] = new Vertex(new Vector3(W / 2f, H / -2f, D / 2f));

			Cuboid.Inds.AddRange(Mesh.Triangulate(0, 1, 2, 3, false));
			Cuboid.Inds.AddRange(Mesh.Triangulate(3, 2, 6, 7, false));
			Cuboid.Inds.AddRange(Mesh.Triangulate(7, 6, 5, 4, false));
			Cuboid.Inds.AddRange(Mesh.Triangulate(4, 5, 1, 0, false));
			Cuboid.Inds.AddRange(Mesh.Triangulate(1, 5, 6, 2, false));
			Cuboid.Inds.AddRange(Mesh.Triangulate(3, 7, 4, 0, false));
			return Cuboid;
		}

		public Vertex[] Verts;
		public List<uint> Inds;

		public Model ModelParent;

		public Mesh(int Len, bool Transparent = false) {
			Verts = new Vertex[Len];
			Inds = new List<uint>();
			IsTransparent = Transparent;
		}

		/*public void Unroll() {
			Vertex[] Vrts = new Vertex[Inds.Count];
			for (int i = 0; i < Inds.Count; i++)
				Vrts[i] = Verts[Inds[i]];
			Verts = Vrts.Reverse().ToArray();
		}*/

		public VAO MeshVAO;
		public Texture Tex;
		public bool IsTransparent;

		public void GLInit() {
			//Unroll();
			Inds.Reverse();

			MeshVAO = new VAO(PrimitiveType.Triangles);
			Bind();

			if (ShaderProgram.ActiveShader.PosAttrib >= 0) {
				VBO Positions = new VBO(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
				Positions.Bind();
				Positions.Data(this.Positions);
				Positions.VertexAttribPointer(ShaderProgram.ActiveShader.PosAttrib);
			}

			if (ShaderProgram.ActiveShader.NormAttrib >= 0) {
				VBO Normals = new VBO(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
				Normals.Bind();
				Normals.Data(this.Normals);
				Normals.VertexAttribPointer(ShaderProgram.ActiveShader.NormAttrib);
			}

			if (ShaderProgram.ActiveShader.UVAttrib >= 0) {
				VBO UVs = new VBO(BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);
				UVs.Bind();
				UVs.Data(this.UVs);
				UVs.VertexAttribPointer(ShaderProgram.ActiveShader.UVAttrib);
			}

			VBO Elements = new VBO(BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw);
			Elements.Bind();
			Elements.Data(Indices);

			Unbind();
		}

		public void Bind() {
			MeshVAO.Bind();
		}

		public void Unbind() {
			MeshVAO.Unbind();
		}

		public void Render(ShaderProgram S) {
			if (Tex != null)
				Tex.Bind();
			Bind();
			S.Use(ModelParent.Position, ModelParent.Rotation, ModelParent.Scale,
				ModelParent.Color, ModelParent.MultiplyColor, () => MeshVAO.DrawElements(Inds.Count));
			Unbind();
			if (Tex != null)
				Tex.Unbind();
		}

		public Vector3[] Positions {
			get {
				Vector3[] Pos = new Vector3[Verts.Length];
				for (int i = 0; i < Pos.Length; i++)
					Pos[i] = Verts[i].Position;
				return Pos;
			}
		}

		public Vector3[] Normals {
			get {
				Vector3[] Norms = new Vector3[Verts.Length];
				for (int i = 0; i < Norms.Length; i++)
					Norms[i] = Verts[i].Normal;
				return Norms;
			}
		}

		public Vector2[] UVs {
			get {
				Vector2[] UVs = new Vector2[Verts.Length];
				for (int i = 0; i < UVs.Length; i++)
					UVs[i] = Verts[i].UV;
				return UVs;
			}
		}

		public uint[] Indices {
			get {
				return Inds.ToArray();
			}
		}

		public Vertex this[int Idx] {
			get {
				return Verts[Idx];
			}
			set {
				Verts[Idx] = value;
			}
		}
	}
}