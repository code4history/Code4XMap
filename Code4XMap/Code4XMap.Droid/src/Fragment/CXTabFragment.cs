using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using System.Web;
using System.Collections.Specialized;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Support.V4.App;

namespace Code4XMap
{
	public class CXTabFragment : Android.Support.V4.App.Fragment, ICXTabFragment
	{

		protected TabHost tabHost;
		protected TabManager tabManager;
		protected CXTabController tController;
		protected Color? textColor = null;
		protected Color? selectedColor = null;

		private const int MENU_INFO  = 100;
		protected IMenuItem menuInfo;

		public CXTabFragment () : base ()
		{
			
		}

		protected CXTabController Controller()
		{
			return new CXTabController (this);
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			this.HasOptionsMenu = true;
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			if (menu.FindItem (MENU_INFO) != null) {
				menu.RemoveItem (MENU_INFO);
			}

			menuInfo  = menu.Add (0, MENU_INFO, 0, "情報");
			menuInfo.SetShowAsAction (ShowAsAction.Always);
			menuInfo.SetIcon (Resource.Drawable.info);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			case MENU_INFO:
				((CXTabController)Controller()).OnInfoSelected();
				return true;
			}
			return base.OnOptionsItemSelected (item);
		}
			
		public static URLAcceptChecker URLAcceptChecker<T>() where T : CXTabFragment
		{
			return (string host, string fragment) => {
				return (host == CXStorageManager.GetInstance<CXStorageManager>().AppUrl && fragment == "#tab") ? typeof(T) : null;
			};
		}

		public virtual void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
			var selected = qs ["selected"];
			if (selected != null) 
			{
				var selectedView = (IQueryStringAcceptor)tabManager.GetTabInfo (selected).fragment;
				selectedView.SetFragmentAndQuery (fragment, qs);
				tabHost.SetCurrentTabByTag (selected);
			}
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.Activity.Title = CXStorageManager.GetInstance<CXStorageManager>().AppTitle;

			var rootView = inflater.Inflate(Resource.Layout.TabView, container, false);
			tabHost = rootView.FindViewById<TabHost>(Android.Resource.Id.TabHost);

			tabHost.Setup();

			tabManager = new TabManager(this.Activity, tabHost, Resource.Id.realtabcontent);

			tController = Controller ();

			tController.SetTabList ();

			if (savedInstanceState != null) {
				tabHost.SetCurrentTabByTag(savedInstanceState.GetString("tab"));
			}

			if (this.Arguments != null) 
			{
				string frag = "";
				NameValueCollection qs = null;
				if (this.Arguments.ContainsKey ("fragment")) 
				{
					frag = this.Arguments.GetString ("fragment");
				}
				if (this.Arguments.ContainsKey ("querystring")) 
				{
					qs = HttpUtility.ParseQueryString (this.Arguments.GetString ("querystring"));
				}
				this.SetFragmentAndQuery (frag, qs);
			}

			return rootView;
		}
			
		public void AddTabTextColor (Color textColor, Color? selectedColor=null)
		{
			this.textColor = textColor;
			if (selectedColor != null) {
				this.selectedColor = selectedColor.Value;
			}
		}

		public void AddTab(Type viewClass, string name, string tag)
		{
			var tab = tabHost.NewTabSpec (tag);
			var view = View.Inflate (this.Activity, Resource.Layout.TabItem, null);

			var res = new StateListDrawable ();
			var selid = Resources.GetIdentifier ("tab_" + tag + "_selected", "drawable", this.Activity.PackageName);
			var unsid = Resources.GetIdentifier ("tab_" + tag + "_unselected", "drawable", this.Activity.PackageName);
			#pragma warning disable 618
			res.AddState(new int[1]{Android.Resource.Attribute.StateSelected},
			    Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop ? this.Resources.GetDrawable(selid) : this.Context.GetDrawable(selid));
			res.AddState(new int[0]{},
				Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop ? this.Resources.GetDrawable(unsid) : this.Context.GetDrawable(unsid));
			#pragma warning restore 618
			var tabImage = (ImageView)view.FindViewById (Resource.Id.TabImage);
			tabImage.SetImageDrawable (res);

			var stateList = new List<int[]> ();
			var colorList = new List<int> ();

			if (selectedColor != null) {
				stateList.Add (new int[1] {Android.Resource.Attribute.StateSelected});
				colorList.Add (Android.Graphics.Color.Rgb(selectedColor.Value.R,selectedColor.Value.G,selectedColor.Value.B));
			}
			if (textColor != null) {
				stateList.Add (new int[0] { });
				colorList.Add (Android.Graphics.Color.Rgb(textColor.Value.R,textColor.Value.G,textColor.Value.B));
			}
			if (stateList.Count != 0) {
				var col = new ColorStateList(stateList.ToArray(),colorList.ToArray());
				var tabText = (TextView)view.FindViewById (Resource.Id.TabText);
				tabText.SetTextColor(col);
			}

			var text = (TextView)view.FindViewById (Resource.Id.TabText);
			text.Text = name;
			tabManager.AddTab(tab.SetIndicator(view), Java.Lang.Class.FromType(viewClass), null);
		}

		public override void OnResume ()
		{
			base.OnResume ();
		}

		public override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);

			outState.PutString("tab", tabHost.CurrentTabTag);

		}

		protected class TabManager : Java.Lang.Object, TabHost.IOnTabChangeListener
		{
			private FragmentActivity _activity;
			private TabHost _tabHost;
			private int _containerId;
			private Dictionary<string, TabInfo> _tabs = new Dictionary<string, TabInfo>();
			TabInfo _lastTab;

			public class TabInfo 
			{
				public string tag;
				public Class clss;
				public Bundle args;
				public Android.Support.V4.App.Fragment fragment {get; set;}

				public TabInfo(string _tag, Class _class, Bundle _args) {
					tag = _tag;
					clss = _class;
					args = _args;
				}
			}

			public class DummyTabFactory : Java.Lang.Object, TabHost.ITabContentFactory 
			{
				private Context _context;

				public DummyTabFactory(Context context) {
					_context = context;
				}

				public View CreateTabContent (string tag)
				{
					var v = new View(_context);
					v.SetMinimumHeight(0);
					v.SetMinimumWidth(0);
					return v;
				}
			}

			public TabManager(FragmentActivity activity, TabHost tabHost, int containerId) 
			{
				_activity = activity;
				_tabHost = tabHost;
				_containerId = containerId;
				_tabHost.SetOnTabChangedListener(this);
			}

			public void AddTab(TabHost.TabSpec tabSpec, Class clss, Bundle args) 
			{
				tabSpec.SetContent(new DummyTabFactory(_activity));
				var tag = tabSpec.Tag;

				var info = new TabInfo(tag, clss, args);

				// Check to see if we already have a fragment for this tab, probably
				// from a previously saved state.  If so, deactivate it, because our
				// initial state is that a tab isn't shown.
				info.fragment = _activity.SupportFragmentManager.FindFragmentByTag(tag);
				if (info.fragment != null && !info.fragment.IsDetached) {
					var ft = _activity.SupportFragmentManager.BeginTransaction();
					ft.Detach(info.fragment);
					ft.Commit();
				}

				_tabs.Add(tag, info);
				_tabHost.AddTab(tabSpec);
			}

			public TabInfo GetTabInfo(string tabId)
			{
				return _tabs [tabId];
			}

			public void OnTabChanged (string tabId)
			{
				var newTab = _tabs[tabId];
				if (_lastTab != newTab) {
					var ft = _activity.SupportFragmentManager.BeginTransaction();
					if (_lastTab != null) {
						if (_lastTab.fragment != null) {
							ft.Detach(_lastTab.fragment);
						}
					}
					if (newTab != null) {
						if (newTab.fragment == null) {
							newTab.fragment = Android.Support.V4.App.Fragment.Instantiate(_activity, newTab.clss.Name, newTab.args);
							ft.Add(_containerId, newTab.fragment, newTab.tag);
						} else {
							ft.Attach(newTab.fragment);
						}
					}

					_lastTab = newTab;
					ft.Commit();
				}
			}

		}
	}
}

