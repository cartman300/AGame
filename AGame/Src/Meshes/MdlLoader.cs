using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using AGame.Src.ModelFormats;
using AGame.Src.ModelFormats.Mdl;
using AGame.Utils;
using AGame.Src.OGL;

using OpenTK;

namespace AGame.Src.Meshes {
	unsafe class MdlLoader : ModelLoader {
		static IntPtr Load(string FileName) {
			byte[] Buffr = File.ReadAllBytes(FileName);

			void* P = Marshal.AllocHGlobal(Buffr.Length).Fill(Buffr.Length);
			Marshal.Copy(Buffr, 0, (IntPtr)P, Buffr.Length);
			return (IntPtr)P;
		}

		public override Model[] CreateModel(string FileName) {
			List<Model> Bodies = new List<Model>();

			string FName = FileName.Substring(0, FileName.Length - 4);

			studiohdr_t* Header = (studiohdr_t*)Load(FileName).ToPointer();
			vertexFileHeader_t* VVDHeader = (vertexFileHeader_t*)Load(FName + ".vvd").ToPointer();
			FileHeader_t* MeshHeader = (FileHeader_t*)Load(FName + ".sw.vtx").ToPointer();

			Assert(Header->id == Mdl.MDL_ID, "Invalid mdl ID");
			Assert(VVDHeader->id == Mdl.MODEL_VERTEX_FILE_ID, "Invalid vvd ID");
			Assert(VVDHeader->version == Mdl.MODEL_VERTEX_FILE_VERSION, "Invalid vvd version");
			Assert(VVDHeader->checksum == Header->checksum, "vvd <-> mdl checksum failed");

			for (int BodyID = 0; BodyID < Header->numbodyparts; BodyID++) {
				BodyPartHeader_t* VTXBodyPart = MeshHeader->pBodyPart(BodyID);
				mstudiobodyparts_t* BodyPart = Header->pBodypart(BodyID);

				Model TheModel = new Model();
				Bodies.Add(TheModel);

				for (int ModelID = 0; ModelID < BodyPart->nummodels; ModelID++) {
					ModelHeader_t* VtxModel = VTXBodyPart->pModel(ModelID);
					mstudiomodel_t* Model = BodyPart->pModel(ModelID);
					ModelLODHeader_t* VtxLOD = VtxModel->pLOD(VtxModel->numLODs - 1);

					for (int NMesh = 0; NMesh < Model->nummeshes; NMesh++) {
						mstudiomesh_t* PMesh = Model->pMesh(NMesh);
						mstudiotexture_t* Txtr = Header->pTexture(PMesh->material);
						MeshHeader_t* VtxMesh = VtxLOD->pMesh(NMesh);
						mstudio_meshvertexdata_t* VertData = PMesh->GetVertexData(VVDHeader);

						Mesh Msh = new Mesh(PMesh->numvertices);
						TheModel.Add(Msh, Engine.Generic3D);

						string IntName = Header->Name;
						string MatPath = "materials/models/" + IntName.Substring(0, IntName.IndexOf('/') + 1) + Txtr->Name + ".png";
						if (!File.Exists(MatPath))
							Console.WriteLine("Not found: {0}", MatPath);
						else
							Msh.Tex = Texture.FromFile(MatPath);


						for (int idx = 0; idx < PMesh->numvertices; idx++) {
							mstudiovertex_t* vert = VertData->Vertex(idx);
							Msh[idx] = new Vertex(vert->m_vecPosition, vert->m_vecNormal, vert->m_vecTexCoord);
						}

						for (int ngroup = 0; ngroup < VtxMesh->numStripGroups; ngroup++) {
							StripGroupHeader_t* stripgroup = VtxMesh->apStripGroup(ngroup);
							for (int nStrip = 0; nStrip < stripgroup->numStrips; nStrip++) {
								StripHeader_t* stripper = stripgroup->pStrip(nStrip);
								if ((stripper->flags & (byte)StripHeaderFlags_t.STRIP_IS_TRILIST) == 1)
									for (int i = 0; i < stripper->numIndices; i++)
										Msh.Inds.Add(stripgroup->pVertex(*stripgroup->pIndex(i +
											stripper->indexOffset))->origMeshVertID);

							}
						}
					}
				}
			}

			Marshal.FreeHGlobal((IntPtr)Header);
			Marshal.FreeHGlobal((IntPtr)VVDHeader);
			Marshal.FreeHGlobal((IntPtr)MeshHeader);
			return Bodies.ToArray();
		}

		public override bool CanLoad(string Filename) {
			return Filename.EndsWith(".mdl");
		}
	}
}