using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Fade fade;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNamedScene(string sceneName)
    {
        StartCoroutine(LoadAsyncScene(sceneName));
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        yield return StartCoroutine(fade.FadeOutCoroutine());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
