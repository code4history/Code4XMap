using System;
using Android.App;
using System.Collections.Specialized;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Support.V4.App;
using Android.Content.PM;

namespace Code4XMap
{
	public class KeyEventTimer : CountDownTimer
	{
		public bool pressed = false;

		public KeyEventTimer (long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval)
		{
		}

		public override void OnTick (long millisUntilFinished)
		{

		}

		public override void OnFinish ()
		{
			pressed = false;
		}

	}

	[Activity(Theme = "@style/Theme.main_theme", ScreenOrientation = ScreenOrientation.Portrait)]
	public class CXMainPage : FragmentActivity, IQueryStringAcceptor
	{
		//private bool fragmentReady = false;
		private KeyEventTimer keyEventTimer;

		public CXMainPage () : base()
		{
		}

		public void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
		}

		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.MainContainer);
			CXAppInitializer.OnActivityCreated (this, savedInstanceState);

			if (savedInstanceState == null) {
				var tm      = this.SupportFragmentManager;
				var transaction = tm.BeginTransaction ();

				var fragmentName = Intent.GetStringExtra ("StartFragment");

				Type startType = Type.GetType (fragmentName);
				var fragment = (Android.Support.V4.App.Fragment)Activator.CreateInstance (startType);

				transaction.Add (Resource.Id.fragment_container, fragment, CXStorageManager.GetInstance<CXStorageManager> ().AppUrl + "#tab");
				transaction.Commit ();

				//fragmentReady = true;
			}

			keyEventTimer = new KeyEventTimer (3000, 100);
		}

		protected override void OnRestoreInstanceState (Android.OS.Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
		}

		protected override void OnSaveInstanceState (Android.OS.Bundle outState)
		{
			CXStorageManager.SaveInstance<CXStorageManager> (outState);
			base.OnSaveInstanceState (outState);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
		}

		public override bool DispatchKeyEvent (Android.Views.KeyEvent e)
		{
			if (e.KeyCode == Android.Views.Keycode.Back && e.Action == Android.Views.KeyEventActions.Up) {
				var tm      = this.SupportFragmentManager;
				if (tm.BackStackEntryCount != 0) return base.DispatchKeyEvent (e);

				if (!keyEventTimer.pressed) {
					// Timerを開始
					keyEventTimer.Cancel (); // いらない？
					keyEventTimer.Start ();

					// 終了する場合, もう一度タップするようにメッセージを出力する
					Toast.MakeText (this, "終了する場合は、もう一度バックボタンを押してください", ToastLength.Short).Show ();
					keyEventTimer.pressed = true;
					return false;
				} else {
					keyEventTimer.pressed = false;
					return base.DispatchKeyEvent (e);
				}
			}

			return base.DispatchKeyEvent (e);
		}
	}
}

