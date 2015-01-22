using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace AGame.Src.OGL {
	class RenderTarget {
		public Texture ColorTex, DSTex/*, NormalTex*/;
		public FBO FrameBuffer;

		void WrappingFiltering(Texture Tex) {
			Tex.Filtering(Texture.Filter.DownScaled, Texture.FilterMode.Nearest);
			Tex.Filtering(Texture.Filter.UpScaled, Texture.FilterMode.Nearest);
			Tex.Wrapping(Texture.Wrap.S, Texture.WrapMode.ClampToEdge);
			Tex.Wrapping(Texture.Wrap.T, Texture.WrapMode.ClampToEdge);
		}

		public RenderTarget(int W, int H, bool UseDepth = false) {
			ColorTex = new Texture(TextureTarget.Texture2D);
			ColorTex.Use(() => {
				WrappingFiltering(ColorTex);
				ColorTex.TexImage2D(0, PixelInternalFormat.Rgba, W, H, PixelFormat.Rgba);
			});

			/*NormalTex = new Texture(TextureTarget.Texture2D);
			NormalTex.Use(() => {
				WrappingFiltering(NormalTex);
				ColorTex.TexImage2D(0, PixelInternalFormat.Rgba, W, H, PixelFormat.Rgba);
			});*/

			if (UseDepth) {
				DSTex = new Texture(TextureTarget.Texture2D);
				DSTex.Use(() => {
					WrappingFiltering(DSTex);
					DSTex.TexImage2D(0, PixelInternalFormat.Depth24Stencil8, W, H,
						PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
				});
			}

			FrameBuffer = new FBO();
			FrameBuffer.Use(() => {
				FrameBuffer.Attach(FramebufferAttachment.ColorAttachment0, ColorTex);
				//FrameBuffer.Attach(FramebufferAttachment.ColorAttachment1, NormalTex);
				if (UseDepth)
					FrameBuffer.Attach(FramebufferAttachment.DepthStencilAttachment, DSTex);
			});
		}

		public void UseColor(Action A) {
			ColorTex.Use(A);
		}

		/*public void UseNormal(Action A) {
			NormalTex.Use(A);
		}*/

		public void UseDepthStencil(Action A) {
			if (DSTex != null)
				DSTex.Use(A);
		}

		public void RenderTo(Action A) {
			FrameBuffer.RenderTo(A);
		}
	}
}