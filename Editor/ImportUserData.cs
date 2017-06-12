/*
MIT License

Copyright (c) 2017 Jeiel Aranal

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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