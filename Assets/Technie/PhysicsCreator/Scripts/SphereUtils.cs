using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace Technie.PhysicsCreator
{

public class SphereUtils
{
	// error checking
	const float kEpsilon = 1e-03f;
	const float kOnePlusEpsilon = 1.0f + kEpsilon;

	// indices of points that support current minimum volume sphere
	public class Support
	{
	    public int m_iQuantity;
	    public int[] m_aiIndex = new int[4];

		public bool Contains (int iIndex, List<Vector3> points)
	    {
	        for (int i = 0; i < m_iQuantity; i++)
	        {
				Vector3 kDiff = points[iIndex] - points[ m_aiIndex[i] ];
				if ( kDiff.sqrMagnitude < kEpsilon )
	                return true;
	        }
	        return false;
	    }
	}

	// All internal minimal sphere calculations store the squared radius in the
	// radius member of Sphere.  Only at the end is a sqrt computed.

	private static bool PointInsideSphere (Vector3 rkP, Sphere rkS)
	{
	    Vector3 kDiff = rkP - rkS.center;
	    float fTest = kDiff.sqrMagnitude;
		return fTest <= kOnePlusEpsilon * rkS.radius;  // theory:  test <= R^2
	}

	private static Sphere ExactSphere1 (Vector3 rkP)
	{
	    Sphere kMinimal = new Sphere();
	    kMinimal.center = rkP;
	    kMinimal.radius = 0.0f;
	    return kMinimal;
	}

	private static Sphere ExactSphere2 (Vector3 rkP0, Vector3 rkP1)
	{
	    Sphere kMinimal = new Sphere();
	    kMinimal.center = 0.5f * (rkP0 + rkP1);
	    Vector3 kDiff = rkP1 - rkP0;
	    kMinimal.radius = 0.25f * kDiff.sqrMagnitude;
	    return kMinimal;
	}

	private static Sphere ExactSphere3 (Vector3 rkP0, Vector3 rkP1, Vector3 rkP2)
	{
	    // Compute the circle (in 3D) containing p0, p1, and p2.  The Center() in
	    // barycentric coordinates is K = u0*p0+u1*p1+u2*p2 where u0+u1+u2=1.
	    // The Center() is equidistant from the three points, so |K-p0| = |K-p1| =
	    // |K-p2| = R where R is the radius of the circle.
	    //
	    // From these conditions,
	    //   K-p0 = u0*A + u1*B - A
	    //   K-p1 = u0*A + u1*B - B
	    //   K-p2 = u0*A + u1*B
	    // where A = p0-p2 and B = p1-p2, which leads to
	    //   r^2 = |u0*A+u1*B|^2 - 2*Dot(A,u0*A+u1*B) + |A|^2
	    //   r^2 = |u0*A+u1*B|^2 - 2*Dot(B,u0*A+u1*B) + |B|^2
	    //   r^2 = |u0*A+u1*B|^2
	    // Subtracting the last equation from the first two and writing
	    // the equations as a linear system,
	    //
	    // +-                 -++   -+       +-        -+
	    // | Dot(A,A) Dot(A,B) || u0 | = 0.5 | Dot(A,A) |
	    // | Dot(B,A) Dot(B,B) || u1 |       | Dot(B,B) |
	    // +-                 -++   -+       +-        -+
	    //
	    // The following code solves this system for u0 and u1, then
	    // evaluates the third equation in r^2 to obtain r.

	    Vector3 kA = rkP0 - rkP2;
	    Vector3 kB = rkP1 - rkP2;
	    float fAdA = Vector3.Dot(kA, kA);
	    float fAdB = Vector3.Dot(kA, kB);
	    float fBdB = Vector3.Dot(kB, kB);
	    float fDet = fAdA * fBdB - fAdB * fAdB;

	    Sphere minimal = new Sphere();

	    float fHalfInvDet = 0.5f / fDet;
	    float fU0 = fHalfInvDet*fBdB*(fAdA-fAdB);
	    float fU1 = fHalfInvDet*fAdA*(fBdB-fAdB);
	    float fU2 = 1.0f-fU0-fU1;
	    minimal.center = fU0*rkP0 + fU1*rkP1 + fU2*rkP2;
	    Vector3 kTmp = fU0*kA + fU1*kB;
	    minimal.radius = kTmp.sqrMagnitude;

	    return minimal;
	}

	private static Sphere ExactSphere4 (Vector3 rkP0, Vector3 rkP1, Vector3 rkP2, Vector3 rkP3)
	{
	    // Compute the sphere containing p0, p1, p2, and p3.  The Center() in
	    // barycentric coordinates is K = u0*p0+u1*p1+u2*p2+u3*p3 where
	    // u0+u1+u2+u3=1.  The Center() is equidistant from the three points, so
	    // |K-p0| = |K-p1| = |K-p2| = |K-p3| = R where R is the radius of the
	    // sphere.
	    //
	    // From these conditions,
	    //   K-p0 = u0*A + u1*B + u2*C - A
	    //   K-p1 = u0*A + u1*B + u2*C - B
	    //   K-p2 = u0*A + u1*B + u2*C - C
	    //   K-p3 = u0*A + u1*B + u2*C
	    // where A = p0-p3, B = p1-p3, and C = p2-p3 which leads to
	    //   r^2 = |u0*A+u1*B+u2*C|^2 - 2*Dot(A,u0*A+u1*B+u2*C) + |A|^2
	    //   r^2 = |u0*A+u1*B+u2*C|^2 - 2*Dot(B,u0*A+u1*B+u2*C) + |B|^2
	    //   r^2 = |u0*A+u1*B+u2*C|^2 - 2*Dot(C,u0*A+u1*B+u2*C) + |C|^2
	    //   r^2 = |u0*A+u1*B+u2*C|^2
	    // Subtracting the last equation from the first three and writing
	    // the equations as a linear system,
	    //
	    // +-                          -++   -+       +-        -+
	    // | Dot(A,A) Dot(A,B) Dot(A,C) || u0 | = 0.5 | Dot(A,A) |
	    // | Dot(B,A) Dot(B,B) Dot(B,C) || u1 |       | Dot(B,B) |
	    // | Dot(C,A) Dot(C,B) Dot(C,C) || u2 |       | Dot(C,C) |
	    // +-                          -++   -+       +-        -+
	    //
	    // The following code solves this system for u0, u1, and u2, then
	    // evaluates the fourth equation in r^2 to obtain r.

	    Vector3 kE10 = rkP0 - rkP3;
	    Vector3 kE20 = rkP1 - rkP3;
	    Vector3 kE30 = rkP2 - rkP3;

	    float[,] aafA = new float[3, 3];
	    aafA[0, 0] = Vector3.Dot(kE10, kE10);
	    aafA[0, 1] = Vector3.Dot(kE10, kE20);
	    aafA[0, 2] = Vector3.Dot(kE10, kE30);
	    aafA[1, 0] = aafA[0, 1];
	    aafA[1, 1] = Vector3.Dot(kE20, kE20);
	    aafA[1, 2] = Vector3.Dot(kE20, kE30);
	    aafA[2, 0] = aafA[0, 2];
	    aafA[2, 1] = aafA[1, 2];
	    aafA[2, 2] = Vector3.Dot(kE30, kE30);

	    float[] afB = new float[3];
	    afB[0] = 0.5f * aafA[0, 0];
	    afB[1] = 0.5f * aafA[1, 1];
	    afB[2] = 0.5f * aafA[2, 2];

	    float[,] aafAInv = new float[3, 3];
	    aafAInv[0, 0] = aafA[1, 1]*aafA[2, 2]-aafA[1, 2]*aafA[2, 1];
	    aafAInv[0, 1] = aafA[0, 2]*aafA[2, 1]-aafA[0, 1]*aafA[2, 2];
	    aafAInv[0, 2] = aafA[0, 1]*aafA[1, 2]-aafA[0, 2]*aafA[1, 1];
	    aafAInv[1, 0] = aafA[1, 2]*aafA[2, 0]-aafA[1, 0]*aafA[2, 2];
	    aafAInv[1, 1] = aafA[0, 0]*aafA[2, 2]-aafA[0, 2]*aafA[2, 0];
	    aafAInv[1, 2] = aafA[0, 2]*aafA[1, 0]-aafA[0, 0]*aafA[1, 2];
	    aafAInv[2, 0] = aafA[1, 0]*aafA[2, 1]-aafA[1, 1]*aafA[2, 0];
	    aafAInv[2, 1] = aafA[0, 1]*aafA[2, 0]-aafA[0, 0]*aafA[2, 1];
	    aafAInv[2, 2] = aafA[0, 0]*aafA[1, 1]-aafA[0, 1]*aafA[1, 0];
	    float fDet = aafA[0, 0]*aafAInv[0, 0] + aafA[0, 1]*aafAInv[1, 0] + aafA[0, 2]*aafAInv[2, 0];

		Sphere kMinimal = new Sphere();

	    float fInvDet = 1.0f / fDet;
	    int iRow, iCol;
	    for (iRow = 0; iRow < 3; iRow++)
	    {
	        for (iCol = 0; iCol < 3; iCol++)
	            aafAInv[iRow, iCol] *= fInvDet;
	    }
	        
	    float[] afU = new float[4];
	    for (iRow = 0; iRow < 3; iRow++)
	    {
	        afU[iRow] = 0.0f;
	        for (iCol = 0; iCol < 3; iCol++)
			{
	            afU[iRow] += aafAInv[iRow, iCol]*afB[iCol];
			}
	    }
	    afU[3] = 1.0f - afU[0] - afU[1] - afU[2];
	        
	    kMinimal.center = afU[0]*rkP0 + afU[1]*rkP1 + afU[2]*rkP2 + afU[3]*rkP3;
	    Vector3 kTmp = afU[0]*kE10 + afU[1]*kE20 + afU[2]*kE30;
	    kMinimal.radius = kTmp.sqrMagnitude;

	    return kMinimal;
	}

	private static Sphere UpdateSupport1 (int i, List<Vector3> apkPerm, Support rkSupp)
	{
	    Vector3 rkP0 = apkPerm[rkSupp.m_aiIndex[0]];
	    Vector3 rkP1 = apkPerm[i];

	    Sphere kMinimal = ExactSphere2(rkP0, rkP1);
	    rkSupp.m_iQuantity = 2;
	    rkSupp.m_aiIndex[1] = i;
		
	    return kMinimal;
	}

	private static Sphere UpdateSupport2 (int i, List<Vector3> apkPerm, Support rkSupp)
	{
	    Vector3 rkP0 = apkPerm[rkSupp.m_aiIndex[0]];
	    Vector3 rkP1 = apkPerm[rkSupp.m_aiIndex[1]];
	    Vector3 rkP2 = apkPerm[i];

	    Sphere[] akS = new Sphere[3];
		float fMinRSqr = Mathf.Infinity;
	    int iIndex = -1;

	    akS[0] = ExactSphere2(rkP0,rkP2);
	    if ( PointInsideSphere(rkP1,akS[0]) )
	    {
	        fMinRSqr = akS[0].radius;
	        iIndex = 0;
	    }

	    akS[1] = ExactSphere2(rkP1,rkP2);
	    if ( PointInsideSphere(rkP0,akS[1]) )
	    {
	        if ( akS[1].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[1].radius;
	            iIndex = 1;
	        }
	    }

	    Sphere kMinimal;
		
	    if ( iIndex != -1 )
	    {
	        kMinimal = akS[iIndex];
	        rkSupp.m_aiIndex[1-iIndex] = i;
	    }
	    else
	    {
	        kMinimal = ExactSphere3(rkP0,rkP1,rkP2);
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[2] = i;
	    }

	    return kMinimal;
	}

	private static Sphere UpdateSupport3 (int i, List<Vector3> apkPerm, Support rkSupp)
	{
		Vector3 rkP0 = apkPerm[rkSupp.m_aiIndex[0]];
		Vector3 rkP1 = apkPerm[rkSupp.m_aiIndex[1]];
		Vector3 rkP2 = apkPerm[rkSupp.m_aiIndex[2]];
		Vector3 rkP3 = apkPerm[i];

	    Sphere[] akS = new Sphere[6];
	    float fMinRSqr = Mathf.Infinity;
	    int iIndex = -1;

	    akS[0] = ExactSphere2(rkP0, rkP3);
	    if ( PointInsideSphere(rkP1,akS[0]) && PointInsideSphere(rkP2,akS[0]) )
	    {
	        fMinRSqr = akS[0].radius;
	        iIndex = 0;
	    }

	    akS[1] = ExactSphere2(rkP1,rkP3);
	    if ( PointInsideSphere(rkP0,akS[1]) && PointInsideSphere(rkP2,akS[1]) )
	    {
	        if ( akS[1].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[1].radius;
	            iIndex = 1;
	        }
	    }

	    akS[2] = ExactSphere2(rkP2,rkP3);
	    if ( PointInsideSphere(rkP0,akS[2]) && PointInsideSphere(rkP1,akS[2]) )
	    {
	        if ( akS[2].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[2].radius;
	            iIndex = 2;
	        }
	    }

	    akS[3] = ExactSphere3(rkP0,rkP1,rkP3);
	    if ( PointInsideSphere(rkP2,akS[3]) )
	    {
	        if ( akS[3].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[3].radius;
	            iIndex = 3;
	        }
	    }

	    akS[4] = ExactSphere3(rkP0,rkP2,rkP3);
	    if ( PointInsideSphere(rkP1,akS[4]) )
	    {
	        if ( akS[4].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[4].radius;
	            iIndex = 4;
	        }
	    }

	    akS[5] = ExactSphere3(rkP1, rkP2, rkP3);
	    if ( PointInsideSphere(rkP0,akS[5]) )
	    {
	        if ( akS[5].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[5].radius;
	            iIndex = 5;
	        }
	    }

	    Sphere kMinimal;

	    switch ( iIndex )
	    {
		    case 0:
		        kMinimal = akS[0];
		        rkSupp.m_iQuantity = 2;
		        rkSupp.m_aiIndex[1] = i;
		        break;
		    case 1:
		        kMinimal = akS[1];
		        rkSupp.m_iQuantity = 2;
		        rkSupp.m_aiIndex[0] = i;
		        break;
		    case 2:
		        kMinimal = akS[2];
		        rkSupp.m_iQuantity = 2;
		        rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[2];
		        rkSupp.m_aiIndex[1] = i;
		        break;
		    case 3:
		        kMinimal = akS[3];
		        rkSupp.m_aiIndex[2] = i;
		        break;
		    case 4:
		        kMinimal = akS[4];
		        rkSupp.m_aiIndex[1] = i;
		        break;
		    case 5:
		        kMinimal = akS[5];
		        rkSupp.m_aiIndex[0] = i;
		        break;
		    default:
		        kMinimal = ExactSphere4(rkP0, rkP1, rkP2, rkP3);
		        rkSupp.m_iQuantity = 4;
		        rkSupp.m_aiIndex[3] = i;
		        break;
	    }
		
	    return kMinimal;
	}
	
	public static Sphere UpdateSupport4 (int i, List<Vector3> apkPerm, Support rkSupp)
	{
	    Vector3 rkP0 = apkPerm[ rkSupp.m_aiIndex[0] ];
	    Vector3 rkP1 = apkPerm[ rkSupp.m_aiIndex[1] ];
	    Vector3 rkP2 = apkPerm[ rkSupp.m_aiIndex[2] ];
	    Vector3 rkP3 = apkPerm[ rkSupp.m_aiIndex[3] ];
	    Vector3 rkP4 = apkPerm[ i ];

	    Sphere[] akS = new Sphere[14];
	    float fMinRSqr = Mathf.Infinity;
	    int iIndex = -1;

	    akS[0] = ExactSphere2(rkP0,rkP4);
	    if ( PointInsideSphere(rkP1,akS[0])
	    &&   PointInsideSphere(rkP2,akS[0])
	    &&   PointInsideSphere(rkP3,akS[0]) )
	    {
	        fMinRSqr = akS[0].radius;
	        iIndex = 0;
	    }

	    akS[1] = ExactSphere2(rkP1,rkP4);
	    if ( PointInsideSphere(rkP0,akS[1])
	    &&   PointInsideSphere(rkP2,akS[1])
	    &&   PointInsideSphere(rkP3,akS[1]) )
	    {
	        if ( akS[1].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[1].radius;
	            iIndex = 1;
	        }
	    }

	    akS[2] = ExactSphere2(rkP2,rkP4);
	    if ( PointInsideSphere(rkP0,akS[2])
	    &&   PointInsideSphere(rkP1,akS[2])
	    &&   PointInsideSphere(rkP3,akS[2]) )
	    {
	        if ( akS[2].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[2].radius;
	            iIndex = 2;
	        }
	    }

	    akS[3] = ExactSphere2(rkP3,rkP4);
	    if ( PointInsideSphere(rkP0,akS[3])
	    &&   PointInsideSphere(rkP1,akS[3])
	    &&   PointInsideSphere(rkP2,akS[3]) )
	    {
	        if ( akS[3].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[3].radius;
	            iIndex = 3;
	        }
	    }

	    akS[4] = ExactSphere3(rkP0,rkP1,rkP4);
	    if ( PointInsideSphere(rkP2,akS[4])
	    &&   PointInsideSphere(rkP3,akS[4]) )
	    {
	        if ( akS[4].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[4].radius;
	            iIndex = 4;
	        }
	    }

	    akS[5] = ExactSphere3(rkP0,rkP2,rkP4);
	    if ( PointInsideSphere(rkP1,akS[5])
	    &&   PointInsideSphere(rkP3,akS[5]) )
	    {
	        if ( akS[5].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[5].radius;
	            iIndex = 5;
	        }
	    }

	    akS[6] = ExactSphere3(rkP0,rkP3,rkP4);
	    if ( PointInsideSphere(rkP1,akS[6])
	    &&   PointInsideSphere(rkP2,akS[6]) )
	    {
	        if ( akS[6].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[6].radius;
	            iIndex = 6;
	        }
	    }

	    akS[7] = ExactSphere3(rkP1,rkP2,rkP4);
	    if ( PointInsideSphere(rkP0,akS[7])
	    &&   PointInsideSphere(rkP3,akS[7]) )
	    {
	        if ( akS[7].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[7].radius;
	            iIndex = 7;
	        }
	    }

	    akS[8] = ExactSphere3(rkP1,rkP3,rkP4);
	    if ( PointInsideSphere(rkP0,akS[8])
	    &&   PointInsideSphere(rkP2,akS[8]) )
	    {
	        if ( akS[8].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[8].radius;
	            iIndex = 8;
	        }
	    }

	    akS[9] = ExactSphere3(rkP2,rkP3,rkP4);
	    if ( PointInsideSphere(rkP0,akS[9])
	    &&   PointInsideSphere(rkP1,akS[9]) )
	    {
	        if ( akS[9].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[9].radius;
	            iIndex = 9;
	        }
	    }

	    akS[10] = ExactSphere4(rkP0,rkP1,rkP2,rkP4);
	    if ( PointInsideSphere(rkP3,akS[10]) )
	    {
	        if ( akS[10].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[10].radius;
	            iIndex = 10;
	        }
	    }

	    akS[11] = ExactSphere4(rkP0,rkP1,rkP3,rkP4);
	    if ( PointInsideSphere(rkP2,akS[11]) )
	    {
	        if ( akS[11].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[11].radius;
	            iIndex = 11;
	        }
	    }

	    akS[12] = ExactSphere4(rkP0,rkP2,rkP3,rkP4);
	    if ( PointInsideSphere(rkP1,akS[12]) )
	    {
	        if ( akS[12].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[12].radius;
	            iIndex = 12;
	        }
	    }

	    akS[13] = ExactSphere4(rkP1,rkP2,rkP3,rkP4);
	    if ( PointInsideSphere(rkP0,akS[13]) )
	    {
	        if ( akS[13].radius < fMinRSqr )
	        {
	            fMinRSqr = akS[13].radius;
	            iIndex = 13;
	        }
	    }

		Sphere kMinimal = akS[iIndex];

	    switch ( iIndex )
	    {
	    case 0:
	        rkSupp.m_iQuantity = 2;
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 1:
	        rkSupp.m_iQuantity = 2;
	        rkSupp.m_aiIndex[0] = i;
	        break;
	    case 2:
	        rkSupp.m_iQuantity = 2;
	        rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[2];
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 3:
	        rkSupp.m_iQuantity = 2;
	        rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 4:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[2] = i;
	        break;
	    case 5:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 6:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[1] = rkSupp.m_aiIndex[3];
	        rkSupp.m_aiIndex[2] = i;
	        break;
	    case 7:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[0] = i;
	        break;
	    case 8:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
	        rkSupp.m_aiIndex[2] = i;
	        break;
	    case 9:
	        rkSupp.m_iQuantity = 3;
	        rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 10:
	        rkSupp.m_aiIndex[3] = i;
	        break;
	    case 11:
	        rkSupp.m_aiIndex[2] = i;
	        break;
	    case 12:
	        rkSupp.m_aiIndex[1] = i;
	        break;
	    case 13:
	        rkSupp.m_aiIndex[0] = i;
	        break;
	    }

	    return kMinimal;
	}
	
	private static Sphere Update(int funcIndex, int numPoints, List<Vector3> points, Support support)
	{
		switch (funcIndex)
		{
			case 0:
				return null;
			case 1:
				return UpdateSupport1(numPoints, points, support);
			case 2:
				return UpdateSupport2(numPoints, points, support);
			case 3:
				return UpdateSupport3(numPoints, points, support);
			case 4:
				return UpdateSupport4(numPoints, points, support);
		}
		return null;
	}

	/** Find the minimum volume bounding sphere that surrounds the input points.
		Based on Welzl's algorithm, see 'Geometry Tools For Computer Graphics', p811
	*/
	public static Sphere MinSphere (List<Vector3> inputPoints)
	{
	    Sphere minimal = new Sphere();
	    Support support = new Support();

		if ( inputPoints.Count >= 1 )
	    {
			// Create shuffled copy of akPoint 
			
			List<Vector3> shuffledPoints = new List<Vector3>(inputPoints);
			Shuffle(shuffledPoints);
			
			minimal = ExactSphere1(shuffledPoints[0]);
						
			support.m_iQuantity = 1;
			support.m_aiIndex[0] = 0;
	        int i = 1;
			
			while ( i < inputPoints.Count )
	        {
				if ( !support.Contains(i, shuffledPoints) )
	            {
					if ( !PointInsideSphere(shuffledPoints[i], minimal) )
	                {
						minimal = Update(support.m_iQuantity, i, shuffledPoints, support);
	                    i = 0;
	                    continue;
	                }
	            }
	            i++;
	        }
	    }

		minimal.radius = Mathf.Sqrt(minimal.radius);
		return minimal;
	}

    public static void Shuffle(List<Vector3> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
			int r = i + Random.Range(0, list.Count-i);
			Vector3 t = list[r];
			list[r] = list[i];
			list[i] = t;
        }
    }
	
}

} // namespace Technie.PhysicsCreator
