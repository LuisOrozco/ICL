using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using ICL.Core.AgentSystem;
using ICL.Core.ICLsolver;


using ABxM.Core.AgentSystem;
using ABxM.Core;



namespace ICL.GH
{
    public class GhcSolver : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        private BeamSolver beamSolver;
        List<AgentSystemBase> iAgentSystems = new List<AgentSystemBase>();

        private bool justReset = false;
        public GhcSolver()
          : base("step-by-step Solver", "BeamSolver",
            "Execute Structural ABM model step-by-step",
            "Curve", "BeamSolver")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Execute", "E", "Execute", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Agent Systems", "AS", "Agent Systems", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Display Geometries", "G", "Display Geometries", GH_ParamAccess.list);
            pManager.AddGenericParameter("All Agent Systems", "AS", "All Agent Systems", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Iteration Count", "I", "Iteration Count", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = false;
            DA.GetData("Reset", ref iReset);

            iAgentSystems = new List<AgentSystemBase>();
            DA.GetDataList("Agent Systems", iAgentSystems);

            if (iReset || beamSolver == null)
            {
                foreach (AgentSystemBase agentSystem in iAgentSystems)
                {
                    agentSystem.Reset();
                }

                beamSolver = new BeamSolver(iAgentSystems);

                justReset = true;

                goto Conclusion; //JUMPS CODE
            }

            // update agent system list in solver
            beamSolver.AgentSystems = iAgentSystems;

            bool iExecute = false;
            DA.GetData("Execute", ref iExecute);

            if (!justReset)
            {
                beamSolver.ICLbeamSolverExecute();
            }

            justReset = false;

            int isNotFinished = 0;
            foreach (AgentSystemBase agentSystem in iAgentSystems)
                if (!agentSystem.IsFinished()) isNotFinished += 1;

            if (iExecute && !iReset && isNotFinished > 0)
            {
                ExpireSolution(true);
            }

        Conclusion:

            DA.SetDataList("Display Geometries", beamSolver.GetDisplayGeometries());
            DA.SetDataList("All Agent Systems", beamSolver.AgentSystems);
            DA.SetData("Iteration Count", beamSolver.IterationCount);
        }

        //protected override System.Drawing.Bitmap Icon { get { return Resources.Solver_StepByStep; } }
        //public override GH_Exposure Exposure { get { return GH_Exposure.quinary; } }
        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        ///// <summary>
        ///// Each component must have a unique Guid to identify it. 
        ///// It is vital this Guid doesn't change otherwise old ghx files 
        ///// that use the old ID will partially fail during loading.
        ///// </summary>
        public override Guid ComponentGuid => new Guid("BC0BADFB-FA9B-49BF-8305-CE43743CBDD4");
    }
}