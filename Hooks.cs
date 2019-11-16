using AIChara;
using AIProject;
using HarmonyLib;
using Manager;
using UnityEngine;

namespace AI_PovX
{
	public partial class AI_PovX
	{
		[HarmonyPrefix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (
				Manager.Housing.Instance.IsCraft ||
				!Controller.toggled ||
				Controller.chaCtrl == null)
				return true;

			if (Controller.focus == 0 && !Tools.IsHScene())
				Controller.FreeRoamPoV();
			else
				Controller.ScenePoV();

			return __instance != Controller.chaCtrl.neckLookCtrl;
		}

		/*[HarmonyPostfix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static void Postfix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (!Controller.shouldStare)
				return;

			if (!Tools.IsHScene())
				return;

			Actor[] females = HSceneManager.Instance.females;
			ChaControl playerChaCtrl = Map.Instance.Player.ChaControl;

			if (__instance == playerChaCtrl.neckLookCtrl)
				Controller.Stare(females[0].ChaControl, playerChaCtrl);
			else
				foreach (Actor female in females)
					if (female != null)
					{
						ChaControl chaCtrl = female.ChaControl;

						if (__instance == chaCtrl.neckLookCtrl)
						{
							Controller.Stare(playerChaCtrl, chaCtrl);

							break;
						}
					}
		}*/
	}
}
