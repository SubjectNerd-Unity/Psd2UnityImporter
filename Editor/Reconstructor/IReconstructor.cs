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

using System.Collections.Generic;
using UnityEngine;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	public struct ReconstructData
	{
		/// <summary>
		/// Sprite that corresponds to the layer indexId
		/// </summary>
		public Dictionary<int[], Sprite> spriteIndex;
		/// <summary>
		/// Anchor information that corresponds to the layer indexId.
		/// Ratio of the Sprite size
		/// </summary>
		public Dictionary<int[], Vector2> spriteAnchors;
		/// <summary>
		/// Size and position data of PSD layer, corresponds to the layer indexId.
		/// Data in document pixel space
		/// </summary>
		public Dictionary<int[], Rect> layerBoundsIndex;
		/// <summary>
		/// Size of the PSD, in pixels
		/// </summary>
		public Vector2 documentSize;
		/// <summary>
		/// Pivot position for the PSD document, in ratio of size
		/// </summary>
		public Vector2 documentPivot;
		/// <summary>
		/// Unity pixels per unit value of the PSD document
		/// </summary>
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

		/// <summary>
		/// Function that rebuilds the PSD structure
		/// </summary>
		/// <param name="root">The root of the PSD to reconstruct</param>
		/// <param name="data">Data gathered by the importer for rebuilding the layers with</param>
		/// <param name="selection">Object user has selected</param>
		/// <returns></returns>
		GameObject Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection);

		/// <summary>
		/// Message to display when CanReconstruct returns false
		/// </summary>
		string HelpMessage { get; }
	}
}