using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private MapPanner panner;

    [SerializeField]
    private float zoomFactor;

    public void OnPointerDown(PointerEventData eventData) => panner.BeginZoom(zoomFactor);

    public void OnPointerUp(PointerEventData eventData) => panner.EndZoom();
}
