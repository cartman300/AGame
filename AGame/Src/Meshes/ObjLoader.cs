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
	unsafe class ObjLoader : ModelLoader {
		public override Model[] CreateModel(string FileName) {
			throw new Exception("Not fucking implemented");
		}

		public override bool CanLoad(string Filename) {
			return Filename.EndsWith(".obj");
		}
	}
}