using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using AGame.Utils;
using AGame.Src.OGL;
using OpenTK;
using OpenTK.Graphics;

using ProtoBuf;

namespace AGame.Src.Meshes {
	[ProtoContract]
	class Model {
		[ProtoMember(1)]
		public List<Mesh> Meshes;

		[ProtoMember(2, IsPacked = true, OverwriteList = true)]
		byte[] SerData {
			get {
				MemoryStream Str = new MemoryStream();
				BinaryWriter SW = new BinaryWriter(Str);
				SW.Write(Color);
				SW.Write(Position);
				SW.Write(Scale);
				SW.Write(Rotation);
				return Str.ToArray();
			}
			set {
				BinaryReader SR = new BinaryReader(new MemoryStream(value));
				Color = SR.ReadColor4();
				Position = SR.ReadVector3();
				Scale = SR.ReadVector3();
				Rotation = SR.ReadQuaternion();
			}
		}

		public Color4 Color;
		public Vector3 Position;
		public Vector3 Scale;
		public Quaternion Rotation;

		public void Serialize(Stream Str) {
			Serializer.Serialize<Model>(Str, this);
		}

		public static Model FromStream(Stream Str) {
			return Serializer.Deserialize<Model>(Str);
		}

		public Model() {
			Meshes = new List<Mesh>();

			Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, 0);
			Color = Color4.White;
			Scale = new Vector3(1, 1, 1);
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