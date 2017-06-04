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

		private readonly List<int[]> importLayersList = new List<int[]>();
		private readonly List<int[]> quickSelect = new List<int[]>();
		private int selectionCount = 0;

		fsSerializer serializer;
		private Type typeTex2D, typeImportUserData;
		private bool showImportSettings;
		private int[] lastSelectedLayer;

		private readonly Dictionary<int[], Rect> layerRectLookup = new Dictionary<int[], Rect>();
		private bool isChangingSelection;
		private bool selectionChangeState;
		private string searchFilter = "";

		#region UI fields
		private readonly GUIContent labelHeader = new GUIContent("Import PSD Layers");
		private readonly GUIContent labelFileNaming = new GUIContent("File Names");
		private readonly GUIContent labelGrpMode = new GUIContent("Group Mode");
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
		private GUIStyle styleHeader, styleBoldFoldout,
						styleLayerEntry, styleLayerSelected,
						styleVisOn, styleVisOff,
						styleToolbar, styleToolSearch, styleToolCancel;
		private Texture2D icnFolder, icnTexture;
		private GUILayoutOption layerHeight;
		private GUILayoutOption noExpandW;

		// Column widths
		private Rect rImportToggle, rVisible, rLayerDisplay, rPivot, rScaling, rMakeDefault;
		private Vector2 rTableSize;
		private bool tableWillScroll;
		private float layerEntryYMax;

		private Vector2 scrollPos;
		private int indentLevel = 0;
		private const float indentWidth = 20;

		private void LoadStyles()
		{
			if (stylesLoaded)
				return;

			layerHeight = GUILayout.Height(EditorGUIUtility.singleLineHeight + 5);
			noExpandW = GUILayout.ExpandWidth(false);
			icnFolder = EditorGUIUtility.FindTexture("Folder Icon");
			icnTexture = EditorGUIUtility.FindTexture("Texture Icon");

			styleHeader = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold
			};

			var tempStyle = GUI.skin.FindStyle("EyeDropperHorizontalLine");

			styleLayerEntry = new GUIStyle(GUI.skin.box)
			{
				margin = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(0, 0, 0, 0),
				contentOffset = new Vector2(0, 0),
				normal = new GUIStyleState() { background = tempStyle.normal.background }
			};

			styleLayerSelected = new GUIStyle(GUI.skin.box)
			{
				margin = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(0, 0, 0, 0),
				contentOffset = new Vector2(0, 0)
			};

			styleBoldFoldout = new GUIStyle(EditorStyles.foldout)
			{
				fontStyle = FontStyle.Bold
			};

			tempStyle = GUI.skin.FindStyle("VisibilityToggle");

			styleVisOff = new GUIStyle(tempStyle)
			{
				margin = new RectOffset(10, 10, 3, 3)
			};
			styleVisOn = new GUIStyle(tempStyle)
			{
				normal = new GUIStyleState() { background = tempStyle.onNormal.background },
				margin = new RectOffset(10, 10, 3, 3)
			};

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

			selectionCount = 0;
			quickSelect.Clear();
			lastSelectedLayer = null;

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
				CollateImportList();
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
			storedData.Iterate(
				layerCallback: layer =>
				{
					if (storedIndex.ContainsKey(layer.path) == false)
						storedIndex.Add(layer.path, layer);
					else
						storedIndex[layer.path] = layer;
				}
			);

			// Iterate through the built data now, checking for settings from storedIndex
			builtData.Iterate(
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
				}
			);

			return builtData;
		}

		private void OnGUI()
		{
			LoadStyles();

			DrawLayerTable();

			DrawPsdOperations();

			DrawImportSettings();

			EditorGUILayout.Space();
		}

		private float CalculateColumns()
		{
			// Calculate the sizes for the columns
			var width = EditorGUIUtility.currentViewWidth;
			var height = EditorGUIUtility.singleLineHeight + 4;
			rImportToggle = new Rect(10, 2, 16, height);
			rVisible = new Rect(rImportToggle.xMax + 3, 3, 10, height);
			rMakeDefault = new Rect(0, 2, 20, EditorGUIUtility.singleLineHeight);

			var distWidth = width - rMakeDefault.width - rVisible.xMax;
			distWidth -= tableWillScroll ? 40 : 25;

			var pivotWidth = Mathf.Clamp(distWidth * 0.2f, 95, 120);
			var scaleWidth = Mathf.Clamp(distWidth * 0.2f, 65, 120);
			var layerWidth = Mathf.Max(distWidth - pivotWidth - scaleWidth, 150);
			rLayerDisplay = new Rect(rVisible.xMax + 10, 0, layerWidth, height);
			rPivot = new Rect(rLayerDisplay.xMax + 3, 2, pivotWidth, height);
			rScaling = new Rect(rPivot.xMax + 3, 2, scaleWidth, height);
			rMakeDefault.x = rScaling.xMax + 3;

			rTableSize = new Vector2(rMakeDefault.xMax, height);
			return width;
		}

		private void DrawLayerTable()
		{
			using (new EditorGUILayout.HorizontalScope(styleToolbar))
			{
				searchFilter = EditorGUILayout.TextField(searchFilter, styleToolSearch);
				if (GUILayout.Button(GUIContent.none, styleToolCancel))
				{
					searchFilter = string.Empty;
					GUI.FocusControl(null);
				}

				GUILayout.FlexibleSpace();

				EditorGUILayout.LabelField(labelHeader, styleHeader);

				GUILayout.FlexibleSpace();
			}

			var width = CalculateColumns();
			
			var rHeader = GUILayoutUtility.GetRect(width, rTableSize.x, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
			GUI.Box(rHeader, GUIContent.none, styleToolbar);
			GUI.Label(new Rect(rLayerDisplay) { y = rHeader.y, x = rLayerDisplay.x - scrollPos.x }, "Layer");
			GUI.Label(new Rect(rPivot) { y = rHeader.y, x = rPivot.x - scrollPos.x }, "Pivot");
			GUI.Label(new Rect(rScaling) { y = rHeader.y, x = rScaling.x - scrollPos.x }, "Scale");
			
			using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
			{
				scrollPos = scrollView.scrollPosition;
				DrawLayerContents();
			}

			if (Event.current.type == EventType.Repaint)
			{
				var scrollArea = GUILayoutUtility.GetLastRect();
				var newWillScroll = layerEntryYMax > scrollArea.yMax;
				if (newWillScroll != tableWillScroll)
				{
					tableWillScroll = newWillScroll;
					Repaint();
				}
			}

			using (new EditorGUI.DisabledGroupScope(importFile == null))
			{
				var btnW = GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / 2f);
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Save Selection", btnW))
					{
						AddQuickSelect();
						Repaint();
						WriteImportSettings();
					}

					if (GUILayout.Button("Clear Selection", btnW))
					{
						quickSelect.Clear();
						selectionCount = 0;
						lastSelectedLayer = null;
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					var bigButton = GUILayout.Height(30);
					string textImport = string.Format("Import Saved ({0})", importLayersList.Count);
					string textQuickImport = string.Format("Import Selected ({0})", selectionCount);

					if (GUILayout.Button(textImport, bigButton, btnW))
						PsdImporter.ImportLayersUI(importFile, importSettings, importLayersList);

					if (GUILayout.Button(textQuickImport, bigButton, btnW))
						PsdImporter.ImportLayersUI(importFile, importSettings, quickSelect);
				}
			}

			GUILayout.Box(GUIContent.none, GUILayout.Height(4f), GUILayout.ExpandWidth(true));
		}

		private void DrawLayerContents()
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
			layerRectLookup.Clear();
			layerEntryYMax = 0;
			
			layerData.Iterate(
				layerCallback: layer =>
				{
					var display = GetDisplayData(layer.indexId);
					if (display == null)
						return;

					if (string.IsNullOrEmpty(searchFilter) == false)
					{
						if (layer.name.ToLower().Contains(searchFilter.ToLower()) == false)
							return;
					}

					DrawLayerEntry(layer, display);
				},
				canEnterGroup: layer =>
				{
					var display = GetDisplayData(layer.indexId);
					if (display == null)
						return true;
					return display.isOpen;
				},
				enterGroupCallback: data => indentLevel++,
				exitGroupCallback: data => indentLevel--
			);

			// Process mouse events

			var evt = Event.current;
			bool willChangeSelection = false;
			int[] targetLayer = null;
			foreach (var kvp in layerRectLookup)
			{
				if (kvp.Value.Contains(evt.mousePosition))
				{
					var checkLayer = GetLayerData(kvp.Key);
					if (checkLayer == null || checkLayer.Childs == null)
						continue;

					bool layerIsGroup = checkLayer.Childs.Count > 0;

					if (evt.type == EventType.MouseDown)
					{
						willChangeSelection = layerIsGroup == false;
						bool willAddSelect = quickSelect.Contains(kvp.Key) == false;
						if (willAddSelect)
							lastSelectedLayer = kvp.Key;

						if (willChangeSelection)
						{
							isChangingSelection = true;
							targetLayer = kvp.Key;
							selectionChangeState = willAddSelect;
						}
						else
						{
							RecursiveQuickSelect(checkLayer, willAddSelect);
							if (willAddSelect == false)
								lastSelectedLayer = null;
							Repaint();
						}
					}

					if (evt.type == EventType.MouseUp)
					{
						isChangingSelection = false;
					}

					if (isChangingSelection && evt.type == EventType.MouseDrag && layerIsGroup == false)
					{
						willChangeSelection = true;
						targetLayer = kvp.Key;
					}
					break;
				}
			}

			if (willChangeSelection && targetLayer != null)
			{
				SetQuickSelect(targetLayer, selectionChangeState);
				GetSelectCount();
				Repaint();
			}
		}

		private void DrawLayerEntry(ImportLayerData layer, DisplayLayerData display)
		{
			bool isGroup = display.isGroup;
			bool isSelected = quickSelect.Contains(layer.indexId);
			GUIStyle entryStyle = isSelected ? styleLayerSelected : styleLayerEntry;

			using (new GUILayout.HorizontalScope(entryStyle, layerHeight))
			{
				Rect rEntry = GUILayoutUtility.GetRect(rTableSize.x, rTableSize.x, rTableSize.y, rTableSize.y);

				Rect rToggle = new Rect(rImportToggle);
				Rect rVis = new Rect(rVisible);
				Rect rLayer = new Rect(rLayerDisplay);
				Rect rPiv = new Rect(rPivot);
				Rect rScale = new Rect(rScaling);
				Rect rReset = new Rect(rMakeDefault);
				rToggle.y += rEntry.y;
				rVis.y += rEntry.y;
				rLayer.y += rEntry.y;
				rPiv.y += rEntry.y;
				rScale.y += rEntry.y;
				rReset.y += rEntry.y;

				bool parentWillImport = ParentWillImport(layer.indexId);

				using (new EditorGUI.DisabledScope(parentWillImport == false))
				{
					var displayImport = layer.import && parentWillImport;

					EditorGUI.BeginChangeCheck();
					displayImport = GUI.Toggle(rToggle, displayImport, GUIContent.none);

					if (EditorGUI.EndChangeCheck() && parentWillImport)
					{
						layer.import = displayImport;
						CollateImportList();
						quickSelect.Clear();
						selectionCount = 0;
						lastSelectedLayer = null;
					}
				}

				using (new EditorGUI.DisabledScope(true))
				{
					var visStyle = display.isVisible ? styleVisOn : styleVisOff;
					GUI.Label(rVis, GUIContent.none, visStyle);
				}
				
				rLayer.xMin += indentLevel*indentWidth;

				GUIContent layerContent = new GUIContent()
				{
					image = isGroup ? icnFolder : icnTexture,
					text = layer.name
				};

				bool settingsChanged = false;

				if (isGroup)
				{
					float min, max;
					EditorStyles.popup.CalcMinMaxWidth(layerContent, out min, out max);
					rLayer.width = min;
					display.isOpen = EditorGUI.Foldout(rLayer, display.isOpen, layerContent);
				}
				else
				{
					EditorGUI.LabelField(rLayer, layerContent);

					using (var check = new EditorGUI.ChangeCheckScope())
					{
						layer.Alignment = (SpriteAlignment)EditorGUI.EnumPopup(rPiv, GUIContent.none, layer.Alignment);
						layer.ScaleFactor = (ScaleFactor)EditorGUI.EnumPopup(rScale, GUIContent.none, layer.ScaleFactor);

						if (check.changed)
						{
							layer.useDefaults = false;
							settingsChanged = true;
						}
					}

					using (new EditorGUI.DisabledGroupScope(layer.useDefaults))
					{
						if (GUI.Button(rReset, "R", EditorStyles.miniButton))
						{
							settingsChanged = true;
							layer.useDefaults = true;
							layer.Pivot = importSettings.DefaultPivot;
							layer.Alignment = importSettings.DefaultAlignment;
							layer.ScaleFactor = importSettings.ScaleFactor;
						}
					}
				}
				
				if (settingsChanged && quickSelect.Contains(layer.indexId))
				{
					QuickSelectApplySettings(layer.Alignment, layer.Pivot, layer.ScaleFactor, layer.useDefaults);
				}
			}

			Rect layerRect = GUILayoutUtility.GetLastRect();
			layerRect.xMin += 40;
			layerRectLookup.Add(layer.indexId, layerRect);
			layerEntryYMax = Mathf.Max(layerEntryYMax, layerRect.yMax);
		}

		private void DrawPsdOperations()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				//using (new EditorGUILayout.VerticalScope())
				//{
				//	string strButton = "Reconstruct";
				//	bool canReconstruct = false;
				//	if (lastSelectedLayer != null)
				//	{
				//		var selLayer = GetLayerData(lastSelectedLayer);
				//		if (selLayer != null && selLayer.Childs.Count > 0)
				//		{
				//			canReconstruct = true;
				//			strButton = string.Format("Reconstruct {0}", selLayer.name);
				//		}
				//	}

				//	using (new EditorGUI.DisabledGroupScope(canReconstruct == false))
				//		GUILayout.Button(strButton);
				//}

				using (new EditorGUILayout.VerticalScope())
				{
					GUIContent fileLabel = importFile == null ? labelFile : new GUIContent(importFile.name);

					EditorGUI.BeginChangeCheck();
					importFile = EditorGUILayout.ObjectField(fileLabel, importFile, typeTex2D, false);
					if (EditorGUI.EndChangeCheck())
						OpenFile(importFile);
				}
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
			
			DrawPathPicker();

			importSettings.fileNaming = (NamingConvention)EditorGUILayout.EnumPopup(labelFileNaming, importSettings.fileNaming);

			if (importSettings.fileNaming != NamingConvention.LayerNameOnly)
			{
				EditorGUI.indentLevel++;
				importSettings.groupMode = (GroupMode) EditorGUILayout.EnumPopup(labelGrpMode, importSettings.groupMode);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();

			importSettings.PackingTag = EditorGUILayout.TextField(labelPackTag, importSettings.PackingTag);
			importPPU = EditorGUILayout.FloatField(labelPixelUnit, importPPU);

			SpriteAlignUI.DrawGUILayout(labelAlignment, importSettings.DefaultAlignment, newAlign =>
			{
				importSettings.DefaultAlignment = newAlign;
				settingsChanged = true;
				ApplyDefaultSettings();
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
				ApplyDefaultSettings();
			}

			using (new EditorGUI.DisabledGroupScope(settingsChanged == false))
			{
				if (GUILayout.Button("Apply"))
					WriteImportSettings();
			}

			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}

		private void QuickSelectApplySettings(SpriteAlignment alignment, Vector2 pivot, ScaleFactor scale, bool defaults)
		{
			if (defaults)
			{
				alignment = importSettings.DefaultAlignment;
				pivot = importSettings.DefaultPivot;
				scale = importSettings.ScaleFactor;
			}
			foreach (int[] layerIdx in quickSelect)
			{
				var layer = GetLayerData(layerIdx);
				if (layer == null || layer.Childs.Count > 0)
					continue;
				layer.useDefaults = defaults;
				layer.Alignment = alignment;
				layer.Pivot = pivot;
				layer.ScaleFactor = scale;
			}
		}

		private void ApplyDefaultSettings()
		{
			importSettings.DocRoot.Iterate(
				layerCallback: layer =>
				{
					if (layer.useDefaults)
					{
						layer.Alignment = importSettings.DefaultAlignment;
						layer.Pivot = importSettings.DefaultPivot;
						layer.ScaleFactor = importSettings.ScaleFactor;
					}
				}
			);
			Repaint();
		}

		private void DrawPathPicker()
		{
			using (new GUILayout.HorizontalScope())
			{
				//EditorGUILayout.PrefixLabel("Import Path");

				string displayPath = importSettings.TargetDirectory;

				EditorGUI.BeginChangeCheck();
				string userPath = EditorGUILayout.DelayedTextField(labelPath, displayPath, GUILayout.ExpandWidth(true));
				if (EditorGUI.EndChangeCheck())
				{
					if (string.IsNullOrEmpty(userPath) == false)
						userPath = string.Format("Assets/{0}", userPath);
					else
						userPath = importPath.Substring(0, importPath.LastIndexOf("/"));
					if (AssetDatabase.IsValidFolder(userPath))
						importSettings.TargetDirectory = userPath;
				}

				if (GUILayout.Button(labelPickPath, EditorStyles.miniButtonRight, noExpandW))
				{
					GUI.FocusControl(null);
					string openPath = importSettings.TargetDirectory;
					if (string.IsNullOrEmpty(openPath))
						openPath = importPath.Substring(0, importPath.LastIndexOf("/"));

					openPath = EditorUtility.SaveFolderPanel("Export Path", openPath, "");

					int inPath = openPath.IndexOf(Application.dataPath);
					if (inPath < 0 || Application.dataPath.Length == openPath.Length)
						openPath = string.Empty;
					else
						openPath = openPath.Substring(Application.dataPath.LastIndexOf("/") + 1);

					importSettings.TargetDirectory = openPath;
				}
			}
		}

		private void RecursiveQuickSelect(ImportLayerData layer, bool inSelection)
		{
			SetQuickSelect(layer.indexId, inSelection);
			layer.Iterate(
				childLayer =>
				{
					if (layer.Childs == null)
						return;
					if (childLayer.import)
						SetQuickSelect(childLayer.indexId, inSelection);
				}
			);
			GetSelectCount();
		}

		private void SetQuickSelect(int[] layerIdx, bool inSelection)
		{
			bool layerInSelection = quickSelect.Contains(layerIdx);
			if (inSelection)
			{
				if (layerInSelection == false)
					quickSelect.Add(layerIdx);
			}
			else
			{
				if (layerInSelection)
					quickSelect.Remove(layerIdx);
			}
		}

		private void AddQuickSelect()
		{
			foreach (int[] layerIdx in quickSelect)
			{
				var setLayer = GetLayerData(layerIdx);
				if (setLayer == null)
					continue;
				setLayer.import = true;
			}
			quickSelect.Clear();
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

		private void CollateImportList()
		{
			importLayersList.Clear();
			importSettings.DocRoot.Iterate(
				layerCallback: layer =>
				{
					if (layer.import && layer.Childs.Count == 0)
						importLayersList.Add(layer.indexId);
				},
				canEnterGroup: layer => layer.import
			);
		}

		private void GetSelectCount()
		{
			selectionCount = 0;
			foreach (int[] layerIdx in quickSelect)
			{
				var layer = GetLayerData(layerIdx);
				if (layer == null)
					continue;
				if (layer.Childs.Count == 0)
					selectionCount++;
			}
		}

		private DisplayLayerData GetDisplayData(int[] layerIdx)
		{
			if (importDisplay == null || layerIdx == null)
				return null;

			DisplayLayerData currentLayer = importDisplay;
			foreach (int idx in layerIdx)
			{
				if (idx < 0 || idx >= currentLayer.Childs.Count)
					return null;
				currentLayer = currentLayer.Childs[idx];
			}
			return currentLayer;
		}

		private ImportLayerData GetLayerData(int[] layerIdx)
		{
			if (importSettings == null || layerIdx == null)
				return null;

			return importSettings.GetLayerData(layerIdx);
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
			CollateImportList();
		}
	}
}