using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardData
{
    public string Title;
    public string Subtitle;
    public string Description;
    public List<ImageInfo> Images = new();
    public List<(string label, string value)> Stats = new();
}

public class InfoCard : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Vector2 offset = new(16, 16);

    [Header("Text")]
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI subtitleText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private TextMeshProUGUI statsText;

    [Header("Photo")]
    [SerializeField]
    private GameObject photoRoot;

    [SerializeField]
    private Image photo;

    [SerializeField]
    private Button prevButton;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private TextMeshProUGUI counterText;

    [SerializeField]
    private TextMeshProUGUI creditText;

    [SerializeField]
    private Button closeButton;

    private List<ImageInfo> images = new();
    private int index;

    [SerializeField]
    private float openTime = 0.22f;

    private CanvasGroup group;
    private Coroutine anim;
    private Vector3 restPos;

    private void Awake()
    {
        prevButton.onClick.AddListener(() => Step(-1));
        nextButton.onClick.AddListener(() => Step(1));
        closeButton.onClick.AddListener(Hide);
        group = GetComponent<CanvasGroup>();
    }

    public void Show(Vector2 screenPoint, CardData d)
    {
        titleText.text = d.Title;
        SetText(subtitleText, d.Subtitle);
        SetText(descriptionText, d.Description);
        SetText(
            statsText,
            string.Join("\n", d.Stats.Select(s => $"<color=#999999>{s.label}</color>  {s.value}"))
        );

        images = (d.Images ?? new()).Where(i => BuildingDatabase.LoadSprite(i) != null).ToList();
        index = 0;
        ShowPhoto();

        bool alreadyOpen = gameObject.activeSelf;
        if (anim != null)
            StopCoroutine(anim);
        gameObject.SetActive(true);
        PlaceAt(screenPoint);
        restPos = transform.localPosition;

        if (alreadyOpen) // clicking another building while open: just swap content, no replay
        {
            group.alpha = 1;
            transform.localScale = Vector3.one;
            return;
        }
        anim = StartCoroutine(Animate(true));
    }

    public void Hide()
    {
        if (!gameObject.activeSelf)
            return;
        if (anim != null)
            StopCoroutine(anim);
        anim = StartCoroutine(Animate(false));
    }

    private static void SetText(TextMeshProUGUI t, string s)
    {
        t.gameObject.SetActive(!string.IsNullOrWhiteSpace(s));
        t.text = s;
    }

    private void Step(int dir)
    {
        index = (index + dir + images.Count) % images.Count;
        ShowPhoto();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void ShowPhoto()
    {
        bool has = images.Count > 0;
        photoRoot.SetActive(has);
        creditText.gameObject.SetActive(has);
        if (!has)
            return;

        var im = images[index];
        photo.sprite = BuildingDatabase.LoadSprite(im);

        bool multi = images.Count > 1;
        prevButton.gameObject.SetActive(multi);
        nextButton.gameObject.SetActive(multi);
        counterText.gameObject.SetActive(multi);
        counterText.text = $"{index + 1}/{images.Count}";

        var who = string.IsNullOrEmpty(im.credit) ? "Unknown author" : Truncate(im.credit, 40);
        var lic = string.IsNullOrEmpty(im.licenseUrl)
            ? im.license
            : $"<link=\"{im.licenseUrl}\"><u>{im.license}</u></link>";
        creditText.text = $"Photo: <link=\"{im.filePage}\"><u>{who}</u></link>, {lic}";
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private void PlaceAt(Vector2 screenPoint)
    {
        var rt = (RectTransform)transform;
        var parent = (RectTransform)rt.parent;
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            screenPoint,
            cam,
            out var p
        );

        rt.pivot = Vector2.zero;
        var size = rt.rect.size;
        var area = parent.rect;
        var pos = p + offset;

        if (pos.x + size.x > area.xMax)
            pos.x = p.x - offset.x - size.x; // flip left
        if (pos.y + size.y > area.yMax)
            pos.y = p.y - offset.y - size.y; // flip down
        pos.x = Mathf.Max(pos.x, area.xMin);
        pos.y = Mathf.Max(pos.y, area.yMin);

        rt.localPosition = pos;
    }

    private IEnumerator Animate(bool open)
    {
        var rt = (RectTransform)transform;
        var rise = new Vector3(0, -16, 0);

        float startA = open ? 0f : group.alpha;
        float startS = open ? 0.9f : rt.localScale.x;
        float endS = open ? 1f : 0.95f;
        float dur = open ? openTime : openTime * 0.6f;

        for (float t = 0; t < 1; t += Time.unscaledDeltaTime / dur)
        {
            float e = open ? EaseOutBack(t) : t * t;
            group.alpha = Mathf.Lerp(startA, open ? 1f : 0f, open ? Mathf.Min(1f, t * 1.5f) : t);
            rt.localScale = Vector3.one * Mathf.LerpUnclamped(startS, endS, e);
            if (open)
                rt.localPosition = Vector3.LerpUnclamped(restPos + rise, restPos, e);
            yield return null;
        }

        if (open)
        {
            group.alpha = 1;
            rt.localScale = Vector3.one;
            rt.localPosition = restPos;
        }
        else
        {
            rt.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f,
            c3 = c1 + 1f;
        t -= 1f;
        return 1f + c3 * t * t * t + c1 * t * t;
    }
}
