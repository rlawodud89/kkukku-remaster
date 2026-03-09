using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;

public class InteriorEditMode : MonoBehaviour, IPointerClickHandler
{
    // edit mode일 때 가구 클릭하면 편집 UI 띄우기
    public void OnPointerClick(PointerEventData eventData)
    {
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsEditMode)
        {
            
        }
    }

}