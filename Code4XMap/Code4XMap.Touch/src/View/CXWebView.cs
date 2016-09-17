using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.IO;
using System.Json;
using System.Web;

namespace Code4XMap
{
	public class CXWebView : UIWebView, IMvvmView
	{
		public object BridgeDelegate {get;set;}

		private const string BRIDGE_JS = @"
            function invokeNative(functionName, args) {
                var iframe = document.createElement('IFRAME');
                iframe.setAttribute('src', 'jsbridge://' + functionName + '#' + JSON.stringify(args));
                document.documentElement.appendChild(iframe);
                iframe.parentNode.removeChild(iframe);
                iframe = null;  
            }

            var console = {
                log: function(msg) {
                    invokeNative('Log', [msg]);
                }
            };  
        ";

		public delegate void DidLoadFinishedHandler ();
		public event DidLoadFinishedHandler DidLoadFinished;
		private static bool CustomCacheInitialized = false;

		private string _Title = null;
		public string Title {
			get {
				return _Title;
			}
			private set {
				_Title = value;
			}
		}

		public CXWebView () :base()
		{
			this.Initialize ();
		}

		public CXWebView (CGRect frame) : base(frame)
		{
			this.Initialize ();
		}

		public CXWebView (NSCoder coder) : base(coder)
		{
			this.Initialize ();
		}

		public CXWebView (NSObjectFlag t) : base(t)
		{
			this.Initialize ();
		}

		public CXWebView (IntPtr handle) : base(handle)
		{
			this.Initialize ();
		}

		protected void Initialize ()
		{
			lock (this) {
				if (!CustomCacheInitialized) {
					iOS8Initialized ();
					CustomCacheInitialized = true;
				}
			}
			this.ShouldStartLoad += LoadHandler;
			this.LoadFinished += OnLoadFinished;
		}

		public static void iOS8Initialized ()
		{
			var paths = NSSearchPath.GetDirectories (NSSearchPathDirectory.CachesDirectory, 
				NSSearchPathDomain.User, true);
			var path = paths [0] + "/webCache";

			var defaultCache = NSUrlCache.SharedCache;
			NSUrlCache.SharedCache = new CXWebViewCache ((uint)defaultCache.MemoryCapacity, (uint)defaultCache.DiskCapacity, path);
		}

		private void OnLoadFinished (object sender, EventArgs e)
		{
			Title = this.EvaluateJavascript ("document.title;");

			if (DidLoadFinished != null)
				DidLoadFinished ();
		}

		public bool LoadHandler (UIWebView webView, Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			var url = request.Url;
			if (url.Scheme.Equals ("jsbridge")) {
				var func = url.Host;
				if (func.Equals ("Log")) {
					// console.log
					var args = JsonObject.Parse (HttpUtility.UrlDecode (url.Fragment));
					var msg = (string)args [0];
					Console.WriteLine (msg);
					return false;
				}
				return true;
			} else if (url.Scheme.Equals ("browser")) {
				var goURL = new NSUrl(url.AbsoluteString.Replace ("browser:", ""));
				UIApplication.SharedApplication.OpenUrl (goURL);
				return false;
			}
			return true;
		} 
	}

	public class CXWebViewCache : NSUrlCache
	{
		public CXWebViewCache (uint memoryCapacity, uint diskCapacity, string diskPath) 
							: base(memoryCapacity, diskCapacity, diskPath)
		{			
		}

		public override NSCachedUrlResponse CachedResponseForRequest (NSUrlRequest request)
		{
			var response = CXWebController.CachedResponseStatic (request.Url.AbsoluteString);

			if (response == null)
				return base.CachedResponseForRequest (request);

			var mime = response.Value.Mime;
			var enc  = response.Value.Encoding;
			var bs   = response.Value.ResponseBody;
			var data = NSData.FromArray (bs);

			var a_res = new NSUrlResponse (request.Url, mime, (int)data.Length, enc);

			var cached = new NSCachedUrlResponse (a_res, data);
			return cached;
		}
	}
}

