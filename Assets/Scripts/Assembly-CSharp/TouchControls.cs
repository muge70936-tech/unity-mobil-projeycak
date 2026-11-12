using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Mobile touch controls for Kola Cikolata. Mobile port by Prime MT.
// Builds its own UI at runtime, so no scene edits are needed.
// It feeds the game's own InputManager (InputManager.TouchHeld) plus a few static values
// (MoveVector, Running, ConsumeLook) that PlayerScript / PlayerMovement read.
[DefaultExecutionOrder(-200)]
public class TouchControls : MonoBehaviour
{
	// ---- values read by the game scripts ----
	public static Vector2 MoveVector;
	public static bool Running;
	private static float lookDegrees;
	private static bool submitFlag;
	private static bool yesFlag;
	private static bool noFlag;

	public static float ConsumeLook()
	{
		float result = lookDegrees;
		lookDegrees = 0f;
		return result;
	}

	public static bool ConsumeSubmit()
	{
		return submitFlag;
	}

	public static bool ConsumeYes()
	{
		return yesFlag;
	}

	public static bool ConsumeNo()
	{
		return noFlag;
	}

	// ---- layout (units of screen height) ----
	public static float LookDegreesPerPixel = 0.12f;
	private const float Margin = 0.05f;
	private const float JoyDiameter = 0.36f;
	private const float ItemDiameter = 0.26f;
	private const float RunDiameter = 0.22f;
	private const float TurnDiameter = 0.20f;
	private const float SmallDiameter = 0.13f;

	private class Btn
	{
		public Image frame;
		public Image icon;
		public Text label;
		public Vector2 center;
		public float radius;
		public int finger = -1;
		public bool visible;
	}

	private static TouchControls instance;

	private Canvas canvas;
	private Sprite sprKnob, sprFrame, sprItem, sprTurn, sprRun;
	private Font font;

	private Image joyFrame, joyKnob;
	private Vector2 joyCenter;
	private float joyRadius;
	private int joyFinger = -1;
	private int lookFinger = -1;

	private Btn btnItem, btnRun, btnTurn, btnSlot, btnPause, btnOk, btnYes, btnNo;
	private List<Btn> allButtons = new List<Btn>();

	private int lastW, lastH;
	private float findTimer;
	private GameControllerScript gc;
	private PlayerScript ps;
	private PlayerMovement pm;
	private float pulseEscape, pulseUse;
	private bool itemInteract;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Boot()
	{
		if (instance != null || !Application.isMobilePlatform)
		{
			return;
		}
		GameObject root = new GameObject("TouchControls");
		Object.DontDestroyOnLoad(root);
		root.AddComponent<TouchControls>();
	}

	private Sprite LoadSprite(string name)
	{
		TextAsset textAsset = Resources.Load<TextAsset>("TouchUI/" + name);
		if (textAsset == null)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		texture2D.LoadImage(textAsset.bytes);
		texture2D.filterMode = FilterMode.Bilinear;
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
	}

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		sprKnob = LoadSprite("knob");
		sprFrame = LoadSprite("frame");
		sprItem = LoadSprite("item");
		sprTurn = LoadSprite("turn");
		sprRun = LoadSprite("run");
		font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		GameObject canvasGo = new GameObject("TouchCanvas");
		canvasGo.transform.SetParent(base.transform, false);
		canvas = canvasGo.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 500;

		joyFrame = MakeImage("JoyFrame", sprFrame, canvasGo.transform);
		joyKnob = MakeImage("JoyKnob", sprKnob, canvasGo.transform);

		btnItem = MakeButton("Item", sprItem, null, canvasGo.transform);
		btnRun = MakeButton("Run", sprRun, null, canvasGo.transform);
		btnTurn = MakeButton("Turn", sprTurn, null, canvasGo.transform);
		btnSlot = MakeButton("Slot", null, ">>", canvasGo.transform);
		btnPause = MakeButton("Pause", null, "II", canvasGo.transform);
		btnOk = MakeButton("Ok", null, "OK", canvasGo.transform);
		btnYes = MakeButton("Yes", null, "Y", canvasGo.transform);
		btnNo = MakeButton("No", null, "N", canvasGo.transform);
		SetAllVisible(false);
	}

	private Image MakeImage(string name, Sprite sprite, Transform parent)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
		gameObject.transform.SetParent(parent, false);
		Image component = gameObject.GetComponent<Image>();
		component.sprite = sprite;
		component.raycastTarget = false;
		component.preserveAspect = true;
		component.color = new Color(1f, 1f, 1f, 0.85f);
		RectTransform rectTransform = component.rectTransform;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.zero;
		return component;
	}

	private Btn MakeButton(string name, Sprite iconSprite, string text, Transform parent)
	{
		Btn btn = new Btn();
		btn.frame = MakeImage(name + "Frame", sprFrame, parent);
		if (iconSprite != null)
		{
			btn.icon = MakeImage(name + "Icon", iconSprite, parent);
		}
		if (text != null)
		{
			GameObject gameObject = new GameObject(name + "Label", typeof(RectTransform), typeof(Text), typeof(Outline));
			gameObject.transform.SetParent(parent, false);
			Text component = gameObject.GetComponent<Text>();
			component.font = font;
			component.text = text;
			component.alignment = TextAnchor.MiddleCenter;
			component.color = Color.white;
			component.raycastTarget = false;
			component.resizeTextForBestFit = true;
			component.resizeTextMinSize = 10;
			component.resizeTextMaxSize = 200;
			RectTransform rectTransform = component.rectTransform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.zero;
			btn.label = component;
		}
		allButtons.Add(btn);
		return btn;
	}

	private void Place(Btn b, Vector2 center, float diameter)
	{
		b.center = center;
		b.radius = diameter * 0.5f;
		b.frame.rectTransform.sizeDelta = new Vector2(diameter, diameter);
		b.frame.rectTransform.anchoredPosition = center;
		if (b.icon != null)
		{
			b.icon.rectTransform.sizeDelta = new Vector2(diameter * 0.8f, diameter * 0.8f);
			b.icon.rectTransform.anchoredPosition = center;
		}
		if (b.label != null)
		{
			b.label.rectTransform.sizeDelta = new Vector2(diameter * 0.6f, diameter * 0.6f);
			b.label.rectTransform.anchoredPosition = center;
		}
	}

	private void Layout()
	{
		lastW = Screen.width;
		lastH = Screen.height;
		float w = lastW;
		float h = lastH;
		float joyD = JoyDiameter * h;
		joyRadius = joyD * 0.5f;
		joyCenter = new Vector2(Margin * h + joyRadius + 0.04f * h, Margin * h + joyRadius);
		joyFrame.rectTransform.sizeDelta = new Vector2(joyD, joyD);
		joyFrame.rectTransform.anchoredPosition = joyCenter;
		joyKnob.rectTransform.sizeDelta = new Vector2(joyD * 0.5f, joyD * 0.5f);
		joyKnob.rectTransform.anchoredPosition = joyCenter;

		Place(btnItem, new Vector2(w - (Margin + ItemDiameter * 0.5f) * h, (Margin + ItemDiameter * 0.5f + 0.02f) * h), ItemDiameter * h);
		Place(btnRun, new Vector2(w - (Margin + ItemDiameter + 0.04f + RunDiameter * 0.5f) * h, (Margin + RunDiameter * 0.5f) * h), RunDiameter * h);
		Place(btnTurn, new Vector2(w - (Margin + TurnDiameter * 0.5f + 0.03f) * h, (Margin + ItemDiameter + 0.06f + TurnDiameter * 0.5f) * h), TurnDiameter * h);
		Place(btnSlot, new Vector2(w - (Margin + ItemDiameter + 0.06f + SmallDiameter * 0.5f) * h, (Margin + RunDiameter + 0.08f + SmallDiameter * 0.5f) * h), SmallDiameter * h);
		Place(btnPause, new Vector2(w - (0.04f + SmallDiameter * 0.5f) * h, h - (0.04f + SmallDiameter * 0.5f) * h), SmallDiameter * h);
		Place(btnOk, new Vector2(w * 0.5f, h * 0.5f), 0.22f * h);
		Place(btnYes, new Vector2(w * 0.5f - 0.16f * h, h * 0.5f), 0.2f * h);
		Place(btnNo, new Vector2(w * 0.5f + 0.16f * h, h * 0.5f), 0.2f * h);
	}

	private void SetVisible(Btn b, bool value)
	{
		b.visible = value;
		b.frame.enabled = value;
		if (b.icon != null)
		{
			b.icon.enabled = value;
		}
		if (b.label != null)
		{
			b.label.enabled = value;
		}
		if (!value)
		{
			b.finger = -1;
		}
	}

	private void SetAllVisible(bool value)
	{
		foreach (Btn b in allButtons)
		{
			SetVisible(b, value);
		}
		joyFrame.enabled = value;
		joyKnob.enabled = value;
		if (!value)
		{
			joyFinger = -1;
			lookFinger = -1;
			MoveVector = Vector2.zero;
			Running = false;
		}
	}

	private void FindGame()
	{
		findTimer -= Time.unscaledDeltaTime;
		if (findTimer > 0f && (gc != null || ps != null || pm != null))
		{
			return;
		}
		if (findTimer > 0f)
		{
			return;
		}
		findTimer = 0.5f;
		gc = Object.FindObjectOfType<GameControllerScript>();
		ps = Object.FindObjectOfType<PlayerScript>();
		pm = Object.FindObjectOfType<PlayerMovement>();
	}

	private bool LooksInteractive()
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return false;
		}
		Ray ray = main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
		if (!Physics.Raycast(ray, out var hitInfo, 12f))
		{
			return false;
		}
		string text = hitInfo.collider.name;
		string text2 = hitInfo.collider.tag;
		string text3 = hitInfo.transform.name;
		string text4 = hitInfo.transform.tag;
		if (text2 == "Door" || text2 == "SwingingDoor" || text2 == "Notebook" || text4 == "Notebook")
		{
			return true;
		}
		if (text.StartsWith("Pickup_") || text3.StartsWith("Pickup_"))
		{
			return true;
		}
		return text == "TapePlayer" || text == "BSODAMachine" || text == "ZestyMachine" || text == "PayPhone";
	}

	private bool Hit(Btn b, Vector2 pos)
	{
		return b.visible && (pos - b.center).magnitude <= b.radius * 1.15f;
	}

	private void Update()
	{
		if (Screen.width != lastW || Screen.height != lastH)
		{
			Layout();
		}
		FindGame();

		bool hasGame = ps != null || pm != null;
		bool gameOver = ps != null && ps.gameOver;
		bool learning = gc != null && gc.learningActive;
		bool paused = gc != null && gc.gamePaused;
		bool full = hasGame && !gameOver && !learning && !paused;

		// show only what makes sense right now
		joyFrame.enabled = full;
		joyKnob.enabled = full;
		SetVisible(btnItem, full);
		SetVisible(btnRun, full);
		SetVisible(btnTurn, full);
		SetVisible(btnSlot, full && gc != null);
		SetVisible(btnPause, hasGame && !gameOver && !learning && gc != null);
		SetVisible(btnOk, hasGame && learning && !gameOver);
		SetVisible(btnYes, hasGame && paused && !gameOver);
		SetVisible(btnNo, hasGame && paused && !gameOver);
		if (!full)
		{
			joyFinger = -1;
			lookFinger = -1;
			MoveVector = Vector2.zero;
			Running = false;
			joyKnob.rectTransform.anchoredPosition = joyCenter;
		}

		// while playing, touches must not act as mouse clicks (that would "interact" every time you touch)
		Input.simulateMouseWithTouches = !full;

		HandleTouches(full);
		UpdateHeldKeys(full);
	}

	private void HandleTouches(bool full)
	{
		if (Input.touchCount == 0)
		{
			joyFinger = -1;
			lookFinger = -1;
			foreach (Btn b in allButtons)
			{
				b.finger = -1;
			}
			if (!full)
			{
				return;
			}
		}
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			int id = touch.fingerId;
			Vector2 pos = touch.position;
			if (touch.phase == TouchPhase.Began)
			{
				if (full && joyFinger < 0 && (pos - joyCenter).magnitude <= joyRadius * 1.4f)
				{
					joyFinger = id;
				}
				else
				{
					Btn pressed = null;
					foreach (Btn b2 in allButtons)
					{
						if (b2.finger < 0 && Hit(b2, pos))
						{
							pressed = b2;
							break;
						}
					}
					if (pressed != null)
					{
						pressed.finger = id;
						OnPress(pressed);
					}
					else if (full && lookFinger < 0 && pos.x > Screen.width * 0.30f)
					{
						lookFinger = id;
					}
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				if (id == joyFinger)
				{
					joyFinger = -1;
				}
				if (id == lookFinger)
				{
					lookFinger = -1;
				}
				foreach (Btn b3 in allButtons)
				{
					if (b3.finger == id)
					{
						b3.finger = -1;
					}
				}
			}
			else
			{
				if (id == lookFinger && touch.phase == TouchPhase.Moved)
				{
					lookDegrees += touch.deltaPosition.x * LookDegreesPerPixel;
				}
			}
			if (id == joyFinger && full)
			{
				Vector2 offset = (pos - joyCenter) / joyRadius;
				if (offset.magnitude > 1f)
				{
					offset = offset.normalized;
				}
				joyKnob.rectTransform.anchoredPosition = joyCenter + offset * joyRadius * 0.6f;
				MoveVector = (offset.magnitude < 0.12f) ? Vector2.zero : offset;
			}
		}
		if (joyFinger < 0)
		{
			MoveVector = Vector2.zero;
			if (joyKnob != null)
			{
				joyKnob.rectTransform.anchoredPosition = joyCenter;
			}
		}
	}

	private void OnPress(Btn b)
	{
		if (b == btnItem)
		{
			itemInteract = LooksInteractive();
			if (!itemInteract && !(ps != null && ps.jumpRope))
			{
				pulseUse = 0.12f;
			}
		}
		else if (b == btnSlot)
		{
			if (gc != null)
			{
				gc.IncreaseItemSelection();
			}
		}
		else if (b == btnPause)
		{
			pulseEscape = 0.12f;
		}
		else if (b == btnOk)
		{
			submitFlag = true;
		}
		else if (b == btnYes)
		{
			yesFlag = true;
		}
		else if (b == btnNo)
		{
			noFlag = true;
		}
	}

	private void UpdateHeldKeys(bool full)
	{
		HashSet<KeyCode> held = InputManager.TouchHeld;
		held.Clear();
		Running = false;
		if (full)
		{
			if (btnRun.finger >= 0)
			{
				Running = true;
				held.Add(KeyCode.LeftShift);
			}
			if (btnTurn.finger >= 0)
			{
				held.Add(KeyCode.Space);
			}
			if (btnItem.finger >= 0)
			{
				if (ps != null && ps.jumpRope)
				{
					held.Add(KeyCode.Space);
				}
				else if (itemInteract)
				{
					held.Add(KeyCode.Mouse0);
				}
			}
			if (pulseUse > 0f)
			{
				held.Add(KeyCode.Mouse1);
				pulseUse -= Time.unscaledDeltaTime;
			}
		}
		if (pulseEscape > 0f)
		{
			held.Add(KeyCode.Escape);
			pulseEscape -= Time.unscaledDeltaTime;
		}
	}

	private void LateUpdate()
	{
		submitFlag = false;
		yesFlag = false;
		noFlag = false;
	}
}
