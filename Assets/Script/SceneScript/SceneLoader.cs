using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นกดปุ่ม
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is empty! Please check the Button's OnClick settings.");
            return;
        }

        // โหลดฉากตามชื่อที่ระบุ (เช่น "Scenes/MenuScene" หรือ "MainMenu")
        // ⭐ หมายเหตุ: จากรูปของคุณ ชื่อฉากเมนูคือ Scenes/MenuScene
        // SceneManager.LoadScene(sceneName); 

        // 💡 แนะนำ: ถ้าคุณต้องการให้มี Loading Screen ก่อน อาจจะใช้ LoadSceneAsync
        // แต่สำหรับตอนนี้เราใช้ LoadScene ปกติไปก่อน

        Debug.Log("Loading Scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // ฟังก์ชันนี้สำหรับปุ่ม Quit Game (เฉพาะ PC, Mac)
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}