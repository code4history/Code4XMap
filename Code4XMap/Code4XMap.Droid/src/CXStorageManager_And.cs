using System;
using Android.App;
using System.IO;

namespace Code4XMap
{
	public partial class CXStorageManager
	{
		private Android.Content.Res.AssetManager assetManager;

		public static void SaveInstance<T>(Android.OS.Bundle outState) where T : CXStorageManager
		{
			var inst = GetInstance<T> ();
			outState.PutString ("StMg.AppName",   inst.AppName);
			outState.PutString ("StMg.AppTitle",  inst.AppTitle);
			outState.PutString ("StMg.AppUrl",    inst.AppUrl);
			outState.PutString ("StMg.ModelUrl",  inst.ModelUrl);
			outState.PutString ("StMg.dataDir",   inst.dataDir);
			outState.PutString ("StMg.path",      inst.path);
			outState.PutString ("StMg.separator", inst.separator);
			outState.PutBoolean ("StMg.useExternal", inst.useExternal);
		}

		public static void RestoreInstance<T>(Android.OS.Bundle savedInstanceState) where T : CXStorageManager
		{
			var inst = GetInstance<T> ();

			if (inst.AppName != null)
				return;

			inst.AppName     = savedInstanceState.GetString ("StMg.AppName");
			inst.AppTitle    = savedInstanceState.GetString ("StMg.AppTitle");
			inst.AppUrl      = savedInstanceState.GetString ("StMg.AppUrl");
			inst.ModelUrl    = savedInstanceState.GetString ("StMg.ModelUrl");
			inst.dataDir     = savedInstanceState.GetString ("StMg.dataDir");
			inst.path        = savedInstanceState.GetString ("StMg.path");
			inst.separator   = savedInstanceState.GetString ("StMg.separator");
			inst.useExternal = savedInstanceState.GetBoolean ("StMg.useExternal");

			inst.PlatformInitialize ();
		}

		private void PlatformInitialize()
		{
			this.assetManager = Application.Context.Resources.Assets;
			//this.separator = Java.IO.File.Separator;
			this.dataDir = useExternal ?
				Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString (), Application.Context.PackageName, path) :
				Application.Context.GetDir (path, Android.Content.FileCreationMode.Private).ToString ();
		}

		private bool IsDirectoryInAsset(string assetPath) {
			bool isDirectory = false;
			try {
				if (assetManager.List(assetPath).Length > 0){
					isDirectory = true;
				} else {
					// check openable file
					assetManager.Open(assetPath);
				}
			} catch (Java.IO.FileNotFoundException) {
				isDirectory = true;
			}
			return isDirectory;
		}

		public bool FileExistsInAsset(string assetPath) {
			bool exists = true;
			try {
				assetManager.Open(assetPath);
			} catch (Java.IO.FileNotFoundException) {
				exists = false;
			}
			return exists;
		}

		private string[] FilesInAsset(string assetPath)
		{
			return assetManager.List(assetPath);
		}

		private void Unzip(string assetPath, string parentDir)
		{
			Unzip (assetManager.Open (assetPath, Android.Content.Res.Access.Streaming), parentDir);
		}

		private void Unzip(Stream inps, string parentDir) {
			Java.Util.Zip.ZipInputStream zis = null;
			try {
				zis = new Java.Util.Zip.ZipInputStream(inps);
				Java.Util.Zip.ZipEntry entry;
				while ((entry = zis.NextEntry) != null) {
					string entryFilePath = entry.Name.Replace("\\", separator);
					var outFile = Path.Combine(parentDir, entryFilePath);
					if (entry.IsDirectory) {
						if (Directory.Exists(outFile)) continue;
						Directory.CreateDirectory(outFile);
					} else {
						if (File.Exists(outFile)) continue;
						WriteData(zis, new FileStream(outFile,FileMode.OpenOrCreate));
						zis.CloseEntry();
					}
				}
			} finally {
				if (zis != null) { try { zis.Close(); } catch (IOException) {} }
			}
		}

		private string GetAssetData(string assetPath)
		{
			string content;
			using (StreamReader sr = new StreamReader (assetManager.Open (assetPath)))
				content = sr.ReadToEnd ();
			return content;
		}

		public byte[] GetAssetByteData(string assetPath)
		{
			var fs = assetManager.Open (assetPath);
			var inps = new Java.IO.BufferedInputStream (fs);
			Java.IO.ByteArrayOutputStream outs = null;

			try {
				outs = new Java.IO.ByteArrayOutputStream ();
				byte[] buffer = new byte[BUFFER_SIZE];
				int len = 0;
				while ( (len = inps.Read(buffer, 0, buffer.Length)) > 0) {
					outs.Write(buffer, 0, len);
				}
				outs.Flush();
			} finally {
				if (outs != null) { try { outs.Close(); } catch (IOException) {} }
			}

			if (outs == null)
				return null;
			var bs = outs.ToByteArray ();
			fs.Close();

			return bs;
		}

		private void CopyData(string assetPath, string ToFile)
		{
			CopyData(assetManager.Open(assetPath), new FileStream(ToFile, FileMode.OpenOrCreate));
		}

		private void CopyData(Stream inps, Stream outs) {
			var bis = new Java.IO.BufferedInputStream(inps);
			try {
				WriteData(bis, outs);
			} finally {
				if (bis != null) { try { bis.Close(); } catch (IOException) {} }
			}
		}

		private void WriteData(Stream inps, Stream outs) {
			Java.IO.BufferedOutputStream bos = null;
			try {
				bos = new Java.IO.BufferedOutputStream(outs);
				byte[] buffer = new byte[BUFFER_SIZE];
				int len = 0;
				while ( (len = inps.Read(buffer, 0, buffer.Length)) > 0) {
					bos.Write(buffer, 0, len);
				}
				bos.Flush();
			} finally {
				if (bos != null) { try { bos.Close(); } catch (IOException) {} }
			}
		}

		private void WriteData(Java.IO.InputStream inps, Stream outs) {
			Java.IO.BufferedOutputStream bos = null;
			try {
				bos = new Java.IO.BufferedOutputStream(outs);
				byte[] buffer = new byte[BUFFER_SIZE];
				int len = 0;
				while ( (len = inps.Read(buffer, 0, buffer.Length)) > 0) {
					bos.Write(buffer, 0, len);
				}
				bos.Flush();
			} finally {
				if (bos != null) { try { bos.Close(); } catch (IOException) {} }
			}
		}
	}
}

