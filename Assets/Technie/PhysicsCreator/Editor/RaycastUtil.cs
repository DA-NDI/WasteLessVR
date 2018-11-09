using UnityEngine;
using System.Collections;

/*
 * There's a bug in MeshCollider.Raycast in Unity 5.0+ which returns bogus triangle indices for meshes with degenerate/skinny/tiny triangles
 * https://issuetracker.unity3d.com/issues/raycasthit-returns-the-wrong-triangle-index-in-unity-5
 * 
 * This is probably going to be fixed in 5.5, but maybe not.
 * 
 * In the meantime, RaycastUtil lets us manually raycast again triangles to perform the check ourselves to avoid this bug.
 * 
 */
public class RaycastUtil
{
	public static bool Raycast(MeshCollider targetCollider, Ray pickRay, out int hitTriIndex, float rayLength)
	{
		bool usePhysX = false;
		if (usePhysX)
		{
			// Let Unity/PhysX perform the raycast (potentially faster, but buggy)
			RaycastHit hit;
			if (targetCollider.Raycast(pickRay, out hit, rayLength))
			{
				hitTriIndex = hit.triangleIndex;
				return true;
			}
			else
			{
				hitTriIndex = -1;
				return false;
			}
		}
		else
		{
			// Manually raycast against the collider's triangles
			return RaycastTriangles(targetCollider, pickRay, out hitTriIndex, rayLength);
		}
	}

	private static bool RaycastTriangles(MeshCollider targetCollider, Ray pickRay, out int hitTriIndex, float rayLength)
	{
		hitTriIndex = -1;

		int[] indices = targetCollider.sharedMesh.triangles;
		Vector3[] localVertices = targetCollider.sharedMesh.vertices;

		// The actual collider might have negative scales on some or all of the x/y/z axies
		// Each negative will flip the winding order, and therefore which side we consider the 'back' face to be
		// See how many times the winding is flipped and pass that onto the intersection routine to change the backface rejection accordingly
		bool flipNormal = false;
		Vector3 colliderScale = targetCollider.transform.lossyScale;
		if (colliderScale.x < 0.0f)
			flipNormal = !flipNormal;
		if (colliderScale.y < 0.0f)
			flipNormal = !flipNormal;
		if (colliderScale.z < 0.0f)
			flipNormal = !flipNormal;

		float bestDist = Mathf.Infinity;
		for (int i=0; i<indices.Length; i+=3)
		{
			Vector3 localP0 = localVertices[ indices[i] ];
			Vector3 localP1 = localVertices[ indices[i+1] ];
			Vector3 localP2 = localVertices[ indices[i+2] ];

			Vector3 worldP0 = targetCollider.transform.TransformPoint(localP0);
			Vector3 worldP1 = targetCollider.transform.TransformPoint(localP1);
			Vector3 worldP2 = targetCollider.transform.TransformPoint(localP2);

			float dist = 0.0f;
			if (Intersect(worldP0, worldP1, worldP2, pickRay, flipNormal, out dist))
			{
				// Is this intersection point closer than our last intersection?

				if (dist < bestDist)
				{
					hitTriIndex = i / 3;
					bestDist = dist;
				}
			}
		}

		return hitTriIndex != -1;
	}

	/*
		Checks if the specified ray hits the triagnlge descibed by p1, p2 and p3.
		Möller–Trumbore ray-triangle intersection algorithm implementation.
		
		returns true when the ray hits the triangle, otherwise false
	*/
	public static bool Intersect(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray, bool flipNormal, out float intersectDist)
	{
		intersectDist = 0.0f;

		// Vectors from p1 to p2/p3 (edges)
		Vector3 e1, e2;

		Vector3 p, q, t;
		float det, invDet, u, v;

		// Find vectors for two edges sharing vertex/point p1
		e1 = p2 - p1;
		e2 = p3 - p1;

		// Calc triangle normal and reject if back facing
		Vector3 normal = Vector3.Cross(e2, e1);
		float dot = Vector3.Dot(ray.direction, normal);
		if (flipNormal)
		{
			if (dot > 0.0f)
				return false;
		}
		else
		{
			if (dot < 0.0f)
				return false;
		}

		// Calculate determinat
		p = Vector3.Cross(ray.direction, e2);
		det = Vector3.Dot(e1, p);

		// if determinant is near zero, ray lies in plane of triangle otherwise not
		if (det > -Mathf.Epsilon && det < Mathf.Epsilon)
		{
			return false;
		}

		invDet = 1.0f / det;

		// Calculate distance from p1 to ray origin
		t = ray.origin - p1;

		// Calculate u parameter
		u = Vector3.Dot(t, p) * invDet;

		// Check for ray hit
		if (u < 0 || u > 1)
		{
			return false;
		}

		// Prepare to test v parameter
		q = Vector3.Cross(t, e1);

		// Calculate v parameter
		v = Vector3.Dot(ray.direction, q) * invDet;

		// Check for ray hit
		if (v < 0 || u + v > 1)
		{
			return false;
		}

		float dist = Vector3.Dot(e2, q) * invDet;

		if (dist > Mathf.Epsilon)
		{ 
			// ray does intersect
			intersectDist = dist;
			return true;
		}

		// No hit at all
		return false;
	}
}
