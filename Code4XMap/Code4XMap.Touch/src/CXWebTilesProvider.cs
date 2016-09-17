using System;
using Google.Maps;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Code4XMap
{
    public class CXWebTilesProvider : TileLayer
	{
		void HandleTileUrlConstructorHandler(nuint x, nuint y, nuint zoom)
		{

		}

		protected UrlTileLayer intern;
		protected CXWebTilesUrlConstructor constructor;
		public string BaseUrl {
			get { 
				return constructor.TileUrl;
			}
		}

		public CXWebTilesProvider (CXWebTilesUrlConstructor constructor)
		{
			intern = UrlTileLayer.FromUrlConstructor((nuint x, nuint y, nuint zoom) => {
				var url = constructor.GetTileUrl((int)x,(int)y,(int)zoom);
				return url == null ? null : new NSUrl(url); 
			});
			this.constructor = constructor;
		}

		public override void RequestTile (nuint x, nuint y, nuint zoom, ITileReceiver receiver)
		{
			//ズームがタイルの最大ズーム以上の場合
			if ((int)zoom > constructor.max_zoom) {
				var deltaZ = (int)zoom - constructor.max_zoom;
				//タイル再材ズームでのx,y算出
				var tileX = (uint)((int)x / Math.Pow(2,deltaZ));
				var tileY = (uint)((int)y / Math.Pow(2,deltaZ));

				//オリジナルサイズの情報も渡して独自コールバックオブジェクト生成
				var wrappedOrig = new WebTilesReceiver (receiver, constructor, (uint)x, (uint)y, (uint)zoom);

				//自分自身に、独自コールバックオブジェクトを与えてタイル要求(自分自身を使う理由は)
				this.RequestTile (tileX, tileY, (uint)constructor.max_zoom, wrappedOrig);
				return;
			}

			var image = constructor.GetCachedTile ((int)x, (int)y, (int)zoom);
			if (image != null) {
				var uiimg = new UIImage(NSData.FromArray(image));
				if (uiimg == null || uiimg.CGImage == null)
					Console.WriteLine ("Something wrong");
				receiver.ReceiveTile(x, y, zoom, uiimg);
				receiver = null;
				return;
			}

			var wrapped = new WebTilesReceiver (receiver, constructor, constructor.ShouldChangeJpeg((int)zoom));

			intern.RequestTile (x, y, zoom, wrapped);
		}
	}

	public class WebTilesReceiver : NSObject, ITileReceiver
	{
		public ITileReceiver origin;
		private CXWebTilesUrlConstructor constructor;
		//電子国土対策
		private bool shouldChangeJpeg;
		//オリジナルサイズ
		private uint originX = 0;
		private uint originY = 0;
		private uint originZoom = 0;

		public WebTilesReceiver (ITileReceiver origin, CXWebTilesUrlConstructor constructor, bool shouldChangeJpeg = false)
		{
			this.origin      = origin;
			this.constructor = constructor;
			this.shouldChangeJpeg = shouldChangeJpeg;
		}

		//オリジナルサイズ付きコンストラクタ
		public WebTilesReceiver (ITileReceiver origin, CXWebTilesUrlConstructor constructor, 
			uint originX, uint originY, uint originZoom, bool shouldChangeJpeg = false) : this(origin, constructor, shouldChangeJpeg)
		{
			this.originX = originX;
			this.originY = originY;
			this.originZoom = originZoom;
		}

		public void ReceiveTile (nuint x, nuint y, nuint zoom, UIImage image)
		{
			if (image == null || image.CGImage == null) {
				origin.ReceiveTile (
					this.originZoom != 0 ? this.originX    : x, 
					this.originZoom != 0 ? this.originY    : y, 
					this.originZoom != 0 ? this.originZoom : zoom, image);
				origin = null;
				return;
			}

			if (this.originZoom != 0) {
				var pow = Math.Pow (2, this.originZoom - (uint)zoom);
				var size = 256.0 / pow;
				var shiftX = (this.originX - (uint)(x * pow)) * size;
				var shiftY = (this.originY - (uint)(y * pow)) * size;

				var rect = new CGRect ((float)shiftX, (float)shiftY, (float)size, (float)size);
				using (CGImage cr = image.CGImage.WithImageInRect (rect)) {
					image = UIImage.FromImage (cr);
				}
				origin.ReceiveTile (this.originX, this.originY, this.originZoom, image);
				origin = null;
				return;	
			}
				
			//ここから、異常画像対応
			if (shouldChangeJpeg) {
				var data = image.AsJPEG (1.0f);
				if (data != null) {
					image = new UIImage (data);
				}
			}
			
			origin.ReceiveTile (x, y, zoom, image);
			origin = null;

			if (constructor.MBTilesPath != null || constructor.UseSharedDB) {
				var data = shouldChangeJpeg ? image.AsJPEG() : image.AsPNG ();
				if ( data != null ) {
					var bytes = new byte[data.Length];

					System.Runtime.InteropServices.Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
					constructor.SetCachedTile ((int)x, (int)y, (int)zoom, bytes);
				}
			}
		}
	}
}

