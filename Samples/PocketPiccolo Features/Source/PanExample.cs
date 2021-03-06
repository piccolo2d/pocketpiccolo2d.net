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
using UMD.HCIL.PocketPiccolo.Events;
using UMD.HCIL.PocketPiccoloX;

namespace PocketPiccoloFeatures {
	/// <summary>
	/// Summary description for PanExample.
	/// </summary>
	public class PanExample : PForm {
		public PanExample() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//Turn of smart minimize.
			this.MinimizeBox = false;
		}

		public override void Initialize() {
			PNode blue = new PNode();
			blue.SetBounds(0, 0, 60, 80);
			blue.Brush = new SolidBrush(Color.Blue);
			Canvas.Layer.AddChild(blue);

			PNode red = new PEllipse();
			red.SetBounds(50, 30, 60, 45);
			red.Brush = new SolidBrush(Color.Red);
			Canvas.Layer.AddChild(red);

			Bitmap bm = new Bitmap(GetType().Module.Assembly.GetManifestResourceStream("PocketPiccoloFeatures.hcil.bmp"));
			PImage image = new PImage(bm);
			image.SetBounds(80, 100, image.Width, image.Height);
			Canvas.Layer.AddChild(image);

			Canvas.ZoomEventHandler = null;
			Canvas.AddInputEventListener(new PPanEventHandler());

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
			this.Text = "PanExample";
		}
		#endregion
	}
}