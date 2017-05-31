using System;
using UnityEngine;

namespace SubjectNerd.PsdImporter
{
	public enum ScaleFactor
	{
		Full,
		Half,
		Quarter
	}

	[Serializable]
	public struct ImportLayerData
	{
		public string pathId;
		public string name;
		public SpriteAlignment Alignment;
		public Vector2 Pivot;
		public ScaleFactor ScaleFactor;

		public ImportLayerData[] ChildLayers;
	}

	[Serializable]
	public class ImportUserData
	{
		public string PackingTag;
		public string TargetDirectory;
		public SpriteAlignment DefaultAlignment = SpriteAlignment.Center;
		public Vector2 DefaultPivot = new Vector2(0.5f, 0.5f);
		public ScaleFactor ScaleFactor = ScaleFactor.Full;

	}
}