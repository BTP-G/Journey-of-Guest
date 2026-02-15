using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

namespace UnityEditorInternal
{
	/// <summary>
	/// 
	/// </summary>
	public class Settings
	{
		private const string JsonFilePath = "Assets/Gabriel Pereira/Events 2.0 for Unity/settings.json";

		private static readonly string[] m_PredefinedAssemblies = new string[]
		{
			"mscorlib",
			"UnityEngine.CoreModule"
		};

		public static Events2Settings GetSettings()
		{
			CheckFileSettings();

			Events2Settings settings = new Events2Settings();

			using (StreamReader reader = new StreamReader(JsonFilePath))
			{
				string content = reader.ReadToEnd();

				settings = JsonUtility.FromJson<Events2Settings>(content);
			}

			return settings;
		}

		private static void CheckFileSettings()
		{
			if (File.Exists(JsonFilePath)) return;

			using (StreamWriter writer = File.CreateText(JsonFilePath))
			{
				var settings = new Events2Settings();

				var solutionPath = GetSolutionPath();
				var content = File.ReadAllText(solutionPath);
				var projReg = new Regex("Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\"", RegexOptions.Compiled);
				var matches = projReg.Matches(content).Cast<Match>();
				var projects = matches.Select(x => x.Groups[2].Value).ToList();

				projects = projects
					.ConvertAll(project =>
					{
						var projectPath = project;
						if (!Path.IsPathRooted(projectPath))
							projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), projectPath);

						return Path.GetFullPath(projectPath);
					});

				projects.Sort();

				settings.assemblies = projects
					.ConvertAll(projectPath =>
					{
						XDocument doc = XDocument.Load(projectPath);
						// MSBuild files often have a default namespace, which must be included in queries
						XNamespace ns = doc.Root.GetDefaultNamespace();

						// Helper function to get property value
						string GetPropertyValue(string propertyName)
						{
							// Look for the property within any PropertyGroup
							var property = doc.Descendants(ns + "PropertyGroup")
											  .Elements(ns + propertyName)
											  .FirstOrDefault();

							return property?.Value ?? "N/A";
						}

						if (GetPropertyValue("UnityProjectType").StartsWith("Editor"))
							return "N/A";

						return GetPropertyValue("AssemblyName");
					})
					.FindAll(assemblyName => assemblyName != "N/A")
					.ToArray();

				var assemblies = settings.assemblies;
				
				Array.Resize(ref assemblies, assemblies.Length + m_PredefinedAssemblies.Length);
				Array.Copy(m_PredefinedAssemblies, 0, assemblies, settings.assemblies.Length, m_PredefinedAssemblies.Length);
				
				settings.assemblies = assemblies;

				content = JsonUtility.ToJson(settings);

				writer.Write(content);
			}
		}

		private static string GetSolutionPath()
		{
			var currentDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			while (currentDirPath != null)
			{
				// Search for a .sln file in the current directory
				var solutionFileName = Directory.GetFiles(currentDirPath)
												.Select(f => Path.GetFileName(f))
												.SingleOrDefault(f => f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase));

				if (solutionFileName != null)
					return Path.Combine(currentDirPath, solutionFileName);

				// Move up to the parent directory
				currentDirPath = Directory.GetParent(currentDirPath)?.FullName;
			}

			throw new FileNotFoundException("Cannot find solution file path");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public struct Events2Settings
	{
		public string[] assemblies;
	}
}