using System;
using MonoTouch;
using System.IO;
using Foundation;

namespace Code4XMap
{
	public partial class CXStorageManager
	{
		private string bundlePath;

		private void PlatformInitialize()
		{
			this.bundlePath = NSBundle.MainBundle.BundlePath;
			//this.separator = "/";
			this.dataDir = useExternal ?
				Path.Combine (NSSearchPath.GetDirectories(NSSearchPathDirectory.CachesDirectory,  NSSearchPathDomain.User,true)[0], this.path) :
				Path.Combine (NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory,NSSearchPathDomain.User,true)[0], this.path);
		}

		private bool IsDirectoryInAsset(string assetPath) {
			bool isDirectory = Directory.Exists (this.AssetFullPath(assetPath));
			return isDirectory;
		}

		private bool FileExistsInAsset(string assetPath) {
			bool exists = File.Exists(this.AssetFullPath(assetPath));
			return exists;
		}

		private string[] FilesInAsset(string assetPath)
		{
			var files = Directory.GetFileSystemEntries(this.AssetFullPath(assetPath));
			for (var i=0; i< files.Length; i++) 
			{
				files [i] = Path.GetFileName (files [i]);
			}
			return files;
		}

		private string GetAssetData(string assetPath)
		{
			return this.GetFileData(this.AssetFullPath(assetPath));
		}

		private void CopyData(string assetPath, string ToFile)
		{
			File.Copy (this.AssetFullPath(assetPath), ToFile, true);
		}

		private void Unzip(string assetPath, string parentDir)
		{
			//var zip = new ZipArchive ();
			//zip.EasyUnzip (this.AssetFullPath(assetPath), parentDir, true, "");
			////Unzip (assetManager.Open (assetpath, Android.Content.Res.Access.Streaming), ParentDir);
		}

		public string AssetFullPath(string assetPath)
		{
			return Path.Combine (this.bundlePath, assetPath);
		}
	}
}

