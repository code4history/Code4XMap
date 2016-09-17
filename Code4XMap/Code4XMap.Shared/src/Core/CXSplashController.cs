using System;
using PerpetualEngine.Storage;

namespace Code4XMap
{
	public class CXSplashController : MvvmController
	{
		public CXSplashController (IMvvmView _View) : base(_View)
		{
		}

		public void Work ()
		{
			var manager = CXStorageManager.GetInstance<CXStorageManager> ();
			var manifest = (CXManifest)manager.InitDataByManifest ();
			CXMemoryManager.SetBoundsLimit (manifest.boundsLimit);
			var start   = manifest.startPosition;
			var storage = SimpleStorage.EditGroup("StartMapPosition");
			if (storage.HasKey ("Lat") && storage.HasKey ("Lng") && storage.HasKey ("Zoom")) {
			} else {
				storage.Put("Lat",String.Format("{0}",start.Latitude));
				storage.Put("Lng",String.Format("{0}",start.Longitude));
				storage.Put("Zoom",String.Format("{0}",start.Zoom));
			}
		}
	}
}



