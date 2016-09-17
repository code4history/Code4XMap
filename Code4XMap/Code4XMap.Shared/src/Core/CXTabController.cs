using System;
using System.Drawing;

namespace Code4XMap
{
	public interface ICXTabFragment : IMvvmQSAcceptorView
	{
		void AddTabTextColor (Color textColor, Color? selectedColor=null);

		void AddTab (Type viewClass, string name, string tag);
	}

	public class CXTabController : MvvmQSAcceptorController
	{
		public CXTabController(ICXTabFragment _View) : base(_View) { }

		public void SetTabList () 
		{ 
			var tabView = (CXTabFragment)this.View;
			tabView.AddTabTextColor (Color.Black, Color.White);

			tabView.AddTab (typeof(CXMapFragment),  "地図", "map");
			tabView.AddTab (typeof(CXItemListFragment), "一覧", "list");
			tabView.AddTab (typeof(CXToggleListFragment), "表示", "display");
		}

		public void OnInfoSelected()
		{
			var infoUrl = new Uri (new Uri (CXStorageManager.GetInstance<CXStorageManager> ().AppUrl), "./hanrei.html").AbsoluteUri;
			CXMemoryManager.OpenNextUrl ((CXTabFragment)this.View, infoUrl);
		}
	}
}

