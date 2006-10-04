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
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
//using System.Drawing.Text;

namespace UMD.HCIL.PocketPiccolo.Util {

	#region Enums
	/// <summary>
	/// This enumeration is used by the <see cref="PPaintContext"/> class.  It represents the
	/// quality level with which the piccolo scene graph will be rendered.
	/// </summary>
	/// <remarks>Lower quality rendering is faster.</remarks>
	public enum RenderQuality {
		/// <summary>
		/// The scene graph will be rendered in low quality mode.
		/// </summary>
		LowQuality,

		/// <summary>
		/// The scene graph will be rendered in high quality mode.
		/// </summary>
		HighQuality
	}
	#endregion

	/// <summary>
	/// <b>PPaintContext</b> is used by piccolo nodes to paint themselves on the screen.
	/// </summary>
	/// <remarks>
	/// This class wraps a Graphics object to implement painting.
	/// </remarks>
	public class PPaintContext {

		#region Fields
		/// <summary>
		/// The current PPaintContext.
		/// </summary>
		public static PPaintContext CURRENT_PAINT_CONTEXT;

		private static PMatrix TEMP_MATRIX = new PMatrix();
		private static PointF[] PTS = new PointF[2];
		private Graphics2D graphics;
		private PCanvas canvas;
		private RenderQuality renderQuality;

		/// <summary>
		/// A stack of the clip regions that the paint context applies.  These regions are not
		/// affected by the matrices in the transform stack.
		/// </summary>
		/// <remarks>
		/// The last clip pushed will be the clip applied before the current clip.
		/// </remarks>
		protected Stack clipStack;

		/// <summary>
		/// A stack of rectangles representing the local clips.  These values will be affected by
		/// the matrices in the transform stack.
		/// </summary>
		/// <remarks>The last clip pushed will be the current clip.</remarks>
		protected Stack localClipStack;

		/// <summary>
		/// A stack of the cameras being painted.
		/// </summary>
		protected Stack cameraStack;

		/// <summary>
		/// A stack of the transforms that the paint context applies.
		/// </summary>
		protected Stack transformStack;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new PPaintContext.
		/// </summary>
		/// <param name="graphics">
		/// The graphics context to associate with this paint context.
		/// </param>
		/// <param name="canvas">The canvas that the paint context will render on.</param>
		public PPaintContext(Graphics2D graphics, PCanvas canvas) {
			this.graphics = graphics;
			this.canvas = canvas;
			clipStack = new Stack();
			localClipStack = new Stack();
			cameraStack = new Stack();
			transformStack = new Stack();
			RenderQuality = RenderQuality.HighQuality;

			Region clip = graphics.Clip;
			if (clip.IsInfinite(graphics.Graphics)) {
				clip = new Region(
					new Rectangle(-int.MaxValue / 2,
					-int.MaxValue / 2,
					int.MinValue,
					int.MaxValue));
				graphics.Clip = clip;
			}

			localClipStack.Push(graphics.ClipBounds);
			CURRENT_PAINT_CONTEXT = this;
		}
		#endregion

		#region Context Attributes
		//****************************************************************
		// Context Attributes - Methods for accessing attributes of the
		// graphics context.
		//****************************************************************

		/// <summary>
		/// Gets the graphics context associated with this paint context.
		/// </summary>
		/// <value>The graphics context associated with this paint context.</value>
		public virtual Graphics2D Graphics {
			get { return graphics; }
		}

		/// <summary>
		/// Gets the canvas that this paint context renders on.
		/// </summary>
		/// <value>The canvas that this paint context renders on.</value>
		public virtual PCanvas Canvas {
			get { return canvas; }
		}

		/// <summary>
		/// Gets the current local clip.
		/// </summary>
		/// <value>The current local clip.</value>
		public virtual RectangleF LocalClip {
			get { return (RectangleF)localClipStack.Peek(); }
		}

		/// <summary>
		/// Gets the scale value applied by the graphics context associated with this paint
		/// context.
		/// </summary>
		public virtual float Scale {
			get {
				PTS[0] = new PointF(0, 0);
				PTS[1] = new PointF(1, 0);
				TEMP_MATRIX.Reset();
				TEMP_MATRIX.Multiply(graphics.Matrix);
				//TEMP_MATRIX.MatrixReference.Multiply(graphics.Matrix);
				TEMP_MATRIX.TransformPoints(PTS);
				//graphics.Transform.TransformPoints(PTS);
				return PUtil.DistanceBetweenPoints(PTS[0], PTS[1]);
			}
		}
		#endregion

		#region Context Attribute Stacks
		//****************************************************************
		// Context Attribute Stacks - Attributes that can be pushed and
		// popped.
		//****************************************************************

		/// <summary>
		/// Pushes the given camera onto the camera stack.
		/// </summary>
		/// <param name="camera">The camera to push.</param>
		public virtual void PushCamera(PCamera camera) {
			cameraStack.Push(camera);
		}

		/// <summary>
		/// Pops a camera from the camera stack.
		/// </summary>
		/// <param name="camera">The camera to pop.</param>
		public virtual void PopCamera(PCamera camera) {
			cameraStack.Pop();
		}

		/// <summary>
		/// Gets the bottom-most camera on the camera stack (the last camera pushed).
		/// </summary>
		public virtual PCamera Camera {
			get { return (PCamera)cameraStack.Peek(); }
		}

		/// <summary>
		/// Pushes the current clip onto the clip stack and sets clip of the graphics context to
		/// the intersection of the current clip and the given clip.
		/// </summary>
		/// <remarks>
		/// The intersection of the current local clip and the new local clip is also pushed onto
		/// the local clip stack.
		/// </remarks>
		/// <param name="aClip">The clip to push.</param>
		public virtual void PushClip(Region aClip) {
			RectangleF aClipBounds = aClip.GetBounds(graphics.Graphics);
			RectangleF transformedClipBounds = Graphics.Matrix.Transform(aClipBounds);
			aClip = new Region(new Rectangle((int)Math.Floor(transformedClipBounds.X),
				(int)Math.Floor(transformedClipBounds.Y),
				(int)Math.Ceiling(transformedClipBounds.Width),
				(int)Math.Ceiling(transformedClipBounds.Height)));

			// Push the untransformed clip onto the local clip stack so that it will be transformed
			// when the matrix is pushed.
			RectangleF newLocalClip = PUtil.IntersectionOfRectangles(LocalClip, aClipBounds);
			localClipStack.Push(newLocalClip);

			// Push the transformed clip onto the clip stack since the graphics objec does not
			// contain a matrix and will not do the transforming itself.
			Region currentClip = Graphics.Clip;
			clipStack.Push(currentClip);
			aClip = aClip.Clone();
			aClip.Intersect(currentClip);
			Graphics.Clip = aClip;
		}

		/// <summary>
		/// Pops a clip from both the clip stack and the local clip stack and sets the clip of the
		/// graphics context to the clip popped from the clip stack.
		/// </summary>
		/// <param name="aClip">The clip to pop.</param>
		public virtual void PopClip(Region aClip) {
			Region newClip = (Region) clipStack.Pop();
			Graphics.Clip = newClip;
			localClipStack.Pop();
		}

		/// <summary>
		/// Pushes the given matrix onto the transform stack.
		/// </summary>
		/// <param name="matrix">The matrix to push.</param>
		/// <remarks>
		/// This method also applies the matrix to the graphics context and the current local clip.
		/// The new local clip is then pushed onto the local clip stack.
		/// </remarks>
		public virtual void PushTransform(PMatrix matrix) {
			if (matrix == null) return;
			RectangleF newLocalClip = matrix.InverseTransform(LocalClip);
			transformStack.Push(graphics.Matrix);
			localClipStack.Push(newLocalClip);
			graphics.MultiplyTransform(matrix);
		}

		/// <summary>
		/// Pops a matrix from the transform stack.
		/// </summary>
		/// <param name="matrix">The matrix to pop.</param>
		/// <remarks>This method also pops a clip from the local clip stack.</remarks>
		public virtual void PopTransform(PMatrix matrix) {
			if (matrix == null) return;
			graphics.Matrix = (PMatrix) transformStack.Pop();
			localClipStack.Pop();
		}
		#endregion

		#region Render Quality
		//****************************************************************
		// Render Quality - Methods for setting the rendering hints.
		//****************************************************************
		/// <summary>
		/// Sets the rendering hints for this paint context. The render quality is most often set
		/// by the rendering PCanvas.  Use <see cref="PCanvas.AnimatingRenderQuality">
		/// PCanvas.AnimatingRenderQuality</see> and <see cref="PCanvas.InteractingRenderQuality">
		/// PCanvas.InteractingRenderQuality</see> to set these values.
		/// </summary>
		/// <value>The rendering hints for this paint context.</value>
		public virtual RenderQuality RenderQuality {
			get {
				return renderQuality;
			}

			set {
				renderQuality = value;
				/*
				switch (value) {
					case UMD.HCIL.PocketPiccolo.Util.RenderQuality.HighQuality:
						graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
						graphics.SmoothingMode = SmoothingMode.HighQuality;  //anitaliased
						graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
						graphics.CompositingQuality = CompositingQuality.HighQuality;
						break;
					case UMD.HCIL.PocketPiccolo.Util.RenderQuality.LowQuality:
						graphics.InterpolationMode = InterpolationMode.Low;
						graphics.SmoothingMode = SmoothingMode.HighSpeed;
						graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
						graphics.CompositingQuality = CompositingQuality.HighSpeed;
						break;
				}
				*/
			}
		}
		#endregion
	}
}
