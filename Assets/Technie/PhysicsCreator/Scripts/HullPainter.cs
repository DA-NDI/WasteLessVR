using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Technie.PhysicsCreator
{
	public class HullPainter : MonoBehaviour
	{
		public PaintingData paintingData;
		public HullData hullData;

		public Dictionary<Hull, Collider> hullMapping;

		void OnDestroy()
		{
#if UNITY_EDITOR
			SceneView.RepaintAll();
#endif
		}

		public void CreateColliderComponents ()
		{
			CreateHullMapping ();

			foreach (Hull hull in paintingData.hulls)
			{
				CreateColliderComponent (hull);
			}
		}

		public void RemoveAllColliders ()
		{
			CreateHullMapping ();

			foreach (Collider c in hullMapping.Values)
			{
#if UNITY_EDITOR
				Undo.DestroyObjectImmediate(c);
#else
				GameObject.DestroyImmediate(c);
#endif
			}
			hullMapping.Clear ();
		}

		private void CreateHullMapping()
		{
			if (hullMapping == null)
				hullMapping = new Dictionary<Hull, Collider> ();

			List<Hull> keys = new List<Hull> (hullMapping.Keys); // take a copy of the keys so we can remove from hullMapping as we iterate over it
			foreach (Hull h in keys)
			{
				if (h == null || hullMapping[h] == null)
				{
					Debug.Log ("Removing invalid entry from hull mapping");
					hullMapping.Remove(h);
				}
			}

			// Check to see if any existing mappings need updating (hull.type doesn't match Collider type)

			foreach (Hull hull in paintingData.hulls)
			{
				if (hullMapping.ContainsKey(hull))
				{
					// We already have a mapping for this, but is it still of the correct type?

					Collider value = hullMapping[hull];

					bool isHullOk = (hull.type == HullType.ConvexHull && value is MeshCollider);
					bool isBoxOk = (hull.type == HullType.Box && value is BoxCollider);
					bool isSphereOk = (hull.type == HullType.Sphere && value is SphereCollider);
					bool isFaceOk = (hull.type == HullType.Face && value is MeshCollider);

					if (!(isHullOk || isBoxOk || isSphereOk || isFaceOk))
					{
						// Mismatch - hull.type doesn't match collider type
						// Delete the collider and remove the mapping
						// This hull will then be orphaned, and a new collider added back in accordingly
						GameObject.DestroyImmediate(value);
						hullMapping.Remove(hull);
					}
				}
			}

			// Connect orphans
			//
			// Find hulls without a Collider
			// Find Colliders without hulls
			// Try and map the two together

			// First find orphans

			List<Hull> orphanedHulls = new List<Hull> ();
			List<Collider> orphanedColliders = new List<Collider> ();

			foreach (Hull h in paintingData.hulls)
			{
				if (!hullMapping.ContainsKey(h))
					orphanedHulls.Add(h);
			}

			foreach (Collider c in GetComponents<Collider>())
			{
				if (!hullMapping.ContainsValue(c))
					orphanedColliders.Add(c);
			}

			// Try and connect orphaned hulls with orphaned colliders

			for (int i=orphanedHulls.Count-1; i>=0; i--)
			{
				Hull h = orphanedHulls[i];

				for (int j=orphanedColliders.Count-1; j>=0; j--)
				{
					Collider c = orphanedColliders[j];

					BoxCollider boxCol = c as BoxCollider;
					SphereCollider sphereCol = c as SphereCollider;
					MeshCollider meshCol = c as MeshCollider;

					bool isMatchingBox = h.type == HullType.Box && c is BoxCollider && Approximately(h.collisionBox.center, boxCol.center) && Approximately(h.collisionBox.size, boxCol.size);
					bool isMatchingSphere = h.type == HullType.Sphere && c is SphereCollider && h.collisionSphere != null && Approximately(h.collisionSphere.center, sphereCol.center) && Approximately(h.collisionSphere.radius, sphereCol.radius);
					bool isMatchingConvexHull = h.type == HullType.ConvexHull && c is MeshCollider && meshCol.sharedMesh == h.collisionMesh;
					bool isMatchingFace = h.type == HullType.Face && c is MeshCollider && meshCol.sharedMesh == h.faceCollisionMesh;

					if (isMatchingBox || isMatchingSphere || isMatchingConvexHull || isMatchingFace)
					{
						// Found a pair, so add a mapping and remove the orphans
						hullMapping.Add(h, c);

						// These are no longer orphaned, so remove them from these lists
						orphanedHulls.RemoveAt(i);
						orphanedColliders.RemoveAt(j);
						break;
					}
				}
			}

			// Create colliders for any left over hulls

			foreach (Hull h in orphanedHulls)
			{
				if (h.type == HullType.Box)
				{
#if UNITY_EDITOR
					BoxCollider box = (BoxCollider)Undo.AddComponent(this.gameObject, typeof(BoxCollider));
#else
					BoxCollider box = this.gameObject.AddComponent<BoxCollider>();
#endif
					hullMapping.Add(h, box);
				}
				else if (h.type == HullType.Sphere)
				{
#if UNITY_EDITOR
					SphereCollider sphere = (SphereCollider)Undo.AddComponent(this.gameObject, typeof(SphereCollider));
#else
					SphereCollider sphere = this.gameObject.AddComponent<SphereCollider>();
#endif
					hullMapping.Add(h, sphere);
				}
				else if (h.type == HullType.ConvexHull)
				{
#if UNITY_EDITOR
					MeshCollider mesh = (MeshCollider)Undo.AddComponent(this.gameObject, typeof(MeshCollider));
#else
					MeshCollider mesh = this.gameObject.AddComponent<MeshCollider>();
#endif
					hullMapping.Add(h, mesh);
				}
				else if (h.type == HullType.Face)
				{
#if UNITY_EDITOR
					MeshCollider mesh = (MeshCollider)Undo.AddComponent(this.gameObject, typeof(MeshCollider));
#else
					MeshCollider mesh = this.gameObject.AddComponent<MeshCollider>();
#endif
					hullMapping.Add(h, mesh);
				}
			}

			// Delete any left over colliders

			foreach (Collider c in orphanedColliders)
			{
				GameObject.DestroyImmediate(c);
			}
		}

		private static bool Approximately(Vector3 lhs, Vector3 rhs)
		{
			return Mathf.Approximately (lhs.x, rhs.x) && Mathf.Approximately (lhs.y, rhs.y) && Mathf.Approximately (lhs.z, rhs.z);
		}
		private static bool Approximately(float lhs, float rhs)
		{
			return Mathf.Approximately(lhs, rhs);
		}

		private void CreateColliderComponent(Hull hull)
		{
			Collider c = null;

			if (hull.type == HullType.Box)
			{
				BoxCollider boxCollider = hullMapping[hull] as BoxCollider;
				boxCollider.center = hull.collisionBox.center;
				boxCollider.size = hull.collisionBox.size;
				c = boxCollider;
			}
			else if (hull.type == HullType.Sphere)
			{
				SphereCollider sphereCollider = hullMapping[hull] as SphereCollider;
				sphereCollider.center = hull.collisionSphere.center;
				sphereCollider.radius = hull.collisionSphere.radius;
				c = sphereCollider;
			}
			else if (hull.type == HullType.ConvexHull)
			{
				MeshCollider meshCollider = hullMapping[hull] as MeshCollider;
				meshCollider.sharedMesh = hull.collisionMesh;
				meshCollider.convex = true;
				c = meshCollider;
			}
			else if (hull.type == HullType.Face)
			{
				MeshCollider faceCollider = hullMapping[hull] as MeshCollider;
				faceCollider.sharedMesh = hull.faceCollisionMesh;
				faceCollider.convex = true;
				c = faceCollider;
			}

			c.material = hull.material;
			c.isTrigger = hull.isTrigger;
		}

		public void SetAllTypes (HullType newType)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.type = newType;
			}
		}

		public void SetAllMaterials (PhysicMaterial newMaterial)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.material = newMaterial;
			}
		}

		public void SetAllAsTrigger(bool isTrigger)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.isTrigger = isTrigger;
			}
		}

		public void OnDrawGizmosSelected()
		{
		//	Debug.Log("Gizmos");
		}
	}

} // namespace Technie.PhysicsCreator

