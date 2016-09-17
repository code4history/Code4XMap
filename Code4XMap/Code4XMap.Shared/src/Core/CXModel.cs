using System;
using SQLite;

using System.Collections.Generic;
using PerpetualEngine.Storage;
using Newtonsoft.Json;

namespace Code4XMap
{
	public class CXModel : CXAbstractSingleton
	{
		protected const int POIGROUP_CATEGORY_ID = 1;
		protected const int MAP_CATEGORY_ID      = 0;
		protected SettingCategory[] settingCategories = null;
		protected Setting[][] settings = null;
		protected List<long> SelectedIds = null;

		public abstract class Item
		{
			[PrimaryKey]
			public int      id        { get; set; }
			[Indexed]
			public int      category  { get; set; }
			public string   name      { get; set; }

			public virtual string MainListText()
			{
				return this.name;
			}

			public virtual string SubListText()
			{
				return this.name;
			}

			public static string TableName ()
			{
				return "";
			}
		}

		public class Poi : Item
		{
			public string   zip       { get; set; }
			public string   address1  { get; set; }
			public string   address2  { get; set; }
			public string   tel       { get; set; }
			public double   latitude  { get; set; }
			public double   longitude { get; set; }
			public DateTime modified  { get; set; }
			public string   editor    { get; set; }
			public string   direction { get; set; }
			public string   website   { get; set; }

			public override string SubListText()
			{
				return this.address1 + this.address2;
			}

			public new static string TableName ()
			{
				return "poi";
			}
		}

		public class Map : Item
		{
			public bool     usermade    { get; set; }
			public string   url         { get; set; }
			public string   description { get; set; }
			public string   attribution { get; set; }
			public bool     is_tms      { get; set; }
			public double   min_lat     { get; set; }
			public double   min_lng     { get; set; }
			public double   max_lat     { get; set; }
			public double   max_lng     { get; set; }
			public int      min_year    { get; set; }
			public int      max_year    { get; set; }
			public int      min_zoom    { get; set; }
			public int      max_zoom    { get; set; }
			public float    zoom_index  { get; set; }
			public string   xml_url     { get; set; }
			public string   storable    { get; set; }
			public string   icon        { get; set; }

			public override string SubListText()
			{
				return this.attribution;
			}
		}

		public abstract class ItemGroup
		{
			[PrimaryKey]
			public int      id        { get; set; }
			public string   url       { get; set; }
			public string   name      { get; set; }

			public virtual string ListText()
			{
				return this.name;
			}

			public static string TableName ()
			{
				return "";
			}
		}

		public class Category : ItemGroup
		{
			public string   icon      { get; set; }

			public new static string TableName ()
			{
				return "category";
			}
		}

		public class Mapcategory : ItemGroup
		{
			public bool     usermade    { get; set; }
			public string   icon        { get; set; }
			[Indexed]
			public int      sequence    { get; set; }
		}

		public class SettingCategory : ItemGroup
		{

		}

		public class Setting : Item
		{
			public string   icon      { get; set; }
		}

		protected string dbPath;
		protected SQLiteConnection conn;

		protected CXModel () : base()
		{
			dbPath = CXStorageManager.GetInstance<CXStorageManager>().GetPathFromUrl (this.DbUrl());
			Console.WriteLine ("at-creation TileMapJP DB" + this.dbPath);
			conn = new SQLiteConnection (this.dbPath);
		}

		~CXModel () 
		{
			Console.WriteLine ("TileMapJP DB Closed");
			conn.Close();
			conn.Dispose ();
		}

		protected virtual string DbUrl()
		{
			return CXStorageManager.GetInstance<CXStorageManager> ().ModelUrl;
		}

		public virtual ItemGroup[] ItemGroupLists()
		{
			return this.ItemGroupLists<Category> ();
		}

		public virtual T[] ItemGroupLists<T>() where T : ItemGroup, new()
		{
			var list = new List<T> (conn.Table<T> ().OrderBy (v => v.id));
			return list.ToArray ();
		}

		public virtual Item[] ItemLists(int category, string qs = "")
		{
			return this.ItemLists<Poi> (category,qs);
		}

		public virtual T[] ItemLists<T>(int category, string qs = "") where T : Item, new()
		{
			return new List<T> (this.ItemListQuery<T> (category,qs)).ToArray();
		}

		public virtual TableQuery<T> ItemListQuery<T>(int category, string qs = "") where T : Item, new()
		{
			if (typeof(T) == typeof(Poi)) {
				var q = conn.Table<Poi> ();
				if (category != 0) {
					q = q.Where (v => v.category == category);
				}
				if (qs != "") {
					q = q.Where (v => v.name.Contains (qs) || v.address1.Contains (qs) || v.address2.Contains (qs));
				}
				if (category != 0) {
					q = q.OrderBy (v => v.id);
				}
				return (TableQuery<T>)(object)q;
			} else {
				var q = conn.Table<Map> ();
				if (category != 0) {
					q = q.Where (v => v.category == category);
				}
				if (qs != "") {
					q = q.Where (v => v.name.Contains (qs));
				}
				if (category != 0) {
					q = q.OrderBy (v => v.min_year);
				}
				return (TableQuery<T>)(object)q;
			}
		}

		public virtual int ItemListCount<T>(int category, string qs = "") where T : Item, new()
		{
			return this.ItemListQuery<T> (category, qs).Count ();
		}

		public virtual int ItemListCount(int category, string qs = "")
		{
			return this.ItemListCount<Poi> (category, qs);
		}

		public virtual T ItemAtRow<T>(int category, int row, string qs = "") where T : Item, new()
		{
			return this.ItemListQuery<T> (category, qs).Skip (row).First ();
		}

		public virtual Item ItemAtRow(int category, int row, string qs = "")
		{
			return this.ItemAtRow<Poi> (category, row, qs);
		}

		public virtual T FindItemById<T>(int cid) where T : Item, new()
		{
			return conn.Table<T> ().Where (v => v.id == cid).First ();
		}

		public virtual Item FindItemById(int cid)
		{
			return this.FindItemById<Poi> (cid);
		}

		public Category[] CategoryLists()
		{
			return (Category[])this.ItemGroupLists<Category> ();
		}

		public Mapcategory[] MapCategoryLists()
		{
			return (Mapcategory[])this.ItemGroupLists<Mapcategory> ();
		}

		public SettingCategory[] SettingCategoryLists()
		{
			if (settingCategories == null) {
				var cat0 = new CXModel.SettingCategory { 
					id   = MAP_CATEGORY_ID,
					name = "地図表示切り替え",
					url  = CXStorageManager.GetInstance<CXStorageManager>().AppUrl + "#mapchange"
				};
				var cat1 = new CXModel.SettingCategory { 
					id   = POIGROUP_CATEGORY_ID,
					name = "ポイント表示切り替え",
					url  = CXStorageManager.GetInstance<CXStorageManager>().AppUrl + "#poigroupchange"
				};
				settingCategories = new SettingCategory[2];
				settingCategories [MAP_CATEGORY_ID]      = cat0;
				settingCategories [POIGROUP_CATEGORY_ID] = cat1;
			}

			return settingCategories;
		}

		public Poi[] PoiLists(int category, string qs = "")
		{
			return this.ItemLists<Poi>(category,qs);
		}

		public Map[] MapLists(int category)
		{
			//return this.ItemLists<Map> (category);
			List<Map> list;
			list = conn.Query<Map>("select * from map where category = ? order by id", category);

			return list.ToArray();
		}

		public Setting[] SettingLists(int category)
		{
			if (settings == null) {
				var mapnum = this.MapLists (1);
				var maps   = new Setting[mapnum.Length];

				for (var i = 0; i < mapnum.Length; i++) 
				{
					maps [i] = new CXModel.Setting { 
						id       = mapnum[i].id,
						name     = mapnum[i].name,
						category = MAP_CATEGORY_ID,
						icon     = null
					};
				}

				var poicats = this.CategoryLists ();
				var pois    = new Setting[poicats.Length];

				for (var i = 0; i < poicats.Length; i++) 
				{
					pois [i] = new CXModel.Setting { 
						id       = 10000 + poicats[i].id,
						name     = poicats[i].name,
						category = POIGROUP_CATEGORY_ID,
						icon     = poicats[i].icon
					};
				}

				settings = new Setting[this.SettingCategoryLists().Length][];
				settings [MAP_CATEGORY_ID]      = maps;
				settings [POIGROUP_CATEGORY_ID] = pois;
			}

			return settings [category];
		}

		public Poi FindPoiById(int id)
		{
			return (Poi)this.FindItemById<Poi> (id);
		}

		public Map FindMapById(int id)
		{
			return (Map)this.FindItemById<Map> (id);
		}

		public void SetSettingIdSelected(long id)
		{
			this._selectedIds ();

			if (id < 10000) {
				var exists = false;
				for (var i = SelectedIds.Count - 1; i >= 0; i--) {
					if (SelectedIds [i] < 10000) {
						if (SelectedIds [i] == id)
							exists = true;
						SelectedIds.Remove (SelectedIds [i]);
					}
				}
				if (!exists) 
					SelectedIds.Add (id);
			} else {
				if (SelectedIds.Contains (id)) {
					SelectedIds.Remove (id);
				} else {
					SelectedIds.Add (id);
				}
			}
			var storage = SimpleStorage.EditGroup("MapShowingSetting");
			storage.Put ("SelectedIds", JsonConvert.SerializeObject((List<long>)SelectedIds));
		}

		public bool IsSettingIdSelected(long id)
		{
			return this._selectedIds().Contains (id);
		}

		private List<long> _selectedIds() 
		{
			if (SelectedIds == null) {
				var storage = SimpleStorage.EditGroup ("MapShowingSetting");
				try {
					if (storage.HasKey ("SelectedIds")) {
						this.SelectedIds = JsonConvert.DeserializeObject<List<long>> (storage.Get ("SelectedIds"));
					} else {
						throw new Exception ("Not Contains Key");
					}
				} catch (Exception) {
					this.SelectedIds = new List<long> ();
					foreach (var item in this.SettingLists(POIGROUP_CATEGORY_ID)) {
						this.SelectedIds.Add (item.id);
					}
					if (this.SettingLists (MAP_CATEGORY_ID).Length > 0) {
						var mapItem = this.SettingLists (MAP_CATEGORY_ID) [0];
						this.SelectedIds.Add (mapItem.id);
					}
					storage.Put ("SelectedIds", JsonConvert.SerializeObject ((List<long>)SelectedIds));
				}
			}

			return SelectedIds;
		}
	}
}

