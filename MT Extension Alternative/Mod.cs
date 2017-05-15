using Harmony;
using System;
using System.Reflection;

namespace MTExtensionAlternative
{
	public class Mod : spaar.ModLoader.Mod
	{
		public override string Name { get; } = "MTExtensionAlternative";
		public override string DisplayName { get; } = "MT Extension Alternative";
		public override string Author { get; } = "MelekTaus";
		public override Version Version { get; } = new Version(1, 0, 0);
		public override void OnLoad() {
			HarmonyInstance.Create(Name).PatchAll(Assembly.GetExecutingAssembly());
		}

		public override void OnUnload() {

		}
	}
}
