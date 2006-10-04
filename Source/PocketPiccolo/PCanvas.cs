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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Runtime.Serialization;

using UMD.HCIL.PocketPiccolo.Util;
using UMD.HCIL.PocketPiccolo.Events;

namespace UMD.HCIL.PocketPiccolo {

	/// <summary>
	/// <b>PCanvas</b> is a simple C# Control that can be used to embed Piccolo into a
	/// C# application.
	/// </summary>
	/// <remarks>
	/// Canvases view the Piccolo scene graph through a camera.  The canvas manages
	/// screen updates coming from this camera, and forwards mouse and keyboard events
	/// to the camera.
	/// </remarks>
	//[Serializable]
	public class PCanvas : System.Windows.Forms.Control {

		#region Fields
		/// <summary>
		/// The current canvas.
		/// </summary>
		public static PCanvas CURRENT_PCANVAS = null;

		private PCamera camera;
		private Stack cursorStack;
		private int interacting;
		private RenderQuality defaultRenderQuality;
		private RenderQuality animatingRenderQuality;
		private RenderQuality interactingRenderQuality;
		private PPanEventHandler panEventHandler;
		private PZoomEventHandler zoomEventHandler;
		private bool paintingImmediately;
		private bool animatingOnLastPaint;

		//For Manual DoubleBuffering
		Bitmap offscreenBitmap = null;
		Graphics offscreenGraphics = null;
		Graphics realGraphics = null;

		/// <summary>
		/// Occurs when the interacting state of a canvas changes.
		/// </summary>
		/// <remarks>
		/// When a canvas is interacting, the canvas will render at lower quality that is
		/// faster.
		/// </remarks>
		public event PPropertyEventHandler InteractingChanged;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		#region Constructors
		/// <summary>
		/// Construct a canvas with the basic scene graph consisting of a root, camera,
		/// and layer. Event handlers for zooming and panning are automatically
		/// installed.
		/// </summary>
		public PCanvas() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			CURRENT_PCANVAS = this;
			cursorStack = new Stack();
			Camera = PUtil.CreateBasicScenegraph();
			DefaultRenderQuality = RenderQuality.HighQuality;
			AnimatingRenderQuality = RenderQuality.LowQuality;
			InteractingRenderQuality = RenderQuality.LowQuality;
			//PanEventHandler = new PPanEventHandler();
			ZoomEventHandler = new PZoomEventHandler();
			//BackColor = Color.White;
			//AllowDrop = true;
	
			//SetStyle(ControlStyles.DoubleBuffer, true);
			//SetStyle(ControlStyles.Selectable, true);
			//SetStyle(ControlStyles.UserPaint, true);
			//SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			realGraphics = CreateGraphics();
			offscreenBitmap = new Bitmap(ClientSize.Width, ClientSize.Height); // 240, 294);
			offscreenGraphics = Graphics.FromImage(offscreenBitmap);
		}
		#endregion

		#region Basics
		//****************************************************************
		// Basics - Methods for accessing common piccolo nodes.
		//****************************************************************

		/// <summary>
		/// Gets or sets the camera associated with this canvas.
		/// </summary>
		/// <value>The camera associated with this canvas.</value>
		/// <remarks>
		/// All input events from this canvas go through this camera. And this is the
		/// camera that paints this canvas.
		/// </remarks>
		//[Category("Scene Graph")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual PCamera Camera {
			get { return camera; }
			set {
				if (camera != null) { camera.Canvas = null; }
				camera = value;
				if (camera != null) {
					camera.Canvas = this;
					camera.Bounds = Bounds;
				}
			}
		}

		/// <summary>
		/// Gets the root of the scene graph viewed by the camera.
		/// </summary>
		/// <value>The root for this canvas.</value>
		//[Category("Scene Graph")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual PRoot Root {
			get { return camera.Root; }
		}

		/// <summary>
		/// Gets the main layer of the scene graph viewed by the camera.
		/// </summary>
		/// <value>The layer for this canvas.</value>
		//[Category("Scene Graph")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PLayer Layer {
			get { return camera.GetLayer(0); }
		}

		/// <summary>
		/// Gets or sets the pan event handler associated with this canvas.
		/// </summary>
		/// <value>The pan event handler for this canvas.</value>
		/// <remarks>
		/// This event handler is set up to get events from the camera associated
		/// with this canvas by default.
		/// </remarks>
		//[Category("PInputEvent Handlers")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual PPanEventHandler PanEventHandler {
			get { return panEventHandler; }
			set {
				if (panEventHandler != null) {
					RemoveInputEventListener(panEventHandler);
				}
		
				panEventHandler = value;
		
				if (panEventHandler != null) {
					AddInputEventListener(panEventHandler);
				}
			}
		}

		/// <summary>
		/// Gets or sets the zoom event handler associated with this canvas.
		/// </summary>
		/// <value>The zoom event handler for this canvas.</value>
		/// <remarks>
		/// This event handler is set up to get events from the camera associated
		/// with this canvas by default.
		/// </remarks>
		//[Category("PInputEvent Handlers")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual PZoomEventHandler ZoomEventHandler {
			get { return zoomEventHandler; }
			set {
				if(zoomEventHandler != null) {
					RemoveInputEventListener(zoomEventHandler);
				}
		
				zoomEventHandler = value;
		
				if(zoomEventHandler != null) {
					AddInputEventListener(zoomEventHandler);
				}
			}
		}

		/// <summary>
		/// Add an input listener to the camera associated with this canvas.
		/// </summary>
		/// <param name="listener">The listener to add.</param>
		public virtual void AddInputEventListener(PInputEventListener listener) {
			Camera.AddInputEventListener(listener);
		}
	
		/// <summary>
		/// Remove an input listener to the camera associated with this canvas.
		/// </summary>
		/// <param name="listener">The listener to remove.</param>
		public virtual void RemoveInputEventListener(PInputEventListener listener) {
			Camera.RemoveInputEventListener(listener);
		}
		#endregion

		#region Painting
		//****************************************************************
		// Painting - Methods for painting the camera and it's view on the
		// canvas.
		//****************************************************************

		/// <summary>
		/// Gets or sets a value indicating if this canvas is interacting.
		/// </summary>
		/// <value>True if the canvas is interacting; otherwise, false.</value>
		/// <remarks>
		/// If this property is true, the canvas will normally render at a lower
		/// quality that is faster.
		/// </remarks>
		//[Category("Appearance")]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool Interacting {
			get { return interacting > 0; }
			set {
				bool wasInteracting = Interacting;

				if (value) {
					interacting++;
				} else {
					interacting--;
				}

				if (!Interacting) { // determine next render quality and repaint if it's greater then the old
					                // interacting render quality.
					RenderQuality nextRenderQuality = defaultRenderQuality;
					if (Animating) nextRenderQuality = animatingRenderQuality;
					if (nextRenderQuality > interactingRenderQuality) {
						Invalidate();
					}
				}

				// If interacting changed, fire the appropriate event.
				bool isInteracting = Interacting;
				if (wasInteracting != isInteracting) {
					OnInteractingChanged(new PPropertyEventArgs(wasInteracting, isInteracting));
				}
			}
		}

		/// <summary>
		/// Raises the InteractingChanged event by invoking the delegates.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		/// <remarks>
		/// This event is raised when the interacting state of the canvas changes.
		/// </remarks>
		protected virtual void OnInteractingChanged(PPropertyEventArgs e) {
			if (InteractingChanged != null) {
				//Invokes the delegates.
				InteractingChanged(this, e); 
			}
		}

		/// <summary>
		/// Gets a value indicating if this canvas is animating.
		/// </summary>
		/// <value>True if the canvas is animating; otherwise, false.</value>
		/// <remarks>
		/// Returns true if any activities that respond with true to the method isAnimating
		/// were run in the last PRoot.ProcessInputs() loop. 
		/// </remarks>
		//[Category("Appearance")]
		public virtual bool Animating {
			get {
				return Root.ActivityScheduler.Animating;
			}
		}

		/// <summary>
		/// Sets the render quality that should be used for rendering this canvas when it
		/// is not interacting or animating.
		/// </summary>
		/// <value>The default render quality for this canvas.</value>
		/// <remarks>The default value is <c>RenderQuality.HighQuality</c>.</remarks>
		public virtual RenderQuality DefaultRenderQuality {
			set {
				defaultRenderQuality = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Sets the render quality that should be used for rendering this canvas when it
		/// is animating.
		/// </summary>
		/// <value>The animating render quality for this canvas.</value>
		/// <remarks>The default value is <c>RenderQuality.LowQuality</c>.</remarks>
		public virtual RenderQuality AnimatingRenderQuality {
			set {
				animatingRenderQuality = value;
				if (Animating) Invalidate();
			}
		}

		/// <summary>
		/// Set the render quality that should be used for rendering this canvas when it
		/// is interacting.
		/// </summary>
		/// <value>The interacting render quality for this canvas.</value>
		/// <remarks>The default value is <c>RenderQuality.LowQuality</c>.</remarks>
		public virtual RenderQuality InteractingRenderQuality {
			set {
				interactingRenderQuality = value;
				if (Interacting) Invalidate();
			}
		}

		/*
		/// <summary>
		/// Set the canvas cursor, and remember the previous cursor on the cursor stack.
		/// </summary>
		/// <param name="cursor">The new canvas cursor.</param>
		public virtual void PushCursor(Cursor cursor) {
			cursorStack.Push(Cursor);
			Cursor = cursor;
		}
	
		/// <summary>
		/// Pop the cursor on top of the cursorStack and set it as the canvas cursor.
		/// </summary>
		public virtual void PopCursor() {
			Cursor = ((Cursor)cursorStack.Pop());
		}
		*/
		#endregion

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			// 
			// PCanvas
			// 
			this.Text = "8";

		}
		#endregion

 		#region Windows Forms Connection
		//****************************************************************
		// Windows Forms Connection - Code to manage connection to windows
		// events.
		//****************************************************************
		protected override void OnResize(EventArgs e) {
			base.OnResize (e);
			offscreenBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
			offscreenGraphics = Graphics.FromImage(offscreenBitmap);
			camera.Bounds = new RectangleF(camera.X, camera.Y, ClientSize.Width, ClientSize.Height);
		}

		/// <summary>
		/// Overridden.  See <see cref="Control.OnSizeChanged">Control.OnSizeChanged</see>.
		/// </summary>
		//protected override void OnSizeChanged(EventArgs e) {
		//	base.OnSizeChanged (e);
		//	camera.Bounds = new RectangleF(camera.X, camera.Y, Bounds.Width, Bounds.Height);
		//}

		/// <summary>
		/// Overridden.  Forwards the KeyDown event to the default input manager.
		/// </summary>
		/// <param name="e">A KeyEventArgs that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.KeyDown, Camera, null);
		}

		/// <summary>
		/// Overridden.  Forwards the KeyPress event to the default input manager.
		/// </summary>
		/// <param name="e">A KeyPressEventArgs that contains the event data.</param>
		protected override void OnKeyPress(KeyPressEventArgs e) {
			base.OnKeyPress (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.KeyPress, Camera, null);
		}

		/*
		/// <summary>
		/// Overridden.  Determines whether the specified key is a regular input key or a
		/// special key that requires preprocessing.
		/// </summary>
		/// <param name="keyData">One of the Keys values.</param>
		/// <returns>True if the specified key is a regular input key; otherwise, false.</returns>
		/// <remarks>
		/// This method is overridden so that events from the arrow keys will be sent to the
		/// control, rather than pre-processed.
		/// </remarks>
		protected override bool IsInputKey(Keys keyData) {
			bool ret = true;

			switch (keyData) {
				case Keys.Left:
					break;
				case Keys.Right:
					break;
				case Keys.Up:
					break;
				case Keys.Down:
					break;
				default:
					ret = base.IsInputKey(keyData);
					break;
			}
			return ret;
		}
		*/

		/// <summary>
		/// Overridden.  Forwards the KeyUp event to the default input manager.
		/// </summary>
		/// <param name="e">A KeyEventArgs that contains the event data.</param>
		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.KeyUp, Camera, null);
		}

		/// <summary>
		/// Overridden.  Forwards the Click event to the default input manager.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnClick(EventArgs e) {
			base.OnClick (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.Click, Camera, null);
		}

		/*
		/// <summary>
		/// Overridden.  Forwards the DoubleClick event to the default input manager.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnDoubleClick(EventArgs e) {
			base.OnDoubleClick (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.DoubleClick, Camera, null);
		}
		*/

		/// <summary>
		/// Overridden.  Forwards the MouseDown event to the default input manager.
		/// </summary>
		/// <param name="e">A MouseEventArgs that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.MouseDown, Camera, null);
		}

		/// <summary>
		/// Overridden.  Forwards the MouseMove event to the default input manager.
		/// </summary>
		/// <param name="e">A MouseEventArgs that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove (e);

			Point prevPos = new Point((int)Root.DefaultInputManager.CurrentCanvasPosition.X,
				(int)Root.DefaultInputManager.CurrentCanvasPosition.Y);
			Point currPos = new Point(e.X, e.Y);

			// This condition is here because of a .NET bug that sometimes MouseMove events are generated
			// when the mouse has not actually moved (for example with context menus).
			if (prevPos != currPos) {
			//	if (e.Button == MouseButtons.None) {
			//		Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.MouseMove, Camera, null);
			//	} else {
					Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.MouseDrag, Camera, null);
			//	}
			}
		}

		/// <summary>
		/// Overridden.  Forwards the MouseUp event to the default input manager.
		/// </summary>
		/// <param name="e">A MouseEventArgs that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.MouseUp, Camera, null);
		}

		/*
		/// <summary>
		/// Overridden.  Forwards the MouseEnter event to the default input manager.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnMouseEnter(EventArgs e) {
			base.OnMouseEnter (e);
			SimulateMouseMoveOrDrag();
		}

		/// <summary>
		/// Overridden.  Forwards the MouseLeave event to the default input manager.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e) {
			SimulateMouseMoveOrDrag();
			base.OnMouseLeave (e);
		}
		*/

		/// <summary>
		/// Simulates a mouse move or drag event.
		/// </summary>
		/// <remarks>
		/// This method simulates a mouse move or drag event, which is sometimes necessary
		/// to ensure that the appropriate piccolo mouse enter and leave events are fired.
		/// </remarks>
		protected virtual void SimulateMouseMoveOrDrag() {
			MouseEventArgs simulated = null;

			Point currPos = PointToClient(Control.MousePosition);
			simulated = new MouseEventArgs(MouseButtons, 0, currPos.X, currPos.Y, 0);

			if (Control.MouseButtons != MouseButtons.None) {
				Root.DefaultInputManager.ProcessEventFromCamera(simulated, PInputType.MouseDrag, Camera, null);
			}
			else {
				Root.DefaultInputManager.ProcessEventFromCamera(simulated, PInputType.MouseMove, Camera, null);
			}
		}

		/*
		/// <summary>
		/// Overridden.  Forwards the MouseWheel event to the default input manager.
		/// </summary>
		/// <param name="e">A MouseEventArgs that contains the event data.</param>
		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel (e);
			Root.DefaultInputManager.ProcessEventFromCamera(e, PInputType.MouseWheel, Camera, null);
		}

		/// <summary>
		/// Overridden.  Forwards the DragDrop event to the default input manager.
		/// </summary>
		/// <param name="drgevent">A DragEventArgs that contains the event data.</param>
		protected override void OnDragDrop(DragEventArgs drgevent) {
			base.OnDragDrop (drgevent);
			Root.DefaultInputManager.ProcessEventFromCamera(drgevent, PInputType.DragDrop, Camera, this);
		}

		/// <summary>
		/// Overridden.  Forwards the DragOver event to the default input manager.
		/// </summary>
		/// <param name="drgevent">A DragEventArgs that contains the event data.</param>
		protected override void OnDragOver(DragEventArgs drgevent) {
			base.OnDragOver (drgevent);
			Root.DefaultInputManager.ProcessEventFromCamera(drgevent, PInputType.DragOver, Camera, this);
		}

		/// <summary>
		/// Overridden.  Forwards the DragEnter event to the default input manager.
		/// </summary>
		/// <param name="drgevent">A DragEventArgs that contains the event data.</param>
		protected override void OnDragEnter(DragEventArgs drgevent) {
			base.OnDragEnter (drgevent);

		}

		/// <summary>
		/// Overridden.  Forwards the DragLeave event to the default input manager.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnDragLeave(EventArgs e) {
			base.OnDragLeave (e);
		}
		*/

		// Necessary to avoid having control first cleared to background color
		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		/// <summary>
		/// Invalidates the specified region of the canvas (adds it to the canvas's update region,
		/// which is the area that will be repainted at the next paint operation), and causes a paint
		/// message to be sent to the canvas.
		/// </summary>
		/// <param name="bounds">A rectangle object that represents the region to invalidate.</param>
		public void InvalidateBounds(RectangleF bounds) {
			PDebug.ProcessInvalidate();

			Rectangle insetRect = new Rectangle((int)Math.Floor(bounds.X)-1, (int)Math.Floor(bounds.Y)-1,
				(int)Math.Ceiling(bounds.Width)+2, (int)Math.Ceiling(bounds.Height)+2);

			Invalidate(insetRect);
		}

		/// <summary>
		/// Overridden.  See <see cref="Control.OnPaint">Control.OnPaint</see>.
		/// </summary>
		protected override void OnPaint(PaintEventArgs pe) {
			PDebug.StartProcessingOutput();

			Graphics2D g = new Graphics2D(offscreenGraphics);
			g.Clip = new Region(pe.ClipRectangle);
			//Graphics2D g = new Graphics2D(pe.Graphics);

			// create new paint context and set render quality to lowest common 
			// denominator render quality.
			//Rectangle clipRect = pe.ClipRectangle;
			//RectangleF fClipRect = new RectangleF(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
			PPaintContext paintContext = new PPaintContext(g, this);
			if (Interacting || Animating) {
				if (interactingRenderQuality < animatingRenderQuality) {
					paintContext.RenderQuality = interactingRenderQuality;
				} else {
					paintContext.RenderQuality = animatingRenderQuality;
				}
			} else {
				paintContext.RenderQuality = defaultRenderQuality;
			}

			// paint 
			camera.FullPaint(paintContext);

			// if switched state from animating to not animating invalidate the entire
			// screen so that it will be drawn with the default instead of animating 
			// render quality.
			if (!Animating && animatingOnLastPaint) {
				Invalidate();
			}
			animatingOnLastPaint = Animating;

			// Calling the base class OnPaint
			base.OnPaint(pe);
			realGraphics.DrawImage(this.offscreenBitmap, 0, 0);

			PDebug.EndProcessingOutput(g);
		}

		/// <summary>
		/// Causes the canvas to immediately paint it's invalidated regions.
		/// </summary>
		public virtual void PaintImmediately() {
			if (paintingImmediately) {
				return;
			}

			paintingImmediately = true;
			Update();
			paintingImmediately = false;
		}
		#endregion
	}
}
