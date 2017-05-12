using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTCore
{
	public abstract class Mod : spaar.ModLoader.Mod
	{
		public readonly FieldInfo MapperTypes = typeof(BlockBehaviour).GetField("mapperTypes", BindingFlags.Instance | BindingFlags.NonPublic);

		private Dictionary<Guid, BlockInfo> _BlockInfos;

		public Mod() : base() {
			_BlockInfos = new Dictionary<Guid, BlockInfo>();
		}

		public override void OnLoad() {
			XmlLoader.OnLoad += Load;
			XmlSaver.OnSave += Save;
		}

		public override void OnUnload() {

		}

		public void Load(MachineInfo info) {
			_BlockInfos.Clear();
			foreach (var blockInfo in info.Blocks) {
				_BlockInfos.Add(blockInfo.Guid, blockInfo);
			}
		}

		public void Save(MachineInfo info) {

		}

		public bool GetBool(BlockBehaviour block, string key) {
			if (_BlockInfos.ContainsKey(block.Guid)) {
				var data = _BlockInfos[block.Guid].BlockData;
				if (data.HasKey(MapperType.XDATA_PREFIX + key)) {
					return data.ReadBool(MapperType.XDATA_PREFIX + key);
				}
			}
			return false;
		}

		public bool SetToggle(BlockBehaviour block, string key, string displayName, ToggleHandler toggleHandler) {
			if (block.MapperTypes.Exists(match => match.Key == key)) {
				return false;
			}
			var current = block.MapperTypes;
			var value = GetBool(block, key);
			var toggle = new MToggle(displayName, key, value);
			toggle.Toggled += toggleHandler;
			current.Add(toggle);
			MapperTypes.SetValue(block, current);
			return true;
		}
	}
}