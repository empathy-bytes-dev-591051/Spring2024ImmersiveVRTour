using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TreeLoader : MonoBehaviour
{
    //     [SerializeField]
    //     private TextAsset json;
    //
    //     [SerializeField]
    //     private RectTransform treeContent;
    //
    //     [SerializeField]
    //     private RectTransform mapContent;
    //
    //     [SerializeField]
    //     private TreeMarker markerPrefab;
    //
    //     [SerializeField]
    //     private TreeInfoCard infoCard;
    //
    //     [SerializeField]
    //     private RectTransform viewport;
    //
    //     private Tree[] trees;
    //
    //     private readonly Dictionary<Tree, TreeMarker> treeObjects = new();
    //
    //     private const double CenterLat = 33.7756;
    //     private const double CenterLon = -84.3963;
    //     private const int ZoomLevel = 19;
    //     private const int GridSize = 13;
    //     private const int TileSize = 256;
    //
    //     private void Start()
    //     {
    //         int gridDimension = (GridSize * 2) + 1;
    //         float totalMapSize = gridDimension * TileSize;
    //
    //         mapContent.sizeDelta = new Vector2(totalMapSize, totalMapSize);
    //         treeContent.sizeDelta = new Vector2(totalMapSize, totalMapSize);
    //
    //         trees = JsonUtility.FromJson<Wrapper>(json.text).trees;
    //         ShowTrees();
    //     }
    //
    //     public void ShowTrees()
    //     {
    //         var centerX = LonToTileX(CenterLon, ZoomLevel);
    //         var centerY = LatToTileY(CenterLat, ZoomLevel);
    //
    //         var originTileX = centerX - GridSize;
    //         var originTileY = centerY - GridSize;
    //
    //         var minX = -mapContent.anchoredPosition.x - 50;
    //         var maxX = -mapContent.anchoredPosition.x + viewport.rect.width + 50;
    //         var maxY = -mapContent.anchoredPosition.y + 50;
    //         var minY = -mapContent.anchoredPosition.y - viewport.rect.height - 50;
    //
    //         foreach (var tree in trees)
    //         {
    //             var lon = tree.geometry.coordinates[0];
    //             var lat = tree.geometry.coordinates[1];
    //
    //             var globalPixels = LatLonToGlobalPixels(lat, lon, ZoomLevel);
    //
    //             var localX = (float)(globalPixels.x - (originTileX * TileSize));
    //             var localY = -(float)(globalPixels.y - (originTileY * TileSize));
    //
    //             if (localX < minX || localX > maxX || localY > maxY || localY < minY)
    //             {
    //                 if (treeObjects.TryGetValue(tree, out var obj))
    //                 {
    //                     Destroy(obj.gameObject);
    //                     treeObjects.Remove(tree);
    //                 }
    //                 continue;
    //             }
    //
    //             if (treeObjects.ContainsKey(tree))
    //                 continue;
    //
    //             var marker = Instantiate(markerPrefab, treeContent);
    //             marker.RectTransform.anchoredPosition = new Vector2(localX, localY);
    //
    //             var scientificName = $"{tree.properties.genus} {tree.properties.species}";
    //             marker.Button.onClick.AddListener(() =>
    //                 infoCard.Show(marker.transform.position, scientificName, tree.properties.commonname)
    //             );
    //
    //             treeObjects.Add(tree, marker);
    //         }
    //     }
    //
    //     public static int LonToTileX(double lon, int zoom) =>
    //         (int)Math.Floor((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom));
    //
    //     public static int LatToTileY(double lat, int zoom)
    //     {
    //         var rad = lat * Math.PI / 180.0;
    //         var proj =
    //             (1.0 - Math.Log(Math.Tan(rad) + 1.0 / Math.Cos(rad)) / Math.PI)
    //             / 2.0
    //             * Math.Pow(2.0, zoom);
    //         return (int)Math.Floor(proj);
    //     }
    //
    //     private Vector2 LatLonToGlobalPixels(double lat, double lon, int zoom)
    //     {
    //         var n = Math.Pow(2.0, zoom);
    //         var x = (lon + 180.0) / 360.0 * n * TileSize;
    //         var latRad = lat * Math.PI / 180.0;
    //         var y =
    //             (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI)
    //             / 2.0
    //             * n
    //             * TileSize;
    //         return new Vector2((float)x, (float)y);
    //     }
    //
    //     [Serializable]
    //     public class Wrapper
    //     {
    //         public Tree[] trees;
    //     }
    //
    //     [Serializable]
    //     public class Tree
    //     {
    //         public Geometry geometry;
    //         public Properties properties;
    //     }
    //
    //     [Serializable]
    //     public class Geometry
    //     {
    //         public double[] coordinates;
    //     }
    //
    //     public class Properties
    //     {
    //         public string building;
    //         public string name;
    //         public string website;
    //         public string description;
    //         public string wikipedia;
    //         public string start_date;
    //
    //         [JsonProperty("short_name")]
    //         public string shortName;
    //
    //         [JsonProperty("addr:housenumber")]
    //         public string houseNumber;
    //
    //         [JsonProperty("addr:street")]
    //         public string street;
    //
    //         [JsonProperty("addr:city")]
    //         public string city;
    //
    //         [JsonProperty("building:levels")]
    //         public string levels;
    //
    //         [JsonProperty("gatech:BLDG_TYPE")]
    //         public string gtType;
    //
    //         [JsonProperty("gatech:BLDG_NUM")]
    //         public string gtNum;
    //     }
}
