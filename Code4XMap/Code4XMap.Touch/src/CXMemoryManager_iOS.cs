using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Web;
using HockeyApp;

namespace Code4XMap
{
	public partial class CXMemoryManager
	{
		public static void OpenAppUrl(IQueryStringAcceptor parent, Type next, string host, string frag, string qs)
		{
			var partype = parent.GetType ();
			UIViewController nextView = null;
			if (partype == next) {
				nextView = (UIViewController)parent;
			} else {
				nextView = (UIViewController)Activator.CreateInstance (next);
				((UIViewController)parent).NavigationController.PushViewController(nextView, true);
			}
			var qsobj = HttpUtility.ParseQueryString (qs);
			((IQueryStringAcceptor)nextView).SetFragmentAndQuery (frag, qsobj);
		}

		public static void OpenWebUrl(IQueryStringAcceptor parent, Type next, string url)
		{
			var par = (UIViewController)parent;
			var nextView = (UIViewController)Activator.CreateInstance (next, new object[] { url });

			par.NavigationController.PushViewController(nextView, true);
		}

		public static void HockeySetUp(string appID)
		{
			HockeyApp.Setup.EnableCustomCrashReporting (() => {

				//Get the shared instance
				var manager = BITHockeyManager.SharedHockeyManager;

				//Configure it to use our APP_ID
				manager.Configure (appID);

				//Start the manager
				manager.StartManager ();

				//Authenticate (there are other authentication options)
				manager.Authenticator.AuthenticateInstallation ();
			});
		}
	}
}

