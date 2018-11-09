using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Technie.PhysicsCreator
{
	public class SelectionUtil
	{	
		public static HullPainter FindSelectedHullPainter()
		{
			// Works for components in the scene, causes NPEs for selected prefabs in the assets dir
			if (Selection.transforms.Length == 1)
			{
				GameObject currentSelection = Selection.transforms[0].gameObject;
				return currentSelection.GetComponent<HullPainter>();
			}
			return null;
		}

		public static MeshFilter FindSelectedMeshFilter()
		{
			if (Selection.transforms.Length == 1)
			{
				GameObject currentSelection = Selection.transforms[0].gameObject;
				return currentSelection.GetComponent<MeshFilter>();
			}
			return null;
		}
	}

} // namespace Technie.PhysicsCreator
