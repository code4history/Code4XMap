using System;
using Android.App;
using Android.Views;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;

using Android.Content;
using Android.Views.InputMethods;
using System.Collections.Specialized;
using Android.Graphics;
using Android.Support.V4.Content;

namespace Code4XMap
{
	public class CXListFragment : Android.Support.V4.App.Fragment, ICXListFragment
	{
		protected CXListController lController;
		protected SearchView searchView;
		protected Android.Widget.ListView   listView;

		protected virtual bool ShowSearchBar {
			get { 
				return true;
			}
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			lController = Controller();
		}

		protected virtual CXListController Controller()
		{
			return new CXListController (this);
		}

		public void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
		}

		protected virtual ChoiceMode GetChoiceMode()
		{
			return ChoiceMode.Single;
		}

		protected virtual CXListAdapter GetAdapter()
		{
			return new CXListAdapter (this.Activity, lController);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var rootView = inflater.Inflate (Resource.Layout.ListView, container, false);

			searchView = (SearchView)rootView.FindViewById(Resource.Id.searchView);
			if (!this.ShowSearchBar) {
				searchView.Visibility = ViewStates.Gone;
			}

			listView = (Android.Widget.ListView)rootView.FindViewById (Resource.Id.listView);
			listView.ChoiceMode = GetChoiceMode ();

			var adapter = GetAdapter ();

			listView.Adapter = adapter;
			listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				//adapter.
				this.OnItemClick(sender, e);
			};

			if (this.ShowSearchBar) {
				searchView.QueryTextChange += (object sender, SearchView.QueryTextChangeEventArgs e) => {
					lController.TextChange (e.NewText);
				};
			}

			listView.ScrollStateChanged += (object sender, AbsListView.ScrollStateChangedEventArgs e) => {
				var imm = (InputMethodManager)this.Activity.GetSystemService (Context.InputMethodService);
				imm.HideSoftInputFromWindow (((View)sender).WindowToken, HideSoftInputFlags.NotAlways);
			};

			return rootView;
		}

		public virtual void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var SecRow = ((CXListAdapter)listView.Adapter).PositionToSectionRow (e.Position);
			lController.SelectCategoryRow (SecRow.section, SecRow.row);
		}

		public virtual void WorkOnSelectItem(CXModel.Item item)
		{
			CXMemoryManager.OpenNextUrl ((IQueryStringAcceptor)this.Activity, CXStorageManager.GetInstance<CXStorageManager>().AppUrl + String.Format("?dbid={0}&selected=map#tab",item.id));
		}

		public void Update()
		{
			var mem = CXMemoryManager.GetInstance<CXMemoryManager> ();
			mem.InvokeUIThread (()=>{
				((CXListAdapter)listView.Adapter).NotifyDataSetChanged();
			});
		}
	}

	public class CXListAdapter : BaseAdapter<CXModel.Item> {
		public struct SectionRow
		{
			public int section;
			public int row;
		}

		protected WeakReference context;
		protected CXListController lController;

		public CXListAdapter(Activity context, CXListController lController) : base() {
			this.context     = new WeakReference (context);
			this.lController = lController;
		}

		public SectionRow PositionToSectionRow(int position)
		{
			int catStart = 0;
			for (int i = 0; i < lController.CategoryCount(); i++)
			{
				var catEnd = catStart + lController.ItemCount(i);
				if (position == catStart)
				{
					return new SectionRow {
						section = i,
						row = -1
					};
				} else if (position > catStart && position <= catEnd) {
					return new SectionRow {
						section = i,
						row = position - catStart - 1
					};
				}
				catStart = catEnd + 1;
			}
			return new SectionRow {
				section = -1,
				row = -1
			};
		}

		public override long GetItemId(int position)
		{
			var sr = this.PositionToSectionRow (position);
			if (sr.row == -1) return -1;
			return lController.Item (sr.section,sr.row).id;
		}

		public override CXModel.Item this[int position] {  
			get { 
				var sr = this.PositionToSectionRow (position);
				return lController.Item (sr.section,sr.row); 
			}
		}

		public override int Count {
			get { 
				var cnt = lController.CategoryCount ();
				var ret = cnt;
				for (int i = 0; i < cnt; i++)
				{
					ret += lController.ItemCount(i);
				}

				return ret; 
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			var sr = this.PositionToSectionRow (position);
			var act = (Activity)context.Target;

			if (sr.row != -1) {
				var item = lController.Item (sr.section,sr.row);
				if (lController.IsIdSelected (item.id)) {
					view = this.GetSelectedCellView (sr, view, parent);
				} else {
					view = this.GetUnselectedCellView (sr, view, parent);
				}
			} else {
				view = this.GetHeaderCellView (sr.section, view, parent);
			}

			return view;
		}

		protected virtual View GetSelectedCellView(SectionRow sr, View view, ViewGroup parent)
		{
			var act = (Activity)context.Target;

			if (view == null) // otherwise create a new one
				view = act.LayoutInflater.Inflate(
					Resource.Layout.SimpleTwoLineList, null);
			var text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			var text2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);

			var item = lController.Item (sr.section,sr.row);
			text1.Text = item.MainListText();
			text2.Text = item.SubListText();

			view.SetBackgroundResource(Resource.Drawable.list_item_selected);
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
			{
				text1.SetTextColor(act.GetColorStateList(Resource.Drawable.list_text1_selected));
				text2.SetTextColor(act.GetColorStateList(Resource.Drawable.list_text2_selected));
			}
			else
			{
				#pragma warning disable 618
				text1.SetTextColor(act.Resources.GetColorStateList(Resource.Drawable.list_text1_selected));
				text2.SetTextColor(act.Resources.GetColorStateList(Resource.Drawable.list_text2_selected));
				#pragma warning restore 618
			}
			return view;
		}

		protected virtual View GetUnselectedCellView(SectionRow sr, View view, ViewGroup parent)
		{
			var act = (Activity)context.Target;

			if (view == null) // otherwise create a new one
				view = act.LayoutInflater.Inflate(
					Resource.Layout.SimpleTwoLineList, null);
			var text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			var text2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);

			var item = lController.Item (sr.section,sr.row);
			text1.Text = item.MainListText();
			text2.Text = item.SubListText();

			view.SetBackgroundResource(Resource.Drawable.list_item);
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M) {
				text1.SetTextColor (act.GetColorStateList (Resource.Drawable.list_text1));
				text2.SetTextColor (act.GetColorStateList (Resource.Drawable.list_text2));
			} else {
				#pragma warning disable 618
				text1.SetTextColor (act.Resources.GetColorStateList (Resource.Drawable.list_text1));
				text2.SetTextColor (act.Resources.GetColorStateList (Resource.Drawable.list_text2));				
				#pragma warning restore 618
			}

			return view;
		}

		protected virtual View GetHeaderCellView(int section, View view, ViewGroup parent)
		{
			var act = (Activity)context.Target;

			if (view == null) // otherwise create a new one
				view = act.LayoutInflater.Inflate(
					Android.Resource.Layout.SimpleListItem1, null);

			var cat = lController.ItemGroup(section);
			((TextView)view).Text = cat.ListText();
			var colorInt = ContextCompat.GetColor (act, Resource.Color.black);
			view.SetBackgroundColor (Color.Rgb(Color.GetRedComponent(colorInt), Color.GetGreenComponent(colorInt), Color.GetBlueComponent(colorInt)));
			colorInt = ContextCompat.GetColor (act, Resource.Color.white);
			((TextView)view).SetTextColor (Color.Rgb(Color.GetRedComponent(colorInt), Color.GetGreenComponent(colorInt), Color.GetBlueComponent(colorInt)));

			return view;
		}

		public override bool IsEnabled (int position)
		{
			var sr = this.PositionToSectionRow (position);
			return sr.row != -1;
		}

		public override int ViewTypeCount {
			get {
				return 2;
			}
		}

		public override int GetItemViewType (int position)
		{
			var sr = this.PositionToSectionRow (position);
			return sr.row != -1 ? 1 : 0;
		}

		public override void NotifyDataSetChanged ()
		{
			base.NotifyDataSetChanged ();
		}
	}
}

