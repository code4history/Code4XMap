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
	}
}

