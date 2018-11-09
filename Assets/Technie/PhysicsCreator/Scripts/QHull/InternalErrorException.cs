
using System;

namespace Technie.PhysicsCreator.QHull
{
	
	/**
	 * Exception thrown when QuickHull3D encounters an internal error.
	 */
	public class InternalErrorException : SystemException
	{
		public InternalErrorException (string msg) : base(msg)
		{
			
		}
	}

} // namespace QHull
