using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapZoom : MonoBehaviour, IScrollHandler
{
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform viewport;

    [SerializeField]
    private float maxZoom = 2f;

    [SerializeField]
    private float zoomSpeed = 0.1f;

    public float Zoom { get; private set; } = 1f;
    public event Action<float> ZoomChanged;

    private RectTransform content;

    private void Awake()
    {
        content = scrollRect.content;
        scrollRect.scrollSensitivity = 0f; // wheel now zooms instead of scrolling
    }

    // Smallest zoom at which the map still fills the viewport
    private float MinZoom =>
        Mathf.Max(
            viewport.rect.width / content.rect.width,
            viewport.rect.height / content.rect.height
        );

    public void OnScroll(PointerEventData e)
    {
        float target = Mathf.Clamp(Zoom * Mathf.Exp(e.scrollDelta.y * zoomSpeed), MinZoom, maxZoom);
        ZoomAt(e.position, e.enterEventCamera, target);
    }

    public void ZoomAt(Vector2 screenPoint, Camera cam, float target)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPoint,
            cam,
            out var before
        );

        Zoom = target;
        content.localScale = Vector3.one * Zoom;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPoint,
            cam,
            out var after
        );

        // Shift the content so the point under the cursor stays put
        content.anchoredPosition += (after - before) * Zoom;

        ZoomChanged?.Invoke(Zoom);
    }
}
