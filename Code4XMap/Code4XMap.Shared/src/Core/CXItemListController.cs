using System;

namespace Code4XMap
{
	public class CXItemListController : CXListController
	{
		public CXItemListController (CXItemListFragment _View) : base(_View)
		{

		}

		protected override CXModel GetModel()
		{
			return CXModel.GetInstance<CXModel>();
		}

		protected override void ModelInitializer()
		{
			base.ModelInitializer ();
			var model = (CXModel)GetModel ();

			for (var i = itemGroups.Count -1 ; i >= 0; i--)
			{
				var grp = itemGroups [i];
				if (!model.IsSettingIdSelected (grp.itemGroup.id + 10000)) 
				{
					itemGroups.Remove (grp);
				}
			}
		}

		protected override void _TextChange (string newText)
		{
			base._TextChange (newText);
			var model = (CXModel)GetModel ();

			for (var i = itemGroups.Count -1 ; i >= 0; i--)
			{
				var grp = itemGroups [i];
				if (!model.IsSettingIdSelected (grp.itemGroup.id + 10000)) 
				{
					itemGroups.Remove (grp);
				}
			}
		}
	}
}

