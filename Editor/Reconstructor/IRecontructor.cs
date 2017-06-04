using System.Collections.Generic;
using UnityEngine;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	public struct ReconstuctData
	{
		public Dictionary<int[], Sprite> spriteIndex;
		public Vector2 documentSize;
		public SpriteAlignment documentAnchor;
		public float documentPPU;
		public Dictionary<int[], Rect> layerBoundsIndex; 
	}

	public interface IRecontructor
	{
		/// <summary>
		/// Name to display in UI
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Given the current hierarchy selection, determine if hierarchy can be created there
		/// </summary>
		/// <param name="selection"></param>
		/// <returns></returns>
		bool CanReconstruct(GameObject selection);

		void Reconstruct(ImportLayerData root, ReconstuctData data);
	}
}