using System;
using System.IO;
using System.Collections.Generic;

namespace Code4XMap
{
	public partial class CXWebController : MvvmController
	{
		public static PackedResponse? AssetCachedResponse(string url)
		{
			var storage = CXStorageManager.GetInstance<CXStorageManager> ();//SharedManager ();
			var assetPath = storage.AssetFullPath(storage.GetAssetPathFromUrl (url));

			return CXWebController.StorageCachedResponse (url, assetPath);
		}
	}
}

