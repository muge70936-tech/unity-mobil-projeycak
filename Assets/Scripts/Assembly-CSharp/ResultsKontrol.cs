using UnityEngine;

public class ResultsKontrol : MonoBehaviour
{
	public AudioSource Ses;

	public GameObject Hud;

	public float timeToMove;

	public bool isPlayed;

	public void Update()
	{
		if (timeToMove > 2.5f)
		{
			timeToMove -= 1f * Time.deltaTime;
			return;
		}
		PlaySound();
		if (timeToMove > 1f)
		{
			timeToMove -= 1f * Time.deltaTime;
			return;
		}
		Hud.SetActive(value: true);
		if (timeToMove > -3.5f)
		{
			timeToMove -= 1f * Time.deltaTime;
			return;
		}
		Application.Quit();
		Debug.Log("Çıktı");
	}

	public void PlaySound()
	{
		if (!isPlayed)
		{
			Ses.Play();
		}
		isPlayed = true;
	}
}
