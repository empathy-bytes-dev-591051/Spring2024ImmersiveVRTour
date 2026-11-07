using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Location
    : MaskableGraphic,
        IPointerEnterHandler,
        IPointerExitHandler,
        ICanvasRaycastFilter,
        IPointerClickHandler
{
    [SerializeField]
    private float lineWidth = 3f;

    [SerializeField]
    private Color normalColor = Color.yellow;

    [SerializeField]
    private Color hoverColor = Color.cyan;

    private Vector2[] points = new Vector2[0];
    private string buildingName;

    [SerializeField]
    private Color landmarkColor = new(1f, 0.8f, 0.2f);

    private BuildingLoader.Properties props;
    private InfoCard infoCard;

    [SerializeField]
    private MapPanner panner;

    public void Initialize(
        BuildingLoader.Properties props,
        Vector2[] mapPoints,
        InfoCard card,
        MapPanner panner
    )
    {
        this.props = props;
        infoCard = card;
        buildingName = props.name;
        this.panner = panner;

        // Buildings with Wikipedia data get a different outline color
        // if (BuildingDatabase.TryGet(props.id, out _))
        //     normalColor = landmarkColor;

        var list = new List<Vector2>(mapPoints.Length);
        foreach (var p in mapPoints)
            if (list.Count == 0 || (p - list[^1]).sqrMagnitude > 0.01f)
                list.Add(p);
        if (list.Count > 1 && (list[0] - list[^1]).sqrMagnitude < 0.01f)
            list.RemoveAt(list.Count - 1);

        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var p in list)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        var topLeft = new Vector2(min.x, max.y);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = topLeft;
        rectTransform.sizeDelta = max - min;

        points = new Vector2[list.Count];
        for (int i = 0; i < list.Count; i++)
            points[i] = list[i] - topLeft;

        color = normalColor;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        int n = points.Length;
        if (n < 3)
            return;

        float half = lineWidth * 0.5f;

        for (int i = 0; i < n; i++)
        {
            var prev = points[(i - 1 + n) % n];
            var cur = points[i];
            var next = points[(i + 1) % n];

            var d0 = (cur - prev).normalized;
            var d1 = (next - cur).normalized;
            var n0 = new Vector2(-d0.y, d0.x);
            var n1 = new Vector2(-d1.y, d1.x);

            var miter = n0 + n1;
            miter = miter.sqrMagnitude < 1e-6f ? n1 : miter.normalized;

            // Clamp so very sharp corners don't spike
            float scale = half / Mathf.Max(Vector2.Dot(miter, n1), 0.25f);

            AddVert(vh, cur + miter * scale);
            AddVert(vh, cur - miter * scale);
        }

        for (int i = 0; i < n; i++)
        {
            int a = i * 2;
            int b = (i + 1) % n * 2;
            vh.AddTriangle(a, a + 1, b);
            vh.AddTriangle(b, a + 1, b + 1);
        }
    }

    private void AddVert(VertexHelper vh, Vector2 pos)
    {
        vh.AddVert(
            new UIVertex
            {
                position = pos,
                color = color,
                uv0 = Vector2.zero,
            }
        );
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                eventCamera,
                out var local
            )
        )
            return false;
        return PointInPolygon(local);
    }

    private bool PointInPolygon(Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
        {
            var a = points[i];
            var b = points[j];
            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData) => color = normalColor;

    public Vector2 Center =>
        rectTransform.anchoredPosition
        + new Vector2(rectTransform.rect.width / 2, -rectTransform.rect.height / 2);

    public void OnPointerClick(PointerEventData e) =>
        panner.CenterOn(Center, () => infoCard.Show(panner.ViewportCenterOnScreen(), BuildCard()));

    private CardData BuildCard()
    {
        var d = new CardData
        {
            Title = string.IsNullOrEmpty(props.name) ? "Unnamed building" : props.name,
        };
        bool has = BuildingDatabase.TryGet(props.id, out var info);

        d.Subtitle = has && !string.IsNullOrEmpty(info.tagline) ? info.tagline : props.gtType;
        if (has)
        {
            d.Description =
                info.description
                + $"\n<size=75%><color=#999999>Excerpt from <link=\"{info.wikiUrl}\"><u>Wikipedia</u></link>, "
                + "<link=\"https://creativecommons.org/licenses/by-sa/4.0/\"><u>CC BY-SA 4.0</u></link></color></size>";
            d.Images = info.images;
        }

        var address = $"{props.houseNumber} {props.street}".Trim();
        if (address.Length > 0)
        {
            d.Stats.Add(("Address", address));
        }
        if (!string.IsNullOrEmpty(props.levels))
            d.Stats.Add(("Floors", props.levels));

        return d;
    }
}
