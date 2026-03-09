using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class EmployeeController : MonoBehaviour
{
    [Header("데이터 연결")]
    RoomInteriorItemSO employeeData;

    [Header("상태")]
    public float currentStamina;
    // Start is called before the first frame update

    [Header("UI 연결")]
    public Slider staminaSlider;

    void Start()
    {
        if (employeeData != null)
        {
            currentStamina = employeeData.maxStamina;
        }
        else
        {
            Debug.LogError("EmployeeController: employeeData가 할당되지 않았습니다.");
        }

        UpdateStaminaUI();
    }

    public void RestoreStamina(int amount)
    {
        currentStamina += amount;
        if (currentStamina > employeeData.maxStamina)
        {
            currentStamina = employeeData.maxStamina;
        }

        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {

        if (staminaSlider != null && employeeData != null)
        {
            staminaSlider.value = currentStamina / employeeData.maxStamina;
        }
        // 스태미나 UI 업데이트 로직 구현
        Debug.Log($"스태미나 업데이트: {currentStamina}/{employeeData.maxStamina}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
