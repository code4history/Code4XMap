using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using System.Drawing;
using Android.Util;

namespace Code4XMap
{
	public class CXMobileUtility
	{
		private static IWindowManager wm = null;
		private static Display display = null;
		private static DisplayMetrics metrics = null;

		public static void SetWindowManager(Activity activity) 
		{
			wm = activity.WindowManager;
			display = wm.DefaultDisplay;
			metrics = new DisplayMetrics();
			display.GetMetrics(metrics);
		}

		public static bool IsPortrait() {
			var config = Application.Context.Resources.Configuration;
			return config.Orientation == Android.Content.Res.Orientation.Portrait;
		}

		public static System.Drawing.SizeF DisplaySize(){
			var size = new Android.Graphics.Point ();
			display.GetSize (size);
			var retSize = new System.Drawing.SizeF((float)size.X, (float)size.Y);

			return retSize;
		}

		public static float DisplayDensity()
		{
			return metrics.Density;
		}

		public static bool IsPhone()
		{
			// ピクセル数（width, height）を取得する
			var widthPx = metrics.WidthPixels;
			var heightPx = metrics.HeightPixels;

			// dpi (xdpi, ydpi) を取得する
			var xdpi = metrics.Xdpi;
			var ydpi = metrics.Ydpi;

			// インチ（width, height) を計算する
			var widthIn  = (float)widthPx  / xdpi;
			var heightIn = (float)heightPx / ydpi;

			// 画面サイズ（インチ）を計算する
			var inch = Math.Sqrt((double)(widthIn * widthIn + heightIn * heightIn));

			return 5.2 >= inch;
		}
	}
}

