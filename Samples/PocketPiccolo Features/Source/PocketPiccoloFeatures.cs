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
using System.Windows.Forms;
using System.Data;

using UMD.HCIL.PocketPiccolo.Util;

namespace PocketPiccoloFeatures
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class PocketPiccoloFeatures : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnClipExample;
		private System.Windows.Forms.CheckBox chkPrintFrameRates;
		private System.Windows.Forms.CheckBox chkShowRegionManagement;
		private System.Windows.Forms.CheckBox chkShowFullBounds;
		private System.Windows.Forms.Button btnPanExample;
		private System.Windows.Forms.Button btnDragExample;
		private System.Windows.Forms.Button btnHelloWorldExample;
		private System.Windows.Forms.Button btnActivityExample;
		private System.Windows.Forms.Button btnPulseExample;
		private System.Windows.Forms.VScrollBar vScrollBar;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Button btnAngleNodeExample;
		private System.Windows.Forms.Button btnCameraExample;
		private System.Windows.Forms.Button btnSquiggleExample;
		private System.Windows.Forms.Button btnNavigationExample;
		private System.Windows.Forms.Button btnSelectionExample;
		private System.Windows.Forms.Button btnLensExample;
		private System.Windows.Forms.Button btnKeyEventFocusExample;
		private System.Windows.Forms.MainMenu mainMenu1;

		public PocketPiccoloFeatures()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			vScrollBar.Maximum = panel.Height - this.ClientRectangle.Height;
			vScrollBar.ValueChanged += new EventHandler(vScrollBar_ValueChanged);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.btnClipExample = new System.Windows.Forms.Button();
			this.chkPrintFrameRates = new System.Windows.Forms.CheckBox();
			this.chkShowRegionManagement = new System.Windows.Forms.CheckBox();
			this.chkShowFullBounds = new System.Windows.Forms.CheckBox();
			this.panel = new System.Windows.Forms.Panel();
			this.btnKeyEventFocusExample = new System.Windows.Forms.Button();
			this.btnLensExample = new System.Windows.Forms.Button();
			this.btnSelectionExample = new System.Windows.Forms.Button();
			this.btnNavigationExample = new System.Windows.Forms.Button();
			this.btnSquiggleExample = new System.Windows.Forms.Button();
			this.btnCameraExample = new System.Windows.Forms.Button();
			this.btnAngleNodeExample = new System.Windows.Forms.Button();
			this.btnPulseExample = new System.Windows.Forms.Button();
			this.btnActivityExample = new System.Windows.Forms.Button();
			this.btnHelloWorldExample = new System.Windows.Forms.Button();
			this.btnDragExample = new System.Windows.Forms.Button();
			this.btnPanExample = new System.Windows.Forms.Button();
			this.vScrollBar = new System.Windows.Forms.VScrollBar();
			// 
			// btnClipExample
			// 
			this.btnClipExample.Location = new System.Drawing.Point(70, 186);
			this.btnClipExample.Size = new System.Drawing.Size(88, 20);
			this.btnClipExample.Text = "ClipExample";
			this.btnClipExample.Click += new System.EventHandler(this.btnClipExample_Click);
			// 
			// chkPrintFrameRates
			// 
			this.chkPrintFrameRates.Location = new System.Drawing.Point(24, 16);
			this.chkPrintFrameRates.Size = new System.Drawing.Size(184, 20);
			this.chkPrintFrameRates.Text = "Print Frame Rates to Console";
			this.chkPrintFrameRates.CheckStateChanged += new System.EventHandler(this.chkPrintFrameRates_CheckStateChanged);
			// 
			// chkShowRegionManagement
			// 
			this.chkShowRegionManagement.Location = new System.Drawing.Point(32, 40);
			this.chkShowRegionManagement.Size = new System.Drawing.Size(168, 20);
			this.chkShowRegionManagement.Text = "Show Region Management";
			this.chkShowRegionManagement.CheckStateChanged += new System.EventHandler(this.chkShowRegionManagement_CheckStateChanged);
			// 
			// chkShowFullBounds
			// 
			this.chkShowFullBounds.Location = new System.Drawing.Point(48, 64);
			this.chkShowFullBounds.Size = new System.Drawing.Size(120, 20);
			this.chkShowFullBounds.Text = "Show Full Bounds";
			this.chkShowFullBounds.CheckStateChanged += new System.EventHandler(this.chkShowFullBounds_CheckStateChanged);
			// 
			// panel
			// 
			this.panel.Controls.Add(this.btnKeyEventFocusExample);
			this.panel.Controls.Add(this.btnLensExample);
			this.panel.Controls.Add(this.btnSelectionExample);
			this.panel.Controls.Add(this.btnNavigationExample);
			this.panel.Controls.Add(this.btnSquiggleExample);
			this.panel.Controls.Add(this.btnCameraExample);
			this.panel.Controls.Add(this.btnAngleNodeExample);
			this.panel.Controls.Add(this.btnPulseExample);
			this.panel.Controls.Add(this.btnActivityExample);
			this.panel.Controls.Add(this.btnHelloWorldExample);
			this.panel.Controls.Add(this.btnDragExample);
			this.panel.Controls.Add(this.btnPanExample);
			this.panel.Controls.Add(this.btnClipExample);
			this.panel.Controls.Add(this.chkShowFullBounds);
			this.panel.Controls.Add(this.chkShowRegionManagement);
			this.panel.Controls.Add(this.chkPrintFrameRates);
			this.panel.Size = new System.Drawing.Size(224, 530);
			// 
			// btnKeyEventFocusExample
			// 
			this.btnKeyEventFocusExample.Location = new System.Drawing.Point(37, 457);
			this.btnKeyEventFocusExample.Size = new System.Drawing.Size(151, 20);
			this.btnKeyEventFocusExample.Text = "KeyEventFocusExample";
			this.btnKeyEventFocusExample.Click += new System.EventHandler(this.btnKeyEventFocusExample_Click);
			// 
			// btnLensExample
			// 
			this.btnLensExample.Location = new System.Drawing.Point(68, 274);
			this.btnLensExample.Size = new System.Drawing.Size(89, 20);
			this.btnLensExample.Text = "LensExample";
			this.btnLensExample.Click += new System.EventHandler(this.btnLensExample_Click);
			// 
			// btnSelectionExample
			// 
			this.btnSelectionExample.Location = new System.Drawing.Point(56, 396);
			this.btnSelectionExample.Size = new System.Drawing.Size(113, 20);
			this.btnSelectionExample.Text = "SelectionExample";
			this.btnSelectionExample.Click += new System.EventHandler(this.btnSelectionExample_Click);
			// 
			// btnNavigationExample
			// 
			this.btnNavigationExample.Location = new System.Drawing.Point(53, 303);
			this.btnNavigationExample.Size = new System.Drawing.Size(122, 20);
			this.btnNavigationExample.Text = "NavigationExample";
			this.btnNavigationExample.Click += new System.EventHandler(this.btnNavigationExample_Click);
			// 
			// btnSquiggleExample
			// 
			this.btnSquiggleExample.Location = new System.Drawing.Point(57, 426);
			this.btnSquiggleExample.Size = new System.Drawing.Size(111, 20);
			this.btnSquiggleExample.Text = "SquiggleExample";
			this.btnSquiggleExample.Click += new System.EventHandler(this.btnSquiggleExample_Click);
			// 
			// btnCameraExample
			// 
			this.btnCameraExample.Location = new System.Drawing.Point(58, 155);
			this.btnCameraExample.Size = new System.Drawing.Size(106, 20);
			this.btnCameraExample.Text = "CameraExample";
			this.btnCameraExample.Click += new System.EventHandler(this.btnCameraExample_Click);
			// 
			// btnAngleNodeExample
			// 
			this.btnAngleNodeExample.Location = new System.Drawing.Point(51, 125);
			this.btnAngleNodeExample.Size = new System.Drawing.Size(125, 20);
			this.btnAngleNodeExample.Text = "AngleNodeExample";
			this.btnAngleNodeExample.Click += new System.EventHandler(this.btnAngleNodeExample_Click);
			// 
			// btnPulseExample
			// 
			this.btnPulseExample.Location = new System.Drawing.Point(64, 366);
			this.btnPulseExample.Size = new System.Drawing.Size(96, 20);
			this.btnPulseExample.Text = "PulseExample";
			this.btnPulseExample.Click += new System.EventHandler(this.btnPulseExample_Click);
			// 
			// btnActivityExample
			// 
			this.btnActivityExample.Location = new System.Drawing.Point(58, 95);
			this.btnActivityExample.Size = new System.Drawing.Size(106, 20);
			this.btnActivityExample.Text = "ActivityExample";
			this.btnActivityExample.Click += new System.EventHandler(this.btnActivityExample_Click);
			// 
			// btnHelloWorldExample
			// 
			this.btnHelloWorldExample.Location = new System.Drawing.Point(51, 245);
			this.btnHelloWorldExample.Size = new System.Drawing.Size(128, 20);
			this.btnHelloWorldExample.Text = "HelloWorldExample";
			this.btnHelloWorldExample.Click += new System.EventHandler(this.btnHelloWorldExample_Click);
			// 
			// btnDragExample
			// 
			this.btnDragExample.Location = new System.Drawing.Point(70, 216);
			this.btnDragExample.Size = new System.Drawing.Size(88, 20);
			this.btnDragExample.Text = "DragExample";
			this.btnDragExample.Click += new System.EventHandler(this.btnDragExample_Click);
			// 
			// btnPanExample
			// 
			this.btnPanExample.Location = new System.Drawing.Point(64, 334);
			this.btnPanExample.Size = new System.Drawing.Size(96, 20);
			this.btnPanExample.Text = "PanExample";
			this.btnPanExample.Click += new System.EventHandler(this.btnPanExample_Click);
			// 
			// vScrollBar
			// 
			this.vScrollBar.Location = new System.Drawing.Point(224, 0);
			this.vScrollBar.Maximum = 91;
			this.vScrollBar.Size = new System.Drawing.Size(16, 272);
			// 
			// PocketPiccoloFeatures
			// 
			this.Controls.Add(this.vScrollBar);
			this.Controls.Add(this.panel);
			this.Location = new System.Drawing.Point(0, -200);
			this.Menu = this.mainMenu1;
			this.Text = "ExampleRunner";

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		static void Main() 
		{
			Application.Run(new PocketPiccoloFeatures());
		}

		private void vScrollBar_ValueChanged(object sender, EventArgs e) {
			panel.Top = -vScrollBar.Value;
		}

		private void btnClipExample_Click(object sender, System.EventArgs e) {
			(new ClipExample()).Show();
		}

		private void chkPrintFrameRates_CheckStateChanged(object sender, System.EventArgs e) {
			PDebug.DebugPrintFrameRate = chkPrintFrameRates.Checked;
		}

		private void chkShowRegionManagement_CheckStateChanged(object sender, System.EventArgs e) {
			PDebug.DebugRegionManagement = chkShowRegionManagement.Checked;
		}

		private void chkShowFullBounds_CheckStateChanged(object sender, System.EventArgs e) {
			PDebug.DebugFullBounds = chkShowFullBounds.Checked;
		}

		private void btnActivityExample_Click(object sender, System.EventArgs e) {
			(new ActivityExample()).Show();
		}

		private void btnPulseExample_Click(object sender, System.EventArgs e) {
			(new PulseExample()).Show();
		}

		private void btnDragExample_Click(object sender, System.EventArgs e) {
			(new DragExample()).Show();
		}

		private void btnPanExample_Click(object sender, System.EventArgs e) {
			(new PanExample()).Show();
		}

		private void btnHelloWorldExample_Click(object sender, System.EventArgs e) {
			(new HelloWorld()).Show();
		}

		private void btnAngleNodeExample_Click(object sender, System.EventArgs e) {
			(new AngleNodeExample()).Show();
		}

		private void btnCameraExample_Click(object sender, System.EventArgs e) {
			(new CameraExample()).Show();
		}

		private void btnSquiggleExample_Click(object sender, System.EventArgs e) {
			(new SquiggleExample()).Show();
		}

		private void btnNavigationExample_Click(object sender, System.EventArgs e) {
			(new NavigationExample()).Show();
		}

		private void btnSelectionExample_Click(object sender, System.EventArgs e) {
			(new SelectionExample()).Show();
		}

		private void btnLensExample_Click(object sender, System.EventArgs e) {
			(new LensExample()).Show();
		}

		private void btnKeyEventFocusExample_Click(object sender, System.EventArgs e) {
			(new KeyEventFocusExample()).Show();
		}
	}
}
