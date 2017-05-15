using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace MTCore
{
	public static class Debug
	{
		private static string _LogRecord = null;

		/// <summary>
		/// 
		/// </summary>
		public static void RecordLog() {
			_LogRecord = "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static string CaptureLog() {
			return CaptureLog(null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string CaptureLog(string path) {
			var result = _LogRecord;
			_LogRecord = null;
			if (path != null) {
				File.WriteAllText(path, result);
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public static void Log(object message) {
			var log = "[MTCore] " + message;
			if (!_AddToLogRecord(log)) {
				UnityEngine.Debug.Log(log);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public static void LogWarning(object message) {
			var log = "[MTCore] " + message;
			if (!_AddToLogRecord(log)) {
				UnityEngine.Debug.LogWarning(log);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public static void LogError(object message) {
			var log = "[MTCore] " + message;
			if (!_AddToLogRecord(log)) {
				UnityEngine.Debug.LogError(log);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public static void VarDump(object obj) {
			Log(VarExport(obj));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string VarExport(object obj) {
			return VarExport(obj, 2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static string VarExport(object obj, int max) {
			return VarExport(obj, max, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="max"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public static string VarExport(object obj, int max, string filter) {
			return _VarExport(obj, max, filter, 0);
		}



		private static string _VarExport(object obj, int max, string filter, int recursion) {
			StringBuilder result = new StringBuilder();
			if (recursion < max) {
				PropertyInfo[] properties = obj.GetType().GetProperties();
				foreach (PropertyInfo property in properties) {
					try {
						object value = property.GetValue(obj, null);
						string indent = String.Empty;
						string spaces = "|  ";
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

		private static bool _AddToLogRecord(string log) {
			if (_LogRecord != null) {
				_LogRecord += log + Environment.NewLine;
				return true;
			}
			return false;
		}
	}
}
