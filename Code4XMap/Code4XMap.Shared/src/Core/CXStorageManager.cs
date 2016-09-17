using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Code4XMap
{
    public partial class CXStorageManager : CXAbstractSingleton
	{
		public  string dataDir;
		protected string path = null;
		protected string separator;
		public  bool   useExternal;
		public  string AppUrl   = null;
		public  string ModelUrl = null;
		public  string AppName  = null;
		public  string AppTitle = null;

		#pragma warning disable 414
		private static int BUFFER_SIZE = 1024;
		#pragma warning restore 414

		private static IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
			{".css", "text/css"},
			{".gif", "image/gif"},
			{".html", "text/html"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".png", "image/png"},
			{".zip", "application/x-zip-compressed"},
		};

		protected CXStorageManager () : base()
		{
			
		}

		public static T InitInstance<T>(String path) where T : CXStorageManager
		{
			return InitInstance<T> (path, false);
		}

		public static T InitInstance<T>(String path, bool useExternal) where T : CXStorageManager
		{
			var inst = GetInstance<T> ();

			if (inst.path == null) {
				inst.path = path;
				inst.useExternal = useExternal;
				inst.separator = "/"; //Path.PathSeparator.ToString();
				inst.PlatformInitialize ();
			}

			return inst;
		}

		public string GetBasePathFromUrl(string url)
		{
			var regex = new Regex("^https?://");
			var urlbase = regex.Replace (url, "");
			return urlbase;
		}
		public string GetBasePathFromUrl(Uri uri)
		{
			return this.GetBasePathFromUrl (uri.AbsoluteUri);
		}

		public string GetAssetPathFromUrl(string url)
		{
			return this.path + "/" + this.GetBasePathFromUrl(url);
		}
		public string GetAssetPathFromUrl(Uri uri)
		{
			return this.GetAssetPathFromUrl (uri.AbsoluteUri);
		}

		public string GetPathFromUrl(string url)
		{
			if (url.Substring (0, 4) == "file") {
				return new Uri (url).AbsolutePath;
			} else {
				return this.dataDir + "/" + this.GetBasePathFromUrl (url);
			}
		}
		public string GetPathFromUrl(Uri uri)
		{
			if (uri.Scheme == "file") {
				return uri.AbsolutePath;
			} else {
				return this.GetPathFromUrl (uri.AbsoluteUri);
			}
		}

		public string GetMBTilesUrl(string url)
		{
			var ext  = Path.GetExtension (url);

			if (ext != ".mbtiles") 
			{
				var last = url.Substring (url.Length - 1, 1);
				if (last != "/") {
					url += ".mbtiles";
				} else {
					url += "store.mbtiles";
				}
			}
			return url;
		}
		public string GetMBTilesUrl(Uri uri)
		{
			return this.GetMBTilesUrl (uri.AbsoluteUri);
		}

		public string GetManifestUrl(string url)
		{
			var regex = new Regex(@"manifest\.json$");

			if (!regex.Match(url).Success) 
			{
				var last = url.Substring (url.Length - 1, 1);
				if (last != "/") {
					url += ".manifest.json";
				} else {
					url += "manifest.json";
				}
			}
			return url;
		}
		public string GetManifestUrl(Uri uri)
		{
			return this.GetManifestUrl(uri.AbsoluteUri);
		}

		public void InitData() {
			//this.InitDataByManifest ();
			CopyFiles(null, path, dataDir);
		}

		public IManifest InitDataByManifest()
		{
			var manifestPath = this.path + "/" + "manifest.json";
			return this.HandleManifest(manifestPath);
		}

		public IManifest HandleManifest(string assetPath, string ParentUrl=null)
		{
			// Getting Source File
			var srcJson = this.GetAssetData (assetPath);

			var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<IManifest> (srcJson, new ManifestConverter(srcJson));
			//var output = Newtonsoft.Json.JsonConvert.SerializeObject(manifest,Formatting.Indented);
			//Console.WriteLine (output);

			//Get App Setting
			if (AppUrl == null) {
				AppUrl   = manifest.TargetUrl.AbsoluteUri;
				ModelUrl = manifest.ModelUrl.AbsoluteUri;
				AppName  = manifest.Name;
				AppTitle = manifest.Title;
			}

			//Getting Distination File
			var tgtUrl = manifest.TargetUrl;
			var rootManUrl = this.GetManifestUrl (tgtUrl.AbsoluteUri);
			var dstPath = this.GetPathFromUrl(rootManUrl);
			var shouldCopy = true;
			var srcUpdate = manifest.Updated;

			if (File.Exists (dstPath)) {
				var dstJson   = this.GetFileData (dstPath);
				var dstMani   = Newtonsoft.Json.JsonConvert.DeserializeObject<IManifest> (dstJson, new ManifestConverter(dstJson));
				var dstUpdate = dstMani.Updated;
				if (srcUpdate <= dstUpdate) {
					shouldCopy = false;
				}
			}

			if (!shouldCopy) return manifest;
			var resources = new List<Uri> ();

			if (this.FileExistsInAsset (this.GetAssetPathFromUrl (tgtUrl.AbsoluteUri))) 
			{
				resources.Add (tgtUrl);
			}
			resources.AddRange(manifest.Resources);

			foreach (var resUrl in resources) 
			{
				var src = this.GetAssetPathFromUrl (resUrl.AbsoluteUri);
				var dst = this.GetPathFromUrl (resUrl.AbsoluteUri);
				var manUrl = this.GetManifestUrl(resUrl.AbsoluteUri);
				var manSrc = this.GetAssetPathFromUrl(manUrl);
				var manDst = this.GetPathFromUrl(manUrl);
				if (src.Equals(manSrc) || (!manUrl.Equals(rootManUrl) && this.FileExistsInAsset(manSrc))) {
					this.HandleManifest (manSrc, tgtUrl.AbsoluteUri);
					continue;
				} else if (File.Exists (manDst)) {
					var dstJson = this.GetFileData (manDst);
					var dstMani = Newtonsoft.Json.JsonConvert.DeserializeObject<IManifest> (dstJson, new ManifestConverter(dstJson));
					if (srcUpdate <= dstMani.Updated) {
						continue;
					}
				}
				this.CopyDataWithCreateDirectories(src, dst);
			}

			this.CopyDataWithCreateDirectories(assetPath, dstPath);

			return manifest;
			//Console.WriteLine ("JSON JSON {0} - {1}", person.Name, person.Birthday);
			/* For JSON test */
		}

		public void DeleteData() {
			DeleteAll(dataDir);
		}

		private void DeleteAll(string file) {
			if (IsDirectoryInAsset(file)) {
				foreach (var f in Directory.EnumerateFiles(file)) {
					DeleteAll(f);
				}
			}
			File.Delete (file);
		}

		private void CopyFiles(string parentPath, string filename, string toDir) {

			var assetpath = (parentPath != null ? Path.Combine(parentPath, filename) : filename);
			if (IsDirectoryInAsset(assetpath)) {
				//if (!toDir.Exists()) {
				//	toDir.Mkdirs();
				//}
				if (!Directory.Exists (toDir)) {
					Directory.CreateDirectory (toDir);
				}
				foreach (string child in this.FilesInAsset(assetpath)) {
					CopyFiles(assetpath, child, Path.Combine(toDir, child));
				}
			} else {
				if (assetpath.ToLower().EndsWith(".zip")) {
					Unzip (assetpath, Path.GetDirectoryName (toDir));
				} else {
					var toPath = Path.Combine (Path.GetDirectoryName (toDir), filename);
					if (File.Exists (toPath)) return;
					CopyData(assetpath, toPath);
				}
			}
		}

		public string GetFileData (string filePath) 
		{
			string content;
			using (StreamReader sr = new StreamReader (filePath))
				content = sr.ReadToEnd ();
			return content;
		}

		private void CopyDataWithCreateDirectories(string assetPath, string ToFile)
		{
			var dir = Path.GetDirectoryName (ToFile);
			if (!Directory.Exists (dir)) 
			{
				Directory.CreateDirectory (dir);
			}
			CopyData(assetPath, ToFile);
		}

		public string GetMimeType(string extension)
		{
			if (extension == null)
			{
				throw new ArgumentNullException("extension");
			}

			if (!extension.StartsWith("."))
			{
				extension = "." + extension;
			}

			string mime;

			return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
		}

		public string GetSharedTileDBPath()
		{
			return this.GetPathFromUrl (this.AppUrl + "SharedTileDB.sqlite");
		}
	}
}

