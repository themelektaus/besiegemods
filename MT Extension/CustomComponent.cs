using MTCore;
using UnityEngine;

namespace MTExtension
{
	public class CustomComponent : MonoBehaviour
	{
		protected virtual void Awake() {
			Controller.Instance.ComponentAwake();
		}

		protected virtual void OnDestroy() {
			var block = GetComponent<BlockBehaviour>();
			if (block != null) {
				Controller.Instance.ComponentDestroy(block);
			}
		}
	}
}
