using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

namespace AGame.Src.States {
	public class State {
		public State() {
		}

		~State() {
			Destroy();
		}

		public virtual void Destroy() {

		}

		public virtual void Deactivate(State NewState) {
		}

		public virtual void Activate(State OldState) {
		}

		public virtual void TextEntered(string S) {

		}

		public virtual void Key(bool Down) {
		}

		public virtual void MouseMove() {
		}

		public virtual void MouseClick(int X, int Y, bool Down) {
		}

		public virtual void Update(float T) {
		}

		public virtual void PreRenderOpaque(float T) {
		}
		public virtual void RenderOpaque(float T) {
		}
		public virtual void PostRenderOpaque(float T) {
		}
		public virtual void PreRenderTransparent(float T) {
		}
		public virtual void RenderTransparent(float T) {
		}
		public virtual void PostRenderTransparent(float T) {
		}
		public virtual void PreRenderGUI(float T) {
		}
		public virtual void RenderGUI(float T) {
		}
		public virtual void PostRenderGUI(float T) {
		}
	}
}