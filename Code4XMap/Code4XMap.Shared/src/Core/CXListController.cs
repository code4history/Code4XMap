using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Code4XMap
{
	public struct CategoryBag
	{
		public CXModel.ItemGroup itemGroup;
		public CXModel.Item[]    items;
		public int             itemsCount;
	}

	public interface ICXListFragment : IMvvmQSAcceptorView
	{
		void Update();
		void WorkOnSelectItem (CXModel.Item item);
	}

	public class CXListController : MvvmController
	{
		protected IList<CategoryBag> itemGroups = null;
		protected IList<long> SelectedIds = new List<long> ();

		public CXListController (ICXListFragment _View) : base(_View)
		{
			this._TextChange ();
		}

		protected virtual void ModelInitializer()
		{
		}

		protected virtual CXModel GetModel()
		{
			return CXModel.GetInstance<CXModel>();
		}

		protected ICXListFragment ListView {
			get {
				return (ICXListFragment)this.View;
			}

			private set {
				this.View = value;
			}
		}

		public virtual CXModel.Item Item(int section, int row)
		{
			var ig = itemGroups [section];
			return ig.items == null ? GetModel ().ItemAtRow (ig.itemGroup.id, row) : ig.items [row];
		}

		public virtual CXModel.ItemGroup ItemGroup(int section)
		{
			return itemGroups [section].itemGroup;
		}

		public virtual int ItemCount(int section)
		{
			return itemGroups [section].itemsCount;
		}

		public virtual int CategoryCount()
		{
			return itemGroups.Count;
		}

		public virtual void SetIdSelected(long id)
		{
			this.SelectedIds = new List<long> () {id};
		}

		public virtual bool IsIdSelected(long id)
		{
			if (this.SelectedIds.Count == 0)
				return false;
			return this.SelectedIds [0] == id;
		}

		public virtual void SelectCategoryRow (int section, int Row)
		{
			var item = this.Item (section, Row);
			if (item != null) {
				this.SetIdSelected (item.id);
				ListView.WorkOnSelectItem (item);
			}
		}

		public virtual void TextChange(string newText)
		{
			this._TextChange (newText);

			ListView.Update();
		}

		protected virtual void _TextChange(string newText = "")
		{
			var list = new List<CategoryBag> ();

			var cats = GetModel().ItemGroupLists();
			foreach (var cat in cats) 
			{
				if (newText == null || newText == "") {
					var poiNum = GetModel ().ItemListCount (cat.id);
					//ItemLists(cat.id, newText);
					if (poiNum != 0) {
						list.Add (
							new CategoryBag {
								itemGroup   = cat,
								items       = null,
								itemsCount  = poiNum
							}
						);
					}
				} else {
					var pois = GetModel ().ItemLists(cat.id, newText);
					if (pois.Length != 0) {
						list.Add (
							new CategoryBag {
								itemGroup  = cat,
								items      = pois,
								itemsCount = pois.Length
							}
						);
					}
				}
			}
			this.itemGroups = list;
		}
	}
}


