using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using vector3df = OpenTK.Vector3;
using vector2df = OpenTK.Vector2;
using quaternion = OpenTK.Quaternion;

namespace AGame.Src.ModelFormats.Mdl {
	unsafe static class Mdl {
		public const int MAX_NUM_LODS = 8;
		public const int MODEL_VERTEX_FILE_ID = (('V' << 24) + ('S' << 16) + ('D' << 8) + 'I');
		public const int MDL_ID = (('T' << 24) + ('S' << 16) + ('D' << 8) + 'I');
		public const int MODEL_VERTEX_FILE_VERSION = 4;
		public const int MAX_NUM_BONES_PER_VERT = 3;

		public const int STUDIO_ANIM_RAWPOS = 0x01; // Vector48
		public const int STUDIO_ANIM_RAWROT = 0x02; // Quaternion48
		public const int STUDIO_ANIM_ANIMPOS = 0x04;// mstudioanim_valueptr_t
		public const int STUDIO_ANIM_ANIMROT = 0x08; // mstudioanim_valueptr_t
		public const int STUDIO_ANIM_DELTA = 0x10;
		public const int STUDIO_ANIM_RAWROT2 = 0x20;// Quaternion64

		public const int MAXSTUDIOTRIANGLES = 25000;
		public const int MAXSTUDIOVERTS = 10000;
		public const int MAXSTUDIOFLEXVERTS = 1000;

		public const int MAXSTUDIOSKINS = 32;	// total textures
		public const int MAXSTUDIOBONES = 128;	// total bones actually used
		public const int MAXSTUDIOFLEXDESC = 1024;	// maximum number of low level flexes (actual morph targets)
		public const int MAXSTUDIOFLEXCTRL = 96;// maximum number of flexcontrollers (input sliders)
		public const int MAXSTUDIOPOSEPARAM = 24;
		public const int MAXSTUDIOBONECTRLS = 4;
		public const int MAXSTUDIOANIMBLOCKS = 256;
		public const int MAXSTUDIOBONEBITS = 7;// NOTE: MUST MATCH MAXSTUDIOBONES

		// Custom
		public const int MAXSTUDIOSTRIPS = MAXSTUDIOVERTS * 3;
		public const int MAXSTUDIOSTRIPGROUPS = 128;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudioboneweight_t {
		public fixed float weight[Mdl.MAX_NUM_BONES_PER_VERT];
		public fixed byte bone[Mdl.MAX_NUM_BONES_PER_VERT];
		public byte numbones;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudiovertex_t {
		public mstudioboneweight_t m_BoneWeights;
		public vector3df m_vecPosition;
		public vector3df m_vecNormal;
		public vector2df m_vecTexCoord;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct vertexFileHeader_t {
		public int id; // MODEL_VERTEX_FILE_ID
		public int version; // MODEL_VERTEX_FILE_VERSION
		public int checksum; // same as studiohdr_t, ensures sync
		public int numLODs; // num of valid lods
		public fixed int numLODVertexes[Mdl.MAX_NUM_LODS]; // num verts for desired root lod
		public int numFixups; // num of vertexFileFixup_t
		public int fixupTableStart; // offset from base to fixup table
		public int vertexDataStart; // offset from base to vertex block
		public int tangentDataStart; // offset from base to tangent block

		public mstudiovertex_t* GetVertexData() {
			fixed (int* idPtr = &id) {
				vertexFileHeader_t* This = (vertexFileHeader_t*)idPtr;

				if ((This->id == Mdl.MODEL_VERTEX_FILE_ID) && (This->vertexDataStart != 0))
					return (mstudiovertex_t*)(This->vertexDataStart + (byte*)This);
				else
					return null;
			}
		}

		public quaternion* GetTangentData() {
			fixed (int* idPtr = &id) {
				vertexFileHeader_t* This = (vertexFileHeader_t*)idPtr;

				if ((This->id == Mdl.MODEL_VERTEX_FILE_ID) && (This->tangentDataStart != 0))
					return (quaternion*)(This->tangentDataStart + (byte*)This);
				else
					return null;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct matrix3x4_t {
		public fixed float mtx[3 * 4];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiobone_t {
		public int sznameindex;
		//public byte* pszName()  { return ((byte *)&this) + sznameindex; } TODO
		public int parent; // parent bone
		public fixed int bonecontroller[6]; // bone controller index, -1 == none
		// default values
		public vector3df pos;
		public quaternion quat;
		public vector3df rot;
		// compression scale
		public vector3df posscale;
		public vector3df rotscale;
		public matrix3x4_t poseToBone;
		public quaternion qAlignment;
		public int flags;
		public int proctype;
		public int procindex; // procedural rule
		public int physicsbone; // index into physically simulated bone
		// TODO
		//inline void *pProcedure() const { if (procindex == 0) return NULL; else return (void *)(((byte *)this) + procindex); };
		public int surfacepropidx; // index into string tablefor property name
		// TODO
		// inline byte * const pszSurfaceProp(void) const { return ((byte *)this) + surfacepropidx; }
		public int contents; // See BSPFlags.h for the contents flags
		public fixed int unused[8]; // remove as appropriate
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiotexture_t {
		public int sznameindex;

		public string Name {
			get {
				fixed (int* sznameindexPtr = &sznameindex) {
					mstudiotexture_t* This = (mstudiotexture_t*)sznameindexPtr;
					return Marshal.PtrToStringAnsi((IntPtr)(((byte*)This) + This->sznameindex));
				}
			}
		}

		public int flags;
		public int used;
		public int unused1;
		public void* material; // fixme: this needs to go away . .isn't used by the engine, but is used by studiomdl
		public void* clientmaterial; // gary, replace with client material pointer if used
		public fixed int unused[10];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudio_modelvertexdata_t {
		// TODO
		/*vector3df *Position(int i) const;
		vector3df *Normal(int i) const;
		quaternion *TangentS(int i) const;
		vector2df *Texcoord(int i) const;
		mstudioboneweight_t     *BoneWeights(int i) const;
		bool    HasTangentData(void) const;
		int     GetGlobalTangentIndex(int i) const;*/
		// base of external vertex data stores

		int GetGlobalVertexIndex(int i) {
			fixed (void** pVertexDataPtr = &pVertexData) {
				mstudio_modelvertexdata_t* This = (mstudio_modelvertexdata_t*)pVertexDataPtr;
				mstudiomodel_t* modelptr = (mstudiomodel_t*)((byte*)This -
					Marshal.OffsetOf(typeof(mstudiomodel_t), "vertexdata").ToInt32());
				return (i + (modelptr->vertexindex / sizeof(mstudiovertex_t)));
			}
		}

		public mstudiovertex_t* Vertex(int i) {
			fixed (void** pVertexDataPtr = &pVertexData) {
				mstudio_modelvertexdata_t* This = (mstudio_modelvertexdata_t*)pVertexDataPtr;
				return (mstudiovertex_t*)This->pVertexData + This->GetGlobalVertexIndex(i);
			}
		}

		public void* pVertexData;
		public void* pTangentData;
	}


	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudio_meshvertexdata_t {
		// TODO
		/*vector3df *Position(int i) const;
		vector3df *Normal(int i) const;
		quaternion *TangentS(int i) const;
		vector2df *Texcoord(int i) const;
		mstudioboneweight_t *BoneWeights(int i) const;
		bool    HasTangentData(void) const;
		int     GetGlobalVertexIndex(int i) const;*/

		public int GetModelVertexIndex(int i) {
			fixed (mstudio_modelvertexdata_t** modelvertexdataPtr = &modelvertexdata) {
				mstudio_meshvertexdata_t* This = (mstudio_meshvertexdata_t*)modelvertexdataPtr;

				mstudiomesh_t* meshptr = (mstudiomesh_t*)((byte*)This -
					Marshal.OffsetOf(typeof(mstudiomesh_t), "vertexdata").ToInt32());
				return meshptr->vertexoffset + i;
			}
		}

		public mstudiovertex_t* Vertex(int i) {
			return modelvertexdata->Vertex(GetModelVertexIndex(i));
		}

		// indirection to this mesh's model's vertex data
		public mstudio_modelvertexdata_t* modelvertexdata;
		// used for fixup calcs when culling top level lods
		// expected number of mesh verts at desired lod
		public fixed int numLODVertexes[Mdl.MAX_NUM_LODS];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiovertanim_t {
		public ushort index;
		public byte speed; // 255/max_length_in_flex
		public byte side; // 255/left_right
		//protected:
		// JasonM changing this type a lot, to prefer fixed point 16 bit...
		public fixed short delta[3];
		public fixed short ndelta[3];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiovertanim_wrinkle_t /*: mstudiovertanim_t*/ {
		public ushort index;
		public byte speed; // 255/max_length_in_flex
		public byte side; // 255/left_right
		//protected:
		// JasonM changing this type a lot, to prefer fixed point 16 bit...
		public fixed short delta[3];
		public fixed short ndelta[3];
		public short wrinkledelta;//f16
	}

	public enum StudioVertAnimType_t {
		STUDIO_VERT_ANIM_NORMAL = 0,
		STUDIO_VERT_ANIM_WRINKLE,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudioflex_t {
		public int flexdesc; // input value
		public float target0; // zero
		public float target1; // one
		public float target2; // one
		public float target3; // zero
		public int numverts;
		public int vertindex;
		// TODO
		/*mstudiovertanim_t *pVertanim(int i) const { return (mstudiovertanim_t *)(((byte *)this) + vertindex) + i; };
		mstudiovertanim_wrinkle_t *pVertanimWrinkle(int i) const { return (mstudiovertanim_wrinkle_t *)(((byte *)this) + vertindex) + i; };
		byte *pBaseVertanim() const { return ((byte *)this) + vertindex; };
		int     VertAnimSizeBytes() const { return (vertanimtype == STUDIO_VERT_ANIM_NORMAL) ? sizeof(mstudiovertanim_t) : sizeof(mstudiovertanim_wrinkle_t); }*/
		public int flexpair; // second flex desc
		public byte vertanimtype; // See StudioVertAnimType_t
		public fixed byte unusedchar[3];
		public fixed int unused[6];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiomesh_t {
		public int material;
		public int modelindex;

		public mstudiomodel_t* pModel() {
			fixed (int* materialPtr = &material) {
				mstudiomesh_t* This = (mstudiomesh_t*)materialPtr;
				return (mstudiomodel_t*)(((byte*)This) + This->modelindex);
			}
		}

		public int numvertices; // number of unique vertices/normals/texcoords
		public int vertexoffset; // vertex mstudiovertex_t
		// Access thin/fat mesh vertex data (only one will return a non-NULL result)
		public mstudio_meshvertexdata_t* GetVertexData(void* pModelData = null) {
			fixed (int* materialPtr = &material) {
				mstudiomesh_t* This = (mstudiomesh_t*)materialPtr;

				// get this mesh's model's vertex data (allow for mstudiomodel_t::GetVertexData
				// returning NULL if the data has been converted to 'thin' vertices)
				This->pModel()->GetVertexData(pModelData);
				This->vertexdata.modelvertexdata = &(This->pModel()->vertexdata);
				if (This->vertexdata.modelvertexdata->pVertexData == null)
					return null;

				return &This->vertexdata;
			}
		}
		//const thinModelVertices_t *GetThinVertexData(void *pModelData = NULL);
		public int numflexes; // vertex animation
		public int flexindex;
		//inline mstudioflex_t *pFlex(int i) const { return (mstudioflex_t *)(((byte *)this) + flexindex) + i; }; TODO
		// special codes for material operations
		public int materialtype;
		public int materialparam;
		// a unique ordinal for this mesh
		public int meshid;
		public vector3df center;
		public mstudio_meshvertexdata_t vertexdata;
		public fixed int unused[8]; // remove as appropriate
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiomodel_t {
		// TODO
		//const byte * pszName(void) const { return name; }
		public fixed byte name[64];
		public int type;
		public float boundingradius;
		public int nummeshes;
		public int meshindex;

		public mstudiomesh_t* pMesh(int i) {
			fixed (byte* namePtr = name) {
				mstudiomodel_t* This = (mstudiomodel_t*)namePtr;
				return (mstudiomesh_t*)(((byte*)This) + This->meshindex) + i;
			}
		}

		// cache purposes
		public int numvertices; // number of unique vertices/normals/texcoords
		public int vertexindex; // vertex Vector
		public int tangentsindex; // tangents Vector
		// These functions are defined in application-specific code:
		// TODO
		//const vertexFileHeader_t *CacheVertexData(void *pModelData);
		// Access thin/fat mesh vertex data (only one will return a non-NULL result)

		public mstudio_modelvertexdata_t* GetVertexData(void* pModelData = null) {
			fixed (byte* namePtr = name) {
				mstudiomodel_t* This = (mstudiomodel_t*)namePtr;

				vertexFileHeader_t* pVertexHdr = (vertexFileHeader_t*)pModelData;
				if (pVertexHdr == null) {
					This->vertexdata.pVertexData = null;
					This->vertexdata.pTangentData = null;
					return null;
				}
				This->vertexdata.pVertexData = pVertexHdr->GetVertexData();
				This->vertexdata.pTangentData = pVertexHdr->GetTangentData();
				if (vertexdata.pVertexData == null)
					return null;
				return &This->vertexdata;
			}
		}
		//const thinModelVertices_t *GetThinVertexData(void *pModelData = NULL);
		public int numattachments;
		public int attachmentindex;
		public int numeyeballs;
		public int eyeballindex;
		//inline mstudioeyeball_t *pEyeball(int i) { return (mstudioeyeball_t *)(((byte *)this) + eyeballindex) + i; };
		public mstudio_modelvertexdata_t vertexdata;
		public fixed int unused[8]; // remove as appropriate
	}
	/*
   inline bool mstudio_modelvertexdata_t::HasTangentData(void) const
   {
	   return (pTangentData != NULL);
   }
   inline int mstudio_modelvertexdata_t::GetGlobalTangentIndex(int i) const
   {
	   mstudiomodel_t *modelptr = (mstudiomodel_t *)((byte *)this - offsetof(mstudiomodel_t, vertexdata));
	   return (i + (modelptr->tangentsindex / sizeof(quaternion)));
   }
   inline vector3df *mstudio_modelvertexdata_t::Position(int i) const
   {
	   return &Vertex(i)->m_vecPosition;
   }
   inline vector3df *mstudio_modelvertexdata_t::Normal(int i) const
   {
	   return &Vertex(i)->m_vecNormal;
   }
 
   inline quaternion *mstudio_modelvertexdata_t::TangentS(int i) const
   {
	   // NOTE: The tangents vector is 16-bytes in a separate array
	   // because it only exists on the high end, and if I leave it out
	   // of the mstudiovertex_t, the vertex is 64-bytes (good for low end)
	   return (quaternion *)pTangentData + GetGlobalTangentIndex(i);
   }
   inline vector2df *mstudio_modelvertexdata_t::Texcoord(int i) const
   {
	   return &Vertex(i)->m_vecTexCoord;
   }
   inline mstudioboneweight_t *mstudio_modelvertexdata_t::BoneWeights(int i) const
   {
	   return &Vertex(i)->m_BoneWeights;
   }

   inline int mstudio_meshvertexdata_t::GetGlobalVertexIndex(int i) const
   {
	   return modelvertexdata->GetGlobalVertexIndex(GetModelVertexIndex(i));
   }
   inline vector3df *mstudio_meshvertexdata_t::Position(int i) const
   {
	   return modelvertexdata->Position(GetModelVertexIndex(i));
   };
   inline vector3df *mstudio_meshvertexdata_t::Normal(int i) const
   {
	   return modelvertexdata->Normal(GetModelVertexIndex(i));
   };
   inline quaternion *mstudio_meshvertexdata_t::TangentS(int i) const
   {
	   return modelvertexdata->TangentS(GetModelVertexIndex(i));
   }
   inline vector2df *mstudio_meshvertexdata_t::Texcoord(int i) const
   {
	   return modelvertexdata->Texcoord(GetModelVertexIndex(i));
   };
   inline mstudioboneweight_t *mstudio_meshvertexdata_t::BoneWeights(int i) const
   {
	   return modelvertexdata->BoneWeights(GetModelVertexIndex(i));
   };
	 */

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiobodyparts_t {
		public int sznameindex;
		// TODO
		//inline byte * const pszName(void) const { return ((byte *)this) + sznameindex; }
		public int nummodels;
		public int Base;
		public int modelindex; // index into models array

		public mstudiomodel_t* pModel(int i) {
			fixed (int* sznameindexptr = &sznameindex) {
				mstudiobodyparts_t* This = (mstudiobodyparts_t*)sznameindexptr;
				return (mstudiomodel_t*)(((byte*)This) + This->modelindex) + i;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BoneStateChangeHeader_t {
		public int hardwareID;
		public int newBoneID;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Vertex_t {
		// these index into the mesh's vert[origMeshVertID]'s bones
		public fixed byte boneWeightIndex[Mdl.MAX_NUM_BONES_PER_VERT];
		public byte numBones;
		public ushort origMeshVertID;
		// for sw skinned verts, these are indices into the global list of bones
		// for hw skinned verts, these are hardware bone indices
		public fixed byte boneID[Mdl.MAX_NUM_BONES_PER_VERT];
	}

	public enum StripHeaderFlags_t {
		STRIP_IS_TRILIST = 0x01,
		STRIP_IS_TRISTRIP = 0x02
	}

	// a strip is a piece of a stripgroup that is divided by bones
	// (and potentially tristrips if we remove some degenerates.)
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StripHeader_t {
		// indexOffset offsets into the mesh's index array.
		public int numIndices;
		public int indexOffset;
		// vertexOffset offsets into the mesh's vert array.
		public int numVerts;
		public int vertOffset;
		// use this to enable/disable skinning.
		// May decide (in optimize.cpp) to put all with 1 bone in a different strip
		// than those that need skinning.
		public short numBones;
		public byte flags;
		public int numBoneStateChanges;
		public int boneStateChangeOffset;
		// TODO
		/*inline BoneStateChangeHeader_t *pBoneStateChange(int i) const
		{
				return (BoneStateChangeHeader_t *)(((byte *)this) + boneStateChangeOffset) + i;
		};*/
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studiomeshgroup_t {
		//scene::IMesh *m_pMesh;
		public void* m_pMesh;
		public int m_NumStrips;
		public int m_Flags; // see studiomeshgroupflags_t
		public StripHeader_t* m_pStripData;
		public ushort* m_pGroupIndexToMeshIndex;
		public int m_NumVertices;
		public int* m_pUniqueTris; // for performance measurements
		public ushort* m_pIndices;
		public bool m_MeshNeedsRestore;
		public short m_ColorMeshID;
		public void* m_pMorph;
		// TODO
		//inline unsigned short MeshIndex(int i) const { return m_pGroupIndexToMeshIndex[m_pIndices[i]]; }
	}

	// studio model data
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studiomeshdata_t {
		public int m_NumGroup;
		public studiomeshgroup_t* m_pMeshGroup;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studioloddata_t {
		// not needed - this is really the same as studiohwdata_t.m_NumStudioMeshes
		//int m_NumMeshes;
		public studiomeshdata_t* m_pMeshData; // there are studiohwdata_t.m_NumStudioMeshes of these.
		public float m_SwitchPoint;
		// one of these for each lod since we can switch to simpler materials on lower lods.
		public int numMaterials;
		public void** ppMaterials; /* will have studiohdr_t.numtextures elements allocated */
		// hack - this needs to go away.
		public int* pMaterialFlags; /* will have studiohdr_t.numtextures elements allocated */
		// For decals on hardware morphing, we must actually do hardware skinning
		// For this to work, we have to hope that the total # of bones used by
		// hw flexed verts is < than the max possible for the dx level we're running under
		public int* m_pHWMorphDecalBoneRemap;
		public int m_nDecalBoneCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studiohwdata_t {
		public int m_RootLOD; // calced and clamped, nonzero for lod culling
		public int m_NumLODs;
		public studioloddata_t* m_pLODs;
		public int m_NumStudioMeshes;
		//TODO
		/*inline float LODMetric(float unitSphereSize) const { return (unitSphereSize != 0.0f) ? (100.0f / unitSphereSize) : 0.0f; }
		inline int GetLODForMetric(float lodMetric) const
		{
				if (!m_NumLODs)
						return 0;
				// shadow lod is specified on the last lod with a negative switch
				// never consider shadow lod as viable candidate
				int numLODs = (m_pLODs[m_NumLODs - 1].m_SwitchPoint < 0.0f) ? m_NumLODs - 1 : m_NumLODs;
				for (int i = m_RootLOD; i < numLODs - 1; i++)
				{
						if (m_pLODs[i + 1].m_SwitchPoint > lodMetric)
								return i;
				}
				return numLODs - 1;
		}*/
	}

	public enum StripGroupFlags_t {
		STRIPGROUP_IS_FLEXED = 0x01,
		STRIPGROUP_IS_HWSKINNED = 0x02,
		STRIPGROUP_IS_DELTA_FLEXED = 0x04,
		STRIPGROUP_SUPPRESS_HW_MORPH = 0x08, // NOTE: This is a temporary flag used at run time.
	}

	// a locking group
	// a single vertex buffer
	// a single index buffer
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct StripGroupHeader_t {
		// These are the arrays of all verts and indices for this mesh. strips index into this.
		public int numVerts;
		public int vertOffset;

		public Vertex_t* pVertex(int i) {
			fixed (int* numVertsPtr = &numVerts) {
				StripGroupHeader_t* This = (StripGroupHeader_t*)numVertsPtr;
				return (Vertex_t*)(((byte*)This) + This->vertOffset) + i;
			}
		}

		public int numIndices;
		public int indexOffset;

		public ushort* pIndex(int i) {
			fixed (int* numVertsPtr = &numVerts) {
				StripGroupHeader_t* This = (StripGroupHeader_t*)numVertsPtr;
				return (ushort*)(((byte*)This) + This->indexOffset) + i;
			}
		}

		public int numStrips;
		public int stripOffset;

		public StripHeader_t* pStrip(int i) {
			fixed (int* numVertsPtr = &numVerts) {
				StripGroupHeader_t* This = (StripGroupHeader_t*)numVertsPtr;
				return (StripHeader_t*)(((byte*)This) + This->stripOffset) + i;
			}
		}
		public byte flags;
	}

	public enum MeshFlags_t {
		// these are both material properties, and a mesh has a single material.
		MESH_IS_TEETH = 0x01,
		MESH_IS_EYES = 0x02
	}

	// a collection of locking groups:
	// up to 4:
	// non-flexed, hardware skinned
	// flexed, hardware skinned
	// non-flexed, software skinned
	// flexed, software skinned
	//
	// A mesh has a material associated with it.
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct MeshHeader_t {
		public int numStripGroups;
		public int stripGroupHeaderOffset;

		public StripGroupHeader_t* apStripGroup(int i) {
			fixed (int* numStripGroupsPtr = &numStripGroups) {
				MeshHeader_t* This = (MeshHeader_t*)numStripGroupsPtr;
				return (StripGroupHeader_t*)(((byte*)This) + This->stripGroupHeaderOffset) + i;
			}
		}

		public byte flags;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct ModelLODHeader_t {
		public int numMeshes;
		public int meshOffset;
		public float switchPoint;

		public MeshHeader_t* pMesh(int i) {
			fixed (int* numMeshesPtr = &numMeshes) {
				ModelLODHeader_t* This = (ModelLODHeader_t*)numMeshesPtr;
				return (MeshHeader_t*)(((byte*)This) + This->meshOffset) + i;
			}
		}
	}

	// This maps one to one with models in the mdl file.
	// There are a bunch of model LODs stored inside potentially due to the qc $lod command
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct ModelHeader_t {
		public int numLODs; // garymcthack - this is also specified in FileHeader_t
		public int lodOffset;

		public ModelLODHeader_t* pLOD(int i) {
			fixed (int* numLODsPtr = &numLODs) {
				ModelHeader_t* This = (ModelHeader_t*)numLODsPtr;
				return (ModelLODHeader_t*)(((byte*)This) + This->lodOffset) + i;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BodyPartHeader_t {
		public int numModels;
		public int modelOffset;

		public ModelHeader_t* pModel(int i) {
			fixed (int* numModelsPtr = &numModels) {
				BodyPartHeader_t* This = (BodyPartHeader_t*)numModelsPtr;
				return (ModelHeader_t*)(((byte*)This) + This->modelOffset) + i;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MaterialReplacementHeader_t {
		public short materialID;
		public int replacementMaterialNameOffset;

		// TODO
		/*inline const byte *pMaterialReplacementName(void)
		{
				const byte *pDebug = (const byte *)(((byte *)this) + replacementMaterialNameOffset);
				return pDebug;
		}*/
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MaterialReplacementListHeader_t {
		public int numReplacements;
		public int replacementOffset;
		// TODO
		/*inline MaterialReplacementHeader_t *pMaterialReplacement(int i) const
		{
				MaterialReplacementHeader_t *pDebug = (MaterialReplacementHeader_t *)(((byte *)this) + replacementOffset) + i;
				return pDebug;
		}*/
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct FileHeader_t {
		// file version as defined by OPTIMIZED_MODEL_FILE_VERSION
		public int version;
		// hardware params that affect how the model is to be optimized.
		public int vertCacheSize;
		public ushort maxBonesPerStrip;
		public ushort maxBonesPerTri;
		public int maxBonesPerVert;
		// must match checkSum in the .mdl
		public int checkSum;
		public int numLODs; // garymcthack - this is also specified in ModelHeader_t and should match
		// one of these for each LOD
		public int materialReplacementListOffset;
		// TODO
		/* MaterialReplacementListHeader_t *pMaterialReplacementList(int lodID) const
		 {
				 MaterialReplacementListHeader_t *pDebug =
						 (MaterialReplacementListHeader_t *)(((byte *)this) + materialReplacementListOffset) + lodID;
				 return pDebug;
		 }*/
		public int numBodyParts;
		public int bodyPartOffset;
		public BodyPartHeader_t* pBodyPart(int i) {
			fixed (int* versionPtr = &version) {
				FileHeader_t* This = (FileHeader_t*)versionPtr;

				return (BodyPartHeader_t*)(((byte*)This) + This->bodyPartOffset) + i;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studiohdr2_t {
		// NOTE: For forward compat, make sure any methods in this struct
		// are also available in studiohdr_t so no leaf code ever directly references
		// a studiohdr2_t structure
		public int numsrcbonetransform;
		public int srcbonetransformindex;
		public int illumpositionattachmentindex;
		// TODO
		// inline int      IllumPositionAttachmentIndex() const { return illumpositionattachmentindex; }
		public float flMaxEyeDeflection;
		//TODO
		//inline float    MaxEyeDeflection() const { return flMaxEyeDeflection != 0.0f ? flMaxEyeDeflection : 0.866f; } // default to cos(30) if not set
		public int linearboneindex;
		//inline mstudiolinearbone_t *pLinearBones() const { return (linearboneindex) ? (mstudiolinearbone_t *)(((byte *)this) + linearboneindex) : NULL; }
		public int sznameindex;
		//TODO
		//inline byte *pszName() { return (sznameindex) ? (byte *)(((byte *)this) + sznameindex) : NULL; }
		public int m_nBoneFlexDriverCount;
		public int m_nBoneFlexDriverIndex;
		//inline mstudioboneflexdriver_t *pBoneFlexDriver(int i) const { Assert(i >= 0 && i < m_nBoneFlexDriverCount); return (mstudioboneflexdriver_t *)(((byte *)this) + m_nBoneFlexDriverIndex) + i; }
		public fixed int reserved[56];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiobbox_t {
		public int bone;
		public int group; // intersection group
		public vector3df bbmin; // bounding box
		public vector3df bbmax;
		public int szhitboxnameindex; // offset to the name of the hitbox.
		public fixed int unused[8];
		// TODO
		/*const byte* pszHitboxName()
		{
				if (szhitboxnameindex == 0)
						return "";
				return ((const byte*)this) + szhitboxnameindex;
		}*/
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudiohitboxset_t {
		public int sznameindex;
		// TODO
		// inline byte * const     pszName(void) const { return ((byte *)this) + sznameindex; }
		public int numhitboxes;
		public int hitboxindex;
		// TODO
		//inline mstudiobbox_t *pHitbox(int i) const { return (mstudiobbox_t *)(((byte *)this) + hitboxindex) + i; };
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudioanimvalue_t_num {
		public byte valid;
		public byte total;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct mstudioanimvalue_t {
		[FieldOffset(0)]
		public mstudioanimvalue_t_num num;
		[FieldOffset(0)]
		public short value;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudioanim_valueptr_t {
		public fixed short offset[3];
		// TODO
		//inline mstudioanimvalue_t *pAnimvalue(int i) const { if (offset[i] > 0) return (mstudioanimvalue_t *)(((byte *)this) + offset[i]); else return NULL; };
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudioanim_t {
		public byte bone;
		public byte flags; // weighing options
		// valid for animating data only
		// TODO
		/*inline byte *pData(void) const { return (((byte *)this) + sizeof(struct mstudioanim_t)); };
		inline mstudioanim_valueptr_t   *pRotV(void) const { return (mstudioanim_valueptr_t *)(pData()); };
		inline mstudioanim_valueptr_t   *pPosV(void) const { return (mstudioanim_valueptr_t *)(pData()) + ((flags & STUDIO_ANIM_ANIMROT) != 0); };
		*/
		// valid if animation unvaring over timeline
		//inline Quaternion48 *pQuat48(void) const { return (Quaternion48 *)(pData()); };
		//inline Quaternion64 *pQuat64(void) const { return (Quaternion64 *)(pData()); };
		//inline Vector48 *pPos(void) const { return (Vector48 *)(pData() + ((flags & STUDIO_ANIM_RAWROT) != 0) * sizeof(*pQuat48()) + ((flags & STUDIO_ANIM_RAWROT2) != 0) * sizeof(*pQuat64())); };
		public short nextoffset;
		// TODO
		// inline mstudioanim_t    *pNext(void) const { if (nextoffset != 0) return (mstudioanim_t *)(((byte *)this) + nextoffset); else return NULL; };
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudiomovement_t {
		public int endframe;
		public int motionflags;
		public float v0; // velocity at start of block
		public float v1; // velocity at end of block
		public float angle; // YAW rotation at end of this blocks movement
		public vector3df vector; // movement vector relative to this blocks initial angle
		public vector3df position; // relative to start of animation???
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudioikerror_t {
		public vector3df pos;
		public quaternion q;
	}

	//union mstudioanimvalue_t;
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiocompressedikerror_t {
		public fixed float scale[6];
		public fixed short offset[6];
		// TODO
		//inline mstudioanimvalue_t *pAnimvalue(int i) const { if (offset[i] > 0) return (mstudioanimvalue_t *)(((byte *)this) + offset[i]); else return NULL; };
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudioikrule_t {
		public int index;
		public int type;
		public int chain;
		public int bone;
		public int slot; // iktarget slot. Usually same as chain.
		public float height;
		public float radius;
		public float floor;
		public vector3df pos;
		public quaternion q;
		public int compressedikerrorindex;
		// TODO
		// inline mstudiocompressedikerror_t *pCompressedError() const { return (mstudiocompressedikerror_t *)(((byte *)this) + compressedikerrorindex); };
		public int unused2;
		public int iStart;
		public int ikerrorindex;
		// TODO
		// inline mstudioikerror_t *pError(int i) const { return (ikerrorindex) ? (mstudioikerror_t *)(((byte *)this) + ikerrorindex) + (i - iStart) : NULL; };
		public float start; // beginning of influence
		public float peak; // start of full influence
		public float tail; // end of full influence
		public float end; // end of all influence
		public float unused3; //
		public float contact; // frame footstep makes ground concact
		public float drop; // how far down the foot should drop when reaching for IK
		public float top; // top of the foot box
		public int unused6;
		public int unused7;
		public int unused8;
		public int szattachmentindex; // name of world attachment
		// TODO
		//inline byte * const pszAttachment(void) const { return ((byte *)this) + szattachmentindex; }
		public fixed int unused[7];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudiolocalhierarchy_t {
		public int iBone; // bone being adjusted
		public int iNewParent; // the bones new parent
		public float start; // beginning of influence
		public float peak; // start of full influence
		public float tail; // end of full influence
		public float end; // end of all influence
		public int iStart; // first frame
		public int localanimindex;
		// TODO
		//inline mstudiocompressedikerror_t *pLocalAnim() const { return (mstudiocompressedikerror_t *)(((byte *)this) + localanimindex); };
		public fixed int unused[4];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudioanimsections_t {
		public int animblock;
		public int animindex;
	}

	// demand loaded sequence groups
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mstudiomodelgroup_t {
		public int szlabelindex; // textual name
		// TODO
		// inline byte * const pszLabel(void) const { return ((byte *)this) + szlabelindex; }
		public int sznameindex; // file name
		// TODO
		// inline byte * const pszName(void) const { return ((byte *)this) + sznameindex; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct studiohdr_t {
		public int id;
		public int version;
		public int checksum; // this has to be the same in the phy and vtx files to load!
		// TODO
		// public static byte * pszName(studiohdr_t* This)  { if (studiohdr2index && pStudioHdr2()->pszName()) return pStudioHdr2()->pszName(); else return name; }

		public fixed byte name[64];

		public string Name {
			get {
				fixed (byte* n = name)
					return Marshal.PtrToStringAnsi((IntPtr)n, 64);
			}
		}

		public int length;
		public vector3df eyeposition; // ideal eye position
		public vector3df illumposition; // illumination center
		public vector3df hull_min; // ideal movement hull size
		public vector3df hull_max;
		public vector3df view_bbmin; // clipping bounding box
		public vector3df view_bbmax;
		public int flags;
		public int numbones; // bones
		public int boneindex;

		public mstudiobone_t* pBone(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;
				return (mstudiobone_t*)(((byte*)This) + This->boneindex) + i;
			}
		}

		// TODO
		/*int     RemapSeqBone(int iSequence, int iLocalBone) const; // maps local sequence bone to global bone
		int     RemapAnimBone(int iAnim, int iLocalBone) const; // maps local animations bone to global bone*/

		public int numbonecontrollers; // bone controllers
		public int bonecontrollerindex;
		//inline mstudiobonecontroller_t *pBonecontroller(int i) const { Assert(i >= 0 && i < numbonecontrollers); return (mstudiobonecontroller_t *)(((byte *)this) + bonecontrollerindex) + i; };
		public int numhitboxsets;
		public int hitboxsetindex;
		// Look up hitbox set by index

		public mstudiohitboxset_t* pHitboxSet(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;
				return (mstudiohitboxset_t*)(((byte*)This) + This->hitboxsetindex) + i;
			}
		}

		// Calls through to hitbox to determine size of specified set
		// TODO
		/*public static mstudiobbox_t *pHitbox(studiohdr_t* This, int i, int set) 
		{
				mstudiohitboxset_t  *s = pHitboxSet(set);
				if (!s)
						return null;
				return s->pHitbox(i);
		}*/

		// Calls through to set to get hitbox count for set
		// TODO
		/* public static int      iHitboxCount(studiohdr_t* This, int set) const
		 {
				 mstudiohitboxset_t const *s = pHitboxSet(set);
				 if (!s)
						 return 0;
				 return s->numhitboxes;
		 }*/

		// file local animations? and sequences
		//private:
		public int numlocalanim; // animations/poses
		public int localanimindex; // animation descriptions
		//      inline mstudioanimdesc_t *pLocalAnimdesc(int i) const { if (i < 0 || i >= numlocalanim) i = 0; return (mstudioanimdesc_t *)(((byte *)this) + localanimindex) + i; };
		public int numlocalseq; // sequences
		public int localseqindex;
		//inline mstudioseqdesc_t *pLocalSeqdesc(int i) const { if (i < 0 || i >= numlocalseq) i = 0; return (mstudioseqdesc_t *)(((byte *)this) + localseqindex) + i; };
		//public:
		// TODO
		/*bool    SequencesAvailable() const;
		int     GetNumSeq() const;*/
		//mstudioanimdesc_t     &pAnimdesc(int i) const;
		//mstudioseqdesc_t      &pSeqdesc(int i) const;
		// TODO
		/*int     iRelativeAnim(int baseseq, int relanim) const; // maps seq local anim reference to global anim index
		int     iRelativeSeq(int baseseq, int relseq) const; // maps seq local seq reference to global seq index*/
		//private:
		public int activitylistversion; // initialization flag - have the sequences been indexed?
		public int eventsindexed;
		//public:
		// TODO
		/*int     GetSequenceActivity(int iSequence);
		void    SetSequenceActivity(int iSequence, int iActivity);
		int     GetActivityListVersion(void);
		void    SetActivityListVersion(int version) const;
		int     GetEventListVersion(void);
		void    SetEventListVersion(int version);*/
		// raw textures
		public int numtextures;
		public int textureindex;
		public mstudiotexture_t* pTexture(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;
				return (mstudiotexture_t*)(((byte*)This) + This->textureindex) + i;
			}
		}
		// raw textures search paths
		public int numcdtextures;
		public int cdtextureindex;
		public byte* pCdtexture(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;
				return (((byte*)This) + *((int*)(((byte*)This) + This->cdtextureindex) + i));
			}
		}
		// replaceable textures tables
		public int numskinref;
		public int numskinfamilies;
		public int skinindex;
		public short* pSkinref(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;
				return (short*)(((byte*)This) + This->skinindex) + i;
			}
		}

		public int numbodyparts;
		public int bodypartindex;
		public mstudiobodyparts_t* pBodypart(int i) {
			fixed (int* idPtr = &id) {
				studiohdr_t* This = (studiohdr_t*)idPtr;

				return (mstudiobodyparts_t*)(((byte*)This) + This->bodypartindex) + i;
			}
		}
		// queryable attachable points
		//private:
		public int numlocalattachments;
		public int localattachmentindex;
		//inline mstudioattachment_t    *pLocalAttachment(int i) const { Assert(i >= 0 && i < numlocalattachments); return (mstudioattachment_t *)(((byte *)this) + localattachmentindex) + i; };
		//public:
		// TODO
		// int     GetNumAttachments(void) const;
		//const mstudioattachment_t &pAttachment(int i) const;
		// TODO
		//int     GetAttachmentBone(int i);
		// used on my tools in hlmv, not persistant
		//TODO
		// void    SetAttachmentBone(int iAttachment, int iBone);
		// animation node to animation node transition graph
		//private:
		public int numlocalnodes;
		public int localnodeindex;
		public int localnodenameindex;
		public static byte* pszLocalNodeName(studiohdr_t* This, int iNode) {
			return (((byte*)This) + *((int*)(((byte*)This) + This->localnodenameindex) + iNode));
		}
		public static byte* pLocalTransition(studiohdr_t* This, int i) {
			return (byte*)(((byte*)This) + This->localnodeindex) + i;
		}
		//public:
		// TODO
		/* int     EntryNode(int iSequence);
		 int     ExitNode(int iSequence);
		 byte    *pszNodeName(int iNode);
		 int     GetTransition(int iFrom, int iTo) const;*/
		public int numflexdesc;
		public int flexdescindex;
		//inline mstudioflexdesc_t *pFlexdesc(int i) const { Assert(i >= 0 && i < numflexdesc); return (mstudioflexdesc_t *)(((byte *)this) + flexdescindex) + i; };
		public int numflexcontrollers;
		public int flexcontrollerindex;
		//inline mstudioflexcontroller_t *pFlexcontroller(LocalFlexController_t i) const { Assert(numflexcontrollers == 0 || (i >= 0 && i < numflexcontrollers)); return (mstudioflexcontroller_t *)(((byte *)this) + flexcontrollerindex) + i; };
		public int numflexrules;
		public int flexruleindex;
		//inline mstudioflexrule_t *pFlexRule(int i) const { Assert(i >= 0 && i < numflexrules); return (mstudioflexrule_t *)(((byte *)this) + flexruleindex) + i; };
		public int numikchains;
		public int ikchainindex;
		//inline mstudioikchain_t *pIKChain(int i) const { Assert(i >= 0 && i < numikchains); return (mstudioikchain_t *)(((byte *)this) + ikchainindex) + i; };
		public int nummouths;
		public int mouthindex;
		//inline mstudiomouth_t *pMouth(int i) const { Assert(i >= 0 && i < nummouths); return (mstudiomouth_t *)(((byte *)this) + mouthindex) + i; };
		//private:
		public int numlocalposeparameters;
		public int localposeparamindex;
		//inline mstudioposeparamdesc_t *pLocalPoseParameter(int i) const { Assert(i >= 0 && i < numlocalposeparameters); return (mstudioposeparamdesc_t *)(((byte *)this) + localposeparamindex) + i; };
		//public:
		// TODO
		// int     GetNumPoseParameters(void) const;
		//const mstudioposeparamdesc_t &pPoseParameter(int i);
		// TODO
		//int     GetSharedPoseParameter(int iSequence, int iLocalPose) const;
		public int surfacepropindex;
		public static byte* pszSurfaceProp(studiohdr_t* This) {
			return ((byte*)This) + This->surfacepropindex;
		}
		// Key values
		public int keyvalueindex;
		public int keyvaluesize;
		public static byte* KeyValueText(studiohdr_t* This) {
			return This->keyvaluesize != 0 ? ((byte*)This) + This->keyvalueindex : null;
		}
		int numlocalikautoplaylocks;
		int localikautoplaylockindex;
		//inline mstudioiklock_t *pLocalIKAutoplayLock(int i) const { Assert(i >= 0 && i < numlocalikautoplaylocks); return (mstudioiklock_t *)(((byte *)this) + localikautoplaylockindex) + i; };
		// TODO   
		// int     GetNumIKAutoplayLocks(void) const;
		//const mstudioiklock_t &pIKAutoplayLock(int i);
		//TODO
		/* int     CountAutoplaySequences() const;
		 int     CopyAutoplaySequences(unsigned short *pOut, int outCount) const;
		 int     GetAutoplayList(unsigned short **pOut) const;*/
		// The collision model mass that jay wanted
		public float mass;
		public int contents;
		// external animations, models, etc.
		public int numincludemodels;
		public int includemodelindex;
		public static mstudiomodelgroup_t* pModelGroup(studiohdr_t* This, int i) {
			return (mstudiomodelgroup_t*)(((byte*)This) + This->includemodelindex) + i;
		}
		// implementation specific call to get a named model
		// TODO
		// const studiohdr_t       *FindModel(void **cache, byte const *modelname) const;
		// implementation specific back pointer to virtual data
		public void* virtualModel;
		//virtualmodel_t        *GetVirtualModel(void) const;
		// for demand loaded animation blocks
		public int szanimblocknameindex;
		public static byte* pszAnimBlockName(studiohdr_t* This) {
			return ((byte*)This) + This->szanimblocknameindex;
		}
		public int numanimblocks;
		public int animblockindex;
		//inline mstudioanimblock_t *pAnimBlock(int i) const { Assert(i > 0 && i < numanimblocks); return (mstudioanimblock_t *)(((byte *)this) + animblockindex) + i; };
		public void* animblockModel;
		// TODO
		//byte * GetAnimBlock(int i) const;
		public int bonetablebynameindex;
		public static byte* GetBoneTableSortedByName(studiohdr_t* This) {
			return (byte*)This + This->bonetablebynameindex;
		}
		// used by tools only that don't cache, but persist mdl's peer data
		// engine uses virtualModel to back link to cache pointers
		public void* pVertexBase;
		public void* pIndexBase;
		// if STUDIOHDR_FLAGS_CONSTANT_DIRECTIONAL_LIGHT_DOT is set,
		// this value is used to calculate directional components of lighting
		// on static props
		public byte constdirectionallightdot;
		// set during load of mdl data to track *desired* lod configuration (not actual)
		// the *actual* clamped root lod is found in studiohwdata
		// this is stored here as a global store to ensure the staged loading matches the rendering
		public byte rootLOD;
		// set in the mdl data to specify that lod configuration should only allow first numAllowRootLODs
		// to be set as root LOD:
		// numAllowedRootLODs = 0 means no restriction, any lod can be set as root lod.
		// numAllowedRootLODs = N means that lod0 - lod(N-1) can be set as root lod, but not lodN or lower.
		public byte numAllowedRootLODs;
		public fixed byte unused[1];
		public int unused4; // zero out if version < 47
		public int numflexcontrollerui;
		public int flexcontrolleruiindex;
		//mstudioflexcontrollerui_t *pFlexControllerUI(int i) const { Assert(i >= 0 && i < numflexcontrollerui); return (mstudioflexcontrollerui_t *)(((byte *)this) + flexcontrolleruiindex) + i; }
		public float flVertAnimFixedPointScale;
		//inline float  VertAnimFixedPointScale() const { return (flags & STUDIOHDR_FLAGS_VERT_ANIM_FIXED_POINT_SCALE) ? flVertAnimFixedPointScale : 1.0f / 4096.0f; }
		public fixed int unused3[1];
		// FIXME: Remove when we up the model version. Move all fields of studiohdr2_t into studiohdr_t.
		public int studiohdr2index;
		public static studiohdr2_t* pStudioHdr2(studiohdr_t* This) {
			return (studiohdr2_t*)(((byte*)This) + This->studiohdr2index);
		}

		// NOTE: No room to add stuff? Up the .mdl file format version
		// [and move all fields in studiohdr2_t into studiohdr_t and kill studiohdr2_t],
		// or add your stuff to studiohdr2_t. See NumSrcBoneTransforms/SrcBoneTransform for the pattern to use.
		public fixed int unused2[1];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct mstudioanimdesc_t {
		public int baseptr;
		// TODO
		//inline studiohdr_t      *pStudiohdr(void) const { return (studiohdr_t *)(((byte *)this) + baseptr); }
		public int sznameindex;
		// TODO
		//inline byte * const pszName(void) const { return ((byte *)this) + sznameindex; }
		public float fps; // frames per second
		public int flags; // looping/non-looping flags
		public int numframes;
		// piecewise movement
		public int nummovements;
		public int movementindex;
		// TODO
		// inline mstudiomovement_t * const pMovement(int i) const { return (mstudiomovement_t *)(((byte *)this) + movementindex) + i; };
		public fixed int unused1[6]; // remove as appropriate (and zero if loading older versions)
		public int animblock;
		public int animindex; // non-zero when anim data isn't in sections
		// TODO
		/*mstudioanim_t *pAnimBlock(int block, int index) const; // returns pointer to a specific anim block (local or external)
		mstudioanim_t *pAnim(int *piFrame, float &flStall) const; // returns pointer to data and new frame index
		mstudioanim_t *pAnim(int *piFrame) const; // returns pointer to data and new frame index*/
		public int numikrules;
		public int ikruleindex; // non-zero when IK data is stored in the mdl
		public int animblockikruleindex; // non-zero when IK data is stored in animblock file
		// mstudioikrule_t *pIKRule(int i) const; TODO
		public int numlocalhierarchy;
		public int localhierarchyindex;
		//mstudiolocalhierarchy_t *pHierarchy(int i) const; TODO
		public int sectionindex;
		public int sectionframes; // number of frames used in each fast lookup section, zero if not used
		// TODO
		// inline mstudioanimsections_t * const pSection(int i) const { return (mstudioanimsections_t *)(((byte *)this) + sectionindex) + i; }
		public short zeroframespan; // frames per span
		public short zeroframecount; // number of spans
		public int zeroframeindex;
		// TODO
		//byte *pZeroFrameData() const { if (zeroframeindex) return (((byte *)this) + zeroframeindex); else return NULL; };
		public float zeroframestalltime; // saved during read stalls
	}
}