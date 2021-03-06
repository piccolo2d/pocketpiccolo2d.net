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
using System.Data;
using System.Runtime.Serialization;

using UMD.HCIL.PocketPiccolo.Util;
using UMD.HCIL.PocketPiccolo.Activities;

namespace UMD.HCIL.PocketPiccolo {

	#region Enums
	/// <summary>
	/// This enumeration is used by the PCamera class.  It represents the types
	/// of constraints that can be applied to a camera's view matrix.
	/// </summary>
	public enum CameraViewConstraint {
		/// <summary>
		/// No constraint should be applied to the view.
		/// </summary>
		None,

		/// <summary>
		/// Try to keep the entire bounds of all the camera's layers within the camera's
		/// view.
		/// </summary>
		All,

		/// <summary>
		/// Try to keep the center point of all the camera's layers within the camera's view.
		/// </summary>
		Center
	};
	#endregion

	/// <summary>
	/// <b>PCamera</b> represents a viewport onto a list of layer nodes.
	/// </summary>
	/// <remarks>
	/// Each camera maintains a view transform through which it views these
	/// layers. Translating and scaling this view transform is how zooming
	/// and panning are implemented.
	/// <para>
 	/// Cameras are also the point through which all PInputEvents enter Piccolo. The
 	/// canvas coordinate system, and the local coordinate system of the topmost camera
	/// should always be the same.
	/// </para>
	/// </remarks>
	//[Serializable]
	public class PCamera : PNode {  //ISerializable

		#region Fields
		/// <summary>
		/// The key that identifies a change in this camera's view matrix. 
		/// </summary>
		/// <remarks>
		/// In a property change event the new value will be a reference to this
		/// node's view matrix, but old value will always be null.
		/// <seealso cref="PCamera.ViewMatrix">PCamera.ViewMatrix</seealso>.
		/// </remarks>
		protected static readonly object viewTransformEventKey = new object();

		[NonSerialized] private PCanvas canvas;
		[NonSerialized] private PLayerList layers;
		private PMatrix viewMatrix;
		private CameraViewConstraint viewConstraint;

		//private Pen boundsPen = new Pen(Brushes.Red, 0);
		//private Brush fullBoundsBrush = new SolidBrush(Color.FromArgb(51, 255, 0, 0));
		private Pen boundsPen = new Pen(Color.Red);
		private Pen fullBoundsPen = new Pen(Color.Blue);
		#endregion
 
		#region Constructors
		/// <summary>
		/// Constructs a new camera with no layers and a default white color.
		/// </summary>
		public PCamera() : base() {
			viewMatrix = new PMatrix();
			layers = new PLayerList();
			viewConstraint = CameraViewConstraint.None;

			Brush = new SolidBrush(Color.White);
		}
		#endregion

		#region Target Canvas
		//****************************************************************
		// Target Canvas - The canvas where this camera will be painted.
		//****************************************************************

		/// <summary>
		/// Gets or sets the canvas associated with this camera.
		/// </summary>
		/// <value>
		/// The canvas associated with this camera.
		/// </value>
		/// <remarks>
		/// This property will return null if no canvas has been associated with this camera,
		/// as may be the case for internal cameras.
		/// <para>
		/// When the camera is repainted it will request repaints on this canvas.
		/// </para>
		/// </remarks>
		public virtual PCanvas Canvas {
			get { return canvas; }
			set { 
				canvas = value;
				InvalidatePaint();
			}
		}
		#endregion

		#region Paint Damage Management
		//****************************************************************
		// Paint Damage Management - Methods used to invalidate the areas
		// of the screen that that need to be repainted.
		//****************************************************************

		/// <summary>
		/// Overridden.  Repaint this camera, and forward the repaint request to the camera's
		/// canvas if it is not null.
		/// </summary>
		/// <param name="bounds">The bounds to repaint, in local coordinates.</param>
		/// <param name="childOrThis">
		/// If childOrThis does not equal this then this camera's matrix will be applied to the
		/// bounds paramater.
		/// </param>
		public override void RepaintFrom(RectangleF bounds, PNode childOrThis) {
			if (Parent != null) {
				if (childOrThis != this) {
					bounds = LocalToParent(bounds);
				}

				if (Canvas != null) {
					Canvas.InvalidateBounds(bounds);
				}
			
				Parent.RepaintFrom(bounds, this);
			}
		}

		/// <summary>
		/// Repaint from one of the camera's layers.
		/// </summary>
		/// <param name="bounds">The bounds to repaint, in view coordinates.</param>
		/// <param name="repaintedLayer">The layer that was repainted.</param>
		/// <remarks>
		/// The repaint region needs to be transformed from view to local coordinates in
		/// this case.  Unlike most repaint methods in piccolo this one must not modify
		/// the bounds parameter.
		/// </remarks>
		public virtual void RepaintFromLayer(RectangleF bounds, PNode repaintedLayer) {	
			bounds = ViewToLocal(bounds);

			//if (Bounds.IntersectsWith(bounds)) {
			if (PUtil.RectanglesIntersect(Bounds, bounds)) {
				RectangleF tempRect = PUtil.IntersectionOfRectangles(bounds, Bounds);
					//RectangleF.Intersect(bounds, Bounds);
				RepaintFrom(tempRect, repaintedLayer);
			}
		}
		#endregion

		#region Layers
		//****************************************************************
		// Layers - Methods for manipulating the layers viewed by the
		// camera.
		//****************************************************************

		/// <summary>
		/// Gets a reference to the list of layers managed by this camera.
		/// </summary>
		/// <value>The list of layers managed by this camera.</value>
		public virtual PLayerList LayersReference {
			get { return layers; }
		}

		/// <summary>
		/// Gets the number of layers managed by this camera.
		/// </summary>
		/// <value>The number of layers managed by this camera.</value>
		public virtual int LayerCount {
			get { return layers.Count; }
		}

		/// <summary>
		/// Return the layer at the specified index.
		/// </summary>
		/// <param name="index">The index of the desired layer.</param>
		/// <returns>The layer at the specified index.</returns>
		public virtual PLayer GetLayer(int index) {
			return layers[index];
		}
		
		/// <summary>
		/// Return the index where the given layer is stored.
		/// </summary>
		/// <param name="layer">The layer whose index is desired.</param>
		/// <returns>The index where the given layer is stored.</returns>
		public virtual int IndexOfLayer(PLayer layer) {
			return layers.IndexOf(layer);
		}

		/// <summary>
		/// Add the layer to the end of this camera's list of layers.  Layers may be
		/// viewed by multiple cameras at once.
		/// </summary>
		/// <param name="layer">The layer to add to this camera.</param>
		public virtual void AddLayer(PLayer layer) {
			AddLayer(LayerCount, layer);
		}
		
		/// <summary>
		/// Add the layer at the given index in this camera's list of layers.  Layers
		/// may be viewed by multiple cameras at once.
		/// </summary>
		/// <param name="index">The index at which to add the layer.</param>
		/// <param name="layer">The layer to add to this camera.</param>
		public virtual void AddLayer(int index, PLayer layer) {
			layers.Insert(index, layer);
			layer.AddCamera(this);
			InvalidatePaint();
		}
		
		/// <summary>
		/// Remove the given layer from the list of layers managed by this camera.
		/// </summary>
		/// <param name="layer">The layer to remove.</param>
		/// <returns>The removed layer.</returns>
		public virtual PLayer RemoveLayer(PLayer layer) {
			return RemoveLayer(layers.IndexOf(layer));
		}
		
		/// <summary>
		/// Remove the layer at the given index from the list of layers managed by this
		/// camera.
		/// </summary>
		/// <param name="index">The index of the layer to remove.</param>
		/// <returns>The removed layer.</returns>
		public virtual PLayer RemoveLayer(int index) {
			PLayer layer = layers[index];
			layers.RemoveAt(index);
			layer.RemoveCamera(this);
			InvalidatePaint();
			return layer;
		}

		/// <summary>
		/// Gets the total bounds of all the layers that this camera looks at.
		/// </summary>
		/// <value>The total bounds of this camera's layers.</value>
		public virtual RectangleF UnionOfLayerFullBounds {
			get {
				RectangleF result = RectangleF.Empty;
		
				foreach (PLayer each in layers) {
					RectangleF eachBounds = each.FullBounds;
					if (result != RectangleF.Empty) {
						if (eachBounds != RectangleF.Empty)
							result = PUtil.UnionOfRectangles(result, each.FullBounds);
								//RectangleF.Union(result, each.FullBounds);
					} 
					else {
						result = eachBounds;
					}
				}
				return result;
			}
		}
		#endregion

		#region Painting Layers
		//****************************************************************
		// Painting Layers - Methods for painting the layers viewed by
		// the camera.
		//****************************************************************

		/// <summary>
		/// Overridden.  Paint this camera (default background color is white) and then paint
		/// the camera's view through the view transform.
		/// </summary>
		/// <param name="paintContext">The paint context to use for painting this camera.</param>
		protected override void Paint(PPaintContext paintContext) {
			base.Paint(paintContext);
			RectangleF b = this.Bounds;
			Rectangle iBounds = new Rectangle((int)b.X, (int)b.Y, (int)b.Width, (int)b.Height);

			paintContext.PushClip(new Region(iBounds));
			paintContext.PushTransform(viewMatrix);

			PaintCameraView(paintContext);
			PaintDebugInfo(paintContext);

			paintContext.PopTransform(viewMatrix);
			paintContext.PopClip(new Region(iBounds));
		}

		/// <summary>
		/// Paint all the layers that the camera is looking at.
		/// </summary>
		/// <param name="paintContext">
		/// The paint context to use for painting this camera's view.
		/// </param>
		/// <remarks>
		/// This method is only called when the cameras view matrix and clip are applied to
		/// the paintContext.
		/// </remarks>
		protected virtual void PaintCameraView(PPaintContext paintContext) {
			foreach (PLayer each in layers) {
				each.FullPaint(paintContext);
			}
		}
 
		/// <summary>
		/// This method paints the bounds and full bounds of nodes when the appropriate debug
		/// flags are set.
		/// </summary>
		/// <param name="paintContext">
		/// The paint context to use for painting debug information.
		/// </param>
		/// <remarks>
		/// Setting debugBounds and/or debugFullBounds flags is useful for visual debugging.
		/// </remarks>
		protected virtual void PaintDebugInfo(PPaintContext paintContext) {
			if (PDebug.DebugBounds || PDebug.DebugFullBounds) {
				Graphics2D g = paintContext.Graphics;

				PNodeList nodes = new PNodeList();
				RectangleF nodeBounds = RectangleF.Empty;
				
				for (int i = 0; i < LayerCount; i++) {
					GetLayer(i).GetAllNodes(null, nodes);
				}				
			
				GetAllNodes(null, nodes);

				foreach (PNode each in nodes) {

					if (PDebug.DebugBounds) {
						nodeBounds = each.Bounds;
					
						if (!nodeBounds.IsEmpty) {
							nodeBounds = each.LocalToGlobal(nodeBounds);
							nodeBounds = GlobalToLocal(nodeBounds);
							if (each == this || each.IsDescendentOf(this)) {
								nodeBounds = LocalToView(nodeBounds);
							}
							g.DrawRectangle(boundsPen, nodeBounds.X, nodeBounds.Y, nodeBounds.Width, nodeBounds.Height);
						}
					}
		
					if (PDebug.DebugFullBounds) {
						nodeBounds = each.FullBounds;

						if (!nodeBounds.IsEmpty) {
							if (each.Parent != null) {
								nodeBounds = each.Parent.LocalToGlobal(nodeBounds);
							}
							nodeBounds = GlobalToLocal(nodeBounds);		
							if (each == this || each.IsDescendentOf(this)) {
								nodeBounds = LocalToView(nodeBounds);
							}
							//g.FillRectangle(fullBoundsBrush, nodeBounds);
							g.DrawRectangle(fullBoundsPen, nodeBounds);
						}
					}
				}
			}		
		}

		/// <summary>
		/// Overridden.  Push the camera onto the paintContext, so that it can later be accessed
		/// by <see cref="PPaintContext.Camera">PPaintContext.Camera</see>, and then paint this
		/// node and all of it's descendents.
		/// </summary>
		/// <param name="paintContext">The paint context to use for painting this camera.</param>
		public override void FullPaint(PPaintContext paintContext) {
			paintContext.PushCamera(this);
			base.FullPaint(paintContext);
			paintContext.PopCamera(this);
		}				
		#endregion

		#region Picking
		//****************************************************************
		// Picking - Methods for picking the camera and it's view.
		//****************************************************************

		/// <summary>
		/// Generate and return a PPickPath for the point x,y specified in the local
		/// coordinate system of this camera.
		/// </summary>
		/// <param name="x">The x coordinate of the pick point.</param>
		/// <param name="y">The y coordinate of the pick point.</param>
		/// <param name="halo">
		/// The value to use for the width and height of the rectangle used for picking.
		/// </param>
		/// <returns>A PPickPath for the given point.</returns>
		/// <remarks>
		/// Picking is done with a rectangle, halo specifies how large that rectangle
		/// will be.
		/// </remarks>
		public virtual PPickPath Pick(float x, float y, float halo) {
			RectangleF b = PUtil.InflatedRectangle(new PointF(x, y), halo, halo);
			PPickPath result = new PPickPath(this, b);

			FullPick(result);
			
			// make sure this camera is pushed.
			if (result.NodeStackReference.Count == 0) {
				result.PushNode(this);
				result.PushMatrix(MatrixReference);
			}

			return result;
		}

		/// <summary>
		/// Overridden.  After the direct children of the camera have been given a chance
		/// to be picked objects viewed by the camera are given a chance to be picked.
		/// </summary>
		/// <param name="pickPath">The pick path used for the pick operation.</param>
		/// <returns>
		/// True if an object viewed by the camera was picked; else false.
		/// </returns>
		protected override bool PickAfterChildren(PPickPath pickPath) {
			if (Intersects(pickPath.PickBounds)) {
				pickPath.PushMatrix(viewMatrix);

				if (PickCameraView(pickPath)) {
					return true;
				}

				pickPath.PopMatrix(viewMatrix);
				return true;
			}
			return false;
		}
	
		/// <summary>
		/// Pick all the layers that the camera is looking at.
		/// </summary>
		/// <param name="pickPath">The pick path to use for the pick operation.</param>
		/// <returns>
		/// True if an object viewed by the camera was picked; else false.
		/// </returns>
		/// <remarks>
		/// This method is only called when the camera's view matrix and clip are
		/// applied to the pickPath.
		/// </remarks>
		protected virtual bool PickCameraView(PPickPath pickPath) {
			int count = LayerCount;
			for (int i = count - 1; i >= 0; i--) {
				PLayer each = layers[i];
				if (each.FullPick(pickPath))
					return true;
			}
			return false;
		}
		#endregion

		#region View Matrix
		//****************************************************************
		// View Matrix - Methods for accessing the view matrix. The view
		// matrix is applied before painting and picking the camera's
		// layers, but not before painting or picking its direct children.
		// 
		// Changing the view matrix is how zooming and panning are
		// accomplished.
		//****************************************************************

		/// <summary>
		/// Occurs when the value of the ViewMatrix property changes.
		/// </summary>
		/// <remarks>
		/// When a user attaches an event handler to the ViewTransformChanged Event as in
		/// ViewTransformChanged += new PPropertyEventHandler(aHandler),
		/// the add method adds the handler to the delegate for the event
		/// (keyed by viewTransformEventKey in the Events list).
		/// When a user removes an event handler from the ViewTransformChanged event as in 
		/// ViewTransformChanged -= new PPropertyEventHandler(aHandler),
		/// the remove method removes the handler from the delegate for the event
		/// (keyed by viewTransformEventKey in the Events list).
		/// </remarks>
		public virtual event PPropertyEventHandler ViewTransformChanged {
			add { HandlerList.AddHandler(viewTransformEventKey, value); }
			remove { HandlerList.RemoveHandler(viewTransformEventKey, value); }
		}

		/// <summary>
		/// Return the bounds of this camera in the view coordinate system.
		/// </summary>
		/// <value>The camera's bounds in view coordinates.</value>
		public virtual RectangleF ViewBounds {
			get { return LocalToView(Bounds); }
		}

		/// <summary>
		/// Gets or sets the scale applied by the view transform to the layers
		/// viewed by this camera.
		/// </summary>
		public virtual float ViewScale {
			get { return viewMatrix.Scale; }
			set { ScaleViewBy(value / ViewScale); }
		}

		/// <summary>
		/// Scale the view transform that is applied to the layers viewed by this camera
		/// by the given amount.
		/// </summary>
		/// <param name="scale">The amount to scale the view by.</param>
		public virtual void ScaleViewBy(float scale) {
			ScaleViewBy(scale, 0, 0);
		}
		
		/// <summary>
		/// Scale the view transform that is applied to the layers viewed by this camera
		/// by the given amount about the given point.
		/// </summary>
		/// <param name="scale">The amount ot scale the view by.</param>
		/// <param name="x">The x coordinate of the point to scale about.</param>
		/// <param name="y">The y coordinate of the point to scale about.</param>
		public virtual void ScaleViewBy(float scale, float x, float y) {
			viewMatrix.ScaleBy(scale, x, y);
			ApplyViewConstraints();
			InvalidatePaint();
			FirePropertyChangedEvent(viewTransformEventKey, null, viewMatrix);
		}

		/// <summary>
		/// Translate the view matrix that is applied to the camera's layers.
		/// </summary>
		/// <param name="dx">The amount to translate in the x direction.</param>
		/// <param name="dy">The amount to translate in the y direction.</param>
		public virtual void TranslateViewBy(float dx, float dy) {
			viewMatrix.TranslateBy(dx, dy);
			ApplyViewConstraints();
			InvalidatePaint();
			FirePropertyChangedEvent(viewTransformEventKey, null, viewMatrix);
		}
		
		/// <summary>
		/// Sets the offset of the view matrix that is applied to the camera's layers.
		/// </summary>
		/// <value>The offset to apply to the camera's layers.</value>
		public virtual PointF ViewOffset {
			set { SetViewOffset(value.X, value.Y); }
		}

		/// <summary>
		/// Sets the offset of the view matrix that is applied to the camera's layers.
		/// </summary>
		/// <param name="x">The x coordinate of the offset to apply to the camera's layers.</param>
		/// <param name="y">The y coordinate of the offset to apply to the camera's layers.</param>
		public virtual void SetViewOffset(float x, float y) {
			viewMatrix.OffsetX = x;
			viewMatrix.OffsetY = y;
			ApplyViewConstraints();
			InvalidatePaint();
			FirePropertyChangedEvent(viewTransformEventKey, null, viewMatrix);
		}

		/// <summary>
		/// Gets or sets the view matrix that is applied to the camera's layers.
		/// </summary>
		/// <value>The view matrix that is applied to the camera's layers.</value>
		/// <remarks>This property returns a copy of the view matrix.</remarks>
		public virtual PMatrix ViewMatrix {
			get { return (PMatrix) viewMatrix.Clone(); }
			set {
				viewMatrix.Reset();
				viewMatrix.Multiply(value);
				ApplyViewConstraints();
				InvalidatePaint();
				FirePropertyChangedEvent(viewTransformEventKey, null, viewMatrix);
			}
		}

		/// <summary>
		/// Gets a reference to the view matrix that is applied to the camera's layers.
		/// </summary>
		/// <value>A reference to the view matrix that is applied to the camera's layers.</value>
		public virtual PMatrix ViewMatrixReference {
			get { return viewMatrix; }
		}
		#endregion

		#region View Matrix Constraints
		//****************************************************************
		// View Matrix Constraints - Methods for setting and applying
		// constraints to the view matrix.
		//****************************************************************

		/// <summary>
		/// Gets or sets a constraint that will be applied to the camera's view matrix.
		/// </summary>
		/// <value>A constraint to apply to the camera's view matrix.</value>
		public virtual CameraViewConstraint ViewConstraint {
			get { return viewConstraint; }
			set {
				viewConstraint = value;
				ApplyViewConstraints();
			}
		}

		/// <summary>
		/// Applies a previously set constraint to the camera's view matrix.
		/// </summary>
		public virtual void ApplyViewConstraints() {
			if (viewConstraint == CameraViewConstraint.None)
				return;

			RectangleF layerBounds = GlobalToLocal(UnionOfLayerFullBounds);
			SizeF constraintDelta = new SizeF(0, 0);

			switch (viewConstraint) {
				case CameraViewConstraint.All:
					constraintDelta = PUtil.DeltaRequiredToContain(ViewBounds, layerBounds);
					break;
				case CameraViewConstraint.Center:
					PointF location = PUtil.CenterOfRectangle(layerBounds);
					//layerBounds.Location = PUtil.CenterOfRectangle(layerBounds);
					layerBounds.X = location.X;
					layerBounds.Y = location.Y;
					layerBounds.Width = 0;
					layerBounds.Height = 0;
					constraintDelta = PUtil.DeltaRequiredToContain(ViewBounds, layerBounds);
					break;
			}

			viewMatrix.TranslateBy(-constraintDelta.Width, -constraintDelta.Height);
		}
		#endregion

		#region Camera View Coordinate Systems
		//****************************************************************
		// Camera View Coordinate System Conversions - Methods to
		// translate from the camera's local coordinate system (above the
		// camera's view matrix) to the camera view coordinate system
		// (below the camera's view matrix). When converting geometry from
		// one of the canvas�s layers you must go through the view matrix.
		//****************************************************************

		/// <summary>
		/// Transform the point from the camera's view coordinate system to the camera's
		/// local coordinate system.
		/// </summary>
		/// <param name="point">
		/// The point in the camera's view coordinate system to be transformed.
		/// </param>
		/// <returns>The point in the camera's local coordinate system.</returns>
		public virtual PointF ViewToLocal(PointF point) {
			return viewMatrix.Transform(point);
		}
		
		/// <summary>
		/// Transform the size from the camera's view coordinate system to the camera's
		/// local coordinate system.
		/// </summary>
		/// <param name="size">
		/// The size in the camera's view coordinate system to be transformed.
		/// </param>
		/// <returns>The size in the camera's local coordinate system.</returns>
		public virtual SizeF ViewToLocal(SizeF size) {
			return viewMatrix.Transform(size);
		}
		
		/// <summary>
		/// Transform the rectangle from the camera's view coordinate system to the
		/// camera's local coordinate system.
		/// </summary>
		/// <param name="rectangle">
		/// The rectangle in the camera's view coordinate system to be transformed.
		/// </param>
		/// <returns>The rectangle in the camera's local coordinate system.</returns>
		public virtual RectangleF ViewToLocal(RectangleF rectangle) {
			return viewMatrix.Transform(rectangle);
		}
		
		/// <summary>
		/// Transform the point from the camera's local coordinate system to the camera's
		/// view coordinate system. 
		/// </summary>
		/// <param name="point">
		/// The point in the camera's local coordinate system to be transformed.
		/// </param>
		/// <returns>The point in the camera's view coordinate system.</returns>
		public virtual PointF LocalToView(PointF point) {
			return viewMatrix.InverseTransform(point);
		}

		/// <summary>
		/// Transform the size from the camera's local coordinate system to the camera's
		/// view coordinate system. 
		/// </summary>
		/// <param name="size">
		/// The size in the camera's local coordinate system to be transformed.
		/// </param>
		/// <returns>The size in the camera's view coordinate system.</returns>
		public virtual SizeF LocalToView(SizeF size) {
			return viewMatrix.InverseTransform(size);
		}
		
		/// <summary>
		/// Transform the rectangle from the camera's local coordinate system to the camera's
		/// view coordinate system. 
		/// </summary>
		/// <param name="rectangle">
		/// The rectangle in the camera's local coordinate system to be transformed.
		/// </param>
		/// <returns>The rectangle in the camera's view coordinate system.</returns>
		public virtual RectangleF LocalToView(RectangleF rectangle) {
			return viewMatrix.InverseTransform(rectangle);
		}
		#endregion

		#region Animation
		//****************************************************************
		// Animation - Methods to animate the camera's view.
		//****************************************************************

		/// <summary>
		/// Animate the camera's view from its current matrix when the activity starts
		/// to a new matrix that centers the given bounds in the camera layers' coordinate
		/// system into the camera's view bounds.
		/// </summary>
		/// <param name="centerBounds">The bounds to center the view on.</param>
		/// <param name="shouldScaleToFit">
		/// Indicates whether the camera should scale it's view when necessary to fully fit
		/// the given bounds within the camera's view bounds.
		/// </param>
		/// <param name="duration">The amount of time that the animation should take.</param>
		/// <returns>
		/// The newly scheduled activity, if the duration is greater than 0; else null.
		/// </returns>
		/// <remarks>
		/// If the duration is 0 then the view will be transformed immediately, and null will
		/// be returned.  Else a new PTransformActivity will get returned that is set to
		/// animate the camera�s view matrix to the new bounds. If shouldScaleToFit is true,
		/// then the camera will also scale its view so that the given bounds fit fully within
		/// the camera's view bounds, else the camera will maintain its original scale.
		/// </remarks>
		public virtual PTransformActivity AnimateViewToCenterBounds(RectangleF centerBounds, bool shouldScaleToFit, long duration) {
			SizeF delta = PUtil.DeltaRequiredToCenter(ViewBounds, centerBounds);
			PMatrix newMatrix = ViewMatrix;
			newMatrix.TranslateBy(delta.Width, delta.Height);
		
			if (shouldScaleToFit) {
				float s = Math.Min(ViewBounds.Width / centerBounds.Width, ViewBounds.Height / centerBounds.Height);
				PointF c = PUtil.CenterOfRectangle(centerBounds);
				newMatrix.ScaleBy(s, c.X, c.Y);
			}

			return AnimateViewToMatrix(newMatrix, duration);
		}

		/// <summary>
		/// Pan the camera's view from its current matrix when the activity starts to a new
		/// matrix so that the view bounds will contain (if possible, intersect if not
		/// possible) the new bounds in the camera layers' coordinate system. 
		/// </summary>
		/// <param name="panToBounds"></param>
		/// <param name="duration">The amount of time that the animation should take.</param>
		/// <returns>
		/// The newly scheduled activity, if the duration is greater than 0; else null.
		/// </returns>
		/// <remarks>
		/// If the duration is 0 then the view will be transformed immediately, and null will
		/// be returned. Else a new PTransformActivity will get returned that is set to
		/// animate the camera�s view matrix to the new bounds.
		/// </remarks>
		public virtual PTransformActivity AnimateViewToPanToBounds(RectangleF panToBounds, long duration) {
			SizeF delta = PUtil.DeltaRequiredToContain(ViewBounds, panToBounds);
		
			if (delta.Width != 0 || delta.Height != 0) {
				if (duration == 0) {
					TranslateViewBy(-delta.Width, -delta.Height);
				} else {
					PMatrix m = ViewMatrix;
					m.TranslateBy(-delta.Width, -delta.Height);
					return AnimateViewToMatrix(m, duration);
				}
			}

			return null;
		}

		/// <summary>
		/// Animate the camera's view matrix from its current value when the activity starts
		/// to the new destination matrix value.
		/// </summary>
		/// <param name="destination">The final matrix value.</param>
		/// <param name="duration">The amount of time that the animation should take.</param>
		/// <returns>
		/// The newly scheduled activity, if the duration is greater than 0; else null.
		/// </returns>
		public virtual PTransformActivity AnimateViewToMatrix(PMatrix destination, long duration) {
			if (duration == 0) {
				ViewMatrix = destination;
				return null;
			}

			PTransformActivity ta = new PTransformActivity(duration, PUtil.DEFAULT_ACTIVITY_STEP_RATE, new PCameraTransformTarget(this), destination);
			
			PRoot r = Root;
			if (r != null) {
				r.AddActivity(ta);
			}
			
			return ta;
		}

		/// <summary>
		/// A target for a transform activity that gets and sets the matrix of the specified
		/// PCamera.
		/// </summary>
		public class PCameraTransformTarget : PTransformActivity.Target {
			private PCamera target;

			public PCameraTransformTarget(PCamera target) {
				this.target = target;
			}
			public PMatrix Matrix {
				get { return target.ViewMatrixReference; }
				set { target.ViewMatrix = value; }
			}
		}
		#endregion

		#region Serialization
		//****************************************************************
		// Serialization - Cameras conditionally serialize their layers.
		// This means that only the layer references that were unconditionally
		// (using GetObjectData) serialized by someone else will be restored
		// when the camera is deserialized.
		//****************************************************************/
		/*
		/// <summary>
		/// Read this this camera and all its children from the given SerializationInfo.
		/// </summary>
		/// <param name="info">The SerializationInfo to read from.</param>
		/// <param name="context">
		/// The StreamingContext of this serialization operation.
		/// </param>
		/// <remarks>
		/// This constructor is required for Deserialization.
		/// </remarks>
		protected PCamera(SerializationInfo info, StreamingContext context)
			: base(info, context) {

			layers = new PLayerList();

			int count = info.GetInt32("layerCount");
			for (int i = 0; i < count; i++) {
				PLayer layer = (PLayer)info.GetValue("layer" + i, typeof(PLayer));
				if (layer != null) {
					layers.Add(layer);
				}
			}
			canvas = (PCanvas)info.GetValue("canvas", typeof(PCanvas));
		}

		/// <summary>
		/// Overridden.  Write this camera and all its children to the given
		/// SerializationInfo.
		/// </summary>
		/// <param name="info">The SerializationInfo to write to.</param>
		/// <param name="context">
		/// The streaming context of this serialization operation.
		/// </param>
		/// <remarks>
		/// Note that the camera's layers are written conditionally, so they will only
		/// get written out if someone else writes them unconditionally.
		/// </remarks>
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData (info, context);

			int count = LayerCount;
			for (int i = 0; i < count; i++) {
				PStream.WriteConditionalObject(info, "layer"+i, layers[i]);
			}
		
			info.AddValue("layerCount", count);
			PStream.WriteConditionalObject(info, "canvas", canvas);
		}
		*/
		#endregion
	}
}