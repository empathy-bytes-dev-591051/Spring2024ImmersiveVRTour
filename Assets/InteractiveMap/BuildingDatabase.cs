using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ImageInfo
{
    public string file;
    public string license;
    public string licenseUrl;
    public string credit;
    public string filePage;
    public bool attributionRequired;
}

[Serializable]
public class BuildingInfo
{
    public string name;
    public string description;
    public string tagline;
    public string wikiUrl;
    public List<ImageInfo> images = new();
}

public static class BuildingDatabase
{
    private static Dictionary<string, BuildingInfo> map;

    public static bool TryGet(string osmId, out BuildingInfo info)
    {
        if (map == null)
        {
            var asset = Resources.Load<TextAsset>("BuildingInfo/buildings_info");
            map =
                asset != null
                    ? JsonConvert.DeserializeObject<Dictionary<string, BuildingInfo>>(asset.text)
                    : new();
        }
        info = null;
        return !string.IsNullOrEmpty(osmId) && map.TryGetValue(osmId, out info);
    }

    // Resources.Load paths have no extension
    public static Sprite LoadSprite(ImageInfo im) =>
        Resources.Load<Sprite>("BuildingInfo/Images/" + Path.GetFileNameWithoutExtension(im.file));
}
