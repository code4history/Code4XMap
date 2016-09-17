using System;
using CoreGraphics;
using Foundation;
using UIKit;

using BigTed;
using System.Threading.Tasks;
using System.Threading;

namespace Code4XMap
{
	public class CXSplashPage : UIViewController, IMvvmView
	{
		public class LocalImageView : UIImageView
		{
			public override CGRect Frame {
				get {
					return base.Frame;
				}
				set {
					var devBound  = UIScreen.MainScreen.Bounds;
					var appBound  = UIScreen.MainScreen.ApplicationFrame;
					var appTop    = CXMobileUtility.IsIOS7() ?
						0f :
						appBound.Height - devBound.Height;
					base.Frame = new CGRect(0, appTop, devBound.Width, devBound.Height);
				}
			}

			public LocalImageView (NSCoder coder) : base(coder)
			{

			}

			public LocalImageView (NSObjectFlag t) : base(t)
			{

			}

			public LocalImageView(UIImage image, UIImage highlightedImage) : base(image, highlightedImage)
			{

			}

			public LocalImageView(UIImage image) : base(image)
			{

			}

			public LocalImageView (IntPtr handle) : base(handle)
			{

			}

			public LocalImageView (CGRect frame) : base(frame)
			{
			}

			public LocalImageView() : base()
			{

			}
		}
			
		public override void LoadView ()
		{
			var nappear = UINavigationBar.Appearance;
			var navBackImg = UIImage.FromBundle ("navigationbar.png")
				.CreateResizableImage (new UIEdgeInsets (0, 0, 5, 0));
			nappear.SetBackgroundImage (navBackImg, UIBarMetrics.Default);

			var tappear = UITabBar.Appearance;
			var tabBackImg = UIImage.FromBundle ("tabbar.png")
				.CreateResizableImage (new UIEdgeInsets (5, 0, 0, 0));
			tappear.BackgroundImage = tabBackImg;

			if (CXMobileUtility.IsIOS7 ()) {
				UINavigationBar.Appearance.TintColor = UIColor.Black;
			} else {
				var rightBtnImg = UIImage.FromBundle ("button_navigation_right.png")
					.CreateResizableImage (new UIEdgeInsets (5, 5, 5, 5));
				Type[] bars = { typeof(UINavigationBar) };
				UIBarButtonItem.AppearanceWhenContainedIn (bars)
					.SetBackgroundImage (rightBtnImg, UIControlState.Normal, UIBarMetrics.Default);

				var leftBtnImg = UIImage.FromBundle ("button_navigation_left.png")
					.CreateResizableImage (new UIEdgeInsets (5, 15, 5, 5));
				UIBarButtonItem.AppearanceWhenContainedIn (bars)
					.SetBackButtonBackgroundImage (leftBtnImg, UIControlState.Normal, UIBarMetrics.Default);
			}
				
			nappear.SetTitleTextAttributes (
				new UITextAttributes {
					TextColor = UIColor.Black
				}
			);

			CXMemoryManager.DefaultUrlOpener = typeof(CXWebFragment);
			CXMemoryManager.URLAcceptCheckers.Add (CXTabFragment.URLAcceptChecker<CXTabFragment>());

			CXStorageManager.InitInstance<CXStorageManager>("data");

			var splashFile = "Default.png";
			if (!CXMobileUtility.IsPhone()) {
				splashFile = "Default-Portrait.png";
			} else if (CXMobileUtility.IsIPhone5()) {
				splashFile = "Default-568h.png";
			}

			var imgView = new LocalImageView (UIImage.FromBundle (splashFile));
			View = imgView;
		}

		protected Type MainView ()
		{
			return typeof(CXTabFragment);
		}

		protected CXSplashController Controller ()
		{
			return new CXSplashController (this);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.NavigationController.SetNavigationBarHidden (true, true);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			var memman = CXMemoryManager.GetInstance<CXMemoryManager> ();

			var me = this;
			BTProgressHUD.Show("ロード中です…", -1, ProgressHUD.MaskType.Clear);
			//var t = 
			Task.Factory.StartNew(() => {
				me.Controller ().Work ();

				memman.InvokeUIThread(()=>{
					var views = me.NavigationController.ViewControllers;
					var nextView = (UIViewController) Activator.CreateInstance (me.MainView());
					me.NavigationController.PushViewController (nextView, true);
				});
			});
		}

		public override void ViewWillDisappear (bool animated)
		{
			this.NavigationController.SetNavigationBarHidden (false, true);

			var views = this.NavigationController.ViewControllers;
			var nextView = views [views.Length - 1];
			nextView.NavigationItem.HidesBackButton = true;

			BTProgressHUD.Dismiss ();
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		public UINavigationController CreateNavigationController()
		{
			return new UINavigationController (this);
		}
	}
}

