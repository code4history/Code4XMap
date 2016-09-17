using System;
using System.Threading;
using System.IO;

using Android.App;
using Android.OS;

using AndroidHUD;
using System.Threading.Tasks;
using Android.Content;

namespace Code4XMap
{
	public class CXSplashPage : Activity, IMvvmView
	{
		protected Type MainView ()
		{
			return typeof(CXTabFragment);
		}

		protected CXSplashController Controller ()
		{
			return new CXSplashController (this);
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.SplashLayout);
			CXAppInitializer.OnActivityCreated (this, savedInstanceState);
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			var memman = CXMemoryManager.GetInstance<CXMemoryManager> ();

			var me = this;
			AndHUD.Shared.Show(this, "ロード中です…", -1, MaskType.Clear,null);

			Task.Factory.StartNew(() => {
				me.Controller ().Work ();

				memman.InvokeUIThread(()=>{
					var intent = new Intent(this, this.BaseActivity());
					intent.PutExtra("StartFragment",this.MainView().AssemblyQualifiedName);
					this.StartActivity(intent);
					AndHUD.Shared.Dismiss (this);
				});
			});

			var appkey = GetString (Resources.GetIdentifier ("hockey", "string", PackageName));
			if (appkey != null && appkey != "") {
				CXMemoryManager.HockeyOnResume (this, appkey);
			}
		}

		protected override void OnPause ()
		{
			var appkey = GetString (Resources.GetIdentifier ("hockey", "string", PackageName));
			if (appkey != null && appkey != "") {
				CXMemoryManager.HockeyOnPause (this, appkey);
			}			
			CXMemoryManager.HockeyOnPause (this, appkey);

			base.OnPause ();
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		protected virtual Type BaseActivity()
		{
			return typeof(CXMainPage);
		}
	}
}
