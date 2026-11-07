using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BuildingLoader : MonoBehaviour
{
    [SerializeField]
    private TextAsset geoJsonAsset;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private Location locationPrefab;

    [SerializeField]
    private InfoCard infoCard;

    [SerializeField]
    private MapPanner panner;

    private const double CenterLat = 33.7756;
    private const double CenterLon = -84.3963;
    private const int ZoomLevel = 19;
    private const int GridSize = 13;
    private const int TileSize = 256;

    private void Start()
    {
        if (geoJsonAsset != null)
        {
            ParseBuildings(geoJsonAsset.text);
        }
        else
        {
            Debug.LogError("GeoJson Asset is not assigned in BuildingLoader!");
        }
    }

    public void ParseBuildings(string jsonText)
    {
        var collection = JsonConvert.DeserializeObject<GeoJsonCollection>(jsonText);

        if (collection == null || collection.features == null)
        {
            Debug.LogError("Failed to deserialize GeoJSON file.");
            return;
        }

        var centerX = LonToTileX(CenterLon, ZoomLevel);
        var centerY = LatToTileY(CenterLat, ZoomLevel);
        var originTileX = centerX - GridSize;
        var originTileY = centerY - GridSize;

        int spawnedCount = 0;

        foreach (var feature in collection.features)
        {
            if (feature.properties == null || feature.geometry == null)
                continue;
            if (string.IsNullOrEmpty(feature.properties.building))
                continue;
            if (feature.geometry.type != "Polygon" && feature.geometry.type != "MultiPolygon")
                continue;

            string name = !string.IsNullOrEmpty(feature.properties.name)
                ? feature.properties.name
                : "Unnamed Building";

            JToken coords = feature.geometry.coordinates;
            if (coords == null)
                continue;

            JToken outerRing = null;

            if (feature.geometry.type == "Polygon")
            {
                if (coords.HasValues)
                    outerRing = coords[0];
            }
            else if (feature.geometry.type == "MultiPolygon")
            {
                if (coords.HasValues && coords[0].HasValues)
                    outerRing = coords[0][0];
            }

            if (outerRing == null)
                continue;

            var localPoints = new List<Vector2>();

            foreach (var point in outerRing)
            {
                double lon = point[0].Value<double>();
                double lat = point[1].Value<double>();

                var globalPixels = LatLonToGlobalPixels(lat, lon, ZoomLevel);

                float localX = (float)(globalPixels.x - (originTileX * TileSize));
                float localY = -(float)(globalPixels.y - (originTileY * TileSize)); // Match inverted Y space

                localPoints.Add(new Vector2(localX, localY));
            }

            if (localPoints.Count > 0)
            {
                var location = Instantiate(locationPrefab, content);
                location.Initialize(feature.properties, localPoints.ToArray(), infoCard, panner);
                spawnedCount++;
            }
        }

        Debug.Log($"Successfully parsed and spawned {spawnedCount} building outlines!");
    }

    private Vector2 LatLonToGlobalPixels(double lat, double lon, int zoom)
    {
        var n = Math.Pow(2.0, zoom);
        var x = (lon + 180.0) / 360.0 * n * TileSize;
        var latRad = lat * Math.PI / 180.0;
        var y =
            (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI)
            / 2.0
            * n
            * TileSize;
        return new Vector2((float)x, (float)y);
    }

    public static int LonToTileX(double lon, int zoom) =>
        (int)Math.Floor((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom));

    public static int LatToTileY(double lat, int zoom)
    {
        var rad = lat * Math.PI / 180.0;
        var proj =
            (1.0 - Math.Log(Math.Tan(rad) + 1.0 / Math.Cos(rad)) / Math.PI)
            / 2.0
            * Math.Pow(2.0, zoom);
        return (int)Math.Floor(proj);
    }

    public class GeoJsonCollection
    {
        public List<Feature> features;
    }

    public class Feature
    {
        public Properties properties;
        public Geometry geometry;
    }

    public class Properties
    {
        [JsonProperty("@id")]
        public string id;

        public string building;
        public string name;
        public string website;

        [JsonProperty("addr:housenumber")]
        public string houseNumber;

        [JsonProperty("addr:street")]
        public string street;

        [JsonProperty("building:levels")]
        public string levels;

        [JsonProperty("gatech:BLDG_TYPE")]
        public string gtType;
    }

    public class Geometry
    {
        public string type;
        public JToken coordinates;
    }
}
