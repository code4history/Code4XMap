using System;
using UIKit;
using CoreGraphics;
using System.Threading;
using Foundation;

namespace Code4XMap
{
	public class CXMobileUtility
	{
		public static bool IsPortrait() {
			return IsPortrait (UIApplication.SharedApplication.StatusBarOrientation);
		}

		public static bool IsPortrait(UIInterfaceOrientation orientation) {
			return (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown);
		}

		public static CGRect OrientedScreenFrame(){
			return OrientedScreenFrame (UIApplication.SharedApplication.StatusBarOrientation);
		}

		public static CGRect OrientedScreenFrame(UIInterfaceOrientation orientation){
			var ScreenFrame = UIScreen.MainScreen.Bounds;
			if (IsPortrait (orientation)) {
				return ScreenFrame;
			}
			var Buffer = ScreenFrame.Height;
			ScreenFrame.Height = ScreenFrame.Width;
			ScreenFrame.Width  = Buffer;
			return ScreenFrame;
		}

		public static float DisplayDensity()
		{
			float density = 1.0f;
			new NSObject().InvokeOnMainThread(()=> {
				density = (float)UIScreen.MainScreen.Scale;
			});
			//UIScreen.MainScreen.Scale;
			return density;
		}

		public static bool IsPhone()
		{
			return (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone);
		}

		public static bool IsIPhone5()
		{
			return IsPhone () && DisplayDensity () == 2.0f && UIScreen.MainScreen.Bounds.Height == 568.0f; 
		}

		public static bool IsIOS7()
		{
			var aOSVersions = UIDevice.CurrentDevice.SystemVersion.Split ('.');
			var iOSVersionMajor = int.Parse (aOSVersions [0]);
			return (iOSVersionMajor >= 7);
		}
	}
}