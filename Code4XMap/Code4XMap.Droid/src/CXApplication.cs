using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Code4XMap
{
	[Application]
	public class CXApplication : Application
	{
		public CXApplication (IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();

			CXAppInitializer.OnApplicationStart (this);

			//RaygunClient.Attach("+Vbhx0SEpgzkqBMx5F/fCg==");
			ManifestConverter.factories.Add (
				(string typeString) => {
					return typeString == "CXManifest" ? typeof(CXManifest) : null;
				}
			);
				
			CXMemoryManager.DefaultUrlOpener = typeof(CXWebFragment);
			CXMemoryManager.URLAcceptCheckers.Add (CXTabFragment.URLAcceptChecker<CXTabFragment>());
		}
	}
}

