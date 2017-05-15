using Harmony;
using System;
using System.Collections.Generic;

namespace MTExtensionAlternative
{
	class RocketPatch
	{
		class Property
		{
			public int State;
			public Property() {
				State = 0;
			}
		}

		static Dictionary<TimedRocket, Property> Properties = new Dictionary<TimedRocket, Property>();

		static bool IsToggleModeActive(TimedRocket block) {
			return block.Toggles.Find(t => t.Key == "togglemode").IsActive;
		}

		static bool IsHoldModeActive(TimedRocket block) {
			return block.Toggles.Find(t => t.Key == "holdmode").IsActive;
		}

		[HarmonyPatch(typeof(TimedRocket), "Awake")]
		class Awake
		{
			static void Postfix(TimedRocket __instance) {
				Properties[__instance] = new Property();
				__instance.CallPrivateMethod<MToggle>(
					"AddToggle",
					new Type[] { typeof(string), typeof(string), typeof(bool) },
					new object[] { "Hold Mode", "holdmode", false }
				).Toggled += active => {
					__instance.Sliders.Find(t => t.Key == "duration").DisplayInMapper = !(IsToggleModeActive(__instance) || active);
				};

				__instance.CallPrivateMethod<MToggle>(
					"AddToggle",
					new Type[] { typeof(string), typeof(string), typeof(bool) },
					new object[] { "Toggle Mode", "togglemode", false }
				).Toggled += active => {
					__instance.Sliders.Find(t => t.Key == "duration").DisplayInMapper = !(IsHoldModeActive(__instance) || active);
				};
			}
		}

		[HarmonyPatch(typeof(TimedRocket), "Update")]
		class Update
		{
			static void Postfix(TimedRocket __instance) {
				if (StatMaster.isSimulating) {
					if (IsHoldModeActive(__instance)) {
						__instance.SetPrivateField<float>("timeFlown", 0);
						foreach (var key in __instance.Keys) {
							if (key.IsReleased) {
								Stop(__instance);
								break;
							}
						}
					} else if (IsToggleModeActive(__instance)) {
						__instance.SetPrivateField<float>("timeFlown", 0);
						foreach (var key in __instance.Keys) {
							if (key.IsPressed) {
								if (Properties[__instance].State == 0) {
									Properties[__instance].State = 1;
								} else if (__instance.hasFired) {
									Properties[__instance].State = 0;
									Stop(__instance);
								}
							}
						}
					}
				}
			}

			static void Stop(TimedRocket __instance) {
				__instance.hasFired = false;
				__instance.SFX.Stop();
				__instance.flightSFX.Stop();
				__instance.StopSmokeTrail();
				__instance.StopAllCoroutines();
			}
		}
	}
}
