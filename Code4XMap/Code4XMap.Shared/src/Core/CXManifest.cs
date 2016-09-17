using System;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Code4XMap
{
	public interface IManifest
	{
		string Name { get; set; }
		string Title { get; set; }
		float  Version { get; set; }
		DateTime Updated { get; set; }
		Uri TargetUrl { get; set; }
		Uri ModelUrl { get; set; }
		List<Uri> Resources { get; set; }
	}

	public class CXManifest : IManifest
	{
		public string Name { get; set; }
		public string Title { get; set; }
		public float  Version { get; set; }
		public DateTime Updated { get; set; }
		public Uri TargetUrl { get; set; }
		private Uri _modelUrl;
		public Uri ModelUrl { 
			get { 
				return _modelUrl;
			} 
			set { 
				_modelUrl = new Uri (TargetUrl, value);
			} 
		}

		private List<Uri> _Resources;
		public List<Uri> Resources { 
			get {
				if (_Resources == null) return null;

				var canons = new List<Uri>();
				foreach (var val in _Resources) 
				{
					var canon = new Uri (TargetUrl, val);
					canons.Add (canon);
				}

				return canons;
			}
			set {
				_Resources = value;
			} 
		}

		public CameraPositionBag startPosition;
		public BoundsLimit boundsLimit;
	}

	public class ManifestConverter : CustomCreationConverter<IManifest>
	{
		public delegate Type TypeFactory (string typeString);

		public static List<TypeFactory> factories = new List<TypeFactory> () {
			(string typeString) => {
				return typeString == "Manifest" ? typeof(CXManifest) : null;
			}
		};
		private Type oType = null;

		public ManifestConverter(string srcJson) : base () 
		{
			var obj = Newtonsoft.Json.Linq.JObject.Parse (srcJson);

			var typ = obj ["Type"];
			if (typ != null) {
				var types = typ.Value<string> ();
				foreach (var factory in factories) {
					oType = factory (types);
					if (oType != null)
						continue;
				}
			}
			if (oType == null)
				oType = typeof(CXManifest);
		}

		public override IManifest Create(Type objectType)
		{
			return (IManifest) Activator.CreateInstance (oType);
		}
	}
}

