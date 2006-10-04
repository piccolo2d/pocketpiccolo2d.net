/* 
 * Copyright (c) 2003-2004, University of Maryland
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided
 * that the following conditions are met:
 * 
 *		Redistributions of source code must retain the above copyright notice, this list of conditions
 *		and the following disclaimer.
 * 
 *		Redistributions in binary form must reproduce the above copyright notice, this list of conditions
 *		and the following disclaimer in the documentation and/or other materials provided with the
 *		distribution.
 * 
 *		Neither the name of the University of Maryland nor the names of its contributors may be used to
 *		endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
 * TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * Piccolo was written at the Human-Computer Interaction Laboratory www.cs.umd.edu/hcil by Jesse Grosjean
 * and ported to C# by Aaron Clamage under the supervision of Ben Bederson.  The Piccolo website is
 * www.cs.umd.edu/hcil/piccolo.
 */

using System;
using System.Drawing;
//using System.Runtime.Serialization;

using UMD.HCIL.PocketPiccolo.Util;

namespace UMD.HCIL.PocketPiccoloX.Util {
	/// <summary>
	/// <b>PLocator</b> provides an abstraction for locating points.
	/// </summary>
	/// <remarks>
	/// <b>Notes to Inheritors:</b>  Subclasses such as <see cref="PNodeLocator"/> and
	/// <see cref="PBoundsLocator"/> specialize this behavior by locating points on nodes,
	/// or on the bounds of nodes.
	/// </remarks>
	//[Serializable]
	public abstract class PLocator {
		/// <summary>
		/// Constructs a new PLocator.
		/// </summary>
		public PLocator() {
		}

		/// <summary>
		/// Gets the located point.
		/// </summary>
		/// <value>The located point.</value>
		public PointF LocatePoint {
			get { return new PointF(LocateX, LocateY); }
		}

		/// <summary>
		/// Gets the x coordinate of the located point.
		/// </summary>
		public abstract float LocateX {
			get;
		}

		/// <summary>
		/// Gets the y coordinate of the located point.
		/// </summary>
		public abstract float LocateY {
			get;
		}
	}
}
