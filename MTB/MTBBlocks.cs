using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTB
{
	public class MTBBlocks
	{
		public readonly FieldInfo MapperTypes = typeof(BlockBehaviour).GetField("mapperTypes", BindingFlags.Instance | BindingFlags.NonPublic);
		private Dictionary<Guid, BlockInfo> _BlockInfos;

		public MTBBlocks() {
			_BlockInfos = new Dictionary<Guid, BlockInfo>();
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

		public void CreateToggle(BlockBehaviour block, string key, string displayName, ToggleHandler toggleHandler) {
			var current = block.MapperTypes;
			var value = GetBool(block, key);
			var toggle = new MToggle(displayName, key, value);
			toggle.Toggled += toggleHandler;
			current.Add(toggle);
			MapperTypes.SetValue(block, current);
		}
	}
}