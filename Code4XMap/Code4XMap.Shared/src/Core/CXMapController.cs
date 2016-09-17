using System;
using System.Collections.Generic;
using System.Linq;
using PerpetualEngine.Storage;
using System.Threading.Tasks;

namespace Code4XMap
{
	public interface IMarker 
	{
		string LinkingId { get; }
		void   Remove();
		void   Select();
		bool   Selecting { get; set; }
		bool   Showing   { get; set; }
	}
		
	public struct LatLngVal
	{
		public double Latitude;
		public double Longitude;
	}

	public struct BoundsLimit
	{
		public LatLngVal? SouthWest;
		public LatLngVal? NorthEast;
		public float? minZoom;
		public float? maxZoom;
	}

	public class CameraPositionBag {
		public float Zoom;
		public double Latitude;
		public double Longitude;
	}

	public struct MarkerBag {
		public IMarker marker;
		public int     db_id;
	}

	public enum MapMoveType {
		MapMoveSimple,
		MapMoveAnimateIfSmooth,
		MapMoveAnimate
	}

	public interface ICXMapFragment : IMvvmQSAcceptorView
	{
		bool EnableRotation      { get; set; }
		bool EnableCompassButton { get; set; }
		bool EnableTilt          { get; set; }
		bool EnableZoom          { get; set; }
		bool EnableMyLocation    { get; set; }

		void MapMoveTo(CameraPositionBag position, MapMoveType moveType);

		IMarker AddMarker (double Latitude, double Longitude, string title, string snippet, int ExclusiveId, string icon);

		void OpenInfoWeb (string webUrl);
		void ShowMapLayer (CXWebTilesUrlConstructor constructor, bool exclusive);
	}



	public class CXMapController : MvvmController
	{
		protected Dictionary<string, MarkerBag> MarkerList = new Dictionary<string, MarkerBag>();
		protected Dictionary<int, MarkerBag> DbMarkerList  = new Dictionary<int, MarkerBag>();
		protected Dictionary<long,List<MarkerBag>> CategoryMarkerList = new Dictionary<long,List<MarkerBag>>();
		private CameraPositionBag nextUpdateBag = null;
		public bool CameraChanging = false;
		private Object lockObj = new Object ();
		private bool InitialMove = false;

		protected ICXMapFragment MapView {
			get {
				return (ICXMapFragment)this.View;
			}

			private set {
				this.View = value;
			}
		}

		public IMarker GetMarkerByDbId(int dbid)
		{
			var bag = DbMarkerList[dbid];

			return bag.marker;
		}

		public CXMapController (ICXMapFragment _View) : base(_View)
		{
		}

		protected CameraPositionBag ReconstructStartPosition()
		{
			//double Lat, Lng;
			//float Zoom;
			try {
				var storage = SimpleStorage.EditGroup("StartMapPosition");
				if (storage.HasKey ("Lat") && storage.HasKey ("Lng") && storage.HasKey ("Zoom")) {
					return new CameraPositionBag { 
						Latitude = double.Parse(storage.Get("Lat")),
						Longitude = double.Parse(storage.Get("Lng")),
						Zoom = float.Parse(storage.Get("Zoom"))
					};
				} else {
					throw new Exception("Not Contains Key or File is gone");
				}
			} catch (Exception) {
				return new CameraPositionBag { 
					Latitude  =  35.105689984390295,
					Longitude = 135.952840715839,
					Zoom      = 14.0f
				};
			}
		}

		public void InitialMapSettings()
		{
			var start = ReconstructStartPosition ();

			//this.ChangeCameraPosition (start);
			InitialMove = true;
			MapView.MapMoveTo (start, MapMoveType.MapMoveSimple);
			InitialMove = false;

			MapView.EnableRotation      = false;
			MapView.EnableZoom          = true;
			MapView.EnableTilt          = false;
			MapView.EnableCompassButton = true;
			MapView.EnableMyLocation    = true;
		}

		protected BoundsLimit? GetBoundsLimit()
		{
			return CXMemoryManager.GetBoundsLimit();
		}

		protected CXModel GetModel()
		{
			return CXModel.GetInstance<CXModel>();
		}

		public void SetOnOffOfMarkerCategory()
		{
			var model = (CXModel)GetModel ();
			var cats  = model.CategoryLists ();
			foreach (var cat in cats) {
				var selected = model.IsSettingIdSelected (cat.id + 10000);
				foreach (var marker in CategoryMarkerList[cat.id]) {
					marker.marker.Selecting = selected;
				}
			}

			var mapCategories = model.MapCategoryLists ();
			if (mapCategories.Length != 0) 
			{
				CXWebTilesUrlConstructor constructor = null;
				var maps = model.MapLists (mapCategories[0].id);
				foreach (var map in maps) {
					var visility = model.IsSettingIdSelected (map.id);
					if (visility) {
						constructor = new CXWebTilesUrlConstructor (map.url, map.is_tms, map.min_zoom, map.max_zoom);
						constructor.MBTilesPath = CXStorageManager.GetInstance<CXStorageManager> ().GetPathFromUrl (map.xml_url);
						break;
					}
				}
				MapView.ShowMapLayer (constructor, true);
			}
		}

		protected bool ThreadingChangeCamera(CXMemoryManager memman, CameraPositionBag innerPos)
		{
			if (InitialMove) return false;
			if (innerPos.Latitude == 0 && innerPos.Longitude == 0) return false;
			var limitBounds = GetBoundsLimit();
			var needUpdate = false;

			if (limitBounds != null) {
				var sw = limitBounds.Value.SouthWest;
				var ne = limitBounds.Value.NorthEast;
				var nz = limitBounds.Value.minZoom;
				var xz = limitBounds.Value.maxZoom;
				if (sw != null && ne != null) 
				{
					if (innerPos.Latitude < sw.Value.Latitude) {
						innerPos.Latitude = sw.Value.Latitude;
						needUpdate = true;
					} else if (innerPos.Latitude > ne.Value.Latitude) {
						innerPos.Latitude = ne.Value.Latitude;
						needUpdate = true;
					}
					if (innerPos.Longitude < sw.Value.Longitude) {
						innerPos.Longitude = sw.Value.Longitude;
						needUpdate = true;
					} else if (innerPos.Longitude > ne.Value.Longitude) {
						innerPos.Longitude = ne.Value.Longitude;
						needUpdate = true;
					}
				}
				if (nz != null && innerPos.Zoom < nz.Value) {
					innerPos.Zoom = nz.Value;
					needUpdate = true;
				} else if (xz != null && innerPos.Zoom > xz.Value) {
					innerPos.Zoom = xz.Value;
					needUpdate = true;
				}

				if (needUpdate) 
				{
					var callPos = innerPos;
					memman.InvokeUIThread(()=>{
						MapView.MapMoveTo (callPos, MapMoveType.MapMoveAnimateIfSmooth);
					});
				}
			}

			var storage = SimpleStorage.EditGroup("StartMapPosition");
			storage.Put("Lat",String.Format("{0}",innerPos.Latitude));
			storage.Put("Lng",String.Format("{0}",innerPos.Longitude));
			storage.Put("Zoom",String.Format("{0}",innerPos.Zoom));

			return needUpdate;
		}

		public void ChangeCameraPosition(CameraPositionBag pos)
		{
			var ret = false;
			lock (lockObj) {
				if (CameraChanging) {
					nextUpdateBag = pos;
					ret = true;
				} else {
					nextUpdateBag = null;
					CameraChanging = true;
				}
			}

			if (ret) return;

			Action act = () =>{};
			var memman = CXMemoryManager.GetInstance<CXMemoryManager> ();
			var innerPos = pos;
			act = () => {
				var needUpdate = this.ThreadingChangeCamera(memman, innerPos);

				innerPos = null;

				lock (lockObj) {
					if (nextUpdateBag == null) {
						CameraChanging = false;
					} else if (needUpdate) {
						CameraChanging = false;
						nextUpdateBag = null;
					} else {
						innerPos = nextUpdateBag;
						nextUpdateBag = null;
					}
				}

				if (innerPos != null) {
					act();
				}
			};

			Task.Factory.StartNew (act);
		}

		public void ShowMarkers(bool showing = true) {

			var cats = GetModel().ItemGroupLists();

			foreach (var cat in cats) 
			{
				var catList  = new List<MarkerBag> ();
				var category = (CXModel.Category)cat;
				var markers = GetModel().ItemLists(cat.id);
				//var model = GetModel ();
				//var markerNum = model.ItemListCount (cat.id);
				foreach (var item in markers) {
					//for (var i=0; i<markerNum; i++) {
					//	var item = model.ItemAtRow (cat.id, i);
					var poi = (CXModel.Poi)item;
					var lat = poi.latitude;
					var lng = poi.longitude;
					var dbid = item.id;
					var markerObj = MapView.AddMarker (lat, lng, item.MainListText(), item.SubListText(), dbid, category.icon);
					markerObj.Showing = showing;

					MarkerBag bag;
					bag.marker = markerObj;
					bag.db_id = dbid;
					MarkerList.Add (markerObj.LinkingId, bag);
					DbMarkerList.Add (dbid, bag);
					catList.Add (bag);
				}
				CategoryMarkerList.Add (cat.id, catList);
			}
		}

		public void ClickInfoWindows(string MarkerLinkingId)
		{
			MarkerBag bag;
			if (MarkerList.TryGetValue(MarkerLinkingId, out bag))
			{
				var poi = (CXModel.Poi)GetModel().FindItemById(bag.db_id);
				if (poi != null) {
					MapView.OpenInfoWeb (poi.website);
				}
			}
		}
	
	}
}

