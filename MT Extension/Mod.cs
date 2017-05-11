using System;
using spaar.ModLoader;
using UnityEngine;
using System.Reflection;
using MTB;

namespace MTExtension
{
	public class Mod : spaar.ModLoader.Mod
	{
		public override string Name { get; } = "MTExtension";
		public override string DisplayName { get; } = "MT Extension";
		public override string Author { get; } = "MelekTaus";
		public override bool CanBeUnloaded { get; } = false;
		public override Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
		
		private static readonly MTBBlocks _Blocks = new MTBBlocks();

		public override void OnLoad() {
			Game.OnBlockPlaced += Game_OnBlockPlaced;
			Game.OnBlockRemoved += Game_OnBlockRemoved;
			Game.OnKeymapperOpen += Game_OnKeymapperOpen;
			XmlLoader.OnLoad += _Blocks.Load;
			XmlSaver.OnSave += _Blocks.Save;
		}
		
		public override void OnUnload() {
			Configuration.Save();
		}
		
		private static void Game_OnBlockPlaced(Transform blockTransform) {
			_Add(blockTransform.GetComponent<BlockBehaviour>());
			if (blockTransform.GetComponent<TimedRocket>()) {
				blockTransform.gameObject.AddComponent<RocketExtension>();
			}
		}

		private void Game_OnBlockRemoved() {
			
		}
		
		private static void Game_OnKeymapperOpen() {
			if (_Add(BlockMapper.CurrentInstance.Block)) {
				BlockMapper.CurrentInstance.Refresh();
			}
			_Add(Machine.Active().BuildingBlocks.ToArray());
		}
		
		private static bool _Add(params BlockBehaviour[] blocks) {
			bool result = false;
			foreach (var block in blocks) {
				if (block.GetBlockID() != (int) BlockType.Rocket) {
					continue;
				}
				if (block.MapperTypes.Exists(match => match.Key == "RocketHoldMode")) {
					continue;
				}
				result = true;

				_Blocks.CreateToggle(block, "RocketHoldMode", "Hold Mode", (isActive) => _RocketToggleHandler(block, true, false));
				_Blocks.CreateToggle(block, "RocketToggleMode", "Toggle Mode", (isActive) => _RocketToggleHandler(block, false, true));
				_RocketToggleHandler(block);
			}
			return result;
		}

		private static void _RocketToggleHandler(BlockBehaviour block, bool hold = false, bool toggle = false) {
			var holdModeToggle = block.GetToggle("RocketHoldMode");
			var toggleModeToggle = block.GetToggle("RocketToggleMode");
			var durationSlider = block.GetSlider("duration");
			
			if (hold && holdModeToggle.IsActive) {
				toggleModeToggle.IsActive = false;
			} else if (toggle && toggleModeToggle.IsActive) {
				holdModeToggle.IsActive = false;
			}

			durationSlider.DisplayInMapper = !(holdModeToggle.IsActive || toggleModeToggle.IsActive);
			if (durationSlider.DisplayInMapper) {
				while (durationSlider.Value > 604800) {
					durationSlider.Value -= 604800;
				}
			} else {
				while (durationSlider.Value < 604800) {
					durationSlider.Value += 604800;
				}
			}
			if (BlockMapper.CurrentInstance != null) {
				BlockMapper.CurrentInstance.Refresh();
			}
		}
	}
}