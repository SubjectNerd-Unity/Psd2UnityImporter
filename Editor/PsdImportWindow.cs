using System;
using SubjectNerd.PsdImporter.FullSerializer;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SubjectNerd.PsdImporter
{
	public class PsdImportWindow : EditorWindow
	{
		#region Static
		private const string MENU_ASSET_IMPORT = "Assets/Import PSD Layers";

		private static PsdImportWindow OpenWindow()
		{
			var win = GetWindow<PsdImportWindow>();
			win.titleContent = new GUIContent("PSD Layer Import");
			win.Show(true);
			return win;
		}

		[MenuItem(MENU_ASSET_IMPORT)]
		private static void ImportPsdAsset()
		{
			var win = OpenWindow();

			Object[] selectionArray = Selection.objects;
			if (selectionArray.Length < 1)
				return;

			for (int i = 0; i < selectionArray.Length; i++)
			{
				var file = selectionArray[i];
				var path = AssetDatabase.GetAssetPath(file);
				if (path.ToLower().EndsWith(".psd"))
				{
					win.OpenFile(file);
					return;
				}
			}
		}

		public static bool OpenLayerImporter(Object file)
		{
			var win = OpenWindow();
			if (file == null)
				return false;

			var path = AssetDatabase.GetAssetPath(file);
			if (string.IsNullOrEmpty(path))
				return false;
			if (path.ToLower().EndsWith(".psd") == false)
				return false;

			win.OpenFile(file);
			return true;
		}
		#endregion

		private float importPPU = 100;
		private Object importFile;
		private string importPath;
		private ImportUserData importSettings;
		private bool settingsChanged;

		private readonly GUIContent labelPackTag = new GUIContent("Packing Tag");
		private readonly GUIContent labelPixelUnit = new GUIContent("Pixels Per Unit");
		private readonly GUIContent labelAlignment = new GUIContent("Pivot");
		private readonly GUIContent labelFile = new GUIContent("Import PSD");
		private readonly GUIContent labelShowImport = new GUIContent("Import Settings");
		private readonly GUIContent labelScale = new GUIContent("Scale Factor");
		private readonly GUIContent labelPath = new GUIContent("Import Path");
		private readonly GUIContent labelPickPath = new GUIContent("Open");
		private readonly GUIContent labelAutoImport = new GUIContent("Auto Import");

		fsSerializer serializer;
		private Type typeTex2D, typeImportUserData;
		private bool showImportSettings;

		private void OnEnable()
		{
			typeTex2D = typeof(Texture2D);
			typeImportUserData = typeof (ImportUserData);
			serializer = new fsSerializer();
		}

		public void OpenFile(Object fileObject)
		{
			importFile = null;
			importPath = string.Empty;
			importPPU = 100;
			importSettings = new ImportUserData();

			var filePath = AssetDatabase.GetAssetPath(fileObject);
			if (filePath.ToLower().EndsWith(".psd") == false)
			{
				return;
			}

			importFile = fileObject;
			importPath = filePath;

			// Read the texture import settings of the asset file
			TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(importPath);
			TextureImporterSettings unityImportSettings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(unityImportSettings);

			importPPU = unityImportSettings.spritePixelsPerUnit;

			// Attempt to deserialize
			string json = textureImporter.userData;
			bool didGetUserData = false;
			if (string.IsNullOrEmpty(json) == false)
			{
				fsData data = fsJsonParser.Parse(json);
				object deserialObj = null;
				if (serializer.TryDeserialize(data, typeImportUserData, ref deserialObj)
							.AssertSuccessWithoutWarnings()
							.Succeeded)
				{
					importSettings = (ImportUserData) deserialObj;
					if (importSettings == null)
						importSettings = new ImportUserData();
					else
						didGetUserData = true;
				}
			}

			if (didGetUserData)
			{
				settingsChanged = false;
			}
			else
			{
				settingsChanged = true;
				showImportSettings = true;
			}
		}

		private void OnGUI()
		{
			float restoreWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = Mathf.Min(restoreWidth, EditorGUIUtility.currentViewWidth * 0.33f);

			EditorGUI.BeginChangeCheck();
			GUIContent fileLabel = importFile == null ? labelFile : new GUIContent(importFile.name);
			importFile = EditorGUILayout.ObjectField(fileLabel, importFile, typeTex2D, false);
			if (EditorGUI.EndChangeCheck())
				OpenFile(importFile);

			DrawImportSettings();

			EditorGUIUtility.labelWidth = restoreWidth;
		}

		private void DrawImportSettings()
		{
			showImportSettings = EditorGUILayout.Foldout(showImportSettings, labelShowImport);
			if (showImportSettings == false)
				return;

			EditorGUI.indentLevel++;
			EditorGUI.BeginDisabledGroup(importFile == null);

			EditorGUI.BeginChangeCheck();

			DrawPathPicker();

			importSettings.PackingTag = EditorGUILayout.TextField(labelPackTag, importSettings.PackingTag);
			importPPU = EditorGUILayout.FloatField(labelPixelUnit, importPPU);

			SpriteAlignUI.DrawGUILayout(labelAlignment, importSettings.DefaultAlignment, newAlign =>
			{
				importSettings.DefaultAlignment = newAlign;
				settingsChanged = true;
				Repaint();
			});

			if (importSettings.DefaultAlignment == SpriteAlignment.Custom)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Space(EditorGUIUtility.labelWidth);
					importSettings.DefaultPivot = EditorGUILayout.Vector2Field(GUIContent.none, importSettings.DefaultPivot);
				}
			}

			importSettings.ScaleFactor = (ScaleFactor) EditorGUILayout.EnumPopup(labelScale, importSettings.ScaleFactor);
			importSettings.AutoImport = EditorGUILayout.Toggle(labelAutoImport, importSettings.AutoImport);

			if (EditorGUI.EndChangeCheck())
			{
				settingsChanged = true;
				importSettings.DefaultPivot.x = Mathf.Clamp01(importSettings.DefaultPivot.x);
				importSettings.DefaultPivot.y = Mathf.Clamp01(importSettings.DefaultPivot.y);
			}

			if (settingsChanged)
			{
				if (GUILayout.Button("Apply"))
					WriteImportSettings();
			}

			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}

		private void DrawPathPicker()
		{
			using (new GUILayout.HorizontalScope())
			{
				//EditorGUILayout.PrefixLabel("Import Path");

				string displayPath = importSettings.TargetDirectory;
				if (string.IsNullOrEmpty(displayPath) == false)
				{
					if (displayPath.StartsWith("Assets/"))
						displayPath = displayPath.Substring(7);
				}
				
				EditorGUI.BeginChangeCheck();
				string userPath = EditorGUILayout.DelayedTextField(labelPath, displayPath, GUILayout.ExpandWidth(true));
				if (EditorGUI.EndChangeCheck())
				{
					userPath = string.Format("Assets/{0}", userPath);
					if (AssetDatabase.IsValidFolder(userPath))
						importSettings.TargetDirectory = userPath;
				}

				if (GUILayout.Button(labelPickPath, EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
				{
					GUI.FocusControl(null);
					string openPath = importSettings.TargetDirectory;
					if (string.IsNullOrEmpty(openPath))
					{
						openPath = importPath.Substring(0, importPath.LastIndexOf("/"));
					}

					openPath = EditorUtility.SaveFolderPanel("Export Path", openPath, "");

					int inPath = openPath.IndexOf(Application.dataPath);
					if (inPath < 0 || Application.dataPath.Length == openPath.Length)
						openPath = string.Empty;
					else
						openPath = openPath.Substring(Application.dataPath.Length + 1);

					importSettings.TargetDirectory = openPath;
				}
			}
		}

		private void WriteImportSettings()
		{

			// Read the texture import settings of the asset file
			TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(importPath);
			TextureImporterSettings settings = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(settings);

			settings.spritePixelsPerUnit = importPPU;

			textureImporter.SetTextureSettings(settings);

			fsData data;
			if (serializer.TrySerialize(typeImportUserData, importSettings, out data)
				.AssertSuccessWithoutWarnings()
				.Succeeded)
			{
				textureImporter.userData = fsJsonPrinter.CompressedJson(data);
			}

			EditorUtility.SetDirty(importFile);
			AssetDatabase.WriteImportSettingsIfDirty(importPath);
			AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceUpdate);

			settingsChanged = false;
		}
	}
}