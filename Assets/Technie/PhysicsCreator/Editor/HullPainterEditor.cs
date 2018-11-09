
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/*
	TODO:
		Make overlay behave properly when HullPainter component removed

		Make sure painting has proper undo/redo support

		Move everything into Technie namespace

	Katie test bugs:
		
		Highlight behaves badly with scales (on self or on parent)

		Hull painter still leaking overlay objects / raycast objects

		HullPainter component drops all asset/collider refs when turned into a prefab

		Focus issue prevents painting on first paint after opening window from component

		Reconnect obj to painting data after deletion
	
	Create overlay object with mesh filter + mesh renderer
	Generate mesh with picked triangles, with vertex colours and material to match

	Need to rebuild overlay mesh on selection changed

	FIXME: Use PrefabUtility.GetPrefabType to avoid generating mesh highlights for prefabs in asset dir?

	FIXME: Components lose the references when turned into assets
*/

namespace Technie.PhysicsCreator
{
	[CustomEditor(typeof(HullPainter))]
	public class HullPainterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow window = HullPainterWindow.instance;

				window.OnInspectorGUI();
			}

			HullPainter selectedPainter = SelectionUtil.FindSelectedHullPainter ();
			if (selectedPainter != null)
			{
				if (selectedPainter.paintingData != null
				    && selectedPainter.hullData != null)
				{
					if (GUILayout.Button("Open Hull Painter"))
					{
						EditorWindow.GetWindow(typeof(HullPainterWindow));
					}
				}
				else
				{
					MeshFilter srcMeshFilter = selectedPainter.gameObject.GetComponent<MeshFilter>();
					Mesh srcMesh = srcMeshFilter != null ? srcMeshFilter.sharedMesh : null;
					if (srcMesh != null)
					{
						CommonUi.DrawGenerateOrReconnectGui(selectedPainter.gameObject, srcMesh);
					}
					else
					{
						GUILayout.Label("No mesh on current object!");
					}
				}
			}
		}



		public void OnSceneGUI ()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow window = HullPainterWindow.instance;

				window.OnSceneGUI();

				if (Event.current.commandName == "UndoRedoPerformed")
				{
					window.Repaint();
				}
			}
		}


	}

} // namespace Techie.PhysicsCreator

