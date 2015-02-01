using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SharpFileSystem;
using SharpFileSystem.FileSystems;
using SharpFileSystem.Resources;
using SharpFileSystem.SevenZip;
using SharpFileSystem.SharpZipLib;

namespace AGame.Src {
	static class FSys {
		static MergedFileSystem FS;
		static List<IFileSystem> FileSystems;

		static void Reload() {
			FS = new MergedFileSystem(FileSystems.ToArray());
		}

		static FSys() {
			FileSystems = new List<IFileSystem>();

			Mount(new SeamlessSevenZipFileSystem(new PhysicalFileSystem("Data/")), false);
			Reload();
		}

		public static IFileSystem Mount(IFileSystem FS, bool DoReload = true) {
			FileSystems.Add(FS);
			if (DoReload)
				Reload();
			return FS;
		}

		public static IFileSystem Mount(string F, bool DoReload = true) {
			Console.WriteLine("Mounting '{0}'", Path.GetFileName(F));
			return Mount(new SevenZipFileSystem(F), DoReload);
		}

		public static string[] GetEntities(string Dir, bool Recursive = false) {
			FileSystemPath FSP = FileSystemPath.Parse(Dir);
			FileSystemPath[] Ents = null;
			if (Recursive)
				Ents = FS.GetEntitiesRecursive(FSP).ToArray();
			else
				Ents = FS.GetEntities(FSP).ToArray();

			string[] EntsStr = new string[Ents.Length];
			for (int i = 0; i < Ents.Length; i++)
				EntsStr[i] = Ents[i].ToString();
			return EntsStr;
		}

		public static string Normalize(string F) {
			if (F.Contains('\\'))
				F = F.Replace('\\', '/');
			if (!F.StartsWith(FileSystemPath.DirectorySeparator.ToString()))
				F = FileSystemPath.DirectorySeparator + F;
			return F;
		}

		public static bool Exists(string F) {
			F = Normalize(F);
			return FS.Exists(FileSystemPath.Parse(F));
		}

		public static byte[] ReadAllBytes(string F) {
			using (Stream S = Open(F))
				return S.ReadAllBytes();
		}

		public static string ReadAllText(string F) {
			using (Stream S = Open(F))
				return S.ReadAllText();
		}

		public static Stream Create(string F) {
			return FS.CreateFile(FileSystemPath.Parse(Normalize(F)));
		}

		public static void WriteAllBytes(string F, byte[] Bytes) {
			using (Stream S = Create(F))
				S.Write(Bytes);
		}

		public static MemoryStream LoadToMemory(string F) {
			return new MemoryStream(ReadAllBytes(F));
		}

		public static Stream Open(string F, FileAccess A = FileAccess.Read) {
			F = Normalize(F);
			return FS.OpenFile(FileSystemPath.Parse(F), A);
		}

	}
}