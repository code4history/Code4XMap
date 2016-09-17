using System;
using UIKit;
using Google.Maps;
using CoreLocation;
using Foundation;
using System.Collections.Specialized;
using CoreGraphics;

namespace Code4XMap
{
	public class MapMarker : IMarker
	{
		protected Marker markerObj;
		protected Google.Maps.MapView map;

		private string _LinkingId;
		public string LinkingId {
			get {
				return _LinkingId;
			}
		}

		public MapMarker(Marker _mObj, int ExId)
		{
			markerObj  = _mObj;
			map        = markerObj.Map;
			_LinkingId = ExId.ToString ();
			markerObj.UserData = new NSString(_LinkingId);
		}

		public void Remove() 
		{
			markerObj.Map = null;
			map           = null;
		}

		public void Select()
		{
			var latlng = markerObj.Position;
			var camera = map.Camera;
			var update = CameraUpdate.SetCamera (new CameraPosition (new CLLocationCoordinate2D (latlng.Latitude, latlng.Longitude), 15.0f, camera.Bearing, camera.ViewingAngle));

			map.MoveCamera (update);
			map.SelectedMarker = markerObj;
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
					markerObj.Map = map;
				} else {
					markerObj.Map = null;
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

	public class CXMapFragment : UIViewController, ICXMapFragment, IQueryStringAcceptor, ICLLocationManagerDelegate
	{
		private const float BOTTOM_MARGIN_OF_COPYIMAGE = 25f;
		private const float LEFT_MARGIN_OF_COPYIMAGE = 60f;
		private const float RIGHT_MARGIN_OF_COPYIMAGE = 60f;
		private const float TEXT_SIZE_OF_COPYRIGHT = 8f;

		protected int? selectedDbId = null;
		protected CXMapController mController;
		protected CXWebTilesProvider       tileLayer = null;
		protected CXWebTilesUrlConstructor tileConstructor = null;
		public UIImageView CopyrightImage;
		public UILabel CopyrightText;

		public CXMapFragment () : base()
		{
			//Title = "Map";
			//TabBarItem.Image = UIImage.FromBundle ("first");
		}

		public Google.Maps.MapView _MapView {
			get {
				return (Google.Maps.MapView)View;
			}
		}
		public bool EnableRotation { 
			get {
				return _MapView.Settings.RotateGestures;
			}
			set {
				_MapView.Settings.RotateGestures = value;
			} 
		}
		public bool EnableCompassButton { 
			get {
				return _MapView.Settings.CompassButton;
			} 
			set {
				_MapView.Settings.CompassButton = value;
			} 
		}
		public bool EnableTilt { 
			get {
				return _MapView.Settings.TiltGestures;
			} 
			set {
				_MapView.Settings.TiltGestures = value;
			} 
		}
		public bool EnableZoom { 
			get {
				return _MapView.Settings.ZoomGestures;
			} 
			set {
				_MapView.Settings.ZoomGestures = value;
			} 
		}
		public bool EnableMyLocation { 
			get {
				return _MapView.Settings.MyLocationButton;
			} 
			set {
				_MapView.MyLocationEnabled = value;
				_MapView.Settings.MyLocationButton = value;
			} 
		}

		public IMarker AddMarker (double Latitude, double Longitude, string title, string snippet, int ExclusiveId, string icon)
		{
			var position = new CLLocationCoordinate2D (Latitude, Longitude);
			var markerObj = new Marker ();
			markerObj.Position = position;
			markerObj.Title    = title;
			markerObj.Snippet  = snippet;
			markerObj.Icon     = UIImage.FromBundle (icon);

			markerObj.Map      = _MapView;

			return new MapMarker (markerObj, ExclusiveId);
		}

		public void MapMoveTo(CameraPositionBag position, MapMoveType moveType)
		{
			var cameraPos = new CameraPosition (new CLLocationCoordinate2D (position.Latitude, position.Longitude), position.Zoom, 0.0, 0.0);

			var mapView = ((Google.Maps.MapView)View);
			if (moveType == MapMoveType.MapMoveAnimate) {
				mapView.Animate (cameraPos);
			} else {
				var update = CameraUpdate.SetCamera(cameraPos);
				mapView.MoveCamera(update);
			}
		}

		public void OpenInfoWeb (string webUrl)
		{
			CXMemoryManager.OpenNextUrl ((CXTabFragment)this.TabBarController, webUrl);
		}

		public void ShowMapLayer (CXWebTilesUrlConstructor constructor, bool exclusive)
		{
			if (constructor == null) {
				if (tileLayer != null) {
					tileLayer.Map = null;
				}
				tileLayer = null;
				tileConstructor = null;
			} else {
				if (tileLayer != null && constructor != null &&
				    tileConstructor.TileUrl == constructor.TileUrl)
					return;

				var oldLayer = tileLayer;
				tileConstructor = constructor;
				tileLayer = new CXWebTilesProvider (constructor);
				/*tileLayer = UrlTileLayer.FromUrlConstructor((uint x, uint y, uint zoom) => {
					var url = constructor.GetTileUrl((int)x,(int)y,(int)zoom);
					return url == null ? null : new NSUrl(url); 
				});*/
				tileLayer.Map = _MapView;

				if (oldLayer != null) {
					oldLayer.Map = null;
				}
			}
		}

		public override void LoadView ()
		{
			var mapView = new Google.Maps.MapView();
			mapView.Settings.MyLocationButton = true;
			mapView.MyLocationEnabled = true;
			View = mapView;

			mController = Controller ();

			mController.InitialMapSettings ();
			mapView.Delegate = new CXMapViewDelegate (this);

			mController.ShowMarkers (true);//mapView.Camera.Zoom > 14.0f);
		}

		public void SetCopyRightImage(UIImage Copy)
		{
			if (CopyrightImage == null)
			{
				CopyrightImage = new UIImageView();
				this.View.Add(CopyrightImage);
			}

			if (Copy == null)
			{
				Copy = UIImage.FromBundle("empty");
			}

			CopyrightImage.Image = Copy;
			var frame = new CGRect(0, View.Frame.Height - BOTTOM_MARGIN_OF_COPYIMAGE - Copy.Size.Height, Copy.Size.Width,Copy.Size.Height);
			CopyrightImage.Frame = frame;
		}

		public void SetCopyRightText(string attribution)
		{
			if (CopyrightText == null)
			{
				CopyrightText = new UILabel ();
				CopyrightText.Font = UIFont.SystemFontOfSize (TEXT_SIZE_OF_COPYRIGHT);
				CopyrightText.Lines = 100;
				CopyrightText.LineBreakMode = UILineBreakMode.WordWrap;
				CopyrightText.TextColor = UIColor.LightGray;
				this.View.Add(CopyrightText);
			}

			if (attribution == null)
				attribution = "";

			var copyTextWidth = View.Frame.Width - LEFT_MARGIN_OF_COPYIMAGE - RIGHT_MARGIN_OF_COPYIMAGE;

			var nsstring = new NSString (attribution);
			var size = nsstring.StringSize (UIFont.SystemFontOfSize (TEXT_SIZE_OF_COPYRIGHT), new CGSize (copyTextWidth, 1000f),
			                                UILineBreakMode.WordWrap);
			var copyTextHeight = size.Height;

			CopyrightText.Text = attribution;

			var frame = new CGRect(LEFT_MARGIN_OF_COPYIMAGE, View.Frame.Height - copyTextHeight, copyTextWidth, copyTextHeight);
			CopyrightText.Frame = frame;
		}

		protected virtual CXMapController Controller()
		{
			return new CXMapController (this);
		}

		public virtual void SetFragmentAndQuery(string fragment, NameValueCollection qs)
		{
			var dbid = qs["dbid"];
			if (dbid != null) {
				selectedDbId = int.Parse (dbid);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			//mController.ShowMarkers ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			mController.SetOnOffOfMarkerCategory ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			var locationManager = new CLLocationManager ();
			locationManager.WeakDelegate = this;

			if (locationManager.RespondsToSelector(new ObjCRuntime.Selector("requestWhenInUseAuthorization"))) {
				// iOS バージョンが 8 以上で、requestAlwaysAuthorization メソッドが
				// 利用できる場合

				// 位置情報測位の許可を求めるメッセージを表示する
				locationManager.RequestWhenInUseAuthorization ();
			}

			if (selectedDbId != null) 
			{
				var dbidval = selectedDbId.Value;
				var marker = mController.GetMarkerByDbId (dbidval);
				marker.Select ();

				selectedDbId = null;
			}
		}

		/*public bool TappedMarker (MapView mapView, Marker marker)
		{

		}*/

		public void DidTapInfoWindowOfMarker (Google.Maps.MapView mapView, Marker marker)
		{
			var LinkingId = ((NSString)marker.UserData).ToString ();
			mapView.SelectedMarker = null;

			mController.ClickInfoWindows(LinkingId);
		}

		public void DidChangeCameraPosition (Google.Maps.MapView mapView, CameraPosition position)
		{
			var srcPos = new CameraPositionBag {
				Latitude  = position.Target.Latitude,
				Longitude = position.Target.Longitude,
				Zoom      = position.Zoom
			};
			if (srcPos.Latitude == 0 && srcPos.Longitude == 0 && srcPos.Zoom == 2) return;

			mController.ChangeCameraPosition(srcPos);
		}
	}

	public class CXMapViewDelegate : Google.Maps.MapViewDelegate
	{
		private WeakReference _MView = null;
		private CXMapFragment MView 
		{
			set {
				if (value == null) {
					_MView = null;
				} else {
					_MView = new WeakReference (value);
				}
			}

			get {
				if (_MView == null) {
					return null;
				} else {
					return (CXMapFragment)_MView.Target;
				}
			}
		}

		public CXMapViewDelegate (CXMapFragment MView) : base()
		{
			this.MView = MView;
		}

		public override void DidTapInfoWindowOfMarker (Google.Maps.MapView mapView, Marker marker)
		{
			this.MView.DidTapInfoWindowOfMarker (mapView, marker);
		}

		public override void DidChangeCameraPosition (Google.Maps.MapView mapView, CameraPosition position)
		{
			this.MView.DidChangeCameraPosition (mapView, position);
		}
	}
}

