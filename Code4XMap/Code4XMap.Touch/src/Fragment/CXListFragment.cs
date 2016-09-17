using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;
using System.Collections.Specialized;

namespace Code4XMap
{
	public class CXListFragment : UIViewController, ICXListFragment
	{
		protected CXListController lController;

		protected UISearchBar searchBar;
		protected UITableView tableView;
		private bool scrolling = false;

		protected virtual bool ShowSearchBar {
			get { 
				return true;
			}
		}

		public CXListFragment () : base()
		{	
		}

		protected virtual CGRect SearchBarFrame()
		{
			var prevFrame = View.Frame;
			var searchBarHeight = this.ShowSearchBar ? 44f : 0f;
			return new CGRect (0, 0, prevFrame.Width, searchBarHeight);
		}

		protected virtual CGRect TableViewFrame()
		{
			var prevFrame = View.Frame;
			var searchBarHeight = this.ShowSearchBar ? 44f : 0f;
			var tabHeight       = this.TabBarController == null ? 0 : this.TabBarController.TabBar.Frame.Height;
			var navHeight       = this.NavigationController.NavigationBar.Frame.Height;
			return new CGRect (0, searchBarHeight, prevFrame.Width, prevFrame.Height - searchBarHeight - tabHeight - navHeight);
		}

		public override void LoadView()
		{
			base.LoadView ();
			lController = Controller();

			if (this.ShowSearchBar) {
				searchBar = new UISearchBar (SearchBarFrame());
				View.Add (searchBar);
			}

			tableView = new UITableView(TableViewFrame(), UITableViewStyle.Plain);
			var delg = new ListViewDelegate (this);
			var dsrc = new ListViewDataSource (this);
			tableView.WeakDelegate = delg;
			tableView.WeakDataSource = dsrc;
			View.Add (tableView);

			if (this.ShowSearchBar) {
				searchBar.TextChanged += (object sender, UISearchBarTextChangedEventArgs e) => {
					lController.TextChange (searchBar.Text);
				};

				searchBar.SearchButtonClicked += (object sender, EventArgs e) => {
					searchBar.ResignFirstResponder ();
				};
			}
		}

		protected virtual CXListController Controller()
		{
			return new CXListController (this);
		}

		public void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
		}

		public void Update()
		{
			tableView.ReloadData();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (this.ShowSearchBar) {
				lController.TextChange (searchBar.Text);
			}
		}

		public override void ViewWillDisappear (bool animated)
		{
			if (this.ShowSearchBar) {
				searchBar.ResignFirstResponder ();
			}
			base.ViewWillDisappear (animated);
		}

		public virtual UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("Poi");
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, "Poi");

			var item = lController.Item (indexPath.Section, indexPath.Row);
			cell.TextLabel.Text       = item.MainListText();
			cell.DetailTextLabel.Text = item.SubListText();

			return cell;
		}

		public virtual float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 44f;
		}

		public string TitleForHeader (UITableView tableView, int section)
		{
			return lController.ItemGroup (section).ListText();
		}

		public int RowsInSection (UITableView tableview, int section)
		{
			return lController.ItemCount (section);
		}

		public int NumberOfSections (UITableView tableView)
		{
			var count = lController.CategoryCount ();
			return count;
		}

		public virtual void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			lController.SelectCategoryRow (indexPath.Section, indexPath.Row);
		}

		public virtual void WorkOnSelectItem (CXModel.Item item)
		{
			CXMemoryManager.OpenNextUrl ((CXTabFragment)this.TabBarController, CXStorageManager.GetInstance<CXStorageManager>().AppUrl + String.Format("?dbid={0}&selected=map#tab",item.id));
		}

		public void Scrolled (UIScrollView scrollView)
		{
			if (scrolling == false) {
				if (this.ShowSearchBar) {
					searchBar.ResignFirstResponder ();
				}
				scrolling = true;
			}
		}

		public void DecelerationEnded (UIScrollView scrollView)
		{
			scrolling = false;
		}

		public void DecelerationStarted (UIScrollView scrollView)
		{
			if (scrolling == false) {
				if (this.ShowSearchBar) {
					searchBar.ResignFirstResponder ();
				}
				scrolling = true;
			}		
		}
	}

	public class ListViewDelegate : UITableViewDelegate
	{
		private WeakReference reference;

		public ListViewDelegate (CXListFragment reference) : base()
		{
			this.reference = new WeakReference (reference);
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			((CXListFragment)reference.Target).RowSelected (tableView, indexPath);
		}

		public override void Scrolled (UIScrollView scrollView)
		{
			((CXListFragment)reference.Target).Scrolled (scrollView);
		}

		public override void DecelerationEnded (UIScrollView scrollView)
		{
			((CXListFragment)reference.Target).DecelerationEnded (scrollView);
		}

		public override void DecelerationStarted (UIScrollView scrollView)
		{
			((CXListFragment)reference.Target).DecelerationStarted (scrollView);
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return ((CXListFragment)reference.Target).GetHeightForRow (tableView, indexPath);
		}
	}

	public class ListViewDataSource : UITableViewDataSource
	{
		private WeakReference reference;

		public ListViewDataSource (CXListFragment reference) : base()
		{
			this.reference = new WeakReference (reference);
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			return ((CXListFragment)reference.Target).GetCell (tableView, indexPath);
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return ((CXListFragment)reference.Target).RowsInSection (tableView, (int)section);
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return ((CXListFragment)reference.Target).NumberOfSections (tableView);
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return ((CXListFragment)reference.Target).TitleForHeader (tableView, (int)section);
		}
	}
}

