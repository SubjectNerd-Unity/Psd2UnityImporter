using System;
using System.Collections.Generic;
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
			win.titleContent = new GUIContent("PSD Importer");
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

		private bool isOpeningFile;
		private float importPPU = 100;
		private Object importFile;
		private string importPath;
		private ImportUserData importSettings;
		private DisplayLayerData importDisplay;
		private bool settingsChanged;

		fsSerializer serializer;
		private Type typeTex2D, typeImportUserData;
		private bool showImportSettings;

		#region UI fields
		private readonly GUIContent labelLayers = new GUIContent("Layers");
		private readonly GUIContent labelFileNaming = new GUIContent("File Names");
		private readonly GUIContent labelPackTag = new GUIContent("Packing Tag");
		private readonly GUIContent labelPixelUnit = new GUIContent("Pixels Per Unit");
		private readonly GUIContent labelAlignment = new GUIContent("Pivot");
		private readonly GUIContent labelFile = new GUIContent("Import PSD");
		private readonly GUIContent labelShowImport = new GUIContent("Import Settings");
		private readonly GUIContent labelScale = new GUIContent("Scale Factor");
		private readonly GUIContent labelPath = new GUIContent("Import Folder");
		private readonly GUIContent labelPickPath = new GUIContent("Open");
		private readonly GUIContent labelAutoImport = new GUIContent("Auto Import");

		private bool stylesLoaded = false;
		private GUIStyle styleHeader, styleBoldFoldout, styleDivider,
						//styleLayerSelected, styleLabelLeft,
						styleVisOn, styleVisOff,
						styleToolbar, styleToolSearch, styleToolCancel;
		private Texture2D icnFolder, icnTexture;
		private GUILayoutOption layerHeight;

		private Vector2 scrollPos;

		private int indentLevel = 0;
		private const float indentWidth = 20;

		private void LoadStyles()
		{
			if (stylesLoaded)
				return;

			layerHeight = GUILayout.Height(EditorGUIUtility.singleLineHeight + 5);
			icnFolder = EditorGUIUtility.FindTexture("Folder Icon");
			icnTexture = EditorGUIUtility.FindTexture("Texture Icon");

			styleHeader = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold
			};

			//styleLabelLeft = new GUIStyle(GUI.skin.label)
			//{
			//	alignment = TextAnchor.MiddleLeft,
			//	padding = new RectOffset(0, 0, 0, 0)
			//};

			//styleLayerSelected = new GUIStyle(GUI.skin.box)
			//{
			//	margin = new RectOffset(0, 0, 0, 0),
			//	padding = new RectOffset(0, 0, 0, 0),
			//	contentOffset = new Vector2(0, 0)
			//};

			styleBoldFoldout = new GUIStyle(EditorStyles.foldout)
			{
				fontStyle = FontStyle.Bold
			};

			var tempStyle = GUI.skin.FindStyle("VisibilityToggle");

			styleVisOff = new GUIStyle(tempStyle)
			{
				margin = new RectOffset(10, 10, 3, 3)
			};
			styleVisOn = new GUIStyle(tempStyle)
			{
				normal = new GUIStyleState() { background = tempStyle.onNormal.background },
				margin = new RectOffset(10, 10, 3, 3)
			};

			tempStyle = GUI.skin.FindStyle("EyeDropperHorizontalLine");
			styleDivider = new GUIStyle(tempStyle);

			styleToolbar = GUI.skin.FindStyle("Toolbar");
			styleToolSearch = GUI.skin.FindStyle("ToolbarSeachTextField");
			styleToolCancel = GUI.skin.FindStyle("ToolbarSeachCancelButton");

			stylesLoaded = true;
		}
		#endregion

		private void OnEnable()
		{
			stylesLoaded = false;
			typeTex2D = typeof(Texture2D);
			typeImportUserData = typeof(ImportUserData);
			serializer = new fsSerializer();
		}

		public void OpenFile(Object fileObject)
		{
			if (isOpeningFile)
				return;

			importFile = null;
			importPath = string.Empty;
			importPPU = 100;

			importSettings = new ImportUserData();
			importDisplay = null;

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
					importSettings = (ImportUserData)deserialObj;
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

			isOpeningFile = true;
			PsdImporter.BuildImportLayerData(importFile, importSettings, (layerData, displayData) =>
			{
				importSettings.DocRoot = ResolveData(importSettings.DocRoot, layerData);

				importDisplay = displayData;
				isOpeningFile = false;

				Repaint();
			});
		}

		/// <summary>
		/// Resolve setting differences from stored data and built data
		/// </summary>
		/// <param name="storedData"></param>
		/// <param name="builtData"></param>
		/// <returns></returns>
		private ImportLayerData ResolveData(ImportLayerData storedData, ImportLayerData builtData)
		{
			// Nothing was stored, used built data
			if (storedData == null)
				return builtData;

			// Flatten out stored to a dictionary, using the path as keys
			Dictionary<string, ImportLayerData> storedIndex = new Dictionary<string, ImportLayerData>();
			IterateImportLayerData(storedData,
				layerCallback: layer =>
				{
					if (storedIndex.ContainsKey(layer.path) == false)
						storedIndex.Add(layer.path, layer);
					else
						storedIndex[layer.path] = layer;
				});

			// Iterate through the built data now, checking for settings from storedIndex
			IterateImportLayerData(builtData,
				layerCallback: layer =>
				{
					ImportLayerData existingSettings;
					if (storedIndex.TryGetValue(layer.path, out existingSettings))
					{
						layer.useDefaults = existingSettings.useDefaults;
						layer.Alignment = existingSettings.Alignment;
						layer.Pivot = existingSettings.Pivot;
						layer.ScaleFactor = existingSettings.ScaleFactor;
						layer.import = existingSettings.import;
					}
				});

			return builtData;
		}

		private void IterateImportLayerData(ImportLayerData layerData,
			Action<ImportLayerData> layerCallback,
			Func<ImportLayerData, bool> canEnterGroup = null,
			Action<ImportLayerData> enterGroupCallback = null,
			Action<ImportLayerData> exitGroupCallback = null)
		{
			var layerChilds = layerData.Childs;
			for (int i = layerChilds.Count - 1; i >= 0; i--)
			{
				var layer = layerChilds[i];
				if (layer == null)
					continue;

				if (layerCallback != null)
					layerCallback(layer);

				bool isGroup = layer.Childs.Count > 0;

				if (isGroup)
				{
					bool enterGroup = true;
					if (canEnterGroup != null)
						enterGroup = canEnterGroup(layer);

					if (enterGroup)
					{
						if (enterGroupCallback != null)
							enterGroupCallback(layer);

						IterateImportLayerData(layer, layerCallback, canEnterGroup,
							enterGroupCallback, exitGroupCallback);

						if (exitGroupCallback != null)
							exitGroupCallback(layer);
					}
				}
			}
		}

		private void OnGUI()
		{
			LoadStyles();

			DrawLayerTable();

			DrawPsdOperations();

			DrawImportSettings();

			EditorGUILayout.Space();
		}

		private void DrawLayerTable()
		{
			using (new EditorGUILayout.HorizontalScope(styleToolbar))
			{
				EditorGUILayout.DelayedTextField("", styleToolSearch);
				GUILayout.Button(GUIContent.none, styleToolCancel);

				GUILayout.FlexibleSpace();

				EditorGUILayout.LabelField(labelLayers, styleHeader);

				GUILayout.FlexibleSpace();
			}

			using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
			{
				scrollPos = scrollView.scrollPosition;

				DrawLayerContent();
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUI.BeginDisabledGroup(importFile == null);

				var bigButton = GUILayout.Height(30);

				int importCount = 0;
				IterateImportLayerData(importSettings.DocRoot, layer =>
				{
					if (layer.import && layer.Childs.Count == 0)
						importCount++;
				}, layer => layer.import);

				string importText = string.Format("Import Selected ({0})", importCount);

				GUILayout.Button(importText, bigButton);
				GUILayout.Button("<", bigButton, GUILayout.ExpandWidth(false));
				GUILayout.Button("Quick Import", bigButton);

				EditorGUI.EndDisabledGroup();
			}

			GUILayout.Box(GUIContent.none, GUILayout.Height(4f), GUILayout.ExpandWidth(true));
		}

		private void DrawLayerContent()
		{
			if (importFile == null)
			{
				EditorGUILayout.HelpBox("No PSD file loaded", MessageType.Error, true);
				return;
			}

			if (importSettings == null || importSettings.DocRoot == null || importDisplay == null)
			{
				if (importFile != null)
					OpenFile(importFile);
				return;
			}

			DrawIterateLayers(importSettings.DocRoot);
		}

		private void DrawIterateLayers(ImportLayerData layerData)
		{
			IterateImportLayerData(layerData,
				layerCallback: layer =>
				{
					var display = GetDisplayData(layer.indexId);
					if (display == null)
						return;

					bool isGroup = layer.Childs.Count > 0;

					using (new GUILayout.HorizontalScope(styleDivider, layerHeight))
					{
						bool parentNoImport = ParentWillImport(layer.indexId) == false;

						using (new EditorGUI.DisabledGroupScope(parentNoImport))
						{
							var displayImport = layer.import && !parentNoImport;
							EditorGUI.BeginChangeCheck();
							displayImport = GUILayout.Toggle(displayImport, GUIContent.none, GUILayout.ExpandWidth(false));

							if (EditorGUI.EndChangeCheck() && !parentNoImport)
								layer.import = displayImport;
						}

						using (new EditorGUI.DisabledGroupScope(true))
						{
							var visStyle = display.isVisible ? styleVisOn : styleVisOff;
							GUILayout.Label(GUIContent.none, visStyle);
						}

						GUILayout.Space(indentLevel * indentWidth);

						GUIContent layerContent = new GUIContent()
						{
							image = isGroup ? icnFolder : icnTexture,
							text = layer.name
						};

						if (isGroup)
							display.isOpen = EditorGUILayout.Foldout(display.isOpen, layerContent);
						else
							EditorGUILayout.LabelField(layerContent, layerHeight);
					}
				},
				canEnterGroup: layer =>
				{
					var display = GetDisplayData(layer.indexId);
					if (display == null)
						return true;
					return display.isOpen;
				},
				enterGroupCallback: data => indentLevel++,
				exitGroupCallback: data => indentLevel--);
		}

		private bool ParentWillImport(int[] layerIdx)
		{
			bool willImport = true;
			ImportLayerData currentLayer = importSettings.DocRoot;
			for (int i = 0; i < layerIdx.Length - 1; i++)
			{
				int idx = layerIdx[i];
				currentLayer = currentLayer.Childs[idx];
				willImport &= currentLayer.import;
				if (willImport == false)
					break;
			}
			return willImport;
		}

		private DisplayLayerData GetDisplayData(int[] layerIdx)
		{
			if (importDisplay == null || layerIdx == null)
				return null;

			DisplayLayerData currentLayer = importDisplay;
			foreach (int idx in layerIdx)
			{
				currentLayer = currentLayer.Childs[idx];
			}
			return currentLayer;
		}

		private void DrawPsdOperations()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUILayout.VerticalScope())
				{
					if (GUILayout.Button("Save"))
						WriteImportSettings();
				}

				GUIContent fileLabel = importFile == null ? labelFile : new GUIContent(importFile.name);

				EditorGUI.BeginChangeCheck();
				importFile = EditorGUILayout.ObjectField(fileLabel, importFile, typeTex2D, false);
				if (EditorGUI.EndChangeCheck())
					OpenFile(importFile);
			}
		}

		private void DrawImportSettings()
		{
			showImportSettings = EditorGUILayout.Foldout(showImportSettings, labelShowImport, styleBoldFoldout);
			if (showImportSettings == false)
				return;

			EditorGUI.indentLevel++;
			EditorGUI.BeginDisabledGroup(importFile == null);

			EditorGUI.BeginChangeCheck();

			importSettings.fileNaming = (NamingConvention)EditorGUILayout.EnumPopup(labelFileNaming, importSettings.fileNaming);

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

			importSettings.ScaleFactor = (ScaleFactor)EditorGUILayout.EnumPopup(labelScale, importSettings.ScaleFactor);
			importSettings.AutoImport = EditorGUILayout.Toggle(labelAutoImport, importSettings.AutoImport);

			if (EditorGUI.EndChangeCheck())
			{
				settingsChanged = true;
				importSettings.DefaultPivot.x = Mathf.Clamp01(importSettings.DefaultPivot.x);
				importSettings.DefaultPivot.y = Mathf.Clamp01(importSettings.DefaultPivot.y);
			}
			
			using (new EditorGUI.DisabledGroupScope(settingsChanged == false))
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
					if (string.IsNullOrEmpty(userPath) == false)
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