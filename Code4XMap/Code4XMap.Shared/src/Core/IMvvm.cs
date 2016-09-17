using System;
using System.Collections.Specialized;

namespace Code4XMap
{
	public interface IMvvmView
	{
	}

	public interface IMvvmQSAcceptorView : IMvvmView, IQueryStringAcceptor
	{
	}

	public class MvvmController
	{
		protected WeakReference _wView;
		protected IMvvmView View {
			set {
				if (value == null) {
					_wView = null;
				} else {
					_wView = new WeakReference(value);
				}
			}

			get {
				if (_wView == null) {
					return null;
				} else {
					return (IMvvmView)_wView.Target;
				}
			}
		}

		public MvvmController(IMvvmView argView)
		{
			this.View = argView;
		}
	}

	public class MvvmQSAcceptorController : MvvmController
	{
		public MvvmQSAcceptorController(IMvvmQSAcceptorView argView) : base(argView)
		{

		}

		public virtual void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{

		}
	}
}

