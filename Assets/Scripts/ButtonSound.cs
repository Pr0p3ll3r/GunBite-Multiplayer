using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    private void Start()
    {
        Button[] buttons = gameObject.GetComponentsInChildren<Button>(true);

        foreach(Button b in buttons)
        {
            b.onClick.AddListener(delegate { SoundManager.Instance.PlayOneShot("Click"); });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject != eventData.pointerCurrentRaycast.gameObject)
            SoundManager.Instance.PlayOneShot("Hover");
    }
}
