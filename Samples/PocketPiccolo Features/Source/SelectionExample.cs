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
using UMD.HCIL.PocketPiccolo.Nodes;
using UMD.HCIL.PocketPiccoloX;
using UMD.HCIL.PocketPiccoloX.Events;
using UMD.HCIL.PocketPiccolo.Util;

namespace PocketPiccoloFeatures {
	/// <summary>
	/// Summary description for SelectionExample.
	/// </summary>
	public class SelectionExample : PForm {
		public SelectionExample() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//Turn of smart minimize.
			this.MinimizeBox = false;
		}

		public override void Initialize() {
			for (int i=0; i<4; i++) {
				for (int j=0; j<5; j++) {
					PNode rect = PPath.CreateRectangle(i*60, j*60, 50, 50);
					rect.Brush = new SolidBrush(Color.Blue);
					Canvas.Layer.AddChild(rect);
				}
			}

			// Turn off default navigation event handlers
			//Canvas.RemoveInputEventListener(Canvas.PanEventHandler);
			Canvas.RemoveInputEventListener(Canvas.ZoomEventHandler);
		
			// Create a selection event handler
			PSelectionEventHandler selectionEventHandler = new PSelectionEventHandler(Canvas.Layer, Canvas.Layer);
			Canvas.AddInputEventListener(selectionEventHandler);
			Canvas.Root.DefaultInputManager.KeyboardFocus = Canvas.Camera.ToPickPath();
		
			//PNotificationCenter.DefaultCenter.AddListener(this, 
			//	"selectionChanged", 
			//	PSelectionEventHandler.SELECTION_CHANGED_NOTIFICATION, 
			//	selectionEventHandler);

			base.Initialize ();
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
			this.Text = "SelectionExample";
		}
		#endregion
	}
}