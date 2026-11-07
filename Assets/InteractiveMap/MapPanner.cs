using System;
using System.Collections;
using UnityEngine;

public class MapPanner : MonoBehaviour
{
    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private RectTransform viewport;

    [SerializeField]
    private InfoCard infoCard;

    [SerializeField]
    private TreeLoader treeLoader;

    [SerializeField]
    private float speed = 600;

    [Header("Zoom")]
    [SerializeField]
    private float maxZoom = 2f;

    [SerializeField]
    private float scrollZoomSpeed = 0.15f;

    [SerializeField]
    private float buttonZoomFactor = 1.25f;

    public float Zoom => content.localScale.x;
    public event Action<float> ZoomChanged;

    private Vector2 direction = Vector2.zero;
    private float zoomFactor = 0;
    private Camera cam;

    private Coroutine flight;

    public void CenterOn(Vector2 contentPoint, Action onDone = null)
    {
        if (flight != null)
            StopCoroutine(flight);
        flight = StartCoroutine(Fly(contentPoint, onDone));
    }

    private IEnumerator Fly(Vector2 q, Action onDone)
    {
        infoCard.Hide();
        var from = content.anchoredPosition;
        var to = ClampPosition(
            new Vector2(viewport.rect.width / 2, -viewport.rect.height / 2) - q * Zoom
        );

        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.45f)
        {
            float e = 1 - Mathf.Pow(1 - t, 3); // ease-out cubic
            content.anchoredPosition = Vector2.Lerp(from, to, e);
            // treeLoader.ShowTrees();
            yield return null;
        }
        content.anchoredPosition = to;
        // treeLoader.ShowTrees();
        onDone?.Invoke();
    }

    private float MinZoom =>
        Mathf.Max(
            viewport.rect.width / content.rect.width,
            viewport.rect.height / content.rect.height
        );

    private void Start()
    {
        var canvas = viewport.GetComponentInParent<Canvas>().rootCanvas;
        cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        content.anchoredPosition = new Vector2(-content.rect.width / 2, content.rect.height / 2);
        // treeLoader.SetZoom(Zoom);
    }

    private void Update()
    {
#if UNITY_EDITOR
        DebugKeys();
#endif

        if (zoomFactor != 0)
            ZoomAt(ViewportCenterOnScreen(), Zoom * Mathf.Pow(zoomFactor, Time.deltaTime));

        if (direction == Vector2.zero)
            return;

        var pos = content.anchoredPosition + speed * Time.deltaTime * direction;
        content.anchoredPosition = ClampPosition(pos);
        // treeLoader.ShowTrees();
    }

    private Vector2 ClampPosition(Vector2 pos)
    {
        float w = content.rect.width * Zoom;
        float h = content.rect.height * Zoom;
        pos.x = Mathf.Clamp(pos.x, viewport.rect.width - w, 0);
        pos.y = Mathf.Clamp(pos.y, 0, h - viewport.rect.height);
        return pos;
    }

    public void ZoomAt(Vector2 screenPoint, float target)
    {
        target = Mathf.Clamp(target, MinZoom, maxZoom);
        if (Mathf.Approximately(target, Zoom))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPoint,
            cam,
            out var before
        );
        content.localScale = Vector3.one * target;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPoint,
            cam,
            out var after
        );

        content.anchoredPosition = ClampPosition(
            content.anchoredPosition + (after - before) * target
        );

        infoCard.Hide();
        // treeLoader.SetZoom(target);
        ZoomChanged?.Invoke(target);
    }

    public Vector2 ViewportCenterOnScreen() =>
        RectTransformUtility.WorldToScreenPoint(cam, viewport.TransformPoint(viewport.rect.center));

    public void Begin(Vector2 direction)
    {
        this.direction = direction;
        infoCard.Hide();
    }

    public void End() => direction = Vector2.zero;

    public void BeginZoom(float zoomFactor)
    {
        this.zoomFactor = zoomFactor;
        infoCard.Hide();
    }

    public void EndZoom() => zoomFactor = 1;

#if UNITY_EDITOR
    private void DebugKeys()
    {
        direction = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow))
            direction += Vector2.down;
        if (Input.GetKey(KeyCode.DownArrow))
            direction += Vector2.up;
        if (Input.GetKey(KeyCode.LeftArrow))
            direction += Vector2.right;
        if (Input.GetKey(KeyCode.RightArrow))
            direction += Vector2.left;

        zoomFactor = 0;
        if (Input.GetKey(KeyCode.Equals))
            zoomFactor = 1.4f;
        if (Input.GetKey(KeyCode.Minus))
            zoomFactor = 0.6f;

        direction = direction.normalized;
    }
#endif
}
