using System;
using System.Text.RegularExpressions;

using SQLite;
using System.IO;
using Code4XMap.MBTiles;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Code4XMap.MBTiles
{
	public class tiles
	{
		public int zoom_level   { get; set; }

		public int tile_column  { get; set; }

		public int tile_row     { get; set; }

		public byte[] tile_data { get; set; }
	}

	public class shared_tiles : tiles
	{
		public string tile_url  { get; set; }

		public long   epoch     { get; set; }
	}

	public class metadata
	{
		public string name  { get; set; }

		public string value { get; set; }
	}

	public class images
	{
		public byte[] tile_data { get; set; }

		public string tile_id   { get; set; }
	}

	public class map
	{
		public int zoom_level  { get; set; }

		public int tile_column { get; set; }

		public int tile_row    { get; set; }

		public string tile_id  { get; set; }

		public string grid_id  { get; set; }
	}
}

namespace Code4XMap
{
	public struct XYZOOM
	{
		public int x;
		public int y;
		public int zoom;
	}

	public class CXWebTilesUrlConstructor
	{
		public const long EPOCH = 621355968000000000;

		protected string   tileUrl = null;
		public    string   TileUrl
		{
			get { 
				return tileUrl;
			}
		}
		protected string[] subServers = null;
		protected bool     is_tms;
		public int      min_zoom;
		public int      max_zoom;
		private   string   _mbTilesPath = null;
		private   string   _sharedDBPath = null;
		private   object   lockObj = new object();
		public    string   MBTilesPath
		{
			get { 
				return _mbTilesPath;
			}

			set { 
				try {
					_mbTilesPath = value;

					if (!File.Exists(_mbTilesPath)) {
						var dir = Path.GetDirectoryName (_mbTilesPath);
						if (!Directory.Exists (dir)) 
						{
							Directory.CreateDirectory (dir);
						}
						File.Create (_mbTilesPath).Close();

						using (var conn = new SQLiteConnection(_mbTilesPath)) {
							conn.Execute ("CREATE TABLE map ( zoom_level INTEGER, tile_column INTEGER, tile_row INTEGER, tile_id TEXT, grid_id TEXT );");
							conn.Execute ("CREATE TABLE grid_key ( grid_id TEXT, key_name TEXT );");
							conn.Execute ("CREATE TABLE keymap ( key_name TEXT, key_json TEXT );");
							conn.Execute ("CREATE TABLE grid_utfgrid ( grid_id TEXT, grid_utfgrid BLOB );");
							conn.Execute ("CREATE TABLE images ( tile_data blob, tile_id text );");
							conn.Execute ("CREATE TABLE metadata ( name text, value text );");
							conn.Execute ("CREATE UNIQUE INDEX map_index ON map (zoom_level, tile_column, tile_row);");
							conn.Execute ("CREATE UNIQUE INDEX grid_key_lookup ON grid_key (grid_id, key_name);");
							conn.Execute ("CREATE UNIQUE INDEX keymap_lookup ON keymap (key_name);");
							conn.Execute ("CREATE UNIQUE INDEX grid_utfgrid_lookup ON grid_utfgrid (grid_id);");
							conn.Execute ("CREATE UNIQUE INDEX images_id ON images (tile_id);");
							conn.Execute ("CREATE UNIQUE INDEX name ON metadata (name);");
							conn.Execute ("CREATE VIEW tiles AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, images.tile_data AS tile_data FROM map JOIN images ON images.tile_id = map.tile_id;");
							conn.Execute ("CREATE VIEW grids AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, grid_utfgrid.grid_utfgrid AS grid FROM map JOIN grid_utfgrid ON grid_utfgrid.grid_id = map.grid_id;");
							conn.Execute ("CREATE VIEW grid_data AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, keymap.key_name AS key_name, keymap.key_json AS key_json FROM map JOIN grid_key ON map.grid_id = grid_key.grid_id JOIN keymap ON grid_key.key_name = keymap.key_name;");
						}
					}
				} catch (Exception) {
					_mbTilesPath = null;
				}
			}
		}
		public bool UseSharedDB
		{
			get { 
				return _sharedDBPath == null ? false : true;
			}

			set { 
				try {
					_sharedDBPath = value ? CXStorageManager.GetInstance<CXStorageManager>().GetSharedTileDBPath() : null;

					if (!File.Exists(_sharedDBPath)) {
						var dir = Path.GetDirectoryName (_sharedDBPath);
						if (!Directory.Exists (dir)) 
						{
							Directory.CreateDirectory (dir);
						}
						File.Create (_sharedDBPath).Close();

						using (var conn = new SQLiteConnection(_sharedDBPath)) {
							conn.Execute ("CREATE TABLE shared_tiles ( zoom_level INTEGER, tile_column INTEGER, tile_row INTEGER, tile_data blob, tile_url TEXT, epoch INTEGER);");
							conn.Execute ("CREATE UNIQUE INDEX tiles_index ON shared_tiles (tile_url, zoom_level, tile_column, tile_row);");
							conn.Execute ("CREATE INDEX epoch_index ON shared_tiles (epoch);");
						}
					}
				} catch (Exception) {
					_sharedDBPath = null;
				}
			}
		}
		private List<XYZOOM> cacheTileQueIndex = new List<XYZOOM> ();
		private Dictionary<XYZOOM, byte[]> cacheTileQueBag = new Dictionary<XYZOOM,byte[]> ();
		private bool cacheTileWorking = false;
		public bool UseOTMFetch = true;

		public CXWebTilesUrlConstructor (string urlBase, bool is_tms, int min_zoom, int max_zoom)
		{
			this.is_tms = is_tms;
			this.min_zoom = min_zoom;
			this.max_zoom = max_zoom;

			tileUrl = urlBase.Replace("{x}", "{0}");
			tileUrl = tileUrl.Replace("{y}", "{1}");
			tileUrl = tileUrl.Replace("{z}", "{2}");

			var regex   = new Regex (@"\{(?<servers>(?:\w+,)+\w+)\}");
			var match = regex.Match(tileUrl);

			if (match.Success) 
			{
				var target = match.Captures[0].Value;
				tileUrl    = tileUrl.Replace (target, "{4}");
				subServers = match.Groups["servers"].Value.Split(',');
			}
		}

		public virtual string GetTileUrl (int x, int y, int z)
		{
			if (tileUrl == "dummy") return null;

			if (min_zoom > z || max_zoom < z) return null;

			if (this.is_tms)
				y = (int)Math.Pow (2.0, (double)z) - y - 1;
			string sURL;

			if (subServers != null) {
				var rndIdx = new Random ().Next (subServers.Length);
				sURL = String.Format (tileUrl, x, y, z, subServers [rndIdx]);
			} else {
				sURL = String.Format (tileUrl, x, y, z);
			}

			return sURL;
		}

		//電子国土対策
		public virtual bool ShouldChangeJpeg(int zoom) {
			return false;
		}

		public virtual byte[] GetCachedTile (int x, int y, int z)
		{
			if (_mbTilesPath == null && _sharedDBPath == null) return null;

			byte[] image = null;

			if (this.is_tms)
				y = (int)Math.Pow (2.0, (double)z) - y - 1;

			if (_mbTilesPath != null) {
				using (var conn = new SQLiteConnection(_mbTilesPath)) {
					try {
						var query = conn.Table<tiles> ().Where (v => v.zoom_level == z && v.tile_column == x && v.tile_row == y);
							
						if (query.Count () != 0) {
							var tile = query.First ();
							image = tile.tile_data;
						}
					} catch (Exception) {
						image = null;
					}
				}
			} else {
				using (var conn = new SQLiteConnection(_sharedDBPath)) {
					try {
						var query = conn.Table<shared_tiles> ().Where (v => v.tile_url == this.TileUrl && v.zoom_level == z && v.tile_column == x && v.tile_row == y);

						if (query.Count () != 0) {
							var tile = query.First ();
							image = tile.tile_data;
						}
					} catch (Exception) {
						image = null;
					}
				}
			}

			return image;
		}

		public virtual void SetCachedTile (int x, int y, int z, byte[] image)
		{
			if ((_mbTilesPath == null && _sharedDBPath == null) || image == null) return;

			if (this.is_tms)
				y = (int)Math.Pow (2.0, (double)z) - y - 1;

			var xyz = new XYZOOM { 
				x = x,
				y = y,
				zoom = z
			};

			lock (lockObj) {
				if (!cacheTileQueIndex.Contains (xyz)) {
					cacheTileQueIndex.Add (xyz);
					cacheTileQueBag.Add (xyz, image);
				}
				if (cacheTileWorking) {
					return;
				} else {
					cacheTileWorking = true;
				}
			}

			var worker = new BackgroundWorker();

			worker.DoWork += (object sender, DoWorkEventArgs e) => {
				while (true) {
					XYZOOM lxyz;
					byte[] limg;
					lock (lockObj) {
						if (cacheTileQueIndex.Count == 0) break;
						lxyz = cacheTileQueIndex[0];
						cacheTileQueIndex.RemoveAt(0);
						limg = cacheTileQueBag[lxyz];
						cacheTileQueBag.Remove(lxyz);
					} 

					if (_mbTilesPath != null) {
						var md5   = new MD5CryptoServiceProvider();
						var bs    = md5.ComputeHash(limg);
						md5.Clear();

						var result = new StringBuilder();
						foreach (var b in bs) 
						{
							result.Append(b.ToString("x2"));
						}
						var sres = result.ToString();

						using (var conn = new SQLiteConnection(_mbTilesPath)) {
							var query = conn.Table<images>().Where (v => v.tile_id == sres);

							if (query.Count() == 0) {
								conn.Insert(new images() {
									tile_id   = sres,
									tile_data = limg
								});
							}

							conn.Insert(new map() {
								tile_column = lxyz.x,
								tile_row    = lxyz.y,
								zoom_level  = lxyz.zoom,
								tile_id     = sres
							});
						}
					} else {
						using (var conn = new SQLiteConnection(_sharedDBPath)) {
							conn.Insert(new shared_tiles() {
								tile_url    = TileUrl,
								tile_column = lxyz.x,
								tile_row    = lxyz.y,
								zoom_level  = lxyz.zoom,
								tile_data   = limg,
								epoch       = (DateTime.Now.ToUniversalTime().Ticks - EPOCH) / 10000
							});
						}
					}
				}

				lock(lockObj) {
					cacheTileWorking = false;
				}

				worker = null;
			};
			worker.RunWorkerAsync ();
		}
	}
}

