using spaar.ModLoader;
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MTCore
{
	[Template]
	public abstract class Mod : spaar.ModLoader.Mod
	{
		public abstract Assembly Assembly { get; }
		public abstract void OnControllerLoad(Controller controller);
		public abstract void OnBlockUpdate(BlockBehaviour block);



		private const BindingFlags _MAPPER_TYPES_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
		private const BindingFlags _METHOD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static readonly FieldInfo _MapperTypes = typeof(BlockBehaviour).GetField("mapperTypes", _MAPPER_TYPES_FLAGS);



		public override string Name {
			get { return DisplayName.Replace(" ", ""); }
		}

		public override string DisplayName {
			get {
				if (Assembly == null) {
					return "";
				}
				var attribute = Attribute.GetCustomAttribute(Assembly, typeof(AssemblyTitleAttribute), false);
				return (attribute as AssemblyTitleAttribute).Title;
			}
		}

		public override string Author {
			get {
				if (Assembly == null) {
					return "";
				}
				var attribute = Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute), false);
				return (attribute as AssemblyCompanyAttribute).Company;
			}
		}

		public override Version Version {
			get {
				if (Assembly == null) {
					return new Version(0, 0, 0, 0);
				}
				return Assembly.GetName().Version;
			}
		}

		public override bool CanBeUnloaded {
			get { return false; }
		}

		public override void OnLoad() {
			Controller.Instance.Mod = this;
			Object.DontDestroyOnLoad(Controller.Instance);

			Game.OnBlockPlaced += Game_OnBlockPlaced;
			Game.OnKeymapperOpen += Game_OnKeymapperOpen;
			Game.OnSimulationToggle += Game_OnSimulationToggle;
			XmlLoader.OnLoad += XmlLoader_Load;
			XmlSaver.OnSave += XmlLoader_Save;
		}

		public override void OnUnload() {
			Configuration.Save();
		}



		private void Game_OnBlockPlaced(Transform block) {
			var blockBehaviour = block.GetComponent<BlockBehaviour>();
			if (blockBehaviour == null) {
				return;
			}
			OnBlockUpdate(blockBehaviour);
		}

		private void Game_OnKeymapperOpen() {
			var instance = BlockMapper.CurrentInstance;
			if (instance != null) {
				OnBlockUpdate(instance.Block);
				instance.Refresh();
			}
			foreach (var block in Machine.Active().BuildingBlocks) {
				OnBlockUpdate(block);
			}
		}

		private void Game_OnSimulationToggle(bool simulating) {
			if (simulating) {
				if (!Controller.Instance.Loaded) {
					foreach (var block in Machine.Active().BuildingBlocks) {
						OnBlockUpdate(block);
					}
				}
			}
		}

		private void XmlLoader_Load(MachineInfo info) {
			Controller.Instance.BlockInfos = info.Blocks;
		}

		private void XmlLoader_Save(MachineInfo info) {
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public static void Destroy(Object obj) {
			Object.Destroy(obj);
		}



		/// <summary>
		/// Get the BlockBehaviour based on the block info's GUID
		/// </summary>
		/// <param name="blockInfo">block info</param>
		/// <returns></returns>
		public static BlockBehaviour GetBlock(BlockInfo blockInfo) {
			return GetBlock(blockInfo.Guid);
		}

		/// <summary>
		/// Get the BlockBehaviour of specified GUID
		/// </summary>
		/// <param name="guid">GUID of block(-info)</param>
		/// <returns></returns>
		public static BlockBehaviour GetBlock(Guid guid) {
			return ReferenceMaster.BuildingBlocks.Find(b => b.Guid == guid);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		/// <param name="toggleHandler"></param>
		/// <returns></returns>
		public static MToggle AddToggle(BlockBehaviour block, string key, string displayName, ToggleHandler toggleHandler) {
			MToggle result = CallPrivateFunc<MToggle>(block, "AddToggle",
				new Type[] { typeof(string), typeof(string), typeof(bool) },
				new object[] { displayName, key, Controller.Instance.GetBool(block, key) }
			);
			result.Toggled += toggleHandler;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static MToggle GetToggle(BlockBehaviour block, string key) {
			return block.Toggles.Find(t => t.Key == key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool GetToggleValue(BlockBehaviour block, string key) {
			var toggle = GetToggle(block, key);
			if (toggle == null) {
				return false;
			}
			return toggle.IsActive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void SetToggleValue(BlockBehaviour block, string key, bool value) {
			var toggle = GetToggle(block, key);
			if (toggle == null) {
				return;
			}
			if (toggle.IsActive != value) {
				toggle.IsActive = value;
				_RefreshBlockMapper();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void SetToggleVisibility(BlockBehaviour block, string key, bool value) {
			var toggle = GetToggle(block, key);
			if (toggle == null) {
				return;
			}
			if (toggle.DisplayInMapper != value) {
				toggle.DisplayInMapper = value;
				_RefreshBlockMapper();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="valueChangeHandler"></param>
		/// <returns></returns>
		public static MSlider AddSlider(BlockBehaviour block, string key, string displayName, float min, float max, ValueChangeHandler valueChangeHandler) {
			MSlider result = CallPrivateFunc<MSlider>(block, "AddSlider",
				new Type[] { typeof(string), typeof(string), typeof(float), typeof(float), typeof(float) },
				new object[] { displayName, key, min, max, Controller.Instance.GetFloat(block, key) }
			);
			result.ValueChanged += valueChangeHandler;
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static MSlider GetSlider(BlockBehaviour block, string key) {
			return block.Sliders.Find(t => t.Key == key);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static float GetSliderValue(BlockBehaviour block, string key) {
			var slider = GetSlider(block, key);
			if (slider == null) {
				return 0;
			}
			return slider.Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void SetSliderValue(BlockBehaviour block, string key, float value) {
			var slider = GetSlider(block, key);
			if (slider == null) {
				return;
			}
			if (slider.Value != value) {
				slider.Value = value;
				_RefreshBlockMapper();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void SetSliderVisibility(BlockBehaviour block, string key, bool value) {
			var slider = GetSlider(block, key);
			if (slider == null) {
				return;
			}
			if (slider.DisplayInMapper != value) {
				slider.DisplayInMapper = value;
				_RefreshBlockMapper();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="block"></param>
		/// <param name="blockType"></param>
		public static void SetComponent<T>(BlockBehaviour block, BlockType blockType) where T : Component {
			T component = block.GetComponent<T>();
			if (block.GetBlockID() == (int) blockType) {
				if (component == null) {
					block.gameObject.AddComponent<T>();
				}
			} else {
				if (component != null) {
					Destroy(component);
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="parameterTypes"></param>
		/// <param name="parameters"></param>
		public static void CallPrivateAction(object instance, string methodName, Type[] parameterTypes, object[] parameters) {
			var method = instance.GetType().GetMethod(methodName, _METHOD_FLAGS, null, parameterTypes, null);
			method.Invoke(instance, parameters);
		}

		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="parameterTypes"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static T CallPrivateFunc<T>(object instance, string methodName, Type[] parameterTypes, object[] parameters) {
			var method = instance.GetType().GetMethod(methodName, _METHOD_FLAGS, null, parameterTypes, null);
			return (T) method.Invoke(instance, parameters);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T GetPrivateField<T>(object instance, string name) {
			var field = instance.GetType().GetField(name, _METHOD_FLAGS);
			return (T) field.GetValue(instance);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void SetPrivateField<T>(object instance, string name, T value) {
			var field = instance.GetType().GetField(name, _METHOD_FLAGS);
			field.SetValue(instance, value);
		}



		private static void _RefreshBlockMapper() {
			var instance = BlockMapper.CurrentInstance;
			if (instance != null) {
				instance.Refresh();
			}
		}



		/// <summary>
		/// 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="blockType"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		[Obsolete]
		public static void SetToggle(BlockBehaviour block, BlockType blockType, string key, string displayName) {
			SetToggle(block, blockType, key, displayName, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="blockType"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		/// <param name="toggleHandler"></param>
		[Obsolete]
		public static void SetToggle(BlockBehaviour block, BlockType blockType, string key, string displayName, ToggleHandler toggleHandler) {
			if (block.GetBlockID() != (int) blockType) {
				return;
			}
			var result = block.Toggles.Find(match => match.Key == key);
			if (result == null) {
				result = new MToggle(displayName, key, Controller.Instance.GetBool(block, key));
				if (toggleHandler != null) {
					result.Toggled += toggleHandler;
				}
				var currentMapperTypes = block.MapperTypes;
				currentMapperTypes.Add(result);
				_MapperTypes.SetValue(block, currentMapperTypes);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		[Obsolete]
		public static void SetSlider(BlockBehaviour block, string key, string displayName, float min, float max) {
			SetSlider(block, key, displayName, min, max, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="key"></param>
		/// <param name="displayName"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="valueChangeHandler"></param>
		[Obsolete]
		public static void SetSlider(BlockBehaviour block, string key, string displayName, float min, float max, ValueChangeHandler valueChangeHandler) {
			var result = block.Sliders.Find(match => match.Key == key);
			if (result == null) {
				result = new MSlider(displayName, key, Controller.Instance.GetFloat(block, key), min, max);
				if (valueChangeHandler != null) {
					result.ValueChanged += valueChangeHandler;
				}
				var currentMapperTypes = block.MapperTypes;
				currentMapperTypes.Add(result);
				_MapperTypes.SetValue(block, currentMapperTypes);
			}
		}
	}
}
