using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonArrowEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject leftArrow;   // ลูกศรซ้าย
    public GameObject rightArrow;  // ลูกศรขวา

    void Start()
    {
        // เริ่มปิดลูกศร
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }
}
