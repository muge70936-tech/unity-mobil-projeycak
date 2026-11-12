using System.Collections;
using TMPro;
using UnityEngine;

public class TextFadeOut : MonoBehaviour
{
	public TMP_Text text;

	public float fadeDuration = 2f;

	private Coroutine routine;

	private void Awake()
	{
		if (text == null)
		{
			text = GetComponent<TMP_Text>();
		}
	}

	private void OnEnable()
	{
		Play();
	}

	public void Play()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
		routine = StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		text.alpha = 1f;
		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			text.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
			yield return null;
		}
		text.alpha = 0f;
		routine = null;
	}
}
