using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGame.Src.Meshes {
	class ModelLoader {
		public static void Assert(bool Cnd, string Msg) {
			if (!Cnd)
				throw new Exception(Msg);
		}

		public virtual Model[] CreateModel(string FileName) {
			return null;
		}

		public virtual bool CanLoad(string Filename) {
			return false;
		}
	}
}