using MTCore;
using spaar.ModLoader;
using System;
using System.Reflection;
using UnityEngine;

namespace MTExtension
{
	public class Mod : MTCore.Mod
	{
		public override string Name { get; } = "MTExtension";
		public override string DisplayName { get; } = "MT Extension";
		public override string Author { get; } = "MelekTaus";
		public override bool CanBeUnloaded { get; } = false;
		public override Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

		public override void OnLoad() {
			base.OnLoad();
			Game.OnBlockPlaced += Game_OnBlockPlaced;
			Game.OnBlockRemoved += Game_OnBlockRemoved;
			Game.OnKeymapperOpen += Game_OnKeymapperOpen;
		}

		public override void OnUnload() {
			Configuration.Save();
			base.OnUnload();
		}

		private void Game_OnBlockPlaced(Transform blockTransform) {
			_Add(blockTransform.GetComponent<BlockBehaviour>());
			if (blockTransform.GetComponent<TimedRocket>()) {
				blockTransform.gameObject.AddComponent<RocketBehaviour>();
			}
		}

		private void Game_OnBlockRemoved() {

		}

		private void Game_OnKeymapperOpen() {
			_Add(BlockMapper.CurrentInstance.Block);
			_Add(Machine.Active().BuildingBlocks.ToArray());
		}

		private bool _Add(params BlockBehaviour[] blocks) {
			bool result = false;
			foreach (var block in blocks) {
				if (block.GetBlockID() != (int) BlockType.Rocket) {
					continue;
				}
				result = result || SetToggle(block, "holdmode", "Hold Mode", (isActive) => _RocketToggleHandler(block, true, false));
				result = result || SetToggle(block, "togglemode", "Toggle Mode", (isActive) => _RocketToggleHandler(block, false, true));
				if (result) {
					_RocketToggleHandler(block);
				}
			}
			return result;
		}

		private void _RocketToggleHandler(BlockBehaviour block, bool hold = false, bool toggle = false) {
			var holdModeToggle = block.GetToggle("holdmode");
			var toggleModeToggle = block.GetToggle("togglemode");
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