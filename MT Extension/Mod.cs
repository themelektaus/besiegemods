using MTCore;
using System.Reflection;

namespace MTExtension
{
	public class Mod : MTCore.Mod
	{
		public override Assembly Assembly {
			get { return Assembly.GetExecutingAssembly(); }
		}

		public override void OnControllerLoad(Controller controller) {
			// controller.AddWindowAction("Action 1", () => {
			// 
			// });
			// controller.ShowWindow();
		}

		public override void OnBlockUpdate(BlockBehaviour block) {
			SetComponent<RocketComponent>(block, BlockType.Rocket);
		}
	}
}
