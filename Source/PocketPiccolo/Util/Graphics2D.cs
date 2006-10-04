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
using System.Drawing.Imaging;

namespace UMD.HCIL.PocketPiccolo.Util
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Graphics2D
	{
		Graphics graphics;
		PMatrix matrix;

		public Graphics2D(Graphics graphics) {
			this.graphics = graphics;
			matrix = new PMatrix();
		}

		public virtual Region Clip {
			get {
				return graphics.Clip;
			}
			set {
				graphics.Clip = value;
			}
		}

		public virtual Graphics Graphics {
			get { return graphics; }
		}

		public virtual PMatrix Matrix {
			get { return (PMatrix)matrix.Clone(); }
			set { matrix = value; }
		}

		public virtual RectangleF ClipBounds {
			get { return graphics.ClipBounds; }
		}

		public virtual void MultiplyTransform(PMatrix matrix) {
			TranslateTransform(matrix.OffsetX, matrix.OffsetY);
			ScaleTransform(matrix.Scale);
		}

		public virtual void TranslateTransform(float dx, float dy) {
			matrix.TranslateBy(dx, dy);
		}

		public virtual void ScaleTransform(float scale) {
			matrix.ScaleBy(scale);
		}

		public virtual void FillRectangle(Brush brush, RectangleF rect) {
			FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public virtual void FillRectangle(Brush brush, float x, float y, float width, float height) {
			RectangleF rectangle = matrix.Transform(new RectangleF(x, y, width, height));
			graphics.FillRectangle(brush, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}

		public virtual void DrawRectangle(Pen pen, RectangleF rect) {
			DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public virtual void DrawRectangle(Pen pen, float x, float y, float width, float height) {
			RectangleF rectangle = matrix.Transform(new RectangleF(x, y, width, height));
			graphics.DrawRectangle(pen, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}
		
		public virtual void DrawEllipse(Pen pen, RectangleF rect) {
			DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public virtual void DrawEllipse(Pen pen, float x, float y, float width, float height) {
			RectangleF rectangle = matrix.Transform(new RectangleF(x, y, width, height));
			graphics.DrawEllipse(pen, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}

		public virtual void FillEllipse(Brush brush, RectangleF rect) {
			FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public virtual void FillEllipse(Brush brush, float x, float y, float width, float height) {
			RectangleF rectangle = matrix.Transform(new RectangleF(x, y, width, height));
			graphics.FillEllipse(brush, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}

		public virtual void DrawImage(Image image, float x, float y) {
			//RectangleF bounds = new RectangleF(x, y, image.Width, image.Height);
			//RectangleF transformedBounds = matrix.Transform(bounds);
			//Rectangle destRect = new Rectangle((int)transformedBounds.X, (int)transformedBounds.Y, (int)transformedBounds.Width, (int)transformedBounds.Height);
			//Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
			//graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);

			RectangleF destRect = new RectangleF(x, y, image.Width, image.Height);
			RectangleF srcRect = new RectangleF(0, 0, image.Width, image.Height);
			DrawImage(image, destRect, srcRect, null);
		}

		public virtual void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, ImageAttributes imageAttr) {
			RectangleF transformedDestRect = matrix.Transform(destRect);
			Rectangle iDestRect = new Rectangle((int)Math.Round(transformedDestRect.X), (int)Math.Round(transformedDestRect.Y),
				(int)Math.Round(transformedDestRect.Width), (int)Math.Round(transformedDestRect.Height));
			Rectangle iSrcRect = new Rectangle((int)Math.Round(srcRect.X), (int)Math.Round(srcRect.Y),
				(int)Math.Round(srcRect.Width), (int)Math.Round(srcRect.Height));

			if (imageAttr != null) {
				graphics.DrawImage(image, iDestRect, iSrcRect.X, iSrcRect.Y, iSrcRect.Width, iSrcRect.Height, GraphicsUnit.Pixel, imageAttr);
			} else {
				graphics.DrawImage(image, iDestRect, iSrcRect, GraphicsUnit.Pixel);
			}
		}

		public virtual void DrawString(string s, Font font, Brush brush, RectangleF rect) {
			RectangleF transformedBounds = matrix.Transform(rect);
			Font transformedFont = new Font(font.Name, matrix.Scale * font.Size, font.Style);
			graphics.DrawString(s, transformedFont, brush, transformedBounds);
		}

		public virtual void DrawString(string s, Font font, Brush brush, float x, float y) 
		{
			PointF[] points = {new PointF(x,y)};
			matrix.TransformPoints(points);
			graphics.DrawString(s, font, brush, points[0].X, points[0].Y);
		}

		public virtual void DrawLine(Pen pen, PointF start, PointF end) 
		{
			PointF[] points = {start, end};
			matrix.TransformPoints(points);
			graphics.DrawLine(pen, (int)points[0].X, (int)points[0].Y, (int)points[1].X, (int)points[1].Y);
		}

		public virtual void  DrawPolygon(Pen pen, PointF[] points) {
			PointF[] transformedPoints = new PointF[points.Length];
			points.CopyTo(transformedPoints, 0);
			matrix.TransformPoints(transformedPoints);

			Point[] iTransformedPoints = new Point[transformedPoints.Length];

			for(int i = 0; i < transformedPoints.Length; i++) {
				iTransformedPoints[i] = (Point)transformedPoints[i];
			}
			graphics.DrawPolygon(pen, iTransformedPoints);
		}

		public virtual void  FillPolygon(Brush brush, PointF[] points) {
			PointF[] transformedPoints = new PointF[points.Length];
			points.CopyTo(transformedPoints, 0);
			matrix.TransformPoints(transformedPoints);

			Point[] iTransformedPoints = new Point[transformedPoints.Length];

			for(int i = 0; i < transformedPoints.Length; i++) {
				iTransformedPoints[i] = (Point)transformedPoints[i];
			}
			graphics.FillPolygon(brush, iTransformedPoints);
		}
	}
}
