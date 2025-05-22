using UnityEngine;
using UnityEngine.EventSystems;

namespace HeroicEngine.Components
{
    public class Draggable2D : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            transform.position += new Vector3(eventData.delta.x, eventData.delta.y);
        }
    }
}