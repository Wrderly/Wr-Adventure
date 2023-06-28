using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainInterfaceScript : MonoBehaviour
{
    public Button start, quit;
    public GameObject CutImage;
    private bool isStartGame = false;
    // Start is called before the first frame update
    void Start()
    {
        start.onClick.AddListener(StartGame);
        quit.onClick.AddListener(QuitGame);
        StartCoroutine(LoadSceneAsync("GameScene"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartGame()
    {
        CutImage.GetComponent<CutScript>().CutFadeIn();
        StartCoroutine(WaitForCut(1f));
    }

    private IEnumerator WaitForCut(float time)
    {
        yield return new WaitForSeconds(time);
        isStartGame = true;
    }

    private IEnumerator LoadSceneAsync(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        while (!asyncLoad.isDone)
        {
            asyncLoad.allowSceneActivation = false;
            float progress = asyncLoad.progress;
            if (progress < 0.899)
            {
                Debug.Log("Loading progress: " + progress);
            }
            if (progress >= 0.9f)
            {
                if (isStartGame)
                {
                    isStartGame = false;
                    asyncLoad.allowSceneActivation = true;
                }
            }

            // Yield until the next frame
            yield return null;
        }
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
