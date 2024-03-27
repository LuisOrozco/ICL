using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using ABxM.Core.AgentSystem;
using ICL.Core.ICLsolver;
using ICL.Core.AgentSystem;
using ABxM.Core.Agent;
using ABxM.Core.Environments;
using Karamba.Elements;
using Karamba.Supports;
using Karamba.Loads;


namespace ICL.GH
{
    public class GhcSlabAgentSystem : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        ICLSlabAgentSystem agentSystem = null;
        List<CartesianAgent> agents = new List<CartesianAgent>();

        public GhcSlabAgentSystem()
          : base(
            "Define ICL Slab Agent System", 
            "ICL Slab Sys.",
            "Define an ICL slab agent system",
            "ABxM", 
            "ICL")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cartesian Environment", "E", "Environment", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Cartesian Agents", "A", "Cartesian Agents", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Compute Diagram", "D", "0 = None, 1 = Delaunay, 2 = Voronoi", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Elements", "Elem", "Input shells of the model", GH_ParamAccess.list);
            pManager.AddGenericParameter("Constant Supports", "Supports", "Support conditions that will not change when the agent system is run", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load", "Load", "Input loads", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ICL Slab Agent System", "AS", "The ICL slab agent system", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Pull from inputs into internal variables
            CartesianEnvironment iEnvironment = null;
            DA.GetData("Cartesian Environment", ref iEnvironment);

            List<CartesianAgent> iAgents = new List<CartesianAgent>();
            DA.GetDataList("Cartesian Agents", iAgents);

            int iDiagram = 0;
            DA.GetData("Compute Diagram", ref iDiagram);

            List<BuilderElement> iModelElements = new List<BuilderElement>();
            DA.GetDataList("Elements", iModelElements);

            List<Support> iSupports = new List<Support>();
            DA.GetDataList("Constant Supports", iSupports);

            List<Load> iLoads = new List<Load>();
            DA.GetDataList("Load", iLoads);

        // check if agents changed
        bool agentsChanged = false;
            if (agents.Count != iAgents.Count)
            {
                agentsChanged = true;
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    if (agents[i] != iAgents[i])
                    {
                        agentsChanged = true;
                    }
                }
            }
            agents = iAgents;

            // if agents changed, create a new agent system
            // otherwise, update agent system's environment, plate generator and threshold
            if (agentsChanged)
            {
                agentSystem = new ICLSlabAgentSystem(agents, iEnvironment)
                {
                    CartesianEnvironment = iEnvironment,
                    ModelElements = iModelElements,
                    ConstantSupports = iSupports,
                    Loads = iLoads
                };
            }

            agentSystem.CartesianEnvironment = iEnvironment;
            agentSystem.ModelElements = iModelElements;
            agentSystem.ConstantSupports = iSupports;
            agentSystem.Loads = iLoads;

            if (iDiagram == 0 || iDiagram > 2)
            {
                agentSystem.ComputeDelaunayConnectivity = false;
                agentSystem.ComputeVoronoiCells = false;
            }
            else if (iDiagram == 1)
            {
                agentSystem.ComputeDelaunayConnectivity = true;
                agentSystem.ComputeVoronoiCells = false;
            }
            else if (iDiagram == 2)
            {
                agentSystem.ComputeDelaunayConnectivity = false;
                agentSystem.ComputeVoronoiCells = true;
            }

            DA.SetData("ICL Slab Agent System", agentSystem);
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
        public override GH_Exposure Exposure => GH_Exposure.primary;

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
            get { return new Guid("{F4E498DD-8A47-40EA-B09C-CF50A4ACB870}"); }
        }
    }
}