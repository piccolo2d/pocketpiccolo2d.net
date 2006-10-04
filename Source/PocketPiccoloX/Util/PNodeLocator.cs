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
//using System.Runtime.Serialization;

using UMD.HCIL.PocketPiccolo;
using UMD.HCIL.PocketPiccolo.Util;

namespace UMD.HCIL.PocketPiccoloX.Util {
	/// <summary>
	/// <b>PNodeLocator</b> provides an abstraction for locating points on a node.
	/// </summary>
	/// <remarks>
	/// Points are located in the local corrdinate system of the node.  The default
	/// behavior is to locate the center point of the node's bounds.  The node where
	/// the point is located is stored internal to this locator (as an instance
	/// variable).  If you want to use the same locator to locate center points on
	/// many different nodes, you will need to set the <see cref="PNodeLocator.Node">
	/// Node</see> property before asking for each location.
	/// </remarks>
	//[Serializable]
	public class PNodeLocator : PLocator {

		#region Fields
		/// <summary>
		/// The node on which points are located.
		/// </summary>
		protected PNode node;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new PNodeLocator that locates points on the given node.
		/// </summary>
		/// <param name="node">The node on which the points are located.</param>
		public PNodeLocator(PNode node) {
			Node = node;
		}
		#endregion

		#region Node
		/// <summary>
		/// Gets or sets the node on which points are located.
		/// </summary>
		/// <value>The node on which points are located.</value>
		public PNode Node {
			get { return node; }
			set { node = value; }
		}
		#endregion

		#region Locate Point
		/// <summary>
		/// Overridden.  Gets the x coordinate of the point located on the node.
		/// </summary>
		/// <value>The x coordinate of the located point.</value>
		public override float LocateX {
			get { return PUtil.CenterOfRectangle(node.Bounds).X; }
		}

		/// <summary>
		/// Overridden.  Gets the y coordinate of the point located on the node.
		/// </summary>
		/// <value>The y coordinate of the located point.</value>
		public override float LocateY {
			get { return PUtil.CenterOfRectangle(node.Bounds).Y; }
		}
		#endregion
	}
}