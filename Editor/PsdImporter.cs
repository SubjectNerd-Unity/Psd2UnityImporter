using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SubjectNerd.PsdImporter.PsdParser;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SubjectNerd.PsdImporter
{
	public class PsdImporter
	{
		private const string DOC_ROOT = "DOCUMENT_ROOT";
		
		private static IEnumerator ParseLayers(IPsdLayer[] layers, bool doYield,
			Action<PsdLayer, int[]> onLayer, Action onComplete, int[] parentIndex = null)
		{
			// Loop through layers in reverse so they are encountered in same order as Photoshop
			for (int i = layers.Length - 1; i >= 0; i--)
			{
				int[] layerIndex = parentIndex;
				if (layerIndex == null)
				{
					layerIndex = new int[] {i};
				}
				else
				{
					int lastIndex = layerIndex.Length;
					Array.Resize(ref layerIndex, lastIndex + 1);
					layerIndex[lastIndex] = i;
				}
				
				PsdLayer layer = layers[i] as PsdLayer;
				if (layer == null)
					continue;

				if (onLayer != null)
					onLayer(layer, layerIndex);

				if (doYield)
					yield return null;

				if (layer.Childs.Length > 0)
				{
					yield return EditorCoroutineRunner.StartCoroutine(
						ParseLayers(layer.Childs, doYield, onLayer, null, layerIndex)
					);
				}
			}

			if (onComplete != null)
				onComplete();
		}

		public static void BuildImportLayerData(Object file, ImportUserData importSettings,
												Action<ImportLayerData, DisplayLayerData> callback)
		{
			string filepath = AssetDatabase.GetAssetPath(file);
			if (string.IsNullOrEmpty(filepath))
			{
				if (callback != null)
					callback(null, null);
				return;
			}
			
			using (PsdDocument psd = PsdDocument.Create(filepath))
			{
				ImportLayerData docImportData = new ImportLayerData()
				{
					name = DOC_ROOT,
					indexId = new int[] {-1},
					Childs = new List<ImportLayerData>()
				};
				DisplayLayerData docDisplayData = new DisplayLayerData()
				{
					indexId = new int[] {-1},
					Childs = new List<DisplayLayerData>()
				};

				EditorCoroutineRunner.StartCoroutine(
					ParseLayers(psd.Childs, false,
					onLayer: (layer, indexId) =>
					{
						// Walk down the index id to get the parent layers
						// and build the full path
						string fullPath = "";
						ImportLayerData parentLayer = docImportData;
						DisplayLayerData parentDisplay = docDisplayData;
						if (indexId.Length > 1)
						{
							for (int idIdx = 0; idIdx < indexId.Length - 1; idIdx++)
							{
								int idx = indexId[idIdx];
								parentLayer = parentLayer.Childs[idx];
								parentDisplay = parentDisplay.Childs[idx];

								if (string.IsNullOrEmpty(fullPath) == false)
									fullPath += "/";
								fullPath += parentLayer.name;
							}
						}
						
						if (string.IsNullOrEmpty(fullPath) == false)
							fullPath += "/";
						fullPath += layer.Name;

						ImportLayerData layerImportData = new ImportLayerData()
						{
							name = layer.Name,
							path = fullPath,
							indexId = indexId,
							import = layer.IsVisible,
							useDefaults = true,
							Alignment = importSettings.DefaultAlignment,
							Pivot = importSettings.DefaultPivot,
							ScaleFactor = importSettings.ScaleFactor,
							Childs = new List<ImportLayerData>()
						};
						
						DisplayLayerData layerDisplayData = new DisplayLayerData()
						{
							indexId = indexId,
							isVisible = layer.IsVisible,
							isGroup = layer.Childs.Length > 0,
							isOpen = layer.IsFolderOpen
						};

						int layerIdx = indexId[indexId.Length - 1];
						
						int maxLayers = layerIdx + 1;
						while (parentLayer.Childs.Count < maxLayers)
							parentLayer.Childs.Add(null);

						parentLayer.Childs[layerIdx] = layerImportData;

						while (parentDisplay.Childs.Count < maxLayers)
							parentDisplay.Childs.Add(null);

						parentDisplay.Childs[layerIdx] = layerDisplayData;
					},
					onComplete: () =>
					{
						if (callback != null)
							callback(docImportData, docDisplayData);
					})
				);
			}
		}
	}
}