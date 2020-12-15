using AIChara;
using AIProject;
using Manager;
using System.Collections.Generic;
using UnityEngine;

namespace AI_PovX
{
	public static class Controller
	{
		public static bool toggled = false;
		public static bool showCursor = false;

		public static Quaternion bodyQuaternion;
		public static float bodyAngle = 0f; // Actual body, not the camera.

		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraAngleY = 0f;

		// 0 = Player; 1 = 1st Partner; 2 = 2nd Partner; 3 = ...
		public static int focus = 0;
		public static ChaControl chaCtrl;
		public static Vector3 eyeOffset = Vector3.zero;
		public static Vector3 backupHead;
		public static float backupFov;

		public static bool inScene;

		public static void TogglePoV(bool flag)
		{
			if (toggled == flag)
				return;

			toggled = flag;

			if (flag)
			{
				SetChaControl(FromFocus());

				cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				cameraAngleY = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone.eulerAngles.y;
				bodyQuaternion = Map.Instance.Player.Rotation;
				bodyAngle = bodyQuaternion.eulerAngles.y;
				backupFov = Camera.main.fieldOfView;
			}
			else
			{
				if (AI_PovX.HSceneLockCursor.Value)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				Camera.main.fieldOfView = backupFov;

				SetChaControl(null);
			}
		}

		public static void Update()
		{
			if (AI_PovX.PovKey.Value.IsDown())
				TogglePoV(!toggled);

			if (AI_PovX.CursorReleaseKey.Value.IsDown())
				showCursor = !showCursor;

			if (!toggled)
				return;

			if (Tools.IsHScene())
			{
				if (AI_PovX.CharaCycleKey.Value.IsDown())
				{
					focus = (focus + 1) % 3;

					if (focus != 0)
					{
						Actor[] females = HSceneManager.Instance.females;

						if (females[focus - 1] == null || females[focus - 1] == Map.Instance.Player)
						{
							focus = 0;

							SetChaControl(Map.Instance.Player.ChaControl);
						}
						else
							SetChaControl(females[focus - 1].ChaControl);
					}
					else
						SetChaControl(Map.Instance.Player.ChaControl);
				}
			}
			else if (focus != 0)
			{
				focus = 0;

				SetChaControl(Map.Instance.Player.ChaControl);
			}

			float sensitivity = AI_PovX.Sensitivity.Value;

			if (AI_PovX.ZoomKey.Value.IsPressed())
				sensitivity *= AI_PovX.ZoomFov.Value / AI_PovX.Fov.Value;

			float x = UnityEngine.Input.GetAxis("Mouse Y") * sensitivity;
			if (AI_PovX.CameraInvertYAxis.Value)
				x *= -1;
			float y = UnityEngine.Input.GetAxis("Mouse X") * sensitivity;

			if (Cursor.lockState != CursorLockMode.None || AI_PovX.CameraDragKey.Value.IsPressed())
			{
				float max = AI_PovX.CameraMaxX.Value;
				float min = AI_PovX.CameraMinX.Value;
				float span = AI_PovX.CameraSpanY.Value;

				cameraAngleOffsetX = Mathf.Clamp(cameraAngleOffsetX - x, -max, min);
				cameraAngleOffsetY = Mathf.Clamp(cameraAngleOffsetY + y, -span, span);
				cameraAngleY = Tools.Mod2(cameraAngleY + y, 360f);
			}
		}

		public static void LateUpdate()
		{
			if (toggled)
			{
				bool hScene = Tools.IsHScene();

				// Make it so that the player doesn't go visible if they're not supposed to be in the scene.
				if (!hScene || Map.Instance.Player.ChaControl.visibleAll)
					Map.Instance.Player.ChaControl.visibleAll = true;

				if (hScene && AI_PovX.HSceneLockCursor.Value && !showCursor)
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}

			if (showCursor)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

			if (AI_PovX.RevealAll.Value)
			{
				foreach (KeyValuePair<int, AgentActor> agent in Map.Instance.AgentTable)
					agent.Value.ChaControl.visibleAll = true;

				MerchantActor merchant = Map.Instance.Merchant;

				if (merchant != null)
					merchant.ChaControl.visibleAll = true;
			}
		}

		public static void SetChaControl(ChaControl next)
		{
			if (chaCtrl != null && backupHead != null)
				chaCtrl.objHeadBone.transform.localScale = backupHead;

			chaCtrl = next;

			if (chaCtrl != null)
			{
				eyeOffset = Tools.GetEyesOffset(chaCtrl);
				backupHead = chaCtrl.objHeadBone.transform.localScale;

				if (Tools.ShouldHideHead())
					chaCtrl.objHeadBone.transform.localScale = Vector3.zero;
			}
		}

		public static ChaControl FromFocus()
		{
			return focus == 0 ?
				Map.Instance.Player.ChaControl :
				HSceneManager.Instance.females[focus - 1]?.ChaControl;
		}

		/*public static void Stare(ChaControl target, ChaControl looker)
		{
			Vector3 target_pos = target.objHeadBone.transform.position;
			Vector3 looker_pos = looker.objHeadBone.transform.position;
			Vector3 looker_dir = looker.objBodyBone.transform.forward;

			if (Vector3.Angle(target_pos - looker_pos, looker_dir) <= AI_PovX.LookMaxRange.Value)
				looker.neckLookCtrl.neckLookScript.aBones[0].neckBone.LookAt(target_pos);
		}*/

		public static void SetCamera(Transform neck)
		{
			Camera.main.fieldOfView =
				AI_PovX.ZoomKey.Value.IsPressed() ?
					AI_PovX.ZoomFov.Value :
					AI_PovX.Fov.Value;
			Camera.main.transform.position =
				neck.position +
				(AI_PovX.OffsetX.Value + eyeOffset.x) * neck.right +
				(AI_PovX.OffsetY.Value + eyeOffset.y) * neck.up +
				(AI_PovX.OffsetZ.Value + eyeOffset.z) * neck.forward;
		}

		// Used for scenes where the focused character cannot be controlled.
		public static void ScenePoV()
		{
			if (!inScene)
			{
				inScene = true;
				// Reset rotation to prevent disorientation.
				cameraAngleOffsetX = cameraAngleOffsetY = 0;

				// Refresh when switching PoV modes.
				SetChaControl(FromFocus());
			}

			Transform neck = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone;
			// Preserve current neck rotation.
			Camera.main.transform.rotation = neck.rotation;
			Camera.main.transform.Rotate(new Vector3(cameraAngleOffsetX, cameraAngleOffsetY, 0f));
			SetCamera(neck);
		}

		// PoV exclusively for the player.
		public static void FreeRoamPoV()
		{
			PlayerActor player = Map.Instance.Player;

			if (player.Controller.State is AIProject.Player.Normal)
			{
				if (inScene)
				{
					inScene = false;

					// Refresh when switching PoV modes.
					SetChaControl(FromFocus());
				}

				if (!AI_PovX.RotateHead.Value || player.StateInfo.move.magnitude > 0f)
				{
					// Move entire body when moving.
					bodyAngle = cameraAngleY;
					bodyQuaternion = Quaternion.Euler(0f, bodyAngle, 0f);
				}
				else
				{
					// Rotate head first. If head rotation is at the limit, rotate body.
					float angle = Tools.GetClosestAngle(bodyAngle, cameraAngleY, out bool clockwise);
					float max = AI_PovX.HeadMax.Value;

					if (angle > max)
					{
						if (clockwise)
							bodyAngle = Tools.Mod2(bodyAngle + angle - max, 360f);
						else
							bodyAngle = Tools.Mod2(bodyAngle - angle + max, 360f);

						bodyQuaternion = Quaternion.Euler(0f, bodyAngle, 0f);
					}
				}

				Transform neck = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone;
				Vector3 neck_euler = neck.eulerAngles;

				Camera.main.transform.rotation = Quaternion.Euler(cameraAngleOffsetX, cameraAngleY, 0f);
				player.Rotation = bodyQuaternion;
				neck.rotation = Quaternion.Euler(
					Tools.AngleClamp(
						Tools.Mod2(neck_euler.x + cameraAngleOffsetX, 360f),
						AI_PovX.NeckMin.Value,
						AI_PovX.NeckMax.Value
					),
					cameraAngleY,
					neck_euler.z
				);

				SetCamera(neck);
			}
			else
				// When the player is unable to move, treat it as a scene.
				ScenePoV();
		}
	}
}
