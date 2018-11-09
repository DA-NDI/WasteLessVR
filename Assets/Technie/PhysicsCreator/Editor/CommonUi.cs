using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Technie.PhysicsCreator
{
	public class CommonUi
	{
		public static void DrawGenerateOrReconnectGui(GameObject selectedObject, Mesh srcMesh)
		{
			if (GUILayout.Button("Generate Asset"))
			{
				GenerateAsset(selectedObject, srcMesh);
			}

			GUILayout.Label("Or reconnect to existing asset:");

			PaintingData newPaintingData = (PaintingData)EditorGUILayout.ObjectField(null, typeof(PaintingData), false);
			if (newPaintingData != null)
			{
				Reconnect(selectedObject, newPaintingData);
			}
		}

		private static void GenerateAsset(GameObject selectedObject, Mesh srcMesh)
		{
			if (!AssetDatabase.IsValidFolder("Assets/Physics Hulls"))
				AssetDatabase.CreateFolder("Assets", "Physics Hulls");

			string path = "Assets/Physics Hulls/";

			// Find suitable asset names
			string paintAssetName, hullAssetName;
			CreateAssetPaths (path, selectedObject.name, out paintAssetName, out hullAssetName);

			// Painting asset
			PaintingData painting = ScriptableObject.CreateInstance<PaintingData>();
			painting.sourceMesh = srcMesh;
			AssetDatabase.CreateAsset(painting, paintAssetName);

			// Mesh asset
			HullData hulls = ScriptableObject.CreateInstance<HullData>();
			AssetDatabase.CreateAsset(hulls, hullAssetName);

			// Connect the painting data to the hull data

			painting.hullData = hulls;

			// Get the hull painter (or create one if it doesn't exist)

			HullPainter selectedPainter = selectedObject.GetComponent<HullPainter>();
			if (selectedPainter == null)
				selectedPainter = selectedObject.AddComponent<HullPainter>();

			// Point the painter at the asset data

			selectedPainter.paintingData = painting;
			selectedPainter.hullData = hulls;

			// Start with a single empty hull
			selectedPainter.paintingData.AddHull(HullType.ConvexHull, null, false);

			EditorUtility.SetDirty (painting);
			EditorUtility.SetDirty (hulls);

			// Ping the painting asset in the ui (can only ping one object at once, so do the more important one)
			EditorGUIUtility.PingObject(painting);

			EditorWindow.GetWindow(typeof(HullPainterWindow));
		}

		private static void CreateAssetPaths(string basePath, string baseName, out string paintingAssetPath, out string hullAssetPath)
		{
			paintingAssetPath = basePath + baseName + " Painting Data.asset";
			hullAssetPath = basePath + baseName + " Hull Data.asset";

		//	string t = AssetDatabase.AssetPathToGUID("Diesbt exist");
		//	Debug.Log ("Test: '"+t+"'");

			int nextNumber = 0;

			while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(paintingAssetPath)) || !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(hullAssetPath)))
			{
				nextNumber++;
				paintingAssetPath = basePath + baseName + " " + nextNumber + " Painting Data.asset";
				hullAssetPath = basePath + baseName + " " + nextNumber + " Hull Data.asset";
			}
		}

		public static void Reconnect(GameObject selectedObject, PaintingData newPaintingData)
		{
			Debug.Log("Reconnect "+selectedObject.name+" to "+newPaintingData.name);

			// Get the hull painter (or create one if it doesn't exist)

			HullPainter hullPainter = selectedObject.GetComponent<HullPainter>();
			if (hullPainter == null)
				hullPainter = selectedObject.AddComponent<HullPainter>();

			// Point the hull painter at the assets

			hullPainter.paintingData = newPaintingData;
			hullPainter.hullData = newPaintingData.hullData;

			EditorWindow.GetWindow (typeof(HullPainterWindow)).Repaint ();
		}
	}

} // namespace Technie.PhysicsCreator
