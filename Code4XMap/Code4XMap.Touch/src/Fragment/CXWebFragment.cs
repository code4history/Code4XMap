using System;
using UIKit;
using Foundation;

namespace Code4XMap
{
	public class CXWebFragment : UIViewController
	{
		private string _url = null;

		public CXWebFragment () : base()
		{
		}
		public CXWebFragment (string url) : base()
		{
			_url = url;
		}

		public override void LoadView ()
		{
			//base.LoadView ();
			var webView = new CXWebView ();
			View = webView;
			if (_url != null) 
			{
				webView.LoadRequest (new NSUrlRequest (new NSUrl (_url)));
			}

			webView.DidLoadFinished += () => {
				this.Title = webView.Title;
			};
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			this.NavigationItem.SetHidesBackButton(true, true);
			var back = new UIBarButtonItem("< Back", UIBarButtonItemStyle.Plain, this, new ObjCRuntime.Selector("didBackButtonTap:"));
			this.NavigationItem.LeftBarButtonItem = back;
		}

		[Export("didBackButtonTap:")]
		public void DidBackButtonTap(UIBarButtonItem item) {
			var webView = (UIWebView)View;
			if (webView.CanGoBack)
			{
				webView.GoBack();
			}
			else 
			{
				this.NavigationController.PopViewController(true);
			}
		}
	}
}

