using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {
    public void LoadHome() {
         CutsceneManager.Instance.PlayCutscene("Home", 0.5f);
        //SceneManager.LoadScene("Home");

    }
}