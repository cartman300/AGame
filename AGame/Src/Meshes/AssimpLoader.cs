using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using AGame.Utils;
using Assimp;
using Assimp.Configs;
using OpenTK;
using Quaternion = OpenTK.Quaternion;

namespace AGame.Src.Meshes {
	class AssimpLoader : ModelLoader {
		AssimpContext Ass;

		public AssimpLoader() {
			Ass = new AssimpContext();
		}

		public override bool CanLoad(string Filename) {
			string[] Formats = Ass.GetSupportedImportFormats();
			for (int i = 0; i < Formats.Length; i++)
				if (Filename.EndsWith(Formats[i]))
					return true;
			return false;
		}

		Vector3 SwapZY(Vector3 V, bool IsBSP) {
			if (IsBSP)
				return V.Xzy;
			return V;
		}

		Quaternion SwapZY(Quaternion Q, bool DoSwap) {
			if (DoSwap)
				return new Quaternion(Q.Xyz.Xzy, Q.W);
			return Q;
		}

		Vector2 SwapUV(Vector3 UV, bool DoSwap) {
			if (DoSwap)
				return -UV.Xy;
			return UV.Xy;
		}

		public override Model[] CreateModel(string FileName) {
			PostProcessSteps Steps = PostProcessSteps.GenerateNormals | PostProcessSteps.GenerateUVCoords;
			Steps |= PostProcessSteps.FindInvalidData /*| PostProcessSteps.OptimizeMeshes | PostProcessSteps.OptimizeGraph*/;
			Steps |= PostProcessSteps.SortByPrimitiveType | PostProcessSteps.Triangulate;

			bool DoSwap = false;
			bool DoSwapUV = false;

			string Ext = Path.GetExtension(FileName);

			if (Ext == ".pk3") {
				DoSwap = true;
				DoSwapUV = true;
			}

			if (Ext == ".pk3")
				Steps |= PostProcessSteps.FlipWindingOrder | PostProcessSteps.FlipUVs;

			Scene S = null;
			if (Ext == ".pk3")
				S = Ass.ImportFile(FileName, Steps);
			else
				S = Ass.ImportFileFromStream(FSys.LoadToMemory(FileName), Steps, Path.GetExtension(FileName));
			Model M = new Model();

			Action<Node> LoadNodes = null;
			LoadNodes = (N) => {
				if (N.HasMeshes)
					for (int i = 0; i < N.MeshCount; i++) {
						Assimp.Mesh AM = S.Meshes[N.MeshIndices[i]];
						Mesh Msh = new Mesh(AM.VertexCount);
						N.Transform.DecomposeToMesh(Msh);

						Msh.Scale = SwapZY(Msh.Scale, DoSwap);
						Msh.Position = SwapZY(Msh.Position, DoSwap);
						Msh.Rotation = SwapZY(Msh.Rotation, DoSwap);

						if (AM.HasVertices) {
							for (int j = 0; j < AM.VertexCount; j++) {
								Vector2 UV = Vector2.Zero;

								if (AM.TextureCoordinateChannelCount != 0)
									UV = SwapUV(AM.TextureCoordinateChannels[0][j].ToVector3(), DoSwapUV);

								Msh[j] = new Vertex(SwapZY(AM.Vertices[j].ToVector3(), DoSwap),
									SwapZY(AM.Normals[j].ToVector3(), DoSwap), UV);
							}
							Msh.Inds.AddRange(AM.GetUnsignedIndices());

							Material Mat = S.Materials[AM.MaterialIndex];
							/*if (Mat.HasColorAmbient)
								Msh.Color = Msh.Color.Mult(Mat.ColorAmbient.ToColor());*/

							if (Mat.HasTextureDiffuse) {
								Msh.Tex = Mat.TextureDiffuse.ToTexture();
								/*TextureSlot TS;
								Mat.GetMaterialTexture(TextureType.Diffuse, 0, out TS);
								Msh.Tex = S.Textures[TS.TextureIndex].ToTexture(TS);*/
							}

							if (DoSwap)
								Msh.Inds.Reverse();
							if (AM.VertexCount > 0)
								M.Add(Msh);
						}
					}

				if (N.HasChildren)
					for (int i = 0; i < N.Children.Count; i++)
						LoadNodes(N.Children[i]);
			};

			LoadNodes(S.RootNode);
			S.Clear();
			return new Model[] { M };
		}
	}
}