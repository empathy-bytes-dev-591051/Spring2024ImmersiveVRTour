using UnityEngine;
using UnityEngine.EventSystems;

public class PanButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private MapPanner panner;

    [SerializeField]
    private Vector2 direction;

    public void OnPointerDown(PointerEventData eventData) => panner.Begin(direction);

    public void OnPointerUp(PointerEventData eventData) => panner.End();
}
