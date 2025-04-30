using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {
    public void LoadHome() {
        SceneManager.LoadScene("Home");
    }
}