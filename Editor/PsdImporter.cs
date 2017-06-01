using System;
using System.Collections;
using SubjectNerd.PsdImporter.PsdParser;

namespace SubjectNerd.PsdImporter
{
	public class PsdImporter 
	{
		private static IEnumerator ParseLayers(PsdLayer[] layers, bool doYield,
			Action<PsdLayer> onLayer, Action onComplete)
		{
			// Loop through layers in reverse so they are encountered in same order as Photoshop
			for (int i = layers.Length - 1; i >= 0; i--)
			{
				PsdLayer layer = layers[i] as PsdLayer;

				if (layer.Childs.Length > 0)
				{
					yield return EditorCoroutineRunner.StartCoroutine(ParseLayers(layer.Childs, doYield, onLayer, null));
				}
				else
				{
					if (onLayer != null)
						onLayer(layer);
					if (doYield)
						yield return null;
				}
			}

			if (onComplete != null)
				onComplete();
		}
	}
}