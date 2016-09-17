using System;
using Android.Widget;
using Android.Views;
using Android.App;
using Android.Content.Res;

namespace Code4XMap
{
	public class CXToggleListFragment : CXListFragment
	{
		protected override bool ShowSearchBar {
			get { 
				return false;
			}
		}

		public CXToggleListFragment () : base()
		{

		}

		protected override CXListController Controller()
		{
			return new CXToggleListController (this);
		}

		protected override ChoiceMode GetChoiceMode()
		{
			return ChoiceMode.Multiple;
		}

		protected override CXListAdapter GetAdapter()
		{
			return new CXToggleListAdapter (this.Activity, lController);
		}

		public override void WorkOnSelectItem(CXModel.Item item)
		{
			this.Update ();
		}
	}

	public class CXToggleListAdapter : CXListAdapter
	{
		public CXToggleListAdapter (Android.App.Activity context, CXListController lController) : base(context, lController)
		{
			
		}

		protected override View GetSelectedCellView(SectionRow sr, View view, ViewGroup parent)
		{
			var act = (Activity)context.Target;

			if (sr.section == 0) {
				view = act.LayoutInflater.Inflate(
					Resource.Layout.SimpleSelectList, null);
			} else {
				view = act.LayoutInflater.Inflate(
					Resource.Layout.IconSelectList, null);
			}
				
			var text  = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			var image = view.FindViewById<ImageView> (Resource.Id.image1);

			var item = (CXModel.Setting)lController.Item (sr.section,sr.row);
			text.Text = item.MainListText();
			image.SetImageResource(Resource.Drawable.check);

			if (item.icon != null) {
				var iconimage = view.FindViewById<ImageView> (Resource.Id.iconimage1);
				var resName = System.IO.Path.GetFileNameWithoutExtension (item.icon);
				var resId = act.Resources.GetIdentifier (resName, "drawable", act.PackageName);
				iconimage.SetImageResource (resId);
			}
				
			return view;
		}

		protected override View GetUnselectedCellView(SectionRow sr, View view, ViewGroup parent)
		{
			var act = (Activity)context.Target;

			//if (view == null) { // otherwise create a new one
				if (sr.section == 0) {
					view = act.LayoutInflater.Inflate(
						Resource.Layout.SimpleSelectList, null);
				} else {
					view = act.LayoutInflater.Inflate(
						Resource.Layout.IconSelectList, null);
				}
			//}
			var text  = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			var image = view.FindViewById<ImageView> (Resource.Id.image1);

			var item = (CXModel.Setting)lController.Item (sr.section,sr.row);
			text.Text = item.MainListText();
			image.SetImageResource(Resource.Drawable.check_empty);

			if (item.icon != null) {
				var iconimage = view.FindViewById<ImageView> (Resource.Id.iconimage1);
				var resName = System.IO.Path.GetFileNameWithoutExtension (item.icon);
				var resId = act.Resources.GetIdentifier (resName, "drawable", act.PackageName);
				iconimage.SetImageResource (resId);
			}

			return view;
		}
	}
}

