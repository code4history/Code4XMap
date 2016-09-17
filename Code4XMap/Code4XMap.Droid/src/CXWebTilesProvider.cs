using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using System.IO;

namespace Code4XMap
{
	public class CXWebTilesProviderInternal : UrlTileProvider
	{
		public static int TILE_WIDTH  = 256;
		public static int TILE_HEIGHT = 256;

		protected CXWebTilesUrlConstructor constructor;
		public string BaseUrl {
			get { 
				return constructor.TileUrl;
			}
		}

		public CXWebTilesProviderInternal (CXWebTilesUrlConstructor constructor) : base(TILE_WIDTH,TILE_HEIGHT)
		{
			this.constructor = constructor;
		}

		public override Java.Net.URL GetTileUrl (int x, int y, int z)
		{
			var url = constructor.GetTileUrl (x, y, z);
			return url ==null ? null : new Java.Net.URL (url);
		}
	}

	public class CXWebTilesProvider : Java.Lang.Object, ITileProvider
	{
		protected CXWebTilesProviderInternal intern;
		protected CXWebTilesUrlConstructor constructor;
		public string BaseUrl {
			get { 
				return intern.BaseUrl;
			}
		}

		public CXWebTilesProvider (CXWebTilesUrlConstructor constructor) : base()
		{
			intern = new CXWebTilesProviderInternal (constructor);
			this.constructor = constructor;
		}

		public Tile GetTile (int x, int y, int zoom)
		{
			//ズームがタイルの最大ズーム以上の場合
			if (zoom > constructor.max_zoom) {
				var pow = Math.Pow (2, zoom - constructor.max_zoom);
				//タイル再材ズームでのx,y算出
				var tileX = (int)(x / pow);
				var tileY = (int)(y / pow);

				var dzTile = this.GetTile (tileX, tileY, constructor.max_zoom);

				var dzImage = new byte[dzTile.Data.Count];
				dzTile.Data.CopyTo (dzImage, 0);
				var dzBitmap = BitmapFactory.DecodeByteArray (dzImage, 0, dzImage.Length);

				var size = 256.0 / pow;
				var shiftX = (x - (uint)(tileX * pow)) * size;
				var shiftY = (y - (uint)(tileY * pow)) * size;
				var dzdBitmap = Bitmap.CreateBitmap (dzBitmap, (int)shiftX, (int)shiftY, (int)size, (int)size);

				byte[] dzdImage;
				using (var stream = new MemoryStream())
				{
					dzdBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
					dzdImage = stream.ToArray();
				}


				return new Tile (CXWebTilesProviderInternal.TILE_WIDTH, CXWebTilesProviderInternal.TILE_HEIGHT, dzdImage);
			}

			var image = constructor.GetCachedTile (x, y, zoom);
			if (image != null) {
				return new Tile (CXWebTilesProviderInternal.TILE_WIDTH, CXWebTilesProviderInternal.TILE_HEIGHT,
				                image);
			}

			var tile = intern.GetTile (x, y, zoom);

			if ((constructor.MBTilesPath != null || constructor.UseSharedDB) && tile != null && tile != TileProvider.NoTile && tile.Data != null) {
				image = new byte[tile.Data.Count];
				tile.Data.CopyTo (image, 0);
				constructor.SetCachedTile (x, y, zoom, image);
			}

			return tile;
		}
	}
}

