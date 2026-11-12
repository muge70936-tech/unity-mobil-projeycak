using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTriggerScript : MonoBehaviour
{
	public GameControllerScript gc;

	public string ResultsScene;

	public string SecretScene;

	private void OnTriggerEnter(Collider other)
	{
		if ((gc.notebooks >= 7) & (other.tag == "Player"))
		{
			if (gc.failedNotebooks >= 7)
			{
				SceneManager.LoadScene(SecretScene);
			}
			else
			{
				SceneManager.LoadScene(ResultsScene);
			}
		}
	}
}
