using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Content;
using Android.Views.InputMethods;
using System.Collections.Specialized;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Android.Support.V4.App;

namespace Code4XMap
{
    public class MapMarker : IMarker
	{
		protected Marker markerObj;

		private string _LinkingId;
		public string LinkingId {
			get {
				return _LinkingId;
			}
		}

		protected WeakReference _map;
		public GoogleMap Map {
			protected get {
				if (_map == null) {
					return null;
				} else {
					return (GoogleMap)_map.Target;
				}
			}

			set {
				if (value == null) {
					_map = null;
				} else {
					_map = new WeakReference (value);
				}
			}
		}

		public MapMarker(Marker _mObj, int ExId)
		{
			markerObj  = _mObj;
			_LinkingId = _mObj.Id;
		}

		public MapMarker(Marker _mObj, int ExId, GoogleMap map) : this(_mObj, ExId)
		{
			this.Map = map;
		}

		public void Remove() 
		{
			this.Map = null;
			markerObj.Remove ();
		}

		public void Select()
		{
			var campos = CameraPosition.InvokeBuilder(Map.CameraPosition).Zoom(15.0f)
				.Target(new LatLng (markerObj.Position.Latitude, markerObj.Position.Longitude)).Build();
			var cam = CameraUpdateFactory.NewCameraPosition (campos);
			Map.MoveCamera (cam);
			markerObj.ShowInfoWindow ();
		}

		bool _Selecting = true;
		public bool Selecting
		{
			get { 
				return _Selecting;
			}

			set {
				_Selecting = value;
				if (_Selecting && _Showing) {
					markerObj.Visible = true;
				} else {
					markerObj.Visible = false;
				}
			}
		}

		bool _Showing = true;
		public bool Showing
		{
			get { 
				return _Showing;
			}

			set { 
				_Showing = value;
				Selecting = Selecting;
			}
		}
	}

	public class CXMapFragment : Fragment, ICXMapFragment, IOnMapReadyCallback
	{
		protected SupportMapFragment mFragment;
		protected int? selectedDbId = null;
		protected CXMapController mController;
		protected GoogleMap mGoogleMap;
		protected TileOverlay   tileOverlay = null;
		protected ITileProvider tileProvider = null;
		public ImageView CopyrightImage;
		public TextView CopyrightText;
        private TaskCompletionSource<GoogleMap> comp;

        readonly string[] PermissionsLocation = {
            Android.Manifest.Permission.AccessCoarseLocation,
            Android.Manifest.Permission.AccessFineLocation
        };

        const int RequestLocationId = 0;

        private const float BOTTOM_MARGIN_TABLET = 15.0f;

		public GoogleMap Map {
			get { 
				return mGoogleMap;
			}
		}

		public CXMapFragment () : base()
		{
		}

        public void OnMapReady(GoogleMap gmap)
        {
            comp.SetResult(gmap);
            comp = null;
        }

		public bool EnableRotation { 
			get {
				return Map.UiSettings.RotateGesturesEnabled; 
			}
			set {
				Map.UiSettings.RotateGesturesEnabled = value;
			} 
		}
		public bool EnableCompassButton { 
			get {
				return Map.UiSettings.CompassEnabled;
			} 
			set {
				Map.UiSettings.CompassEnabled = value;
			} 
		}
		public bool EnableTilt { 
			get {
				return Map.UiSettings.TiltGesturesEnabled;
			} 
			set {
				Map.UiSettings.TiltGesturesEnabled = value;
			} 
		}
		public bool EnableZoom { 
			get {
				return Map.UiSettings.ZoomGesturesEnabled;
			} 
			set {
				Map.UiSettings.ZoomGesturesEnabled = value;
			} 
		}
		public bool EnableMyLocation { 
			get {
				return Map.UiSettings.MyLocationButtonEnabled;
			} 
			set {
                if (value == false) {
                    Map.MyLocationEnabled = false;
                    Map.UiSettings.MyLocationButtonEnabled = false;
                    return;
                } else if ((int)Build.VERSION.SdkInt < 23) {
                    Map.MyLocationEnabled = true;
				    Map.UiSettings.MyLocationButtonEnabled = true;
                    return;
                } else {
                    const string permission = Android.Manifest.Permission.AccessFineLocation;
                    if (Context.CheckCallingOrSelfPermission(permission) == (int)Permission.Granted)
                    {
                        Map.MyLocationEnabled = true;
                        Map.UiSettings.MyLocationButtonEnabled = true;
                        return;
                    }

                    if (ShouldShowRequestPermissionRationale(permission))
                    {
                        //Explain to the user why we need to read the contacts
                        Snackbar.Make(this.View, "地図に現在地表示を表示するために現在地を取得します", Snackbar.LengthIndefinite)
                                .SetAction("OK", v => RequestPermissions(PermissionsLocation, RequestLocationId))
                                .Show();
                        return;
                    }
                    //Finally request permissions with the list of permissions and Id
                    RequestPermissions(PermissionsLocation, RequestLocationId);

                }
			} 
		}

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            Map.MyLocationEnabled = true;
                            Map.UiSettings.MyLocationButtonEnabled = true;
                        }
                        else
                        {
                            Map.MyLocationEnabled = false;
                            Map.UiSettings.MyLocationButtonEnabled = false;
                        }
                    }
                    break;
            }
        }

        public IMarker AddMarker (double Latitude, double Longitude, string title, string snippet, int ExclusiveId, string icon)
		{
			var position = new LatLng (Latitude, Longitude);
			var opt = new MarkerOptions ().SetPosition (position).SetTitle (title).SetSnippet (snippet);
			var markerObj = Map.AddMarker (opt);
			var resName = System.IO.Path.GetFileNameWithoutExtension (icon);
			var resId = Resources.GetIdentifier (resName, "drawable", this.Activity.PackageName);
			var iconObj = BitmapDescriptorFactory.FromResource (resId);
			markerObj.SetIcon (iconObj);

			return new MapMarker (markerObj, ExclusiveId, Map);
		}

		public void MapMoveTo(CameraPositionBag position, MapMoveType moveType)
		{
			var newLatLng = new LatLng(position.Latitude, position.Longitude);
			var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(newLatLng, position.Zoom);

			if (moveType == MapMoveType.MapMoveSimple) {
				Map.MoveCamera(cameraUpdate);
			} else {
				Map.AnimateCamera(cameraUpdate);
			}
		}

		public void OpenInfoWeb (string webUrl)
		{
			CXMemoryManager.OpenNextUrl ((IQueryStringAcceptor)this.Activity, webUrl);
		}

		public void ShowMapLayer (CXWebTilesUrlConstructor constructor, bool exclusive)
		{
			if (constructor == null) {
				if (tileOverlay != null) {
					tileOverlay.Remove ();
				}
				tileOverlay = null;
				tileProvider = null;
			} else {
				if (tileOverlay != null && constructor != null &&
				    ((CXWebTilesProvider)tileProvider).BaseUrl == constructor.TileUrl)
					return;

				var oldOverlay = tileOverlay;
				tileProvider = new CXWebTilesProvider (constructor);
				var tileOptions = new TileOverlayOptions ();
				tileOverlay = Map.AddTileOverlay (tileOptions.InvokeTileProvider (tileProvider));

				if (oldOverlay != null) {
					oldOverlay.Remove ();
				}
			}
		}

		public override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			mController = Controller();
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			FrameLayout v = (FrameLayout)inflater.Inflate (Resource.Layout.MapFragment, container, false);
			return v;
		}

		protected virtual CXMapController Controller()
		{
			return new CXMapController (this);
		}

		public void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
			var dbid = qs["dbid"];
			if (dbid != null) {
				selectedDbId = int.Parse (dbid);
			}
		}

		public override async void OnResume ()
		{
            base.OnResume ();

			await SetUpMapIfNeeded();
			mController.SetOnOffOfMarkerCategory ();

			var imm = (InputMethodManager)this.Activity.GetSystemService(Context.InputMethodService);
			imm.HideSoftInputFromWindow(this.View.WindowToken, HideSoftInputFlags.NotAlways);

			if (selectedDbId != null) 
			{
				var dbid = selectedDbId.Value;
				var marker = mController.GetMarkerByDbId (dbid);
				marker.Select ();

				selectedDbId = null;
			}
		}

		public override void OnViewCreated (View view, Android.OS.Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);

			var fm = this.ChildFragmentManager;
			mFragment = (SupportMapFragment) fm.FindFragmentById(Resource.Id.map_fragment);
			if (mFragment == null) {
				mFragment = SupportMapFragment.NewInstance ();
				fm.BeginTransaction ().Replace (Resource.Id.map_fragment, mFragment).Commit ();
			}
			CopyrightImage = (ImageView)view.FindViewById (Resource.Id.MapCopyRight);
			CopyrightText  = (TextView)view.FindViewById (Resource.Id.TextCopyRight);
			if (!CXMobileUtility.IsPhone ()) {
				var origin = (FrameLayout.LayoutParams)CopyrightText.LayoutParameters;
				var lp = new FrameLayout.LayoutParams (origin);
				lp.SetMargins (origin.LeftMargin, origin.TopMargin, origin.RightMargin, origin.BottomMargin + (int)(BOTTOM_MARGIN_TABLET * CXMobileUtility.DisplayDensity ()));
				lp.Gravity = origin.Gravity;
				lp.LayoutDirection = origin.LayoutDirection;
				CopyrightText.LayoutParameters = lp;
			}
		}

		private async Task<int> SetUpMapIfNeeded() {
            if (mGoogleMap == null) {
                comp = new TaskCompletionSource<GoogleMap>();
                mFragment.GetMapAsync(this);
                var task = comp.Task;
                mGoogleMap = await task;

				Map.CameraChange += (object sender, GoogleMap.CameraChangeEventArgs e) => {
					var srcPos = new CameraPositionBag {
						Latitude  = e.Position.Target.Latitude,
						Longitude = e.Position.Target.Longitude,
						Zoom      = e.Position.Zoom
					};
					mController.ChangeCameraPosition(srcPos);
				};

				// We create an instance of CameraUpdate, and move the map to it.
				mController.InitialMapSettings ();
				mController.ShowMarkers (true); //mGoogleMap.CameraPosition.Zoom > 14.0f);
				Map.InfoWindowClick += (object sender, GoogleMap.InfoWindowClickEventArgs e) => {
					var marker = e.Marker;

					var LinkingId = marker.Id;
					marker.HideInfoWindow();

					mController.ClickInfoWindows(LinkingId);
				};
			}

            return 1;
		}
	}
}


