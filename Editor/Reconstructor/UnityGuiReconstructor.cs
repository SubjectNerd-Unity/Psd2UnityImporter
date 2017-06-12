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
using UnityEngine.UI;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	public class UnityGuiReconstructor : IReconstructor
	{
		private const string DISPLAY_NAME = "Unity UI";
		public string DisplayName { get {return DISPLAY_NAME;} }
		
		public bool CanReconstruct(GameObject selection)
		{
			if (selection == null)
				return false;

			var canvas = selection.GetComponentInParent<Canvas>();
			return canvas != null;
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
			
			return GetLayerPosition(layerRect, layerAnchor);
		}

		private Vector2 GetLayerPosition(Rect layerRect, Vector2 layerAnchor)
		{
			Vector2 layerPos = new Vector2(Mathf.Lerp(layerRect.xMin, layerRect.xMax, layerAnchor.x),
											Mathf.Lerp(layerRect.yMin, layerRect.yMax, layerAnchor.y));
			return layerPos;
		}

		public GameObject Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection)
		{
			if (selection == null)
				return null;
			if (CanReconstruct(selection) == false)
				return null;

			var rootT = CreateObject(root.name);
			rootT.SetParent(selection.transform);

			rootT.sizeDelta = data.documentSize;
			rootT.pivot = data.documentPivot;
			rootT.localPosition = Vector3.zero;

			// Create a stack that represents the current parent
			// as the hierarchy is being traversed
			Stack<RectTransform> hierarchy = new Stack<RectTransform>();
			// Add the root object as the first parent
			hierarchy.Push(rootT);

			// Calculate the document pivot position
			Vector2 docRoot = data.documentSize;
			docRoot.x *= data.documentPivot.x;
			docRoot.y *= data.documentPivot.y;

			root.Iterate(
				layer =>
				{
					// Only process non group layers or layers marked for import
					if (layer.Childs.Count > 0 || layer.import == false)
						return;

					// Create an object
					RectTransform layerT = CreateObject(layer.name);

					// And attach it to the last parent
					layerT.SetParent(hierarchy.Peek());
					// Order correctly for UI
					layerT.SetAsFirstSibling();

					// Find the sprite for this layer in the data sprite index
					Sprite layerSprite;
					if (data.spriteIndex.TryGetValue(layer.indexId, out layerSprite))
					{
						var layerImg = layerT.gameObject.AddComponent<Image>();
						layerImg.sprite = layerSprite;
					}

					// Get the layer position

					Rect layerRect;
					if (data.layerBoundsIndex.TryGetValue(layer.indexId, out layerRect) == false)
						layerRect = Rect.zero;

					Vector2 layerAnchor;
					if (data.spriteAnchors.TryGetValue(layer.indexId, out layerAnchor) == false)
						layerAnchor = Vector2.zero;

					Vector2 layerPos = GetLayerPosition(layerRect, layerAnchor);
					// Express it as a vector
					Vector2 layerVector = layerPos - docRoot;
					// Position using the rootT as reference
					layerT.position = rootT.TransformPoint(layerVector.x, layerVector.y, 0);

					layerT.pivot = layerAnchor;
					layerT.sizeDelta = new Vector2(layerRect.width, layerRect.height);
				},
				checkGroup => checkGroup.import, // Only enter groups if part of the import
				enterGroupCallback: layer =>
				{
					// Enter a group, create an object for it
					RectTransform groupT = CreateObject(layer.name);
					// Parent to the last hierarchy parent
					groupT.SetParent(hierarchy.Peek());
					groupT.SetAsFirstSibling();

					groupT.anchorMin = Vector2.zero;
					groupT.anchorMax = Vector2.one;
					groupT.offsetMin = Vector2.zero;
					groupT.offsetMax = Vector2.zero;
					
					// Look at me, I'm the hierarchy parent now
					hierarchy.Push(groupT);
				},
				exitGroupCallback: layer =>
				{
					// Go back to the last parent
					hierarchy.Pop();
				});

			return rootT.gameObject;
		}

		private RectTransform CreateObject(string name)
		{
			GameObject rootObject = new GameObject(name);
			RectTransform rectT = rootObject.GetComponent<RectTransform>();
			if (rectT == null)
				rectT = rootObject.AddComponent<RectTransform>();

			return rectT;
		}

		public string HelpMessage { get { return "Select an object in a Unity UI hierarchy"; } }
	}
}