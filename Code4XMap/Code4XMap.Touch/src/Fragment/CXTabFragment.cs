using System;
using System.Drawing;
using CoreGraphics;
using UIKit;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace Code4XMap
{
	public class CXTabFragment : UITabBarController, ICXTabFragment
	{
		protected Dictionary<string,int> tagList = new Dictionary<string,int> (); 
		protected CXTabController tController;
		protected Color? textColor = null;
		protected Color? selectedColor = null;

		public CXTabFragment () : base()
		{
		}

		protected CXTabController Controller()
		{
			return new CXTabController (this);
		}

		public override void LoadView ()
		{
			base.LoadView ();

			tController = Controller ();

			tController.SetTabList ();

			this.NavigationItem.RightBarButtonItem = 
				new UIBarButtonItem (
					UIImage.FromBundle ("info_selected"),
					UIBarButtonItemStyle.Bordered,
					(object sender, EventArgs e)=>{
						((CXTabController)Controller()).OnInfoSelected();
					}
				);
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
				this.SelectedIndex = tagList [selected];
				var selectedView = (IQueryStringAcceptor)this.ViewControllers [this.SelectedIndex];
				selectedView.SetFragmentAndQuery (fragment, qs);
			}
		}

		public void AddTabTextColor (Color textColor, Color? selectedColor=null)
		{
			this.textColor = textColor;
			if (selectedColor != null) {
				this.selectedColor = selectedColor.Value;
			}
		}

		public void AddTab(Type viewClass, string name, string tag)//, string icon, string selected_icon=null)
		{
			var index = (this.ViewControllers == null) ? 0 : this.ViewControllers.Length;

			var view = (UIViewController)Activator.CreateInstance (viewClass);
			view.Title = name;
			var normal_img = UIImage.FromBundle ("tab_" + tag + "_unselected.png");
			var select_img = UIImage.FromBundle ("tab_" + tag + "_selected.png");
			view.TabBarItem.SelectedImage = select_img.ImageWithRenderingMode (UIImageRenderingMode.AlwaysOriginal);
			view.TabBarItem.Image = normal_img.ImageWithRenderingMode (UIImageRenderingMode.AlwaysOriginal);
			if (textColor != null) {
				var bColor = textColor.Value;
				var color = new UIColor (bColor.R, bColor.G, bColor.B, bColor.A);
				view.TabBarItem.SetTitleTextAttributes (
					new UITextAttributes {
						TextColor = color
					},
					UIControlState.Normal);
			}
			if (selectedColor != null) {
				var bColor = selectedColor.Value;
				var color  = new UIColor (bColor.R, bColor.G, bColor.B, bColor.A);
				view.TabBarItem.SetTitleTextAttributes (
					new UITextAttributes {
						TextColor = color
					},
					UIControlState.Selected);
			}

			var views = new UIViewController[index + 1];
			if (this.ViewControllers != null) {
				this.ViewControllers.CopyTo (views, 0);
			}
			views [index] = view;
			this.ViewControllers = views;
			this.tagList.Add (tag, index);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Title = CXStorageManager.GetInstance<CXStorageManager>().AppTitle;
		}
	}
}



