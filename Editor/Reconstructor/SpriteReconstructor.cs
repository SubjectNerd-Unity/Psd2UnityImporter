using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SubjectNerd.PsdImporter.Reconstructor
{
	public class SpriteReconstructor : IRecontructor
	{
		private const string DISPLAY_NAME = "Unity Sprites";

		public string DisplayName { get { return DISPLAY_NAME; } }

		public bool CanReconstruct(GameObject selection)
		{
			return true;
		}

		private Vector2 GetLayerPosition(ReconstructData data, int[] layerIdx)
		{
			Rect layerRect;
			if (data.layerBoundsIndex.TryGetValue(layerIdx, out layerRect) == false)
				return Vector2.zero;

			Vector2 layerAnchor;
			if (data.spriteAnchors.TryGetValue(layerIdx, out layerAnchor) == false)
				return Vector2.zero;

			Vector2 layerPos = new Vector2(Mathf.Lerp(layerRect.xMin, layerRect.xMax, layerAnchor.x),
											Mathf.Lerp(layerRect.yMin, layerRect.yMax, layerAnchor.y));
			return layerPos;
		}

		public void Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection)
		{
			GameObject rootObject = new GameObject(root.name);
			if (selection != null)
				rootObject.transform.SetParent(selection.transform);

			Stack<Transform> hierarchy = new Stack<Transform>();
			hierarchy.Push(rootObject.transform);

			Vector2 docRoot = data.documentSize;
			docRoot.x *= data.documentPivot.x;
			docRoot.y *= data.documentPivot.y;

			int sortIdx = 0;
			root.Iterate(
				layer =>
				{
					if (layer.Childs.Count > 0)
						return;
					GameObject layerObject = new GameObject(layer.name);
					Transform layerT = layerObject.transform;
					layerT.SetParent(hierarchy.Peek());
					layerT.SetAsLastSibling();

					Sprite layerSprite;
					if (data.spriteIndex.TryGetValue(layer.indexId, out layerSprite))
					{
						SpriteRenderer layerRender = layerObject.AddComponent<SpriteRenderer>();
						layerRender.sprite = layerSprite;
						layerRender.sortingOrder = sortIdx;
						sortIdx--;
					}

					Vector2 layerPos = GetLayerPosition(data, layer.indexId);
					Vector2 layerVector = layerPos - docRoot;
					layerVector /= data.documentPPU;
					layerT.position = layerVector;
				},
				checkGroup => checkGroup.import,
				enterGroupCallback: layer =>
				{
					GameObject groupObject = new GameObject(layer.name);
					Transform groupT = groupObject.transform;
					groupT.SetParent(hierarchy.Peek());

					hierarchy.Push(groupT);
				},
				exitGroupCallback: layer =>
				{
					hierarchy.Pop();
				});

#if UNITY_5_6_OR_NEWER
			rootObject.AddComponent<SortingGroup>();
#endif
		}
	}
}