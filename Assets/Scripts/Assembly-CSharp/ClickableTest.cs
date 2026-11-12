using UnityEngine;

public class ClickableTest : MonoBehaviour
{
	private Transform player;

	public AudioSource test;

	public AudioClip testHit;

	private void Start()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
		if (gameObject != null)
		{
			player = gameObject.transform;
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo) && ((hitInfo.transform.tag == "Notebook") & (Vector3.Distance(player.position, base.transform.position) < 10f)))
		{
			test.PlayOneShot(testHit);
			base.gameObject.SetActive(value: false);
		}
	}
}
