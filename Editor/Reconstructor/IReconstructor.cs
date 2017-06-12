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