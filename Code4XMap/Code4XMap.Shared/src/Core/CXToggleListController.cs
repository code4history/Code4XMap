using System;
using System.Collections.Generic;
using PerpetualEngine.Storage;
using Newtonsoft.Json;

namespace Code4XMap
{
	public class CXToggleListController : CXListController
	{
		public CXToggleListController (CXToggleListFragment _View) : base(_View)
		{
		}

		protected override CXModel GetModel()
		{
			return CXModel.GetInstance<CXModel>();
		}

		protected override void ModelInitializer ()
		{
		}

		public override CXModel.Item Item(int section, int row)
		{
			return ((CXModel)GetModel ()).SettingLists (section) [row];
		}

		public override CXModel.ItemGroup ItemGroup(int section)
		{
			return ((CXModel)GetModel ()).SettingCategoryLists () [section];
		}

		public override int ItemCount(int section)
		{
			return ((CXModel)GetModel ()).SettingLists (section).Length;
		}

		public override int CategoryCount()
		{
			return ((CXModel)GetModel ()).SettingCategoryLists ().Length;
		}

		public override void SetIdSelected(long id)
		{
			((CXModel)GetModel ()).SetSettingIdSelected (id);
		}

		public override bool IsIdSelected(long id)
		{
			return ((CXModel)GetModel ()).IsSettingIdSelected (id);
		}

		public override void SelectCategoryRow (int Category, int Row)
		{
			var setting = this.Item (Category, Row);
			if (setting != null) {
				this.SetIdSelected (setting.id);
				ListView.WorkOnSelectItem (setting);
			}
		}

		public override void TextChange(string newText)
		{
		}
	}
}

