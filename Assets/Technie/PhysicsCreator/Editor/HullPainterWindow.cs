
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace Technie.PhysicsCreator
{
	public class HullPainterWindow : EditorWindow
	{
		// The actual install path with be detected at runtime with FindInstallPath
		// If for some reason that fails, the default install path will be used instead
		public const string defaultInstallPath = "Assets/Technie/PhysicsCreator/";

		private static bool isOpen;
		public static bool IsOpen() { return isOpen; }
		public static HullPainterWindow instance;

		private int activeMouseButton = -1;

		private bool repaintSceneView = false;
		private bool regenerateOverlay = false;
		private int hullToDelete = -1;

		private SceneManipulator sceneManipulator;

		private Vector2 scrollPosition;

		private Texture addHullIcon;
		private Texture errorIcon;
		private Texture deleteIcon;
		private Texture paintOnIcon;
		private Texture paintOffIcon;
		private Texture triggerOnIcon;
		private Texture triggerOffIcon;

		private HullType defaultType = HullType.ConvexHull;
		private PhysicMaterial defaultMaterial;
		private bool defaultIsTrigger;

		[MenuItem("Window/Technie Collider Creator/Hull Painter", false, 1)]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(HullPainterWindow));
		}

		void OnEnable()
		{
			string installPath = FindInstallPath();

			addHullIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "AddHullIcon.png");
			errorIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "ErrorIcon.png");
			deleteIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "DeleteIcon.png");

			paintOnIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "PaintOnIcon.png");
			paintOffIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "PaintOffIcon.png");

			triggerOnIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TriggerOnIcon.png");
			triggerOffIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TriggerOffIcon.png");

			Texture icon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TechnieIcon.png");
#if UNITY_5_0
			this.title = "Hull Painter";
#else
			this.titleContent = new GUIContent ("Hull Painter", icon, "Technie Hull Painter");
#endif

			sceneManipulator = new SceneManipulator();

			isOpen = true;
			instance = this;
		}

		void OnDestroy()
		{
		//	Debug.Log("WINDOW.OnDestroy");

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

			sceneManipulator.Destroy();
			sceneManipulator = null;

			isOpen = false;
			instance = null;
		}

		void OnFocus()
		{
			// Remove to make sure it's not added, then add it once
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		void OnSelectionChange()
		{
		//	Debug.Log ("Window.OnSelectionChange");

			if (sceneManipulator.Sync ())
			{
		//		Debug.Log ("Changed");
			}

			// Always repaint as we need to change inactive gui
			Repaint();
		}

		// Called from HullPainterEditor
		public void OnInspectorGUI()
		{
			if (sceneManipulator.Sync ())
			{
				Repaint();
			}
		}

		void OnGUI ()
		{
			// Only sync on repaint so ui gets same calls
			if (Event.current.type == EventType.Repaint)
			{
				sceneManipulator.Sync ();
			}
			
			repaintSceneView = false;
			regenerateOverlay = false;
			hullToDelete = -1;

			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				DrawActiveGui(currentHullPainter);
			}
			else
			{
				DrawInactiveGui();
			}
		}

		/** Gui drawn if the selected object has a vaild hull painter and initialised asset data
		 */
		private void DrawActiveGui(HullPainter currentHullPainter)
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			GUILayout.Space (10);

			DrawColliderButtons();
			
			GUILayout.Space (4);

			DrawHullGUI();

			GUILayout.Space (20);

			float baseWidth = EditorGUIUtility.currentViewWidth - 20; // -32px for window chrome
			float fixedWidth = 120 + 120; // sum of fixed widths below
			float flexibleWidth = baseWidth - fixedWidth;
			float[] collumnWidth =
			{
				120,
				flexibleWidth,
				120,
			};

			DrawDefaultType(collumnWidth);
			DrawDefaultMaterial(collumnWidth);
			DrawDefaultTrigger(collumnWidth);
			DrawFaceDepth (collumnWidth);

			DrawHullWarnings (currentHullPainter);

			DrawAssetPaths();
			
			if (currentHullPainter.paintingData.hulls.Count == 0)
			{
				GUILayout.Label("No hulls created. Add a hull to start painting.");
			}

			GUILayout.Space (16);

			GUILayout.EndScrollView ();

			// Now actually perform queued up actions

			if (hullToDelete != -1)
			{
				Undo.RecordObject (currentHullPainter.paintingData, "Delete Hull");

				currentHullPainter.paintingData.RemoveHull (hullToDelete);

				EditorUtility.SetDirty (currentHullPainter.paintingData);
			}

			if (regenerateOverlay)
				sceneManipulator.Sync (); // may need to explicitly resync overlay data?

			if (repaintSceneView)
				SceneView.RepaintAll();
		}

		/** Gui drawn if the selected object does not have a valid and initialised hull painter on it
		 */
		private void DrawInactiveGui()
		{
			if (Selection.transforms.Length == 1)
			{
				// Have a single scene selection, is it viable?

				GameObject selectedObject = Selection.transforms[0].gameObject;
				MeshFilter srcMesh = SelectionUtil.FindSelectedMeshFilter();

				if (srcMesh != null)
				{
					GUILayout.Space(10);
					GUILayout.Label("Generate an asset to start painting:");
					CommonUi.DrawGenerateOrReconnectGui(selectedObject, srcMesh.sharedMesh);
				}
				else
				{
					// No mesh filter, might have a hull painter (or not)

					GUILayout.Space(10);
					GUILayout.Label("To start painting, select a single scene object");
					GUILayout.Label("The object must contain a MeshFilter");

					GUILayout.Space(10);
					GUILayout.Label("No MeshFilter on selected object", EditorStyles.centeredGreyMiniLabel);
				}
			}
			else
			{
				// No single scene selection
				// Could be nothing selected
				// Could be multiple selection
				// Could be an asset in the project selected

				GUILayout.Space(10);
				GUILayout.Label("To start painting, select a single scene object");
				GUILayout.Label("The object must contain a MeshFilter");
			}
		}

		private void DrawColliderButtons()
		{
			GUILayout.BeginHorizontal ();
			{
				if (GUILayout.Button("Generate colliders"))
				{
					GenerateColliders();
				}

				if (GUILayout.Button("Delete colliders"))
				{
					DeleteColliders();
				}
			}
			GUILayout.EndHorizontal ();
		}

		private void DrawHullGUI()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			float baseWidth = EditorGUIUtility.currentViewWidth;
			float fixedWidth = 45 + 80 + 50 + 40 + 50 + 32; // sum of fixed widths below, plus 30px extra for window chrome
			float flexibleWidth = baseWidth - fixedWidth;
			float[] collumnWidth =
			{
				flexibleWidth * 0.5f,
				45,
				80,
				flexibleWidth * 0.5f,
				50,
				40,
				50
			};

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Name",		GUILayout.Width(collumnWidth[0]) );
				GUILayout.Label("Colour",	GUILayout.Width(collumnWidth[1]) );
				GUILayout.Label("Type",		GUILayout.Width(collumnWidth[2]) );
				GUILayout.Label("Material",	GUILayout.Width(collumnWidth[3]) );
				GUILayout.Label("Trigger",	GUILayout.Width(collumnWidth[4]) );
				GUILayout.Label("Paint",	GUILayout.Width(collumnWidth[5]) );
				GUILayout.Label("Delete",	GUILayout.Width(collumnWidth[6]) );
			}
			GUILayout.EndHorizontal();

			for (int i=0; i<currentHullPainter.paintingData.hulls.Count; i++)
			{
				DrawHullGUILine(i, currentHullPainter.paintingData.hulls[i], collumnWidth);
			}

			GUILayout.BeginHorizontal();
			{
			//	if (GUILayout.Button("Add Hull", addHullIcon, GUILayout.Width(collumnWidth[0])))
				if (GUILayout.Button(new GUIContent("Add Hull", addHullIcon), GUILayout.Width(collumnWidth[0])))
				{
					AddHull();
				}

				GUILayout.Label("", GUILayout.Width(collumnWidth[1]));
				GUILayout.Label("", GUILayout.Width(collumnWidth[2]));
				GUILayout.Label("", GUILayout.Width(collumnWidth[3]));
				GUILayout.Label("", GUILayout.Width(collumnWidth[4]));

				if (GUILayout.Button("Stop", GUILayout.Width(collumnWidth[5])))
				{
					StopPainting();
				}

				if (GUILayout.Button("Del All", GUILayout.Width(collumnWidth[6])))
				{
					DeleteHulls();
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawHullGUILine(int hullIndex, Hull hull, float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			Undo.RecordObject (currentHullPainter.paintingData, "Edit Hull");

			GUILayout.BeginHorizontal ();
			{
				hull.name = EditorGUILayout.TextField(hull.name, GUILayout.MinWidth(60), GUILayout.Width(collumnWidths[0]) );
				
				Color prevColour = hull.colour;
				hull.colour = EditorGUILayout.ColorField("", hull.colour, GUILayout.Width(collumnWidths[1]));
				if (prevColour != hull.colour)
				{
					regenerateOverlay = true;
					repaintSceneView = true;
				}
				
				hull.type = (HullType)EditorGUILayout.EnumPopup(hull.type, GUILayout.Width(collumnWidths[2]) );

				hull.material = (PhysicMaterial)EditorGUILayout.ObjectField(hull.material, typeof(PhysicMaterial), false, GUILayout.Width(collumnWidths[3]) );

				if (GUILayout.Button(hull.isTrigger ? triggerOnIcon : triggerOffIcon, GUILayout.Width(collumnWidths[4])))
				{
					hull.isTrigger = !hull.isTrigger;
				}

				int prevHullIndex = currentHullPainter.paintingData.activeHull;

				bool isPainting = (currentHullPainter.paintingData.activeHull == hullIndex);
				int nowSelected = GUILayout.Toolbar (isPainting ? 0 : -1, new Texture[] { isPainting ? paintOnIcon : paintOffIcon }, UnityEditor.EditorStyles.miniButton, GUILayout.Width(collumnWidths[5]) );
				if (nowSelected == 0 && prevHullIndex != hullIndex)
				{
					// Now painting this index!
					currentHullPainter.paintingData.activeHull = hullIndex;
				}

				if (GUILayout.Button(deleteIcon, GUILayout.Width(collumnWidths[6]) ))
				{
					hullToDelete = hullIndex;
					regenerateOverlay = true;
					repaintSceneView = true;
				}
			}
			GUILayout.EndHorizontal ();
		}

		private void DrawDefaultType (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Default type:", GUILayout.Width(collumnWidths[0]));

				defaultType = (HullType)EditorGUILayout.EnumPopup(defaultType, GUILayout.Width(100));
			//	defaultType = (HullType)EditorGUILayout.EnumPopup(defaultType, GUILayout.Width(collumnWidths[1]));

				GUILayout.Label("", GUILayout.Width(collumnWidths[1]-100));

				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])) )
				{
					currentHullPainter.SetAllTypes(defaultType);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawDefaultMaterial(float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Default material:", GUILayout.Width(collumnWidths[0]));

				defaultMaterial = (PhysicMaterial)EditorGUILayout.ObjectField(defaultMaterial, typeof(PhysicMaterial), false, GUILayout.Width(collumnWidths[1]+4));
				
				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])))
				{
					currentHullPainter.SetAllMaterials(defaultMaterial);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawDefaultTrigger (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Default trigger:", GUILayout.Width(collumnWidths[0]));

				if (GUILayout.Button(defaultIsTrigger ? triggerOnIcon : triggerOffIcon, GUILayout.Width(100)))
				{
					defaultIsTrigger = !defaultIsTrigger;
				}

				GUILayout.Label("", GUILayout.Width(collumnWidths[1]-100));

				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])))
				{
					currentHullPainter.SetAllAsTrigger(defaultIsTrigger);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawFaceDepth (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Face thickness:", GUILayout.Width(collumnWidths[0]));

				currentHullPainter.paintingData.faceThickness = EditorGUILayout.FloatField(currentHullPainter.paintingData.faceThickness, GUILayout.Width(collumnWidths[1]+4));

				float inc = 0.1f;
				if (GUILayout.Button("+"))
				{
					currentHullPainter.paintingData.faceThickness = currentHullPainter.paintingData.faceThickness + inc;
				}
				if (GUILayout.Button("-"))
				{
					currentHullPainter.paintingData.faceThickness = currentHullPainter.paintingData.faceThickness - inc;
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawHullWarnings (HullPainter currentHullPainter)
		{
			List<string> warnings = new List<string> ();

			for (int i=0; i<currentHullPainter.paintingData.hulls.Count; i++)
			{
				Hull hull = currentHullPainter.paintingData.hulls[i];
				if (hull.hasColliderError)
				{
					warnings.Add("'"+hull.name+"' generates a collider with "+hull.numColliderFaces+" faces");
				}
			}

			if (warnings.Count > 0)
			{
				GUILayout.BeginHorizontal();

				GUIStyle iconStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
				GUILayout.Label(errorIcon, iconStyle, GUILayout.Height(40), GUILayout.ExpandWidth(true));

				GUILayout.BeginVertical();
				{
					GUILayout.Label ("Errors", EditorStyles.boldLabel);

					foreach (string str in warnings)
					{
						GUILayout.Label (str);
					}
					
					GUILayout.Label ("Unity only allows max 256 faces per hull");
					GUILayout.Label ("Simplify this hull or split into multiple hulls");
				}
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}
		}

		private void DrawAssetPaths ()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.Space(20);

		//	GUILayout.BeginHorizontal ();
			{
				string path = AssetDatabase.GetAssetPath(currentHullPainter.paintingData);
				GUILayout.Label("Painting data: "+path, EditorStyles.centeredGreyMiniLabel);
			}
		//	GUILayout.EndHorizontal();

		//	GUILayout.BeginHorizontal ();
			{
				string path = AssetDatabase.GetAssetPath(currentHullPainter.hullData);
				GUILayout.Label("Hull data: "+path, EditorStyles.centeredGreyMiniLabel);
			}
		//	GUILayout.EndHorizontal();

			if (GUILayout.Button("Disconnect from assets"))
			{
				sceneManipulator.DisconnectAssets();

				currentHullPainter = null;
				repaintSceneView = true;
				regenerateOverlay = true;
			}
		}

		public void OnSceneGUI ()
		{
		//	Debug.Log ("Window.OnSceneGUI");

			if (sceneManipulator.Sync ())
			{
				Repaint();
			}
			
			int controlId = GUIUtility.GetControlID (FocusType.Passive);
			
			if (Event.current.type == EventType.MouseDown && (Event.current.button == 0))
			{
				bool eventConsumed = sceneManipulator.DoMouseDown();
				if (eventConsumed)
				{
					activeMouseButton = Event.current.button;
					GUIUtility.hotControl = controlId;
					Event.current.Use();
				}

			}
			else if (Event.current.type == EventType.MouseDrag && Event.current.button == activeMouseButton)
			{
				bool eventConsumed = sceneManipulator.DoMouseDrag();
				if (eventConsumed)
				{
					GUIUtility.hotControl = controlId;
					Event.current.Use();
					Repaint();
				}

			}
			else if (Event.current.type == EventType.MouseUp && Event.current.button == activeMouseButton)
			{
				bool eventConsumed = sceneManipulator.DoMouseUp();
				if (eventConsumed)
				{
					activeMouseButton = -1;
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
			}
		}

		private void GenerateColliders()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter == null)
				return;
			
			Undo.RegisterCompleteObjectUndo (currentHullPainter.gameObject, "Generate Colliders");

			// Fetch the data assets

			PaintingData paintingData = currentHullPainter.paintingData;
			HullData hullData = currentHullPainter.hullData;

			string hullAssetPath = AssetDatabase.GetAssetPath (hullData);
			
			// Create / update the hull meshes

			foreach (Hull hull in paintingData.hulls)
			{
				paintingData.GenerateCollisionMesh(hull, sceneManipulator.GetTargetVertices(), sceneManipulator.GetTargetTriangles());
			}

			// Sync the in-memory hull meshes with the asset meshes in hullAssetPath

			List<Mesh> existingMeshes = GetAllMeshesInAsset (hullAssetPath);

			foreach (Mesh existing in existingMeshes)
			{
				if (!paintingData.ContainsMesh(existing))
				{
					GameObject.DestroyImmediate(existing, true);
				}
			}

			foreach (Hull hull in paintingData.hulls)
			{
				if (hull.collisionMesh != null)
				{
					if (!existingMeshes.Contains(hull.collisionMesh))
					{
						AssetDatabase.AddObjectToAsset(hull.collisionMesh, hullAssetPath);
					}
				}
				if (hull.faceCollisionMesh != null)
				{
					if (existingMeshes.Contains(hull.faceCollisionMesh))
					{
						AssetDatabase.AddObjectToAsset(hull.faceCollisionMesh, hullAssetPath);
					}
				}
			}

			EditorUtility.SetDirty (hullData);

			AssetDatabase.SaveAssets ();

			// Add collider components to the target object

			currentHullPainter.CreateColliderComponents ();
		}

		private void AddHull()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null)
			{
				Undo.RecordObject (currentHullPainter.paintingData, "Add Hull");
				currentHullPainter.paintingData.AddHull(defaultType, defaultMaterial, defaultIsTrigger);

				EditorUtility.SetDirty (currentHullPainter.paintingData);
			}
		}

		private void StopPainting()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				currentHullPainter.paintingData.activeHull = -1;
			}
		}

		private void DeleteColliders()
		{
			Undo.SetCurrentGroupName ("Destroy Colliders");

			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();
			currentHullPainter.RemoveAllColliders ();
		}

		private void DeleteHulls ()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter ();
			if (currentHullPainter != null && currentHullPainter.hullData != null)
			{
				currentHullPainter.paintingData.RemoveAllHulls ();
				repaintSceneView = true;
			}
		}

		private List<Mesh> GetAllMeshesInAsset(string assetPath)
		{
			List<Mesh> meshes = new List<Mesh> ();

			foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
			{
				if (o is Mesh)
				{
					meshes.Add((Mesh)o);
				}
			}

			return meshes;
		}

		void OnSceneGUI(SceneView sceneView)
		{
			sceneManipulator.OnSceneGUI(sceneView);

			// Do your drawing here using Handles.
			Handles.BeginGUI();
			// Do your drawing here using GUI.
		
		//	Gizmos.DrawCube(Vector3.zero, Vector3.one);

			Handles.EndGUI();
		}

		private static string FindInstallPath()
		{
			string installPath = defaultInstallPath;
			
			string[] foundIds = AssetDatabase.FindAssets ("AddHullIcon t:texture2D");
			if (foundIds.Length > 0)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath (foundIds [0]);
				int lastSlashPos = assetPath.LastIndexOf("/");
				if (lastSlashPos != -1)
				{
					string newInstallPath = assetPath.Substring(0, lastSlashPos+1);
					installPath = newInstallPath;
				}
			}

			return installPath;
		}
	}

} // namespace Technie.PhysicsCreator
