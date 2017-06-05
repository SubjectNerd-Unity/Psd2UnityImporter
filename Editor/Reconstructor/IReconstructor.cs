using System.Collections.Generic;
using UnityEngine;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	public struct ReconstructData
	{
		public Dictionary<int[], Sprite> spriteIndex;
		public Dictionary<int[], Vector2> spriteAnchors; 
		public Dictionary<int[], Rect> layerBoundsIndex;
		public Vector2 documentSize;
		public Vector2 documentPivot;
		public float documentPPU;

		public ReconstructData(Vector2 docSize, Vector2 docPivot, float PPU)
		{
			spriteIndex = new Dictionary<int[], Sprite>();
			spriteAnchors = new Dictionary<int[], Vector2>();
			layerBoundsIndex = new Dictionary<int[], Rect>();
			documentPivot = docPivot;
			documentSize = docSize;
			documentPPU = PPU;
		}

		public void AddSprite(int[] layerIdx, Sprite sprite, Vector2 anchor)
		{
			spriteIndex.Add(layerIdx, sprite);
			spriteAnchors.Add(layerIdx, anchor);
		}
	}

	public interface IReconstructor
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

		GameObject Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection);

		/// <summary>
		/// Message to display when CanReconstruct returns false
		/// </summary>
		string HelpMessage { get; }
	}
}