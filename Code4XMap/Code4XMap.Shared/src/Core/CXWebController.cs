using System;
using System.IO;
using System.Collections.Generic;

namespace Code4XMap

{
	public struct PackedResponse {
		public string Mime;
		public string Encoding;
		public byte[] ResponseBody;
	}

	public partial class CXWebController : MvvmController
	{
		private static Dictionary<string, PackedResponse?> memoryCache = new Dictionary<string, PackedResponse?> ();
		public  static int MaxCacheMBytes = 0;
		//private static int CachedBytes = 0;

		public bool UseEachResourseCheck = true;

		public CXWebController(IMvvmView _View) : base(_View) { }

		public PackedResponse? CachedResponse(string url)
		{
			if (!UseEachResourseCheck)
				return null;

			return CXWebController.CachedResponseStatic (url);
		}

		public static PackedResponse? MemoryCachedResponse(string url, PackedResponse? value = null)
		{
			if (value != null) {
				memoryCache [url] = value;
			}

			return memoryCache.ContainsKey (url) ? memoryCache [url] : null;
		}

		public static PackedResponse? StorageCachedResponse(string url, string localPath = null)
		{
			var storage = CXStorageManager.GetInstance<CXStorageManager> ();//SharedManager ();
			if (localPath == null) {
				localPath = storage.GetPathFromUrl (url);
			}

			if (File.Exists (localPath)) 
			{
				var ext  = Path.GetExtension (localPath).ToLower();
				var mime = storage.GetMimeType (ext);

				var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
				var bs = new byte[fs.Length];
				fs.Read(bs, 0, bs.Length);
				fs.Close();
				Console.WriteLine (bs.ToString ());

				var ret = new PackedResponse
				{ 
					Mime = mime,
					Encoding = "UTF-8",
					ResponseBody = bs
				};
						
				memoryCache [url] = ret;
				return ret;
			}

			return null;
		}

		public static PackedResponse? CachedResponseStatic(string url) 
		{
			if (url.IndexOf ("http") != 0)
				return null;

			var urls = url.Split ('?');
			url = urls [0];

			var ret = CXWebController.MemoryCachedResponse (url);
			if (ret != null)
				return ret;

			ret = CXWebController.AssetCachedResponse (url);
			if (ret != null)
				return ret;

			ret = CXWebController.StorageCachedResponse (url);
			if (ret != null)
				return ret;

			memoryCache [url] = null;
			return null;
		}
	}
}

