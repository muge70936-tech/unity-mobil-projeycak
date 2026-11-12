using UnityEngine;
using UnityEngine.SceneManagement;

public class WarningScreenScript : MonoBehaviour
{
	public string LoadScene;

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			SceneManager.LoadScene(LoadScene);
		}
	}
}
