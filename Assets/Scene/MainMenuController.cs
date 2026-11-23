using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // ฟังก์ชันสำหรับปุ่มเริ่มเกม
    public void StartGame()
    {
        SceneManager.LoadScene("BossRoom");
    }

    // ฟังก์ชันสำหรับปุ่มออกเกม
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("ออกจากเกมแล้ว"); // แสดงผลตอนทดสอบใน Unity Editor
    }
}