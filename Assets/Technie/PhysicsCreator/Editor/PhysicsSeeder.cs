using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PhysicsSeeder : MonoBehaviour
{
	[MenuItem ("Window/Technie Collider Creator/Generate Colliders From Selection &p")]
	private static void SeedPhysics()
	{
		GameObject seedRoot = FindGeneratedPhysicsRoot ();

		List<GameObject> seeds = CreatePhysicsClones (UnityEditor.Selection.transforms, seedRoot);

		UnityEditor.Selection.objects = seeds.ToArray();
	}

	[MenuItem ("Window/Technie Collider Creator/Span From Selection &f")]
	private static void SpanPhysics()
	{
		GameObject seedRoot = FindGeneratedPhysicsRoot ();
		
		List<GameObject> seeds = CreatePhysicsClones (UnityEditor.Selection.transforms, seedRoot);

		GameObject spanned = SpanPhysics (seeds);

		UnityEditor.Selection.objects = new UnityEngine.Object[] { spanned };
	}

	private static GameObject FindGeneratedPhysicsRoot()
	{
		GameObject seedRoot = GameObject.Find ("Generated Physics");
		if (seedRoot == null)
		{
			seedRoot = new GameObject("Generated Physics");
		}
		return seedRoot;
	}

	private static List<GameObject> CreatePhysicsClones(Transform[] inputObjects, GameObject newParent)
	{
		List<GameObject> newObjects = new List<GameObject>();

		foreach (Transform t in inputObjects)
		{
			if (t.GetComponent<MeshRenderer>() != null && t.GetComponent<MeshFilter>() != null)
			{
				GameObject clone = GameObject.Instantiate(t.gameObject);
				
				// NB: Just cloning the original object does a literal clone of the transform, but puts it at the root of the hierarchy
				// This means that if the original has parent(s) with a non-identity transform, it's now actually in a different place
				// So if it does have a parent, reparent it back (leaving the translation/etc. values alone) so it is now in the identical place it was cloned from
				if (t.parent != null)
				{
					clone.transform.SetParent(t.parent, false);
				}
				
				// Reparent to our seed root
				clone.transform.SetParent(newParent.transform, true);
				
				// Add a new box collider (which will auto size based on mesh filter+renderer
				BoxCollider box = clone.AddComponent<BoxCollider>();
				
				EnsureDepth(clone);
				
				// Keep hold of this so we can set it as the new selection
				newObjects.Add(clone);
				
				// Delete all the components except for the transform and the new box collider
				foreach (Component comp in clone.GetComponents<Component>())
				{
					if (comp is Transform || comp == box)
						continue;
					
					GameObject.DestroyImmediate(comp);
				}
			}
		}

		return newObjects;
	}

	private static void EnsureDepth(GameObject obj)
	{
		BoxCollider box = obj.GetComponent<BoxCollider> ();

		bool extrudeX = false;
		bool extrudeY = false;
		bool extrudeZ = false;

		Vector3 localRaycastDir = Vector3.forward;

		if (EpsilonEquals(box.size.x, 0.0f))
		{
			//Debug.Log("Extrude on local X");

			localRaycastDir = Vector3.right;
			extrudeX = true;
		}
		else if (EpsilonEquals(box.size.y, 0.0f))
		{
			//Debug.Log("Extrude on local Y");

			localRaycastDir = Vector3.up;
			extrudeY = true;
		}
		else if (EpsilonEquals(box.size.z, 0.0f))
		{
			//Debug.Log("Extrude on local Z");

			localRaycastDir = Vector3.forward;
			extrudeZ = true;
		}

		if (extrudeX || extrudeY || extrudeZ)
		{
			Vector3 origin = box.center;
			origin = box.gameObject.transform.TransformPoint(origin);

			Vector3 raycastDir = box.gameObject.transform.TransformVector(localRaycastDir);

			// Add mesh collider
			MeshCollider meshCol = obj.AddComponent<MeshCollider>();

			float extrudeFactor = 0.0f;

			// Fire rays to find collision dir
			float dist = 10.0f;
			Ray ray0 = new Ray(origin - raycastDir * dist, raycastDir);
			Ray ray1 = new Ray(origin + raycastDir * dist, raycastDir * -1.0f);
			RaycastHit hitInfo;
			if (meshCol.Raycast(ray0, out hitInfo, dist * 2.0f))
			{
				//Debug.Log ("Extrude +ve");

				extrudeFactor = 1.0f;
			}
			else if (meshCol.Raycast(ray1, out hitInfo, dist * 2.0f))
			{
				//Debug.Log ("Extrude -ve");

				extrudeFactor = -1.0f;
			}

			// Delete mesh collider
			GameObject.DestroyImmediate(meshCol);

			// Extrude in collision dir
			float thickness = 0.2f;
			float nudge = thickness * 0.5f;

			Vector3 size = box.size;
			Vector3 center = box.center;
			if (extrudeX)
			{
				size.x += thickness;
				center.x += nudge * extrudeFactor;
			}
			else if (extrudeY)
			{
				size.y += thickness;
				center.y += nudge * extrudeFactor;
			}
			else if (extrudeZ)
			{
				size.z += thickness;
				center.z += nudge * extrudeFactor;
			}
			box.size = size;
			box.center = center;
		}
	}

	private static bool EpsilonEquals(float lhs, float rhs)
	{
		float delta = Mathf.Abs (lhs - rhs);
		return delta <= 0.0001f;
	}

	private static GameObject SpanPhysics (List<GameObject> inputObjects)
	{
		if (inputObjects.Count == 0)
			return null;

		if (inputObjects.Count == 1)
			return inputObjects [0];

		GameObject baseObj = inputObjects [0];
		inputObjects.RemoveAt (0);

		BoxCollider baseBox = baseObj.GetComponent<BoxCollider> ();

		foreach (GameObject other in inputObjects)
		{
			BoxCollider box = other.GetComponent<BoxCollider>();

			if (box == null)
				continue;

			Inflate(baseBox, box);
		}

		foreach (GameObject other in inputObjects)
		{
			GameObject.DestroyImmediate(other);
		}

		return baseObj;
	}

	private static void Inflate(BoxCollider baseBox, BoxCollider otherBox)
	{
		Vector3[] worldCoords = CalcWorldVertices (otherBox);

		Vector3[] localCoords = new Vector3[worldCoords.Length];
		for (int i=0; i<localCoords.Length; i++)
		{
			Vector3 localPos = baseBox.transform.InverseTransformPoint(worldCoords[i]);
			localCoords[i] = localPos;
		}

		for (int i=0; i<localCoords.Length; i++)
		{
			Vector3 newPos = localCoords[i];
			Inflate (baseBox, newPos);
		}
	}

	private static Vector3[] CalcWorldVertices(BoxCollider box)
	{
		Vector3 extents = box.size * 0.5f;

		Vector3[] verts = new Vector3[8];

		verts[0] = box.center + new Vector3 (-extents.x, -extents.y, -extents.z);
		verts[1] = box.center + new Vector3 ( extents.x, -extents.y, -extents.z);
		verts[2] = box.center + new Vector3 (-extents.x,  extents.y, -extents.z);
		verts[3] = box.center + new Vector3 ( extents.x,  extents.y, -extents.z);

		verts[4] = box.center + new Vector3 (-extents.x, -extents.y,  extents.z);
		verts[5] = box.center + new Vector3 ( extents.x, -extents.y,  extents.z);
		verts[6] = box.center + new Vector3 (-extents.x,  extents.y,  extents.z);
		verts[7] = box.center + new Vector3 ( extents.x,  extents.y,  extents.z);

		for (int i=0; i<verts.Length; i++)
		{
			verts[i] = box.transform.TransformPoint(verts[i]);
		}

		return verts;
	}

	private static void Inflate(BoxCollider box, Vector3 newLocalPoint)
	{
		// Recalculate the min and max along each axis

		// First along x
		float minX = box.center.x - (box.size.x * 0.5f);
		float maxX = box.center.x + (box.size.x * 0.5f);

		minX = Mathf.Min (minX, newLocalPoint.x);
		maxX = Mathf.Max (maxX, newLocalPoint.x);

		// Then along y
		float minY = box.center.y - (box.size.y * 0.5f);
		float maxY = box.center.y + (box.size.y * 0.5f);
		
		minY = Mathf.Min (minY, newLocalPoint.y);
		maxY = Mathf.Max (maxY, newLocalPoint.y);

		// Then along z
		float minZ = box.center.z - (box.size.z * 0.5f);
		float maxZ = box.center.z + (box.size.z * 0.5f);
		
		minZ = Mathf.Min (minZ, newLocalPoint.z);
		maxZ = Mathf.Max (maxZ, newLocalPoint.z);

		// Now recalculate box center and size with the new min/max along each axis

		Vector3 center = new Vector3( (minX+maxX) * 0.5f, (minY+maxY) * 0.5f, (minZ+maxZ) * 0.5f );
		Vector3 size = new Vector3(maxX-minX, maxY-minY, maxZ-minZ);
		box.center = center;
		box.size = size;
	}
}
