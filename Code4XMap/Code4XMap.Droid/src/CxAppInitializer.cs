using System;
using Android.App;
using PerpetualEngine.Storage;
using Android.OS;

namespace Code4XMap
{
	public class CXAppInitializer
	{
		public static void OnApplicationStart(Application app)
		{
			// SimpleStorageのイニシャライズ
			SimpleStorage.SetContext(app);
			// MemoryManagerに、メインスレッド実行用のハンドラー登録
			CXMemoryManager.GetInstance<CXMemoryManager> ();
		}

		public static void OnActivityCreated(Activity act, Android.OS.Bundle savedInstanceState)
		{
			OnActivityCreated (act, savedInstanceState, "data");
		}

		public static void OnActivityCreated(Activity act, Android.OS.Bundle savedInstanceState, string path)
		{
			OnActivityCreated (act, savedInstanceState, path, false);
		}

		public static void OnActivityCreated(Activity act, Android.OS.Bundle savedInstanceState, string path, bool useExternal)
		{
			if (savedInstanceState != null) {
				CXStorageManager.RestoreInstance<CXStorageManager> (savedInstanceState);
			} else {
				CXStorageManager.InitInstance<CXStorageManager> (path, useExternal);
			}
			CXMobileUtility.SetWindowManager (act);
		}
	}
}

