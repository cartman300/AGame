using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace AGame.Src.OGL {
	class FBO : GLObject {
		public FBO() {
			ID = GL.GenFramebuffer();
		}

		public void Attach(FramebufferAttachment A, Texture T) {
			Bind();
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, A, T.Target, T.ID, 0);
			Unbind();
		}

		public void Attach(FramebufferAttachment A, RBO R) {
			Bind();
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, A, RenderbufferTarget.Renderbuffer, R.ID);
			Unbind();
		}

		public void Detach(FramebufferAttachment A, Texture T) {
			Bind();
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, A, T.Target, 0, 0);
			Unbind();
		}

		public void Detach(FramebufferAttachment A, RBO R) {
			Bind();
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, A, RenderbufferTarget.Renderbuffer, 0);
			Unbind();
		}

		public override void Delete() {
			GL.DeleteFramebuffer(ID);
		}

		public override void Bind() {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
		}

		public void RenderTo(Action A) {
			Bind();
			A();
			Unbind();
		}

		public override void Unbind() {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}
	}

	class RBO : GLObject {
		public RBO() {
			ID = GL.GenRenderbuffer();
		}

		public override void Delete() {
			GL.DeleteRenderbuffer(ID);
		}

		public void SetStorage(RenderbufferStorage S, int W, int H) {
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, S, W, H);
		}

		public override void Bind() {
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, ID);
		}

		public override void Unbind() {
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
		}
	}
}