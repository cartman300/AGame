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
	class Menu : State {
		static Menu Singleton;

		public static void SwitchTo() {
			if (Singleton == null)
				Singleton = new Menu();
			Engine.Game.ActiveState = Singleton;
		}

		ColoredText MenuText;
		List<Tuple<string, Action>> MenuEntries;
		byte SelectedEntry;
		bool MenuDirty;

		public Menu() {
			Flib.Font MenuFont = new Flib.Font("Data/Fonts/Vera.ttf", 18);
			MenuText = new ColoredText(MenuFont, 80, 100);

			SelectedEntry = 0;
			MenuEntries = new List<Tuple<string, Action>>();

			MenuEntries.Add(new Tuple<string, Action>("New Game", () => {
			}));

			MenuEntries.Add(new Tuple<string, Action>("Map Editor", () => Engine.Game.ActiveState = new MapEditor()));

			MenuEntries.Add(new Tuple<string, Action>("Options", () => {
			}));

			MenuEntries.Add(new Tuple<string, Action>("Exit", Engine.Game.Exit));

			MenuDirty = true;
		}

		public override void OnKey(KeyboardKeyEventArgs K, bool Down) {
			if (Down) {
				if (K.Key == Key.W || K.Key == Key.Up) {
					if (SelectedEntry > 0)
						SelectedEntry--;
				} else if (K.Key == Key.S || K.Key == Key.Down) {
					if (SelectedEntry < MenuEntries.Count - 1)
						SelectedEntry++;
				} else if (K.Key == Key.Space || K.Key == Key.Enter || K.Key == Key.KeypadEnter)
					MenuEntries[SelectedEntry].Item2();
				MenuDirty = true;
			}
		}

		public override void Update(float T) {
			if (MenuDirty) {
				MenuDirty = false;
				MenuText.Clear();
				for (int i = 0; i < MenuEntries.Count; i++) {
					bool Selected = i == SelectedEntry;
					if (Selected)
						MenuText.Print("> ");
					MenuText.Print(MenuEntries[i].Item1 + "\n", Selected ? Color4.White : Color4.Gray);
				}
			}
		}

		public override void RenderGUI(float T) {
			// TODO: Remove hardcoded position
			Engine.Text2D.Use(new Vector3(100, 200, 0), Quaternion.FromAxisAngle(Vector3.UnitZ,
				(float)Math.Sin(Engine.Game.Time / 7) * 0.05f),
				1.0f, Color4.White, true, MenuText.Render);
		}
	}
}