
using UnityEngine;
using System.Collections.Generic;

using Technie.PhysicsCreator.QHull;

namespace Technie.PhysicsCreator
{
	public enum HullType
	{
		Box,
		ConvexHull,
		Sphere,
		Face
	}

	[System.Serializable]
	public class Hull
	{
		public string name = "<unnamed hull>";
		
		public HullType type = HullType.ConvexHull;
		
		public Color colour = Color.white;

		public PhysicMaterial material;

		public bool isTrigger = false;
		
		public List<int> selectedFaces = new List<int> ();

		public Mesh collisionMesh; // Mesh for convex hull. Reference to the stored mesh asset in HullData
		public Mesh faceCollisionMesh; // Mesh for face. Reference to the stored mesh asset in HullData
		public Bounds collisionBox; // If hull type is 'Box' then the computed box is stored here
		public Sphere collisionSphere; // If hull type is 'Sphere' then the computed sphere is stored here

		public bool hasColliderError;
		public int numColliderFaces;

		public void Destroy() {}
	}

	public class PaintingData : ScriptableObject
	{
		public readonly Color[] hullColours = new Color[]
		{
			new Color(0.0f, 1.0f, 1.0f, 0.3f),
			new Color(1.0f, 0.0f, 1.0f, 0.3f),
			new Color(1.0f, 1.0f, 0.0f, 0.3f),
			
			new Color(1.0f, 0.0f, 0.0f, 0.3f),
			new Color(0.0f, 1.0f, 0.0f, 0.3f),
			new Color(0.0f, 0.0f, 1.0f, 0.3f),

			new Color(1.0f, 1.0f, 1.0f, 0.3f),

			new Color(1.0f, 0.5f, 0.0f, 0.3f),
			new Color(1.0f, 0.0f, 0.5f, 0.3f),
			new Color(0.5f, 1.0f, 0.0f, 0.3f),
			new Color(0.0f, 1.0f, 0.5f, 0.3f),
			new Color(0.5f, 0.0f, 1.0f, 0.3f),
			new Color(0.0f, 0.5f, 1.0f, 0.3f),
		};

		// Serialised Data

		public HullData hullData;

		public Mesh sourceMesh;

		public int activeHull = -1;

		public float faceThickness = 0.1f;

		public List<Hull> hulls = new List<Hull>();

		public void AddHull(HullType type, PhysicMaterial material, bool isTrigger)
		{
			hulls.Add( new Hull() );
			
			// Name the new hull
			hulls [hulls.Count - 1].name = "Hull " + hulls.Count;
			
			// Set selection to new hull
			activeHull = hulls.Count - 1;
			
			// Set the colour for the new hull
			hulls[hulls.Count-1].colour = hullColours[ activeHull % hullColours.Length ];
			hulls[hulls.Count-1].type = type;
			hulls[hulls.Count-1].material = material;
			hulls[hulls.Count-1].isTrigger = isTrigger;
		}

		public void RemoveHull (int index)
		{
			hulls [index].Destroy ();
			hulls.RemoveAt (index);
		}

		public void RemoveAllHulls ()
		{
			for (int i = 0; i < hulls.Count; i++)
			{
				hulls[i].Destroy();
			}
			hulls.Clear();
		}

		public bool HasActiveHull()
		{
			return activeHull >= 0 && activeHull < hulls.Count;
		}
		
		public Hull GetActiveHull()
		{
			if (activeHull < 0 || activeHull >= hulls.Count)
				return null;
			
			return hulls [activeHull];
		}

		public void GenerateCollisionMesh(Hull hull, Vector3[] meshVertices, int[] meshIndices)
		{
			hull.hasColliderError = false;

			if (hull.type == HullType.Box)
			{
				if (hull.selectedFaces.Count > 0)
				{
					Vector3 first = meshVertices[meshIndices[ hull.selectedFaces[0] * 3 ]];
					
					Vector3 min = first;
					Vector3 max = first;
					
					for (int i=0; i<hull.selectedFaces.Count; i++)
					{
						int faceIndex = hull.selectedFaces[i];
						
						Vector3 p0 = meshVertices[meshIndices[faceIndex * 3]];
						Vector3 p1 = meshVertices[meshIndices[faceIndex * 3 + 1]];
						Vector3 p2 = meshVertices[meshIndices[faceIndex * 3 + 2]];
						
						Inflate(p0, ref min, ref max);
						Inflate(p1, ref min, ref max);
						Inflate(p2, ref min, ref max);
					}

					hull.collisionBox.center = (min + max) * 0.5f;
					hull.collisionBox.size = max - min;
				}
			}
			else if (hull.type == HullType.Sphere)
			{
				Vector3 sphereCenter;
				float sphereRadius;
				if (CalculateBoundingSphere(hull, meshVertices, meshIndices, out sphereCenter, out sphereRadius))
				{
					if (hull.collisionSphere == null)
					{
						hull.collisionSphere = new Sphere();
					}

					hull.collisionSphere.center = sphereCenter;
					hull.collisionSphere.radius = sphereRadius;
				}
			}
			else if (hull.type == HullType.ConvexHull)
			{
				if (hull.collisionMesh == null)
				{
					hull.collisionMesh = new Mesh();
				}

				hull.collisionMesh.name = hull.name;

				hull.collisionMesh.triangles = new int[0];
				hull.collisionMesh.vertices = new Vector3[0];

				GenerateConvexHull(hull, meshVertices, meshIndices, hull.collisionMesh);
			}
			else if (hull.type == HullType.Face)
			{
				if (hull.faceCollisionMesh == null)
				{
					hull.faceCollisionMesh = new Mesh();
				}

				hull.faceCollisionMesh.name = hull.name;

				hull.faceCollisionMesh.triangles = new int[0];
				hull.faceCollisionMesh.vertices = new Vector3[0];

				GenerateFace(hull, meshVertices, meshIndices, faceThickness);
			}
		}

		private bool CalculateBoundingSphere (Hull hull, Vector3[] meshVertices, int[] meshIndices, out Vector3 sphereCenter, out float sphereRadius)
		{
			if (hull.selectedFaces.Count == 0)
			{
				sphereCenter = Vector3.zero;
				sphereRadius = 0.0f;
				return false;
			}

			List<Vector3> points = new List<Vector3>();

			for (int i=0; i<hull.selectedFaces.Count; i++)
			{
				int faceIndex = hull.selectedFaces[i];
				
				Vector3 p0 = meshVertices[meshIndices[faceIndex * 3]];
				Vector3 p1 = meshVertices[meshIndices[faceIndex * 3 + 1]];
				Vector3 p2 = meshVertices[meshIndices[faceIndex * 3 + 2]];

				points.Add(p0);
				points.Add(p1);
				points.Add(p2);
			}

			Sphere s = SphereUtils.MinSphere(points);
			sphereCenter = s.center;
			sphereRadius = s.radius;

			return true;
		}

		private void GenerateConvexHull(Hull hull, Vector3[] meshVertices, int[] meshIndices, Mesh destMesh)
		{
			// Generate array of input points

			int totalFaces = hull.selectedFaces.Count;
			Point3d[] inputPoints = new Point3d[totalFaces * 3];
			
			for (int i=0; i<hull.selectedFaces.Count; i++)
			{
				int faceIndex = hull.selectedFaces[i];

				Vector3 p0 = meshVertices[meshIndices[faceIndex * 3]];
				Vector3 p1 = meshVertices[meshIndices[faceIndex * 3 + 1]];
				Vector3 p2 = meshVertices[meshIndices[faceIndex * 3 + 2]];
				
				inputPoints[i * 3]		= new Point3d(p0.x, p0.y, p0.z);
				inputPoints[i * 3 + 1]	= new Point3d(p1.x, p1.y, p1.z);
				inputPoints[i * 3 + 2]	= new Point3d(p2.x, p2.y, p2.z);
			}

			// Calculate the convex hull

            QuickHull3D qHull = new QuickHull3D();
            try
            {
                qHull.build (inputPoints);
            }
            catch (System.Exception)
            {
                Debug.LogError ("Could not generate hull for " + this.name + "'s '" + hull.name + "' (input "+inputPoints.Length+" points)");
            }
			
			// Get calculated hull vertices and indices
			
			Point3d[] hullVertices = qHull.getVertices();
			int[][] hullFaceIndices = qHull.getFaces();

			hull.numColliderFaces = hullFaceIndices.Length;

			Debug.Log ("Calculated collider for '"+hull.name+"' has " + hullFaceIndices.Length + " faces");
			if (hullFaceIndices.Length >= 256)
			{
				hull.hasColliderError = true;
				return;
			}

			// Convert to dest vertices

			Vector3[] destVertices = new Vector3[hullVertices.Length];
			for (int i=0; i<destVertices.Length; i++)
			{
				destVertices[i] = new Vector3( (float)hullVertices[i].x, (float)hullVertices[i].y, (float)hullVertices[i].z );
			}

			// Convert to dest incices

			List<int> destIndices = new List<int>();
			
			for (int i=0; i<hullFaceIndices.Length; i++)
			{
				int faceVerts = hullFaceIndices[i].Length;
				for (int j=1; j<faceVerts-1; j++)
				{
					destIndices.Add (hullFaceIndices[i][0]);
					destIndices.Add (hullFaceIndices[i][j]);
					destIndices.Add (hullFaceIndices[i][j+1]);
				}
			}
			
			int[] destIndicesArray = new int[destIndices.Count];
			for (int i=0; i<destIndices.Count; i++)
				destIndicesArray[i] = destIndices[i];

			// Push to collision mesh

			hull.collisionMesh.vertices = destVertices;
			hull.collisionMesh.triangles = destIndicesArray;
			hull.collisionMesh.RecalculateBounds ();
		}

		private void GenerateFace(Hull hull, Vector3[] meshVertices, int[] meshIndices, float thickness)
		{
			int totalFaces = hull.selectedFaces.Count;
			Vector3[] facePoints = new Vector3[totalFaces * 3 * 2];
			
			for (int i=0; i<hull.selectedFaces.Count; i++)
			{
				int faceIndex = hull.selectedFaces[i];
				
				Vector3 p0 = meshVertices[meshIndices[faceIndex * 3]];
				Vector3 p1 = meshVertices[meshIndices[faceIndex * 3 + 1]];
				Vector3 p2 = meshVertices[meshIndices[faceIndex * 3 + 2]];

				Vector3 d0 = (p1 - p0).normalized;
				Vector3 d1 = (p2 - p0).normalized;

				Vector3 normal = Vector3.Cross(d1, d0);

				int baseIndex = i * 3 * 2;

				facePoints[baseIndex]		= p0;
				facePoints[baseIndex + 1]	= p1;
				facePoints[baseIndex + 2]	= p2;

				facePoints[baseIndex + 3]	= p0 + (normal * thickness);
				facePoints[baseIndex + 4]	= p1 + (normal * thickness);
				facePoints[baseIndex + 5]	= p2 + (normal * thickness);
			}

			int[] indices = new int[totalFaces * 3 * 2];
			for (int i=0; i<indices.Length; i++)
				indices [i] = i;

			// Push to collision mesh
			
			hull.faceCollisionMesh.vertices = facePoints;
			hull.faceCollisionMesh.triangles = indices;
			hull.faceCollisionMesh.RecalculateBounds ();
		}
		
		public bool ContainsMesh(Mesh m)
		{
			foreach (Hull h in hulls)
			{
				if (h.collisionMesh == m)
					return true;
			}
			return false;
		}

		private static void Inflate(Vector3 point, ref Vector3 min, ref Vector3 max)
		{
			min.x = Mathf.Min(min.x, point.x);
			min.y = Mathf.Min(min.y, point.y);
			min.z = Mathf.Min(min.z, point.z);
			
			max.x = Mathf.Max(max.x, point.x);
			max.y = Mathf.Max(max.y, point.y);
			max.z = Mathf.Max(max.z, point.z);
		}
	}

} // namespace Technie.PhysicsCreator

