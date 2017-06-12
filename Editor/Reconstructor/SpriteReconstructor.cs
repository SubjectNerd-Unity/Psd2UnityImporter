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
using UnityEngine.Rendering;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	/// <summary>
	/// Reconstructs a PSD document as a hierarchy of SpriteRenderers
	/// </summary>
	public class SpriteReconstructor : IReconstructor
	{
		private const string DISPLAY_NAME = "Unity Sprites";

		public string DisplayName { get { return DISPLAY_NAME; } }

		public bool CanReconstruct(GameObject selection)
		{
			return true;
		}

		private Vector2 GetLayerPosition(ReconstructData data, int[] layerIdx)
		{
			// Get the layer rect and anchor points
			Rect layerRect;
			if (data.layerBoundsIndex.TryGetValue(layerIdx, out layerRect) == false)
				return Vector2.zero;

			Vector2 layerAnchor;
			if (data.spriteAnchors.TryGetValue(layerIdx, out layerAnchor) == false)
				return Vector2.zero;

			// The layer rect is the region the layer occupies in the document
			// Lerp using the anchor point to find the layer position
			Vector2 layerPos = new Vector2(Mathf.Lerp(layerRect.xMin, layerRect.xMax, layerAnchor.x),
											Mathf.Lerp(layerRect.yMin, layerRect.yMax, layerAnchor.y));
			return layerPos;
		}

		public GameObject Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection)
		{
			GameObject rootObject = new GameObject(root.name);
			if (selection != null)
				rootObject.transform.SetParent(selection.transform);

			// Create a stack that represents the current parent
			// as the hierarchy is being traversed
			Stack<Transform> hierarchy = new Stack<Transform>();
			// Add the root object as the first parent
			hierarchy.Push(rootObject.transform);

			// Calculate the document pivot position
			Vector2 docRoot = data.documentSize;
			docRoot.x *= data.documentPivot.x;
			docRoot.y *= data.documentPivot.y;

			// Sorting index accumulator, so sprites draw in correct order
			int sortIdx = 0;
			root.Iterate(
				layer =>
				{
					// Only process non group layers and layers marked for import
					if (layer.Childs.Count > 0 || layer.import == false)
						return;

					// Create an object
					GameObject layerObject = new GameObject(layer.name);
					Transform layerT = layerObject.transform;
					// And attach it to the last
					layerT.SetParent(hierarchy.Peek());
					layerT.SetAsLastSibling();

					// Find the sprite for this layer in the data sprite index
					Sprite layerSprite;
					if (data.spriteIndex.TryGetValue(layer.indexId, out layerSprite))
					{
						// Attach a sprite renderer and set the sorting order
						SpriteRenderer layerRender = layerObject.AddComponent<SpriteRenderer>();
						layerRender.sprite = layerSprite;
						layerRender.sortingOrder = sortIdx;
						sortIdx--;
					}

					// Get the layer position
					Vector2 layerPos = GetLayerPosition(data, layer.indexId);
					// Express it as a vector
					Vector2 layerVector = layerPos - docRoot;
					// This is in pixel units, convert to unity world units
					layerVector /= data.documentPPU;
					layerT.position = layerVector;
				},
				checkGroup => checkGroup.import, // Only enter groups if part of the import
				enterGroupCallback: layer =>
				{
					// Enter a group, create an object for it
					GameObject groupObject = new GameObject(layer.name);
					Transform groupT = groupObject.transform;
					// Parent to the last hierarchy parent
					groupT.SetParent(hierarchy.Peek());
					// Look at me, I'm the hierarchy parent now
					hierarchy.Push(groupT);
				},
				exitGroupCallback: layer =>
				{
					// Go back to the last parent
					hierarchy.Pop();
				});

			// Unity 5.6 introduces the sorting group component
#if UNITY_5_6_OR_NEWER
			rootObject.AddComponent<SortingGroup>();
#endif
			return rootObject;
		}

		public string HelpMessage { get { return string.Empty; } }
	}
}