using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using AGame.Utils;
using AGame.Src.OGL;
using AGame.Src.Meshes;

namespace AGame.Src.States {
	class MapEditor : State {
		Model Missile, Missile2, Dice;

		public MapEditor() {
			Missile = Engine.Game.CreateModel("models/missile/missile3.mdl")[0];
			Missile.GLInit(Engine.Generic3D);

			Dice = Engine.Game.CreateModel("models/cube/cube_96x96x96.mdl")[0];
			Dice.Scale = 0.20833333333f;
			Dice.GLInit(Engine.Generic3D);

			Missile2 = Engine.Game.CreateModel("models/missile/missile3.mdl")[0];
			Missile2.Position = new Vector3(0, 0, 50);
			Missile2.Rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);
			Missile2.GLInit(Engine.Generic3D);
		}

		public override void Activate(State OldState) {
			if (!Engine.Game.MouseEnabled)
				Engine.Game.ToggleMouse();
		}

		public override void Deactivate(State NewState) {
			if (Engine.Game.MouseEnabled)
				Engine.Game.ToggleMouse();
		}

		void MapPos(int X, int Y, int Z) {
			Dice.Position = new Vector3(X * 20, Y * 20, Z * 20).GridClamp(new Vector3(20, 20, 20));
			Dice.RenderOpaque();
		}

		public override void OnKey(KeyboardKeyEventArgs K, bool Down) {
			if (Down) {
				if (K.Key == Key.C)
					Engine.Game.ToggleMouse();
				if (K.Key == Key.Escape)
					Menu.SwitchTo();
			}
		}

		public override void Update(float T) {
			const float MoveSpeed = 400.0f;
			Camera Cam = Engine.Generic3D.Cam;

			if (Engine.Game.Keyboard[Key.W])
				Cam.Move(-new Vector3((float)Math.Sin(Cam.RotationX), 0,
					(float)Math.Cos(Cam.RotationX)) * T * MoveSpeed);

			if (Engine.Game.Keyboard[Key.S])
				Cam.Move(new Vector3((float)Math.Sin(Cam.RotationX), 0,
					(float)Math.Cos(Cam.RotationX)) * T * MoveSpeed);

			if (Engine.Game.Keyboard[Key.A])
				Cam.Move(-new Vector3((float)Math.Sin(Cam.RotationX + MathHelper.PiOver2), 0,
					(float)-Math.Sin(Cam.RotationX)) * T * MoveSpeed);

			if (Engine.Game.Keyboard[Key.D])
				Cam.Move(new Vector3((float)Math.Sin(Cam.RotationX + MathHelper.PiOver2), 0,
					(float)-Math.Sin(Cam.RotationX)) * T * MoveSpeed);

			if (Engine.Game.Keyboard[Key.Space])
				Cam.Move(new Vector3(0, MoveSpeed * T, 0));
			if (Engine.Game.Keyboard[Key.LControl])
				Cam.Move(-new Vector3(0, MoveSpeed * T, 0));

			if (Engine.Game.MouseEnabled)
				Cam.MouseRotate(T);
		}

		public override void RenderOpaque(float T) {
			Camera Cam = Engine.Generic3D.Cam;
			Vector3 Pos = Cam.GetPosition() + Cam.GetForward() * 100;
			Dice.Position = Pos.GridClamp(new Vector3(20, 20, 20));

			Missile.RenderOpaque();
			Missile2.RenderOpaque();
			Dice.RenderOpaque();
		}
	}
}