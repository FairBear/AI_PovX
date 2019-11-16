using UnityEngine;
using Manager;
using BepInEx;
using BepInEx.Harmony;
using BepInEx.Configuration;

namespace AI_PovX
{
	[BepInPlugin(GUID, Name, Version)]
	public partial class AI_PovX : BaseUnityPlugin
	{
		const string GUID = "com.2155x.fairbair.ai_povx";
		const string Name = "AI PoV X";
		const string Version = "1.0.1";

		const string SECTION_GENERAL = "General";
		const string SECTION_CAMERA = "Camera";
		const string SECTION_ANIMATION = "Animation";
		const string SECTION_HOTKEYS = "Hotkeys";

		const string DESCRIPTION_REVEAL_ALL =
			"Should all the girls (including the merchant) be visible at all times? " +
			"Also applies outside of PoV mode. " +
			"Does not apply during housing mode.";
		const string DESCRIPTION_H_SCENE_LOCK_CURSOR =
			"Should the cursor be locked during H scenes? " +
			"Use the 'Cursor Release Key' to reveal the cursor. " +
			"This also include situations where the focus is not the player.";

		const string DESCRIPTION_OFFSET_X =
			"Sideway offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Y =
			"Vertical offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Z =
			"Forward offset from the character's eyes.";
		const string DESCRIPTION_CAMERA_MIN_X =
			"Highest downward and leftward angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_MAX_X =
			"Highest upward and rightware angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_SPAN_Y =
			"How far can the camera be rotated horizontally? " +
			"Only applies during scenes where the player character can't move.";

		const string DESCRIPTION_ROTATE_HEAD =
			"Should the head rotate first before turning the whole body? " +
			"Only applies with free roam camera.";
		const string DESCRIPTION_NECK_MIN =
			"Highest downward angle the head can rotate.";
		const string DESCRIPTION_NECK_MAX =
			"Highest upward angle the head can rotate.";
		const string DESCRIPTION_HEAD_MAX =
			"The farthest the head can rotate until the body would rotate. " +
			"Only applies with free roam camera and the player isn't moving.";

		const string DESCRIPTION_CHARA_CYCLE_KEY =
			"Switch between characters during PoV mode. " +
			"Only applies during H scene.";
		const string DESCRIPTION_CAMERA_DRAG_KEY =
			"During PoV mode, holding down this key will move the camera if the mouse isn't locked.";
		const string DESCRIPTION_CURSOR_RELEASE_KEY =
			"Pressing this key will force the cursor to be revealed in any scenes. " +
			"Press the key again to turn off.";
		const string DESCRIPTION_HIDE_HEAD =
			"Should the head be invisible when in PoV mode? " +
			"Head is always invisible during H scenes or " +
			"situations where the player can't move.";

		internal static ConfigEntry<bool> HideHead { get; set; }
		internal static ConfigEntry<bool> RevealAll { get; set; }
		internal static ConfigEntry<bool> HSceneLockCursor { get; set; }

		internal static ConfigEntry<float> Sensitivity { get; set; }
		internal static ConfigEntry<float> Fov { get; set; }
		internal static ConfigEntry<float> ZoomFov { get; set; }
		internal static ConfigEntry<float> OffsetX { get; set; }
		internal static ConfigEntry<float> OffsetY { get; set; }
		internal static ConfigEntry<float> OffsetZ { get; set; }
		internal static ConfigEntry<float> CameraMinX { get; set; }
		internal static ConfigEntry<float> CameraMaxX { get; set; }
		internal static ConfigEntry<float> CameraSpanY { get; set; }

		internal static ConfigEntry<bool> RotateHead { get; set; }
		internal static ConfigEntry<float> NeckMin { get; set; }
		internal static ConfigEntry<float> NeckMax { get; set; }
		internal static ConfigEntry<float> HeadMax { get; set; }

		internal static ConfigEntry<KeyboardShortcut> PovKey { get; set; }
		internal static ConfigEntry<KeyboardShortcut> CharaCycleKey { get; set; }
		internal static ConfigEntry<KeyboardShortcut> CameraDragKey { get; set; }
		internal static ConfigEntry<KeyboardShortcut> CursorReleaseKey { get; set; }
		internal static ConfigEntry<KeyboardShortcut> ZoomKey { get; set; }

		private void Awake()
		{
			HideHead = Config.AddSetting(SECTION_GENERAL, "Hide Head", false, DESCRIPTION_HIDE_HEAD);
			RevealAll = Config.AddSetting(SECTION_GENERAL, "Reveal All Girls", true, DESCRIPTION_REVEAL_ALL);
			HSceneLockCursor = Config.AddSetting(SECTION_GENERAL, "Lock Cursor During H Scenes", false, DESCRIPTION_H_SCENE_LOCK_CURSOR);

			Sensitivity = Config.AddSetting(SECTION_CAMERA, "Camera Sensitivity", 2f);
			Fov = Config.AddSetting(SECTION_CAMERA, "Field of View", 35f);
			ZoomFov = Config.AddSetting(SECTION_CAMERA, "Zoom Field of View", 5f);
			OffsetX = Config.AddSetting(SECTION_CAMERA, "Offset X", 0f, DESCRIPTION_OFFSET_X);
			OffsetY = Config.AddSetting(SECTION_CAMERA, "Offset Y", 0f, DESCRIPTION_OFFSET_Y);
			OffsetZ = Config.AddSetting(SECTION_CAMERA, "Offset Z", 0f, DESCRIPTION_OFFSET_Z);
			CameraMinX = Config.AddSetting(SECTION_CAMERA, "Min Camera Angle X", 80f, DESCRIPTION_CAMERA_MIN_X);
			CameraMaxX = Config.AddSetting(SECTION_CAMERA, "Max Camera Angle X", 80f, DESCRIPTION_CAMERA_MAX_X);
			CameraSpanY = Config.AddSetting(SECTION_CAMERA, "Camera Angle Span Y", 90f, DESCRIPTION_CAMERA_SPAN_Y);

			RotateHead = Config.AddSetting(SECTION_ANIMATION, "Rotate Head", true, DESCRIPTION_ROTATE_HEAD);
			NeckMin = Config.AddSetting(SECTION_ANIMATION, "Min Neck Angle X", 0f, DESCRIPTION_NECK_MIN);
			NeckMax = Config.AddSetting(SECTION_ANIMATION, "Max Neck Angle X", 90f, DESCRIPTION_NECK_MAX);
			HeadMax = Config.AddSetting(SECTION_ANIMATION, "Max Head Angle Y", 60f, DESCRIPTION_HEAD_MAX);

			PovKey = Config.AddSetting(SECTION_HOTKEYS, "PoV Toggle Key", new KeyboardShortcut(KeyCode.Comma));
			CharaCycleKey = Config.AddSetting(SECTION_HOTKEYS, "Character Cycle Key", new KeyboardShortcut(KeyCode.Period), DESCRIPTION_CHARA_CYCLE_KEY);
			CameraDragKey = Config.AddSetting(SECTION_HOTKEYS, "Camera Drag Key", new KeyboardShortcut(KeyCode.Mouse0), DESCRIPTION_CAMERA_DRAG_KEY);
			CursorReleaseKey = Config.AddSetting(SECTION_HOTKEYS, "Cursor Release Key", new KeyboardShortcut(KeyCode.LeftControl), DESCRIPTION_CURSOR_RELEASE_KEY);
			ZoomKey = Config.AddSetting(SECTION_HOTKEYS, "Zoom Key", new KeyboardShortcut(KeyCode.Z));

			HideHead.SettingChanged += (sender, args) =>
			{
				Controller.SetChaControl(Controller.FromFocus());
			};

			HSceneLockCursor.SettingChanged += (sender, args) =>
			{
				if (Controller.toggled && !HSceneLockCursor.Value)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			};

			HarmonyWrapper.PatchAll(typeof(AI_PovX));
		}

		private void Update()
		{
			if (!Map.IsInstance() ||
				Map.Instance.Player == null ||
				Manager.Housing.Instance.IsCraft ||
				Time.timeScale == 0)
				return;

			Controller.Update();
		}

		private void LateUpdate()
		{
			if (!Map.IsInstance() ||
				Map.Instance.Player == null ||
				Manager.Housing.Instance.IsCraft ||
				Time.timeScale == 0)
				return;

			Controller.LateUpdate();
		}
	}
}