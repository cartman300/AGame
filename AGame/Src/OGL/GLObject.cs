using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGame.Src.OGL {
	class GLObject {
		public int ID;

		public virtual void Delete() {
			throw new NotImplementedException();
		}

		public virtual void Bind() {
			throw new NotImplementedException();
		}

		public virtual void Use(Action A) {
			Bind();
			A();
			Unbind();
		}

		public virtual void Unbind() {
			throw new NotImplementedException();
		}
	}
}