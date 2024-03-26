using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using ABxM.Core.AgentSystem;
using ICL.Core.ICLsolver;

namespace ICL.GH
{
    public class GhcSlabSolver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        private SlabSolver slabSolver;
        List<AgentSystemBase> iAgentSystems = new List<AgentSystemBase>();
        private bool justReset = false;

        public GhcSlabSolver()
          : base(
            "ICL Slab Solver", 
            "SlabSolver",
            "Execute the ICL Column agent system by a single step",
            "ABxM", 
            "ICL")
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
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = false;
            DA.GetData("Reset", ref iReset);

            iAgentSystems = new List<AgentSystemBase>();
            DA.GetDataList("Agent Systems", iAgentSystems);

            if (iReset || slabSolver == null)
            {
                foreach (AgentSystemBase agentSystem in iAgentSystems)
                {
                    agentSystem.Reset();
                }

                slabSolver = new SlabSolver(iAgentSystems);

                justReset = true;

                // First analysis of slab here?

                goto Conclusion; //JUMPS CODE
            }

            // update agent system list in solver
            slabSolver.AgentSystems = iAgentSystems;

            bool iExecute = false;
            DA.GetData("Execute", ref iExecute);

            if (!justReset)
            {
                slabSolver.ICLslabSolverExecute();
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

            DA.SetDataList("Display Geometries", slabSolver.GetDisplayGeometries());
            DA.SetDataList("All Agent Systems", slabSolver.AgentSystems);
            DA.SetData("Iteration Count", slabSolver.IterationCount);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("034C310B-B3BE-4198-9CB3-6457450A67DA"); }
        }
    }
}