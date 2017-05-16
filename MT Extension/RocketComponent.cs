namespace MTExtension
{
	public class RocketComponent : CustomComponent
	{
		private bool _RocketToggled;
		private TimedRocket _Rocket;

		protected override void Awake() {
			base.Awake();
			_RocketToggled = false;
			_Rocket = GetComponent<TimedRocket>();
			Mod.AddToggle(_Rocket, "togglemode", "Toggle Mode", (active) => {
				if (active) {
					Mod.SetToggleValue(_Rocket, "holdmode", false);
				}
				_RefreshDurationSliderVisibility();
			});
			Mod.AddToggle(_Rocket, "holdmode", "Hold Mode", (active) => {
				if (active) {
					Mod.SetToggleValue(_Rocket, "togglemode", false);
				}
				_RefreshDurationSliderVisibility();
			});
			_RefreshDurationSliderVisibility();
		}

		private void Update() {
			if (StatMaster.isSimulating) {
				if (Mod.GetToggleValue(_Rocket, "togglemode")) {
					Mod.SetPrivateField<float>(_Rocket, "timeFlown", 0);
					foreach (var key in _Rocket.Keys) {
						if (key.IsPressed) {
							if (!_RocketToggled) {
								_RocketToggled = true;
							} else if (_Rocket.hasFired) {
								_StopRocket();
							}
							break;
						}
					}
				} else if (Mod.GetToggleValue(_Rocket, "holdmode")) {
					Mod.SetPrivateField<float>(_Rocket, "timeFlown", 0);
					foreach (var key in _Rocket.Keys) {
						if (key.IsReleased) {
							_StopRocket();
							break;
						}
					}
				}
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy();
		}

		private void _RefreshDurationSliderVisibility() {
			var toggleMode = Mod.GetToggleValue(_Rocket, "togglemode");
			var holdMode = Mod.GetToggleValue(_Rocket, "holdmode");
			Mod.SetSliderVisibility(_Rocket, "duration", !(toggleMode || holdMode));
		}

		private void _StopRocket() {
			_RocketToggled = false;
			_Rocket.hasFired = false;
			_Rocket.SFX.Stop();
			_Rocket.flightSFX.Stop();
			_Rocket.StopSmokeTrail();
			_Rocket.StopAllCoroutines();
		}
	}
}
