using MTB;
using UnityEngine;

namespace MTExtension
{
	public class RocketExtension : MonoBehaviour
	{
		private TimedRocket _Rocket;
		private int _State;

		private void Start() {
			_Rocket = GetComponent<TimedRocket>();
			_State = 0;
		}

		private void Update() {
			if (spaar.ModLoader.Game.IsSimulating) {
				var rocketHold = _Rocket.GetToggle("RocketHoldMode").IsActive;
				if (rocketHold) {
					foreach (var key in _Rocket.Keys) {
						if (key.IsReleased) {
							_State = 2;
							break;
						}
					}
				}
				if (_State == 2) {
					_State = 0;
					_Rocket.hasFired = false;
					_Rocket.SFX.Stop();
					_Rocket.flightSFX.Stop();
					_Rocket.StopSmokeTrail();
					_Rocket.StopAllCoroutines();
				} else {

				}
			} else {
				_State = 0;
			}
		}

		private void LateUpdate() {
			if (spaar.ModLoader.Game.IsSimulating) {
				var rocketToggle = _Rocket.GetToggle("RocketToggleMode").IsActive;
				if (rocketToggle) {
					foreach (var key in _Rocket.Keys) {
						if (key.IsPressed) {
							if (_State == 1) {
								_State = 2;
							} else if (_Rocket.hasFired) {
								_State = 1;
							}
							break;
						}
					}
				}
			} else {

			}
		}
	}
}
