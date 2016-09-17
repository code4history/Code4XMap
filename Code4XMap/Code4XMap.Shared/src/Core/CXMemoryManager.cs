using System;
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;
using System.Threading;

namespace Code4XMap
{
	public delegate Type URLAcceptChecker (string host, string fragment);

	public interface IQueryStringAcceptor
	{
		void SetFragmentAndQuery(string fragment, NameValueCollection qs);
	}

	public partial class CXMemoryManager : CXAbstractSingleton
	{
		private static Dictionary<String, Object> memVals = new Dictionary<String, Object>();
		private static SynchronizationContext syncContext = SynchronizationContext.Current;

		public  static List<URLAcceptChecker> URLAcceptCheckers = new List<URLAcceptChecker> ();
		public  static Type DefaultUrlOpener = null;

		public Object this[String index] {
			get {
				if (memVals.ContainsKey (index)) {
					return memVals [index];
				} else {
					return null;
				}
			}

			set {
				memVals [index] = value;
			}
		}

		private static BoundsLimit? _boundsLimit = null;

		public static void SetBoundsLimit(BoundsLimit? limit)
		{
			_boundsLimit = limit;
		}
		public static BoundsLimit? GetBoundsLimit()
		{
			return _boundsLimit;
		}
		public void ClearBoundsLimit()
		{
			_boundsLimit = null;
		}

		protected CXMemoryManager () : base()
		{

		}

		public void InvokeUIThread(Action action)
		{
			syncContext.Post (state => {action();}, null);
		}

		public static void OpenNextUrl(IQueryStringAcceptor parent, string nextUrl)
		{
			var nextUri = new Uri (nextUrl);
			var host = nextUri.Scheme + "://" + nextUri.Host + nextUri.LocalPath;
			var frag = nextUri.Fragment;
			var qs   = nextUri.Query;//HttpUtility.ParseQueryString(nextUri.Query);

			foreach (var checker in URLAcceptCheckers) 
			{
				var type = checker(host, frag);
				if (type != null) { 
					OpenAppUrl (parent, type, host, frag, qs);
					return;
				}
			}

			OpenWebUrl (parent, DefaultUrlOpener, nextUrl);
		}
	}
}

