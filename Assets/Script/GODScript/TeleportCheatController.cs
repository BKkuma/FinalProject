using UnityEngine;

public class TeleportCheatController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลาก GameObject ของผู้เล่นมาใส่")]
    public Transform playerTransform;

    [Header("Cheat Settings")]
    [Tooltip("ปุ่มที่ใช้เพื่อเปิดใช้งานคำสั่งโกง (เช่น O)")]
    public KeyCode toggleKey = KeyCode.O; // ใช้ 'O' ตามที่เห็นใน Log

    [Tooltip("ปุ่มที่ใช้ยืนยันการ Teleport")]
    public KeyCode confirmKey = KeyCode.Return; // ปุ่ม Enter

    public float fixedYPosition = 0f;
    public float fixedZPosition = 0f;

    private string inputX = "";
    private bool isInputActive = false;

    void Update()
    {
        // 1. ตรวจสอบว่าผู้เล่นกดปุ่มเปิด/ปิด
        if (Input.GetKeyDown(toggleKey))
        {
            isInputActive = !isInputActive;
            inputX = "";

            if (isInputActive)
            {
                Debug.Log($"<color=yellow>[CHEAT] Teleport Input ON. Press {toggleKey} again to cancel. Current X: '{inputX}'</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>[CHEAT] Teleport Input OFF.</color>");
            }
        }

        // 2. ถ้ากำลังรอรับ Input
        if (isInputActive)
        {
            HandleTextAndConfirmInput();
        }
    }

    void HandleTextAndConfirmInput()
    {
        // ใช้ Input.anyKeyDown สำหรับดักจับทุกคีย์
        if (Input.anyKeyDown)
        {
            // ดักจับตัวอักษรที่ผู้ใช้พิมพ์
            string inputChar = Input.inputString;

            foreach (char c in inputChar)
            {
                // ตรวจสอบว่าเป็นตัวเลข, เครื่องหมายลบ, หรือจุดทศนิยม
                if (char.IsDigit(c) || c == '-' || c == '.')
                {
                    // เงื่อนไขสำหรับเครื่องหมายลบ: ต้องอยู่ต้นสุดเท่านั้น
                    if (c == '-')
                    {
                        if (inputX.Length == 0 && !inputX.Contains("-"))
                        {
                            inputX += c;
                        }
                    }
                    // เงื่อนไขสำหรับจุดทศนิยม: ต้องมีแค่จุดเดียวเท่านั้น
                    else if (c == '.')
                    {
                        if (!inputX.Contains("."))
                        {
                            inputX += c;
                        }
                    }
                    // ตัวเลข
                    else
                    {
                        inputX += c;
                    }

                    // แสดงผลทันทีที่รับ Input
                    Debug.Log($"[CHEAT] Input received: '{c}'. Current X: '{inputX}'");
                }
            }

            // ดักจับ Enter/Return
            if (Input.GetKeyDown(confirmKey))
            {
                ExecuteTeleport();
                isInputActive = false;
                return; // ออกจากฟังก์ชันหลังจาก Execute
            }

            // ดักจับ Backspace
            if (Input.GetKeyDown(KeyCode.Backspace) && inputX.Length > 0)
            {
                inputX = inputX.Substring(0, inputX.Length - 1);
                Debug.Log($"[CHEAT] Backspace. Current X: '{inputX}'");
            }
        }
    }

    void ExecuteTeleport()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[CHEAT ERROR] Player Transform is not assigned!");
            inputX = "";
            return;
        }

        if (float.TryParse(inputX, out float targetX))
        {
            Vector3 newPosition = new Vector3(targetX, fixedYPosition, fixedZPosition);
            playerTransform.position = newPosition;

            // อัปเดตกล้องทันที
            CameraFollowLockY cameraFollow = FindObjectOfType<CameraFollowLockY>();
            if (cameraFollow != null)
            {
                cameraFollow.TeleportToTarget(playerTransform.position);
            }

            Debug.Log($"<color=green>[CHEAT SUCCESS] Teleported Player to X: {targetX} (Y: {fixedYPosition})</color>");
        }
        else
        {
            // แสดงผลเมื่อ inputX เป็นค่าว่างหรือรูปแบบไม่ถูกต้อง
            Debug.LogWarning($"<color=red>[CHEAT FAILED] Invalid X input: '{inputX}' (Input must be a valid number, e.g., 100 or -50.5)</color>");
        }

        inputX = ""; // เคลียร์ Input
    }
}