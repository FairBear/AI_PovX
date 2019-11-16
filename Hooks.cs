using HarmonyLib;

namespace AI_PovX
{
	public partial class AI_PovX
	{
		[HarmonyPrefix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (Manager.Housing.Instance.IsCraft ||
				!Controller.toggled ||
				Controller.chaCtrl == null)
				return true;

			if (Controller.focus == 0 && !Tools.IsHScene())
				Controller.FreeRoamPoV();
			else
				Controller.ScenePoV();

			return false;
		}
	}
}
