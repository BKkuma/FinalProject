using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;

    [Header("Key Icons")]
    public Image keyW;
    public Image keyA;
    public Image keyS;
    public Image keyD;
    public Image keyShift;
    public Image keyJump;   // ปุ่ม J
    public Image keyShoot;  // ปุ่ม K (ยิง)

    [Header("Weapon Switch Icons")]
    public Image key1;
    public Image key2;
    public Image key3;

    [Header("Texts")]
    public Text moveText;
    public Text aimText;
    public Text crouchText;
    public Text jumpText;
    public Text shootText;
    public Text weaponText;

    

    private bool isOpen = false;

    void Start()
    {
        OpenTutorial();
        SetupTutorialText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isOpen)
                CloseTutorial();
            else
                OpenTutorial();
        }
    }

    public void OpenTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        Time.timeScale = 0f;
        isOpen = true;
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Time.timeScale = 1f;
        isOpen = false;

        // เรียก Spawn / Landing Player หลังปิด Tutorial
        
    }

    void SetupTutorialText()
    {
        if (moveText != null)
            moveText.text = "Walk :   /  ";

        if (aimText != null)
            aimText.text = "Aim :  (AimUp)/  (AimDown)";

        if (crouchText != null)
            crouchText.text = "Crouch : ";

        if (jumpText != null)
            jumpText.text = "Jump :  ";

        if (shootText != null)
            shootText.text = "Shoot : ";

        if (weaponText != null)
            weaponText.text = "Change weapon : /  /  ";
    }
}
