using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using System.IO;
using Java.Interop;

namespace Code4XMap
{
	public class CXWebView : WebView, IMvvmView
	{
		public delegate void DidLoadFinishedHandler ();
		public event DidLoadFinishedHandler DidLoadFinished;

		private CXWebController _cachedController = null;

		private string _Title = null;
		public override string Title {
			get {
				return _Title;
			}
		}

		private CXWebViewClient _WebViewClient;
		private CXWebViewClient WebViewClient {
			set {
				_WebViewClient = value;
				this.SetWebViewClient (value);
			}

			get {
				return _WebViewClient;
			}
		}

		public int MaxCacheMBytes
		{
			get {
				return CXWebController.MaxCacheMBytes;
			}

			set {
				CXWebController.MaxCacheMBytes = value;
			}
		}

		public bool UseEachResourseCheck
		{
			get {
				return this.Controller ().UseEachResourseCheck;
			}

			set {
				this.Controller ().UseEachResourseCheck = value;
			}
		}

		public Activity OwnerActivity {
			set {
				this.WebViewClient.OwnerActivity = value;
			}

			get {
				return this.WebViewClient.OwnerActivity;
			}
		}

		public virtual CXWebController Controller()
		{
			if (_cachedController == null) {
				_cachedController = new CXWebController (this);
				//_cachedController.UseEachResourseCheck = true;
			}
			return _cachedController;
		}

		protected void InitViewClient()
		{
			this.Settings.JavaScriptEnabled = true;
			this.WebViewClient = new CXWebViewClient ();
			this.AddJavascriptInterface (this, "GetTitle");
		}

		public CXWebView (Context context) : base(context)
		{
			this.InitViewClient ();
		}

		public CXWebView (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			this.InitViewClient ();
		}

		public CXWebView (Context context, IAttributeSet attrs) : base(context, attrs)
		{
			this.InitViewClient ();
		}

		public CXWebView (Context context, IAttributeSet attrs, int defStyle) : base(context,attrs, defStyle)
		{
			this.InitViewClient ();
		}

		public CXWebView (Context context, IAttributeSet attrs, int defStyle, bool privateBrowsing) 
			: base(context,attrs,defStyle,privateBrowsing)
		{
			this.InitViewClient ();
		}

		public override void LoadUrl (string url)
		{
			base.LoadUrl (url);
		}

		public void OnLoadFinished (string url)
		{
			this.LoadUrl ("javascript:GetTitle.getTitle(document.title)");
		}

		[Export ("getTitle")]
		[JavascriptInterface]
		public void GetTitle(Java.Lang.String _title)
		{
			this._Title = _title.ToString();
			if (DidLoadFinished != null) 
				DidLoadFinished ();
		}
	}

	public class CXWebViewClient : WebViewClient
	{
		WeakReference _OwnerActivity;
		public Activity OwnerActivity {
			get { 
				return _OwnerActivity == null ? null : (Activity)_OwnerActivity.Target;
			}

			set {
				_OwnerActivity = value == null ? null : new WeakReference (value);
			}
		}

		#pragma warning disable 672
		public override WebResourceResponse ShouldInterceptRequest(WebView view, string url)
		{
			var response = ((CXWebView)view).Controller().CachedResponse(url);

			if (response == null)
				return null;

			var mime = response.Value.Mime;
			var enc = response.Value.Encoding;
			var bs = response.Value.ResponseBody;

			var webResourceResponse = new WebResourceResponse(mime, enc, new MemoryStream(bs));
			return webResourceResponse;
			//return base.ShouldInterceptRequest (view, url);
		}
		#pragma warning restore 672

		public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
			#pragma warning disable 618
			return this.ShouldInterceptRequest(view, request.Url.ToString());
			#pragma warning restore 618
		}

		public override void OnPageFinished (WebView view, string url)
		{
			((CXWebView)view).OnLoadFinished (url);
		}

		public override bool ShouldOverrideUrlLoading(WebView View, string Url) {
			if (Url.StartsWith ("tel:")) { 
				Intent intent = new Intent (Intent.ActionDial, Android.Net.Uri.Parse (Url));
				OwnerActivity.StartActivity (intent); 
				return true;
			} else if (Url.StartsWith ("browser:")) {
				Url = Url.Replace ("browser:", "");
				Intent intent = new Intent (Intent.ActionView, Android.Net.Uri.Parse (Url));
				OwnerActivity.StartActivity (intent);
				return true;
			}
			return false;
		}
	}
}

