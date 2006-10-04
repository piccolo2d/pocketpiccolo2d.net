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

namespace UMD.HCIL.PocketPiccolo.Util {

	/// <summary>
	/// <b>PMatrix</b> defines an affine transform.  The compact .NET framework does not provide
	/// a matrix class.  This class is implemented by storing the scale and offsets rather than
	/// the actual matrix values.  It does not currently support rotation.  And it does not
	/// currently support all of the methods provided by PMatrix in Piccolo.NET.
	/// </summary>
	public class PMatrixOneScale : ICloneable {

		private float scale;
		private float offsetX;
		private float offsetY;

		public PMatrixOneScale() {
			scale = 1;
			offsetX = 0;
			offsetY = 0;
		}

		public Object Clone() {
			PMatrixOneScale r = new PMatrixOneScale();
			r.Multiply(this);
			return r;
		}

		public override bool Equals(object obj) {
			PMatrixOneScale otherMatrix = (PMatrixOneScale)obj;
			return (offsetX == otherMatrix.offsetX &&
				offsetY == otherMatrix.offsetY &&
				scale == otherMatrix.scale);
		}

		public override int GetHashCode() {
			return offsetX.GetHashCode() ^ offsetY.GetHashCode() ^ scale.GetHashCode();
		}

		public bool IsInvertible {
			get { return scale != 0; }
		}

		public void Invert() {
			if (IsInvertible) {
				scale = 1 / scale;
				offsetX = -offsetX * scale;
				offsetY = -offsetY * scale;
			}
		}

		public void Multiply(PMatrixOneScale aTransform) {
			ScaleBy(aTransform.scale);
			offsetX = aTransform.scale * offsetX + aTransform.OffsetX;
			offsetY = aTransform.scale * offsetY + aTransform.OffsetY;
		}

		public void Reset() {
			scale = 1;
			offsetX = 0;
			offsetY = 0;
		}

		public float OffsetX {
			get { return offsetX; }
			set { offsetX = value; }
		}

		public float OffsetY {
			get { return offsetY; }
			set { offsetY = value; }
		}

		public void TranslateBy(float dx, float dy) {
			offsetX += (scale * dx);
			offsetY += (scale * dy);
		}

		public float Scale {
			get { return scale; }
			set { scale = value; }
		}

		public void ScaleBy(float scale) {
			this.scale *= scale;
		}

		public void ScaleBy(float scale, float x, float y) {
			TranslateBy(x, y);
			this.scale *= scale;
			TranslateBy(-x, -y);
		}


		public PointF Transform(PointF point) {
			point.X = scale * point.X + offsetX;
			point.Y = scale * point.Y + offsetY;
			return point;
		}

		public SizeF Transform(SizeF size) {
			size.Width = scale * size.Width;
			size.Height = scale * size.Height;
			return size;
		}

		public RectangleF Transform(RectangleF rect) {
			rect.X = scale * rect.X + offsetX;
			rect.Y = scale * rect.Y + offsetY;
			rect.Width = scale * rect.Width;
			rect.Height = scale * rect.Height;
			return rect;
		}

		/// <summary>
		/// Applies the geometric transform represented by this <see cref="PMatrix"/> object to all
		/// of the points in the given array.
		/// <see cref="PMatrix"/>.
		/// </summary>
		/// <param name="pts">The array of points to transform.</param>
		public virtual void TransformPoints(PointF[] pts) {			
			int count = pts.Length;
			for (int i = 0; i < count; i++) {
				pts[i] = this.Transform(pts[i]);
			}
		}

		public PointF InverseTransform(PointF point) {
			point.X = (1/scale) * (point.X - offsetX);
			point.Y = (1/scale) * (point.Y - offsetY);
			return point;
		}

		public SizeF InverseTransform(SizeF size) {
			size.Width = (1/scale) * size.Width;
			size.Height = (1/scale) * size.Height;
			return size;
		}

		public RectangleF InverseTransform(RectangleF rect) {
			rect.X = (1/scale) * (rect.X - offsetX);
			rect.Y = (1/scale) * (rect.Y - offsetY);
			rect.Width = (1/scale) * rect.Width;
			rect.Height = (1/scale) * rect.Height;
			return rect;
		}
	}
}
