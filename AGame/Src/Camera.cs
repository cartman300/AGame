using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using FCursor = System.Windows.Forms.Cursor;

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
		public Matrix4 Translation, Zoom, Projection;
		public float MouseSensitivity;

		public Quaternion Rotation;

		public float RotationX;
		public float RotationY;

		public Camera() {
			Translation = Zoom = Projection = Matrix4.Identity;
			Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, 0);
			MouseSensitivity = 1.0f;
		}

		public void Move(Vector3 Delta) {
			Translation *= Matrix4.CreateTranslation(-Delta);
		}

		public void SetPos(Vector3 Pos) {
			Translation = Matrix4.CreateTranslation(Pos);
		}

		public Vector3 GetForward() {
			Matrix4 M = Matrix4.CreateFromQuaternion(Rotation);
			M.Invert();
			return new Vector3(0, 0, -1).Transform(M);
		}

		public Vector3 GetPosition() {
			return -Translation.ExtractTranslation();
		}

		public void MouseRotate(float T) {
			Point WindowCenter = Engine.Game.GetWindowCenter();
			Point CurPos = FCursor.Position;

			float Accel = MouseSensitivity * T;
			float DX = (WindowCenter.X - CurPos.X) * Accel;
			float DY = (WindowCenter.Y - CurPos.Y) * Accel;

			if (DX == 0 && DY == 0)
				return;

			RotationX += DX;
			RotationY += DY;
			RotationY = (float)MathHelper.Clamp(RotationY, -MathHelper.PiOver2, MathHelper.PiOver2);
			Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, -RotationY) *
				Quaternion.FromAxisAngle(Vector3.UnitY, -RotationX);

			FCursor.Position = WindowCenter;
		}

		public Matrix4 Collapse() {
			return Translation * Matrix4.CreateFromQuaternion(Rotation) * Zoom * Projection;
		}
	}
}