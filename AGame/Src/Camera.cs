using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Src;
using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.States;
using AGame.Src.Meshes;

namespace AGame.Src {
	class Camera {
		public Matrix4 Translation, Rotation, Zoom, Projection;

		bool DirtyRot;

		float RotX;
		public float RotationX {
			get {
				return RotX;
			}
			set {
				if (value == 0)
					return;
				RotX = value;
				DirtyRot = true;
			}
		}

		float RotY;
		public float RotationY {
			get {
				return RotY;
			}
			set {
				if (value == 0)
					return;
				RotY = value;
				DirtyRot = true;
			}
		}

		public Camera() {
			Translation = Rotation = Zoom = Projection = Matrix4.Identity;
		}

		public void Move(Vector3 Delta) {
			Translation *= Matrix4.CreateTranslation(-Delta);
		}

		public void SetPos(Vector3 Pos) {
			Translation = Matrix4.CreateTranslation(Pos);
		}

		public Matrix4 Collapse() {
			if (DirtyRot)
				Rotation = Matrix4.CreateRotationY(-RotX) * Matrix4.CreateRotationX(-RotY);

			return Translation * Rotation * Zoom * Projection;
		}
	}
}