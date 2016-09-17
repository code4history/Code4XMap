using System;

namespace Code4XMap
{
	public class CXItemListFragment : CXListFragment
	{
		public CXItemListFragment () : base()
		{	
		}

		protected override CXListController Controller ()
		{
			return new CXItemListController (this);
		}
	}
}

