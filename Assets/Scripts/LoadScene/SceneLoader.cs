using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName; // Tên scene muốn chuyển đến
    [SerializeField] private float transitionDuration = 0.5f; // Thời gian chuyển cảnh
    [SerializeField] private bool useCutscene = true; // Có sử dụng cutscene không

    public void LoadScene()
    {
        if (useCutscene && CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(targetSceneName, transitionDuration);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // Sử dụng cho OnMouseDown
    private void OnMouseDown()
    {
        LoadScene();
    }

    // Sử dụng cho Button onClick
    public void OnButtonClick()
    {
        LoadScene();
    }
} 