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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpriteAlignUI : PopupWindowContent
{
	private static string DisplayCamelCaseString(string camelCase)
	{
		List<char> chars = new List<char>();
		chars.Add(camelCase[0]);
		foreach (char c in camelCase.Skip(1))
		{
			if (char.IsUpper(c))
			{
				chars.Add(' ');
				chars.Add(c);
			}
			else
				chars.Add(c);
		}

		return new string(chars.ToArray());
	}

	public static void Popup(Rect rect, SpriteAlignment alignment, Action<SpriteAlignment> callback)
	{
		var popup = new SpriteAlignUI();
		popup.SetData(rect, alignment, callback);
		PopupWindow.Show(rect, popup);
	}

	public static void DrawGUILayout(GUIContent label, SpriteAlignment alignment, Action<SpriteAlignment> callback, params GUILayoutOption[] layoutOptions)
	{
		using (new EditorGUILayout.HorizontalScope(layoutOptions))
		{
			if (label != GUIContent.none)
			{
				EditorGUILayout.PrefixLabel(label);
			}

			float height = EditorGUIUtility.singleLineHeight;
			Rect rect = GUILayoutUtility.GetRect(10, EditorGUIUtility.currentViewWidth,
												height, height, EditorStyles.popup);

			string text = DisplayCamelCaseString(alignment.ToString());

			if (GUI.Button(rect, text, EditorStyles.popup))
				Popup(rect, alignment, callback);
		}
	}

	public static void DrawGUI(Rect position, GUIContent label, SpriteAlignment alignment, Action<SpriteAlignment> callback)
	{
		Rect rectButton = position;
		if (label != GUIContent.none)
			rectButton = EditorGUI.PrefixLabel(position, label);

		string text = DisplayCamelCaseString(alignment.ToString());
		bool didPress = GUI.Button(rectButton, text, EditorStyles.popup);

		if (didPress)
		{
			var popup = new SpriteAlignUI();
			popup.SetData(rectButton, alignment, callback);
			PopupWindow.Show(rectButton, popup);
		}
	}

	private float cellSize;
	private Vector2 windowSize;
	private SpriteAlignment alignment;
	private Action<SpriteAlignment> callback;

	public override Vector2 GetWindowSize()
	{
		return windowSize;
	}

	public override void OnGUI(Rect rect)
	{
		float cellsWidth = cellSize * 3;
		float padding = (rect.width - cellsWidth) / 2;
		Rect row1 = new Rect(rect)
		{
			height = cellSize,
			width = cellsWidth,
			x = rect.x + padding
		};
		Rect row2 = new Rect(row1);
		row2.y += cellSize;
		Rect row3 = new Rect(row2);
		row3.y += cellSize;

		SpriteAlignment originalValue = alignment;
		var isTopLeft = alignment == SpriteAlignment.TopLeft;
		var isTopCenter = alignment == SpriteAlignment.TopCenter;
		var isTopRight = alignment == SpriteAlignment.TopRight;
		var isLeftCenter = alignment == SpriteAlignment.LeftCenter;
		var isCenter = alignment == SpriteAlignment.Center;
		var isRightCenter = alignment == SpriteAlignment.RightCenter;
		var isBottomLeft = alignment == SpriteAlignment.BottomLeft;
		var isBottomCenter = alignment == SpriteAlignment.BottomCenter;
		var isBottomRight = alignment == SpriteAlignment.BottomRight;

		SpriteAlignment[] aligns = { SpriteAlignment.TopLeft, SpriteAlignment.TopCenter, SpriteAlignment.TopRight };
		DrawAlignmentRow(new[] { isTopLeft, isTopCenter, isTopRight }, aligns,
			row1, ref alignment, true);

		aligns = new[] { SpriteAlignment.LeftCenter, SpriteAlignment.Center, SpriteAlignment.RightCenter };
		DrawAlignmentRow(new[] { isLeftCenter, isCenter, isRightCenter }, aligns,
			row2, ref alignment, alignment == originalValue);

		aligns = new[] { SpriteAlignment.BottomLeft, SpriteAlignment.BottomCenter, SpriteAlignment.BottomRight };
		DrawAlignmentRow(new[] { isBottomLeft, isBottomCenter, isBottomRight }, aligns,
			row3, ref alignment, alignment == originalValue);

		Rect rectCustom = new Rect(rect) { yMin = rect.yMax - EditorGUIUtility.singleLineHeight };
		if (GUI.Button(rectCustom, SpriteAlignment.Custom.ToString()))
			alignment = SpriteAlignment.Custom;

		if (alignment != originalValue)
		{
			editorWindow.Close();
			if (callback != null)
				callback(alignment);
		}
	}

	private void DrawAlignmentRow(bool[] vals, SpriteAlignment[] aligns,
		Rect rect, ref SpriteAlignment align, bool allowChange)
	{
		int sel = -1;
		for (int i = 0; i < vals.Length; i++)
		{
			if (vals[i])
			{
				sel = i;
				break;
			}
		}

		int set = GUI.Toolbar(rect, sel, new[] { GUIContent.none, GUIContent.none, GUIContent.none });
		if (set > -1 && allowChange)
		{
			align = aligns[set];
		}
	}

	private void SetData(Rect position, SpriteAlignment alignment, Action<SpriteAlignment> callback)
	{
		this.alignment = alignment;
		this.callback = callback;
		windowSize = position.size;
		cellSize = Mathf.Min(EditorGUIUtility.singleLineHeight, windowSize.x / 3);

		windowSize.y = cellSize * 3 + EditorGUIUtility.singleLineHeight;
	}
}