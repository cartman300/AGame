using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Scr = System.Windows.Forms.Screen;

namespace AGame.Utils {
	static class Screen {
		static Rectangle Resolution = Scr.PrimaryScreen.Bounds;

		public static int W = Resolution.Width;
		public static int H = Resolution.Height;
	}
}
