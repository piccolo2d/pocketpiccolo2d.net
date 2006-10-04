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
using System.Collections;

using UMD.HCIL.PocketPiccolo;
using UMD.HCIL.PocketPiccolo.Util;

namespace UMD.HCIL.PocketPiccolo.Nodes {
	/// <summary>
	/// Summary description for PPath.
	/// </summary>
	public class PPath : PNode {
		ArrayList points;
		PointF[] resizePoints;
		Pen pen = new Pen(Color.Black);
		bool closed = false;
		bool updatingBoundsFromPath;

		private static PMatrix TEMP_MATRIX = new PMatrix();

		public static PPath CreateLine(float x1, float y1, float x2, float y2) {
			PPath line = new PPath();
			line.AddPoint(new PointF(x1, y1));
			line.AddPoint(new PointF(x2, y2));
			return line;
		}

		public static PPath CreateRectangle(float x, float y, float width, float height) {
			PPath rect = new PPath();
			SetPathToRectangle(rect, x, y, width, height);
			return rect;
		}

		public virtual void SetPathToRectangle(float x, float y, float width, float height) {
			SetPathToRectangle(this, x, y, width, height);
			UpdateBoundsFromPath();
			InvalidatePaint();
		}

		private static void SetPathToRectangle(PPath path, float x, float y, float width, float height) {
			path.Reset();
			path.AddPoint(new PointF(x, y));
			path.AddPoint(new PointF(x+width, y));
			path.AddPoint(new PointF(x+width, y+height));
			path.AddPoint(new PointF(x, y+height));
			path.Closed = true;
		}

		public PPath() {
			points = new ArrayList();
		}

		public PPath(PointF[] points) : this() {
			this.points.AddRange(points);
			UpdateBoundsFromPath();
		}

		public Pen Pen {
			get { return pen; }
			set {
				pen = value;
			}
		}

		public virtual bool Closed {
			get { return closed; }
			set {
				closed = value;
				InvalidatePaint();
			}
		}

		public virtual void Reset() {
			points.Clear();
			UpdateBoundsFromPath();
			InvalidatePaint();
		}

		public virtual void AddPoint(PointF point) {
			points.Add(point);
			UpdateBoundsFromPath();
			InvalidatePaint();
		}

		public override void StartResizeBounds() {
			resizePoints = (PointF[])points.ToArray(typeof(PointF));
		}

		public override void EndResizeBounds() {
			resizePoints = null;
		}

		protected override void InternalUpdateBounds(float x, float y, float width, float height) {
			if (updatingBoundsFromPath || points.Count == 0) {
				return;
			}

			if (resizePoints != null) {
				points.Clear();
				points.AddRange(resizePoints);
			}

			RectangleF pathBounds = PathBounds;

			TEMP_MATRIX.Reset();
			TEMP_MATRIX.TranslateBy(x, y);
			TEMP_MATRIX.ScaleBy(width / pathBounds.Width, height / pathBounds.Height);
			TEMP_MATRIX.TranslateBy(-pathBounds.X, -pathBounds.Y);

			for (int i = 0; i < points.Count; i++) {
				points[i] = TEMP_MATRIX.Transform((PointF)points[i]);
			}
		}

		private RectangleF PathBounds {
			get {
				RectangleF pathBounds = RectangleF.Empty;
				if (points.Count > 0) {
					float minX = ((PointF)points[0]).X;
					float minY = ((PointF)points[0]).Y;
					float maxX = ((PointF)points[0]).X;
					float maxY = ((PointF)points[0]).Y;
					foreach(PointF point in points) {
						if (point.X < minX) minX = point.X;
						if (point.Y < minY) minY = point.Y;
						if (point.X > maxX) maxX = point.X;
						if (point.Y > maxY) maxY = point.Y;
					}
			
					pathBounds = new RectangleF(minX, minY, maxX-minX, maxY-minY);
				}
				
				return pathBounds;
			}
		}

		public virtual void UpdateBoundsFromPath() {
			updatingBoundsFromPath = true;
			Bounds = PathBounds;
			updatingBoundsFromPath = false;
		}

		protected override void Paint(UMD.HCIL.PocketPiccolo.Util.PPaintContext paintContext) {
			Graphics2D g = paintContext.Graphics;

			if (points.Count > 0) {
				if (Brush != null && closed) {
					g.FillPolygon(Brush, (PointF[])points.ToArray(typeof(PointF)));
				}

				if (pen != null) {
					if (closed) {
						g.DrawPolygon(pen, (PointF[])points.ToArray(typeof(PointF)));
					} else {
						for (int i = 0; i < points.Count-1; i++) {
							g.DrawLine(pen, (PointF)points[i], (PointF)points[i+1]);
						}
					}
				}
			}
		}
	}
}