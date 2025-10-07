using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject winCanvas; // ����� Canvas ����� Text + Button

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (winCanvas != null)
            winCanvas.SetActive(false); // �Դ��͹�������
    }

    public void ShowWinUI()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(true); // �Դ Canvas
        }
        Time.timeScale = 0f; // ��ش��
    }

    public void OnPlayAgain()
    {
        Time.timeScale = 1f; // ��������
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ��Ŵ Scene �Ѩ�غѹ
    }
}
