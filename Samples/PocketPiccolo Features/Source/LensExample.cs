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
using System.ComponentModel;
using System.Windows.Forms;

using UMD.HCIL.PocketPiccolo;
using UMD.HCIL.PocketPiccolo.Events;
using UMD.HCIL.PocketPiccolo.Nodes;
using UMD.HCIL.PocketPiccolo.Util;
using UMD.HCIL.PocketPiccoloX;
using UMD.HCIL.PocketPiccoloX.Handles;
using UMD.HCIL.PocketPiccoloX.Nodes;

namespace PocketPiccoloFeatures {
	/// <summary>
	/// Summary description for LensExample.
	/// </summary>
	public class LensExample : PForm {
		public LensExample() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//Turn of smart minimize.
			this.MinimizeBox = false;
		}

		public override void Initialize() {
			PRoot root = Canvas.Root;
			PCamera camera = Canvas.Camera;		
			PLayer mainLayer = Canvas.Layer;	// viewed by the PCanvas camera, the lens is added to this layer.
			PLayer sharedLayer = new PLayer();			// viewed by both the lens camera and the PCanvas camera
			PLayer lensOnlyLayer = new PLayer();	// viewed by only the lens camera
		
			root.AddChild(lensOnlyLayer);
			root.AddChild(sharedLayer);
			camera.AddLayer(0, sharedLayer);
		
			PLens lens = new PLens();
			lens.SetBounds(10, 10, 80, 110);
			lens.AddLayer(0, lensOnlyLayer);		
			lens.AddLayer(1, sharedLayer);
			mainLayer.AddChild(lens);
			PBoundsHandle.AddBoundsHandlesTo(lens);

			// Create an event handler that draws squiggles on the first layer of the bottom
			// most camera.
			PDragSequenceEventHandler squiggleEventHandler = new SquiggleEventHandler();

			// add the squiggle event handler to both the lens and the
			// canvas camera.
			lens.Camera.AddInputEventListener(squiggleEventHandler);
			camera.AddInputEventListener(squiggleEventHandler);

			// remove default event handlers, not really nessessary since the squiggleEventHandler
			// consumes everything anyway, but still good to do.
			//Canvas.RemoveInputEventListener(Canvas.PanEventHandler);
			Canvas.RemoveInputEventListener(Canvas.ZoomEventHandler);

			PNode sharedNode = new SharedNode(lens);
			sharedNode.Brush = new SolidBrush(Color.Green); // Brushes.Green;
			sharedNode.SetBounds(0, 0, 100, 200);
			sharedNode.TranslateBy(100, 220);
			sharedLayer.AddChild(sharedNode);
		
			PText label = new PText("Move the lens \n (by dragging title bar) over the green rectangle, and it will appear red. press and drag the mouse on the canvas and it will draw squiggles. press and drag the mouse over the lens and drag squiggles that are only visible through the lens.");
			label.Font = new Font("Arial", 10, FontStyle.Regular);
			label.ConstrainWidthToTextWidth = false;
			label.SetBounds(100, 70, 130, 200);
		
			sharedLayer.AddChild(label);				

			base.Initialize ();
		}

		class SharedNode : PNode {
			PLens lens;
			Brush redBrush = new SolidBrush(Color.Red);

			public SharedNode(PLens lens) {
				this.lens = lens;
			}

			protected override void Paint(PPaintContext paintContext) {
				if (paintContext.Camera == lens.Camera) {
					Graphics2D g = paintContext.Graphics;
					g.FillRectangle(redBrush, Bounds);
				} else {
					base.Paint(paintContext);
				}
			}
		}

		class SquiggleEventHandler : PDragSequenceEventHandler {
			protected PPath squiggle;
			private PointF p;

			protected override void OnStartDrag(object sender, PInputEventArgs e) {
				base.OnStartDrag (sender, e);
				p = e.Position;
				squiggle = new PPath();
				
				// Add squiggles to the first layer of the bottom camera. In the case of the
				// lens these squiggles will be added to the layer that is only visible by the lens,
				// In the case of the canvas camera the squiggles will be added to the shared layer
				// viewed by both the canvas camera and the lens.
				e.Camera.GetLayer(0).AddChild(squiggle);
			}

			protected override void OnDrag(object sender, PInputEventArgs e) {
				base.OnDrag (sender, e);
				UpdateSquiggle(e);
			}
		
			protected override void OnEndDrag(object sender, PInputEventArgs e) {
				base.OnEndDrag (sender, e);
				UpdateSquiggle(e);
				squiggle = null;
			}
				
			public void UpdateSquiggle(PInputEventArgs e) {
				PointF p2 = e.Position;
				if (p.X != p2.X || p.Y != p2.Y) {
					squiggle.AddPoint(p2);
					//squiggle.AddLine(p.X, p.Y, p2.X, p2.Y);
				}
				p = p2;
			}

			// make sure that the event handler consumes events so that it doesn't
			// conflic with other event handlers or with itself (since its added to two
			// event sources).
			public override bool DoesAcceptEvent(PInputEventArgs e) {
				if (base.DoesAcceptEvent (e)) {
					e.Handled = true;
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.Text = "LensExample";
		}
		#endregion
	}
}