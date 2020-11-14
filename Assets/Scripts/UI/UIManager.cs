using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Canvas[] canvasArray;
	// Singleton instance.
	public static UIManager Instance = null;

	// Initialize the singleton instance.
	private void Awake()
	{
		// If there is not already an instance of SoundManager, set it to this.
		if (Instance == null)
		{
			Instance = this;
		}
		//If an instance already exists, destroy whatever this object is to enforce the singleton.
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad(gameObject);
		canvasArray[0].enabled = true;
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
