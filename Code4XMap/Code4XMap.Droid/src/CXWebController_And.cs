using System;
using System.IO;

namespace Code4XMap
{
	public partial class CXWebController : MvvmController
	{
		public static PackedResponse? AssetCachedResponse(string url)
		{
			var storage = CXStorageManager.GetInstance<CXStorageManager> ();//SharedManager ();
			var assetPath = storage.GetAssetPathFromUrl (url);

			if (storage.FileExistsInAsset(assetPath)) {
				var ext  = Path.GetExtension (assetPath).ToLower();
				var mime = storage.GetMimeType (ext);

				var bs = storage.GetAssetByteData (assetPath);

				var ret = new PackedResponse
				{ 
					Mime = mime,
					Encoding = "UTF-8",
					ResponseBody = bs
				};

				//var webResourceResponse = new WebResourceResponse (mime, null, inputStream);
				memoryCache [url] = ret;
				return ret;
			}

			return null;
		}
	}
}

