using UnityEngine;

public class PulsingScript : MonoBehaviour
{
	public Camera playerCamera;

	public GameControllerScript gc;

	public BaldiScript baldi;

	public bool isPulsing;

	[SerializeField]
	private float maxCooldown = 0.54f;

	private float cooldown;

	public ZoomBlurEffect zoomBlur;

	public float targetFOV = 120f;

	public float defaultFOV = 120f;

	public float pulseFOV = 110f;

	public float startFOV;

	private void Start()
	{
		if (playerCamera != null)
		{
			startFOV = playerCamera.fieldOfView;
		}
		cooldown = maxCooldown;
	}

	private void Update()
	{
		if (isPulsing)
		{
			zoomBlur.enabled = true;
		}
		else
		{
			zoomBlur.enabled = false;
		}
		if (baldi == null || playerCamera == null)
		{
			return;
		}
		if (gc != null && gc.GCChase)
		{
			isPulsing = true;
		}
		else
		{
			isPulsing = false;
			targetFOV = startFOV;
		}
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
		}
		if (isPulsing && cooldown <= 0f)
		{
			playerCamera.fieldOfView = pulseFOV;
			cooldown = maxCooldown;
		}
		float num = (isPulsing ? defaultFOV : startFOV);
		if (playerCamera.fieldOfView != num)
		{
			playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, num, 10f * Time.deltaTime);
			if (Mathf.Abs(num - playerCamera.fieldOfView) < 0.05f)
			{
				playerCamera.fieldOfView = num;
			}
		}
	}
}
