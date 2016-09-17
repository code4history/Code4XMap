using System;
using UIKit;

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

		public override void WorkOnSelectItem(CXModel.Item item)
		{
		}

		public override UIKit.UITableViewCell GetCell (UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("Setting");
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, "Setting");

			var item = (CXModel.Setting)lController.Item (indexPath.Section, indexPath.Row);
			cell.TextLabel.Text       = item.MainListText();
			//cell.DetailTextLabel.Text = item.SubListText();
			if (item.icon != null) {
				cell.ImageView.Image = UIImage.FromBundle (item.icon);
			} else {
				cell.ImageView.Image = null;
			}

			if (lController.IsIdSelected (item.id)) {
				cell.Accessory = UITableViewCellAccessory.Checkmark;
			} else {
				cell.Accessory = UITableViewCellAccessory.None;
			}

			return cell;
		}

		public override void RowSelected (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			base.RowSelected (tableView, indexPath);

			this.Update ();
		}
	}
}

