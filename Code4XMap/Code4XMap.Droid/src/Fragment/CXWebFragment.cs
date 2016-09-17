using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Data.Sqlite;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Webkit;
using Java.Interop;

namespace Code4XMap
{
	[Activity (Label="")]//, Theme = "@style/slideAnim")]
	public class CXWebFragment : Android.Support.V4.App.Fragment
	{
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Set our view from the "main" layout resource
			//SetContentView (Resource.Layout.WebView);
			var rootView = inflater.Inflate(Resource.Layout.WebView, container, false);
			string url = "";

			if (this.Arguments != null) 
			{
				if (this.Arguments.ContainsKey ("url")) 
				{
					url = this.Arguments.GetString ("url");
				}
			}

			var view = rootView.FindViewById<CXWebView> (Resource.Id.webview);
			view.OwnerActivity = this.Activity;
			view.Settings.JavaScriptEnabled = true;

			view.LoadUrl (url);

			var me = this.Activity;
			view.DidLoadFinished += () => {
				me.RunOnUiThread(()=>{me.Title = view.Title;});
			};

			var onkey = new OnKeyListner(view);
			view.SetOnKeyListener(onkey);

			return rootView;
		}
	}

	public class OnKeyListner : Java.Lang.Object, View.IOnKeyListener
	{
		WeakReference<CXWebView> WebView;

		public OnKeyListner (CXWebView webView) : base()
		{
			WebView = new WeakReference<CXWebView>(webView);
		}
	
        public bool OnKey(View v, Keycode k, KeyEvent e)
		{
			if (e.Action == KeyEventActions.Down && k.Equals(Keycode.Back))
			{
				CXWebView webView;
				WebView.TryGetTarget(out webView);
				if (webView == null) return false;
				if (webView.CanGoBack())
				{
					webView.GoBack();
					return true;
				}
				else 
				{
					return false;
				}
			}
			return false;
		}
	}
}

