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
using System.Drawing.Drawing2D;
using System.Text;

using UMD.HCIL.PocketPiccolo.Util;

namespace UMD.HCIL.PocketPiccolo.Activities {
	/// <summary> 
	/// <b>PTransformActivity</b> interpolates between two transforms setting its
	/// target's transform as it goes.
	/// </summary>
	/// <remarks>
	/// See PNode. animate*() for an example of this activity in used. The source
	/// transform is retrieved from the target just before the animation is scheduled
	/// to start.
	/// </remarks>
	public class PTransformActivity : PInterpolatingActivity {

		#region Fields
		private static PMatrix STATIC_MATRIX = new PMatrix();

		//private float[] source;
		//private float[] destination;
		private PMatrix source;
		private PMatrix destination;
		private Target target;
		#endregion

		#region Target Interface
		/// <summary>
		/// <b>Target</b> objects that want to get transformed by the transform 
		/// activity must implement this interface.
		/// </summary>
		public interface Target {
			PMatrix Matrix {
				// This is called right before the transform activity starts. That
				// way an object is always animated from its current position.
				get;

				// This will be called by the transform activity for each new transform
				// that it computes while it is stepping.
				set;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new PTransformActivity that will animate from the source matrix
		/// to the destination matrix.
		/// </summary>
		/// <param name="duration">The length of one loop of the activity.</param>
		/// <param name="stepInterval">
		/// The minimum number of milliseconds that this activity should delay between
		/// steps.
		/// </param>
		/// <param name="aTarget">
		/// The object that the activity will be applied to and where the source
		/// state will be taken from.
		/// </param>
		/// <remarks>
		/// This constructs a PTransformActivity with a null destination matrix.  The
		/// destination matrix must be set before this activity is scheduled.
		/// </remarks>
		public PTransformActivity(long duration, long stepInterval, Target aTarget) :
			this(duration, stepInterval, aTarget, null) {
		}

		/// <summary>
		/// Constructs a new PTransformActivity that will animate from the source matrix
		/// to the destination matrix.
		/// </summary>
		/// <param name="duration">The length of one loop of the activity.</param>
		/// <param name="stepInterval">
		/// The minimum number of milliseconds that this activity should delay between
		/// steps.
		/// </param>
		/// <param name="aTarget">
		/// The object that the activity will be applied to and where the source
		/// state will be taken from.
		/// </param>
		/// <param name="aDestination">The destination matrix.</param>
		public PTransformActivity(long duration, long stepInterval, Target aTarget, PMatrix aDestination) :
			this(duration, stepInterval, 1, ActivityMode.SourceToDestination, aTarget, aDestination) {
		}

		/// <summary>
		/// Constructs a new PTransformActivity that animate between the source and destination
		/// matrices in the order specified by the mode, looping the given number of iterations.
		/// </summary>
		/// <param name="duration">The length of one loop of the activity.</param>
		/// <param name="stepInterval">
		/// The minimum number of milliseconds that this activity should delay between steps.
		/// </param>
		/// <param name="loopCount">
		/// The number of times the activity should reschedule itself.
		/// </param>
		/// <param name="mode">
		/// Defines how the activity interpolates between states.
		/// </param>
		/// <param name="aTarget">
		/// The object that the activity will be applied to and where the source
		/// state will be taken from.
		/// </param>
		/// <param name="aDestination">The destination matrix.</param>
		public PTransformActivity(long duration, long stepInterval, int loopCount, ActivityMode mode, Target aTarget, PMatrix aDestination) :
			base(duration, stepInterval, loopCount, mode) {

			//source = new float[6];
			//destination = new float[6];
			target = aTarget;
			//if (aDestination != null) {
			//	destination = aDestination.MatrixReference.Elements;
			//}

			source = new PMatrix();
			destination = new PMatrix();
			destination.Multiply(aDestination);
		}
		#endregion

		#region Basics
		/// <summary>
		/// Overridden.  Gets a value indicating whether this activity is performing an animation.
		/// </summary>
		/// <value>True if this activity is performing an animation; false otherwise.</value>
		/// <remarks>
		/// This property will always return true since a PTransformActivity is an animating
		/// activity.
		/// <para>
		/// This is used by the PCanvas to determine if it should set the render quality to
		/// PCanvas.animatingRenderQuality or not for each frame it renders. 
		/// </para>
		///</remarks>
		public override bool IsAnimation {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets or sets the final matrix that will be used on the transform activity's
		/// target when the transform activity stops stepping.
		/// </summary>
		/// <value>The final matrix that will be used on the target.</value>
		public virtual PMatrix DestinationMatrix {
			get { return (PMatrix)destination.Clone(); }
			set { destination = value; }
		}
		#endregion

		#region Stepping
		/// <summary>
		/// Overridden.  See <see cref="PInterpolatingActivity.OnActivityStarted">
		/// PInterpolatingActivity.OnActivityStarted</see>.
		/// </summary>
		protected override void OnActivityStarted() {
			//if (FirstLoop) source = target.GetMatrix().MatrixReference.Elements;
			if (FirstLoop) source = (PMatrix)target.Matrix.Clone(); //.MatrixReference.Elements;
			base.OnActivityStarted();
		}

		/// <summary>
		/// Overridden.  See <see cref="PInterpolatingActivity.SetRelativeTargetValue">
		/// PInterpolatingActivity.SetRelativeTargetValue</see>.
		/// </summary>
		public override void SetRelativeTargetValue(float zeroToOne) {
			base.SetRelativeTargetValue(zeroToOne);
			/*
			Matrix m = new Matrix(
				source[0] + (zeroToOne * (destination[0] - source[0])),
				source[1] + (zeroToOne * (destination[1] - source[1])),
				source[2] + (zeroToOne * (destination[2] - source[2])),
				source[3] + (zeroToOne * (destination[3] - source[3])),
				source[4] + (zeroToOne * (destination[4] - source[4])),
				source[5] + (zeroToOne * (destination[5] - source[5]))
				);

			STATIC_MATRIX.Matrix = m;
			*/
			STATIC_MATRIX.OffsetX = source.OffsetX + (zeroToOne * (destination.OffsetX - source.OffsetX));
			STATIC_MATRIX.OffsetY = source.OffsetY + (zeroToOne * (destination.OffsetY - source.OffsetY));
			STATIC_MATRIX.Scale = source.Scale + (zeroToOne * (destination.Scale - source.Scale));

			target.Matrix = STATIC_MATRIX;
		}
		#endregion

		#region Debugging
		/// <summary>
		/// Overridden.  Gets a string representing the state of this object.
		/// </summary>
		/// <value>A string representation of this object's state.</value>
		/// <remarks>
		/// This method is intended to be used only for debugging purposes, and the content and
		/// format of the returned string may vary between implementations. The returned string
		/// may be empty but may not be <c>null</c>.
		/// </remarks>
		protected override String ParamString {
			get {
				StringBuilder result = new StringBuilder();

				result.Append("source=" + (source == null ? "null" : source.ToString() + "{" + GetElementString(source) + "}"));
				result.Append(",destination=" + (destination == null ? "null" : destination.ToString() + "{" + GetElementString(destination) + "}"));
				result.Append(',');
				result.Append(base.ParamString);
		
				return result.ToString();
			}
		}

		/// <summary>
		/// Gets a string representation of the elements of a matrix.
		/// </summary>
		/// <param name="elements">The elements of a matrix.</param>
		/// <returns>A string representation of the elements of a matrix.</returns>
		protected static String GetElementString(PMatrix elements) {
			return (elements.OffsetX + " " + elements.OffsetY + " " + elements.Scale);
			/*
			StringBuilder result = new StringBuilder();

			int length = elements.Length;
			for (int i = 0; i < elements.Length; i++) {
				result.Append(elements[i].ToString());
				if (i < length-1) result.Append(", ");
			}

			return result.ToString();
			*/
		}		
		#endregion
	}
}
