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
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization;

using UMD.HCIL.PocketPiccolo.Activities;
using UMD.HCIL.PocketPiccolo.Util;

namespace UMD.HCIL.PocketPiccolo {

	#region Delegates
	/// <summary>
	/// A delegate used to invoke the <c>ProcessScheduledInputs</c> method on
	/// the main UI thread.
	/// </summary>
	public delegate void ProcessScheduledInputsDelegate();
	#endregion

	#region Input Source Interface
	/// <summary>
	/// This interface is for advanced use only. If you want to implement a
	/// different kind of input framework than Piccolo provides you can hook
	/// it in here.
	/// </summary>
	public interface InputSource {
		/// <summary>
		/// Process pending input events.
		/// </summary>
		void ProcessInput();
	}
	#endregion

	/// <summary>
	/// <b>PRoot</b> serves as the top node in Piccolo's runtime structure.
	/// </summary>
	/// <remarks>
	/// The PRoot is responsible for running the main UI loop that processes
	/// input from activities and external events.
	/// </remarks>
	//[Serializable]
	public class PRoot : PNode {

		#region Fields
		/// <summary>
		/// A flag that indicates whether Piccolo is currently processing inputs.
		/// </summary>
		[NonSerialized] protected bool processingInputs;

		/// <summary>
		/// A flag that indicates whether inputs are scheduled to be processed.
		/// </summary>
		[NonSerialized] protected bool processInputsScheduled;

		/// <summary>
		/// Used to invoke the <c>ProcessScheduledInputs</c> method on the main UI
		/// thread.
		/// </summary>
		protected ProcessScheduledInputsDelegate processScheduledInputsDelegate;

		private PInputManager defaultInputManager;
		[NonSerialized] private ArrayList inputSources;
		[NonSerialized] private long globalTime;
		private PActivityScheduler activityScheduler;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new PRoot.
		/// </summary>
		/// <remarks>
		/// Note the PCanvas already creates a basic scene graph for you so usually you
		/// will not need to construct your own roots.
		/// </remarks>
		public PRoot() {
			inputSources = new ArrayList();
			processScheduledInputsDelegate = new ProcessScheduledInputsDelegate(this.ProcessScheduledInputs);
			globalTime = PUtil.CurrentTimeMillis;
			activityScheduler = new PActivityScheduler(this);
		}
		#endregion

		#region Activities
		//****************************************************************
		// Activities - Methods for scheduling activities to run.
		//****************************************************************

		/// <summary>
		/// Overridden.  Add an activity to the activity scheduler associated with
		/// this root.
		/// </summary>
		/// <param name="activity">The new activity to scheduled.</param>
		/// <returns>
		/// True if the activity is successfully scheduled; otherwise, false.
		/// </returns>
		/// <remarks>
		/// Activities are given a chance to run during each call to the root's
		/// <c>ProcessInputs</c> method. When the activity has finished running it
		/// will automatically get removed.
		/// </remarks>
		public override bool AddActivity(PActivity activity) {
			ActivityScheduler.AddActivity(activity);
			return true;
		}

		/// <summary>
		/// Get the activity scheduler associated with this root.
		/// </summary>
		public virtual PActivityScheduler ActivityScheduler {
			get { return activityScheduler; }
		}
		
		/// <summary>
		/// Wait for all scheduled activities to finish before returning from
		///	this method. This will freeze out user input, and so it is generally
		/// recommended that you use <c>PActivity.StartTime</c> and
		/// <c>PActivity.StartAfter</c> to offset activities instead of using
		/// this method.
		/// </summary>
		public virtual void WaitForActivities() {
			CameraWithCanvasFilter cameraWithCanvas = new CameraWithCanvasFilter();

			while (activityScheduler.ActivitiesReference.Count > 0) {
				ProcessInputs();

				PNodeList nodes = GetAllNodes(cameraWithCanvas, null);
				foreach (PCamera each in nodes) {
					each.Canvas.PaintImmediately();
				}
			}				
		}

		/// <summary>
		/// A node filter that only accepts cameras that are associated with canvases.
		/// </summary>
		private class CameraWithCanvasFilter : PNodeFilter {
			public bool Accept(PNode aNode) {
				return (aNode is PCamera) && (((PCamera)aNode).Canvas != null);
			}
			public bool AcceptChildrenOf(PNode aNode) {
				return true;
			}
		}
		#endregion

		#region Basics
		/// <summary>
		/// Overridden.  Get's this.
		/// </summary>
		/// <value>This root node.</value>
		public override PRoot Root {
			get { return this; }
		}

		/// <summary>
		/// Gets the default input manager to be used when processing input events.
		/// </summary>
		/// <value>The default input manager.</value>
		/// <remarks>
		/// PCanvas's use this method when they forward new input events to the
		/// PInputManager.
		/// </remarks>
		public virtual PInputManager DefaultInputManager {
			get {
				if (defaultInputManager == null) {
					defaultInputManager = new PInputManager();
					AddInputSource(defaultInputManager);
				}
				return defaultInputManager;
			}
		}

		/// <summary>
		/// Advanced. If you want to add additional input sources to the root's UI process
		/// you can do that here.
		/// </summary>
		/// <param name="inputSource">The new input source to add.</param>
		/// <remarks>
		/// You will seldom do this unless you are making additions to the piccolo framework.
		/// </remarks>
		public virtual void AddInputSource(InputSource inputSource) {
			inputSources.Add(inputSource);
		}

		/// <summary>
		/// Advanced. If you want to remove an input source from the root's UI process you
		/// can do that here.
		/// </summary>
		/// <param name="inputSource">The input source to remove.</param>
		/// <remarks>
		/// You will seldom do this unless you are making additions to the piccolo framework.
		/// </remarks>
		public virtual void RemoveInputSource(InputSource inputSource) {
			inputSources.Remove(inputSource);
		}
		#endregion

		#region UI Loop
		//****************************************************************
		// UI Loop - Methods for running the main UI loop of Piccolo. 
		//****************************************************************

		/// <summary>
		/// Gets the global Piccolo time.
		/// </summary>
		/// <remarks>
		/// This is set to <c>PUtil.CurrentTimeMillis</c> at the beginning of the root's
		/// <c>ProcessInputs</c> method.  Activities should usually use this global time
		/// instead of <c>PUtil.CurrentTimeMillis</c> so that multiple activities will be
		/// synchronized.
		/// </remarks>
		public virtual long GlobalTime {
			get { return globalTime; }
		}

		/// <summary>
		/// This is the heartbeat of the Piccolo framework, where all processing is done.
		/// </summary>
		/// <remarks>
		/// In this method, pending input events are processed, Activities are given a
		/// chance to run, and the bounds caches and any paint damage are validated.
		/// </remarks>
		public virtual void ProcessInputs() {
			PDebug.StartProcessingInput();
			processingInputs = true;

			globalTime = PUtil.CurrentTimeMillis;
			foreach (InputSource each in inputSources) {
				each.ProcessInput();
			}

			activityScheduler.ProcessActivities(globalTime);

			ValidateFullBounds();
			ValidateFullPaint();

			processingInputs = false;
			PDebug.EndProcessingInput();
		}

		/// <summary>
		/// Overridden.  See <see cref="PNode.FullBoundsInvalid">PNode.FullBoundsInvalid</see>.
		/// </summary>
		protected override bool FullBoundsInvalid {
			set { 
				base.FullBoundsInvalid = value;
				ScheduleProcessInputsIfNeeded();
			}
		}

		/// <summary>
		/// Overridden.  See <see cref="PNode.ChildBoundsInvalid">PNode.ChildBoundsInvalid</see>.
		/// </summary>
		protected override bool ChildBoundsInvalid {
			set { 
				base.ChildBoundsInvalid = value;
				ScheduleProcessInputsIfNeeded();
			}
		}

		/// <summary>
		/// Overridden.  See <see cref="PNode.PaintInvalid">PNode.PaintInvalid</see>.
		/// </summary>
		public override bool PaintInvalid {
			set {
				base.PaintInvalid = value;
				ScheduleProcessInputsIfNeeded();
			}
		}
	
		/// <summary>
		/// Overridden.  See <see cref="PNode.ChildPaintInvalid">PNode.ChildPaintInvalid</see>.
		/// </summary>
		public override bool ChildPaintInvalid {
			set { 
				base.ChildPaintInvalid = value;
				ScheduleProcessInputsIfNeeded();
			}
		}

		/// <summary>
		/// Processes currently scheduled inputs and resets processInputsScheduled flag.
		/// </summary>
		protected virtual void ProcessScheduledInputs() {
			ProcessInputs();
			processInputsScheduled = false;
		}

		/// <summary>
		/// If something in the scene graph needs to be updated, this method will schedule
		/// ProcessInputs run at a later time.
		/// </summary>
		public virtual void ScheduleProcessInputsIfNeeded() {
			if (processScheduledInputsDelegate == null) {
				return;
			}
			/*
			if (!Application.MessageLoop) {
				// Piccolo is not thread safe and should almost always be called from the 
				// event dispatch thread. It should only reach this point when a new canvas
				// is being created.
				return;
			}
			*/

			if (!processInputsScheduled && !processingInputs && 
				(FullBoundsInvalid || ChildBoundsInvalid || PaintInvalid || ChildPaintInvalid)) {
			//	if (PCanvas.CURRENT_PCANVAS != null && PCanvas.CURRENT_PCANVAS.IsHandleCreated) {
				if (PCanvas.CURRENT_PCANVAS != null) {				
					processInputsScheduled = true;
					//PCanvas.CURRENT_PCANVAS.BeginInvoke(processScheduledInputsDelegate);
					//PCanvas.CURRENT_PCANVAS.Invoke(processScheduledInputsDelegate);
					ProcessScheduledInputs();
				}
			}
		}
		#endregion

		#region Serialization
		/*
		/// <summary>
		/// Read this this root and all its children from the given SerializationInfo.
		/// </summary>
		/// <param name="info">The SerializationInfo to read from.</param>
		/// <param name="context">
		/// The StreamingContext of this serialization operation.
		/// </param>
		/// <remarks>
		/// This constructor is required for Deserialization.
		/// </remarks>
		protected PRoot(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}
		*/
		#endregion
	}
}