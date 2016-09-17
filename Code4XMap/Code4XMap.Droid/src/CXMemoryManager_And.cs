using System;
using Android.Content;
using Android.App;
using Android.OS;
using System.Collections.Specialized;
using System.Web;
using Android.Support.V4.App;
using HockeyApp;

namespace Code4XMap
{
	public partial class CXMemoryManager
	{
		public static void OpenAppUrl(IQueryStringAcceptor parent, Type next, string host, string frag, string qs)
		{
			Android.Support.V4.App.FragmentManager fm;
			try {
				fm = ((FragmentActivity)parent).SupportFragmentManager;
			} catch {
				fm = ((Android.Support.V4.App.Fragment)parent).Activity.SupportFragmentManager;
			}
			var InBuffer = (IQueryStringAcceptor)fm.FindFragmentByTag (host + frag);

			if (InBuffer != null) {
				fm.PopBackStack (host, 0);
				var qsval = HttpUtility.ParseQueryString (qs);
				InBuffer.SetFragmentAndQuery (frag, qsval);
			} else {
				var transaction  = fm.BeginTransaction ();
				var nextFragment = (Android.Support.V4.App.Fragment)Activator.CreateInstance (next);
				var args = new Bundle();
				args.PutString("fragment",frag);
				args.PutString("querystring", qs);
				nextFragment.Arguments = args;

				transaction.Add(Resource.Id.fragment_container, nextFragment, host);
				transaction.Commit ();
			}
		}

		public static void OpenWebUrl(IQueryStringAcceptor parent, Type next, string url)
		{
			Android.Support.V4.App.FragmentManager fm;
			try {
				fm = ((FragmentActivity)parent).SupportFragmentManager;
			} catch {
				fm = ((Android.Support.V4.App.Fragment)parent).Activity.SupportFragmentManager;
			}

			var transaction  = fm.BeginTransaction ();
			var nextFragment = (Android.Support.V4.App.Fragment)Activator.CreateInstance (next);
			var args = new Bundle();
			args.PutString("url",url);
			nextFragment.Arguments = args;

			transaction.AddToBackStack (null);
			transaction.Replace (Resource.Id.fragment_container, nextFragment);//"WebView");
			transaction.Commit ();
		}

		public static void HockeyOnResume(Activity target, string appID)
		{
			//Register for Crash detection / handling
			// You should do this in your main activity
			HockeyApp.CrashManager.Register (target, appID);

			//Start Tracking usage in this activity
			HockeyApp.Tracking.StartUsage (target);
		}

		public static void HockeyOnPause(Activity target, string appID)
		{
			//Stop Tracking usage in this activity
			HockeyApp.Tracking.StopUsage (target);
		}
	}
}

