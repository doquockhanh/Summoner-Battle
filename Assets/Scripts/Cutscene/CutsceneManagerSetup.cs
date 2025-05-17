using UnityEngine;
using UnityEngine.UI;

public class CutsceneManagerSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(3, 10)]
    public string setupInstructions = 
        "1. Tạo một Canvas mới trong scene\n" +
        "2. Thêm một Image component vào Canvas và đặt tên là 'FadePanel'\n" +
        "3. Đặt FadePanel để phủ toàn bộ màn hình\n" +
        "4. Kéo FadePanel vào trường 'Fade Panel' của CutsceneManager\n" +
        "5. Điều chỉnh Fade Duration và Fade Color theo ý muốn\n\n" +
        "Để sử dụng cutscene, gọi:\n" +
        "CutsceneManager.Instance.PlayCutscene(\"TênScene\", thờiGianCutscene);";

    private void Start()
    {
        Debug.Log(setupInstructions);
    }
} 