using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTB
{
    public static class MTBExtensions
    {
		public static void ToggleConsole(this spaar.ModLoader.Mod obj) {
			// WindowsInput.InputSimulator.SimulateKeyDown(WindowsInput.VirtualKeyCode.LCONTROL);
			// WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.VK_K);
			// WindowsInput.InputSimulator.SimulateKeyUp(WindowsInput.VirtualKeyCode.LCONTROL);
		}

		public static void VarDump(this object obj) {
			Debug.Log(obj.VarExport());
		}

		public static string VarExport(this object obj) {
			return obj.VarExport(2);
		}

		public static string VarExport(this object obj, int max) {
			return obj.VarExport(max, null);
		}

		public static string VarExport(this object obj, int max, string filter) {
			return obj._VarExport(max, filter, 0);
		}

		private static string _VarExport(this object obj, int max, string filter, int recursion) {
			StringBuilder result = new StringBuilder();
			if (recursion < max) {
				PropertyInfo[] properties = obj.GetType().GetProperties();
				foreach (PropertyInfo property in properties) {
					try {
						object value = property.GetValue(obj, null);
						string indent = String.Empty;
						string spaces = "|   ";
						string trail = "|...";
						if (recursion > 0) {
							indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();
						}
						if (value != null) {
							string displayValue = value.ToString();
							if (value is string) displayValue = String.Concat('"', displayValue, '"');
							if (filter == null || indent.ToLower().Contains(filter) || property.Name.ToLower().Contains(filter) || displayValue.ToLower().Contains(filter)) {
								result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);
							}
							try {
								if (!(value is ICollection)) {
									result.Append(_VarExport(value, max, filter, recursion + 1));
								} else {
									int elementCount = 0;
									foreach (object element in ((ICollection) value)) {
										string elementName = String.Format("{0}[{1}]", property.Name, elementCount);
										indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();
										result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());
										result.Append(_VarExport(element, max, filter, recursion + 2));
										elementCount++;
									}
									result.Append(_VarExport(value, max, filter, recursion + 1));
								}
							} catch { }
						} else {
							result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
						}
					} catch {

					}
				}
			}
			return result.ToString();
		}

		public static MToggle GetToggle(this BlockBehaviour block, string key) {
			var result = block.Toggles.Find(t => t.Key == key);
			if (result == null) {
				var buildingBlock = block.BuildingBlock;
				if (buildingBlock != null) {
					result = buildingBlock.Toggles.Find(t => t.Key == key);
				}
			}
			return result;
		}

		public static MSlider GetSlider(this BlockBehaviour block, string key) {
			var result = block.Sliders.Find(t => t.Key == key);
			if (result == null) {
				var buildingBlock = block.BuildingBlock;
				if (buildingBlock != null) {
					result = buildingBlock.Sliders.Find(t => t.Key == key);
				}
			}
			return result;
		}
	}
}