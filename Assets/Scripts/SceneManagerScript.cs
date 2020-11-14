using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public float startTimer = 10.0f;
    private SceneManager sm = null;
    public string scenePath;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Countdown());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Countdown()
    {
        float counter = startTimer;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }
        LoadMenuScene();
    }

    void LoadMenuScene()
    {
        SceneManager.LoadScene(1);
    }
}
