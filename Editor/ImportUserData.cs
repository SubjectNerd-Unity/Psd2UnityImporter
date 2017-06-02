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
		PrefixGroupNames,
		CreateGroupFolders
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
	}
	
	public class ImportUserData
	{
		public NamingConvention fileNaming;
		public string PackingTag;
		public string TargetDirectory;
		public bool AutoImport;
		public SpriteAlignment DefaultAlignment = SpriteAlignment.Center;
		public Vector2 DefaultPivot = new Vector2(0.5f, 0.5f);
		public ScaleFactor ScaleFactor = ScaleFactor.Full;

		public ImportLayerData DocRoot;
	}
}