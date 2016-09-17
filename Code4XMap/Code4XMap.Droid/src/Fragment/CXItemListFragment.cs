using System;

namespace Code4XMap
{
	public class CXItemListFragment : CXListFragment
	{
		protected override CXListController Controller ()
		{
			return new CXItemListController (this);
		}
	}
}

