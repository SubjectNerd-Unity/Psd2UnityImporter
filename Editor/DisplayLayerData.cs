using System.Collections.Generic;

namespace SubjectNerd.PsdImporter
{
	public class DisplayLayerData
	{
		public int[] indexId;
		public bool isVisible;
		public bool isGroup;
		public bool isOpen;
		public bool isLinked;
		public int[] linkId;

		public List<DisplayLayerData> Childs = new List<DisplayLayerData>();
	}
}