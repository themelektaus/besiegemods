using spaar.ModLoader;
using spaar.ModLoader.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTCore
{
	public class Controller : SingleInstance<Controller>
	{
		public override string Name {
			get { return "MTCore.Controller"; }
		}



		public bool Loaded { get; set; } = false;
		public bool UpdateBlockInfos { get; set; } = true;
		public Mod Mod { get; internal set; } = null;

		private Dictionary<Guid, BlockInfo> _BlockInfos;
		private int _WindowID;
		private Rect _Window;
		private List<_WindowAction> _WindowActions;
		private bool _WindowVisible;

		private struct _WindowAction
		{
			internal string Name;
			internal Action Action;
		}

		private void Start() {
			_BlockInfos = new Dictionary<Guid, BlockInfo>();
			_WindowID = Util.GetWindowID();
			_Window = new Rect(20, 20, 250, 300);
			_WindowActions = new List<_WindowAction>();
			_WindowVisible = false;
			Mod.OnControllerLoad(this);
		}

		private void Update() {
			if (Game.IsSimulating) {
				Loaded = true;
			}
		}

		private void OnGUI() {
			GUI.skin = ModGUI.Skin;
			if (_WindowVisible) {
				_Window = GUI.Window(_WindowID, _Window, (windowID) => {
					GUILayout.BeginHorizontal();
					foreach (var windowAction in _WindowActions) {
						if (GUILayout.Button(windowAction.Name)) {
							windowAction.Action?.Invoke();
						}
					}
					GUILayout.EndHorizontal();
					GUI.DragWindow();
				}, Name);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void ComponentAwake() {
			UpdateBlockInfos = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		public void ComponentDestroy(BlockBehaviour block) {
			if (UpdateBlockInfos) {
				var value = BlockInfo.FromBlockBehaviour(block);
				if (_BlockInfos.ContainsKey(block.Guid)) {
					_BlockInfos[block.Guid] = value;
				} else {
					_BlockInfos.Add(block.Guid, value);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public List<BlockInfo> BlockInfos {
			set {
				_BlockInfos.Clear();
				foreach (var blockInfo in value) {
					_BlockInfos.Add(blockInfo.Guid, blockInfo);
				}
				Loaded = false;
				UpdateBlockInfos = false;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public void AddWindowAction(string name, Action action) {
			_WindowActions.Add(new _WindowAction() {
				Name = name,
				Action = action
			});
		}

		/// <summary>
		/// 
		/// </summary>
		public void ShowWindow() {
			_WindowVisible = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void HideWindow() {
			_WindowVisible = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool GetBool(BlockBehaviour block, string key) {
			if (_BlockInfos.ContainsKey(block.Guid)) {
				var data = _BlockInfos[block.Guid].BlockData;
				if (data.HasKey(MapperType.XDATA_PREFIX + key)) {
					return data.ReadBool(MapperType.XDATA_PREFIX + key);
				}
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public float GetFloat(BlockBehaviour block, string key) {
			if (_BlockInfos.ContainsKey(block.Guid)) {
				var data = _BlockInfos[block.Guid].BlockData;
				if (data.HasKey(MapperType.XDATA_PREFIX + key)) {
					return data.ReadFloat(MapperType.XDATA_PREFIX + key);
				}
			}
			return 0;
		}
	}
}
