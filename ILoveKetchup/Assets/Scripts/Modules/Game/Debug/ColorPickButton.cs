using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPickButton : MonoBehaviour
{
    public UnityEvent<Color> ColorPickerEvent;
    [SerializeField] Texture2D colorChart;
    [SerializeField] GameObject chart;
    [SerializeField] RectTransform cursor;
    [SerializeField] Image button;
    [SerializeField] Image cursorColor;
    [SerializeField] private RectTransform rectColorChart;
    [SerializeField] private Camera camUI;

    public void PickColor(BaseEventData data)
    {
        PointerEventData pointer = data as PointerEventData;
        Vector3 cursorPos = camUI.ScreenToWorldPoint(pointer.position);
        cursorPos.z = 0.0f;
        cursor.position =  cursorPos;
        Vector2 pixelPos = new Vector2(colorChart.width * (cursor.anchoredPosition.x / rectColorChart.sizeDelta.x), colorChart.height * (cursor.anchoredPosition.y / rectColorChart.sizeDelta.y));
        Color pickedColor = colorChart.GetPixel((int)pixelPos.x, (int)pixelPos.y);
        Development.Log(pixelPos.x + " - " + pixelPos.y);
        button.color = pickedColor;
        cursorColor.color = pickedColor;
        ColorPickerEvent.Invoke(pickedColor);
    }
}
