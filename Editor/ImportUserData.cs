using System;
using System.Collections.Generic;
using UnityEngine;

namespace SubjectNerd.PsdImporter
{
	public enum ScaleFactor
	{
		Full,
		Half,
		Quarter
	}

	public enum NamingConvention
	{
		LayerNameOnly,
		CreateGroupFolders,
        PrefixGroupNames
	}

	public enum GroupMode
	{
		ParentOnly,
		FullPath
	}
	
	public class ImportLayerData
	{
		public string name;
		public string path;
		public int[] indexId;
		public bool import;
		public bool useDefaults;
		public SpriteAlignment Alignment;
		public Vector2 Pivot;
		public ScaleFactor ScaleFactor;

		public List<ImportLayerData> Childs;

		public void Iterate(Action<ImportLayerData> layerCallback,
							Func<ImportLayerData, bool> canEnterGroup = null,
							Action<ImportLayerData> enterGroupCallback = null,
							Action<ImportLayerData> exitGroupCallback = null)
		{
			for (int i = Childs.Count - 1; i >= 0; i--)
			{
				var layer = Childs[i];
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

						layer.Iterate(layerCallback, canEnterGroup, enterGroupCallback, exitGroupCallback);

						if (exitGroupCallback != null)
							exitGroupCallback(layer);
					}
				}
			}
		}
	}
	
	public class ImportUserData
	{
		public NamingConvention fileNaming;
		public GroupMode groupMode;
		public string PackingTag;
		public string TargetDirectory;
		public bool AutoImport;
		public SpriteAlignment DefaultAlignment = SpriteAlignment.Center;
		public Vector2 DefaultPivot = new Vector2(0.5f, 0.5f);
		public ScaleFactor ScaleFactor = ScaleFactor.Full;
		public SpriteAlignment DocAlignment = SpriteAlignment.Center;
		public Vector2 DocPivot = new Vector2(0.5f, 0.5f);

		public ImportLayerData DocRoot;

		public ImportLayerData GetLayerData(int[] layerIdx)
		{
			if (DocRoot == null)
				return null;

			ImportLayerData currentLayer = DocRoot;
			foreach (int idx in layerIdx)
			{
				if (idx < 0 || idx >= currentLayer.Childs.Count)
					return null;
				currentLayer = currentLayer.Childs[idx];
			}
			return currentLayer;
		}
	}
}