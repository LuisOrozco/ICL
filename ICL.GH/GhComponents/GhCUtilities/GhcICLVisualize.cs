using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using ABxM.Core.AgentSystem;
using ICL.Core;
using ICL.Core.AgentSystem;
using ABxM.Core.Agent;
using ABxM.Core.Environments;
using Karamba.Elements;
using Karamba.Supports;
using Karamba.Loads;
using Karamba.Models;
using Karamba.GHopper.Geometry;

using System.Linq;
using Karamba.GHopper.Elements;
using Karamba.GHopper.Supports;
using Karamba.GHopper.Loads;
using Rhino.Geometry;
using Karamba.Geometry;


namespace ICL.GH.GhComponents
{
    public class GhcICLVisualize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        ICLSlabAgentSystem agentSystem = null;
        List<CartesianAgent> agents = new List<CartesianAgent>();
        List<Curve> exclusionCurves = new List<Curve>();

        public GhcICLVisualize()
          : base(
            "Visualize ICL Slab Agent System", 
            "ICL Viz",
            "Output visualization for an ICL slab agent system",
            "ABxM", 
            "ICL")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ICL Slab Agent System", "AS", "The ICL slab agent system", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Karamba Model", "M", "Solved Karamba Structural Analysis Model", GH_ParamAccess.item);
            pManager.AddPointParameter("Agent Positions", "A", "Cartesian Agent Positions", GH_ParamAccess.list);
            pManager.AddCurveParameter("Exclusion Curves", "C", "Curves defining no-go-zones", GH_ParamAccess.list);
            pManager.AddPointParameter("Excluded Vertices", "X", "Vertices where columns cannot go", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Pull from inputs into internal variables
            ICLSlabAgentSystem iAgentSystem = null;
            DA.GetData(0, ref iAgentSystem);
            agentSystem = iAgentSystem;

            // Output Karamba Model for Viz
            Model karambaModel = agentSystem.KarambaModel;
            var ghKarambaModel = new Karamba.GHopper.Models.GH_Model(karambaModel);

            DA.SetData(0, ghKarambaModel);

            // Output Agent Positions
            List<Point3d> positions = new List<Point3d>();
            foreach (CartesianAgent agent in agentSystem.Agents)
            {
                positions.Add(agent.Position);
            }
            DA.SetData(1, positions);

            // Verify No Go Zones
            List<Point3d> noPoints = new List<Point3d>();
            List<Curve> exclusionCurves = new List<Curve>(agentSystem.ExclusionCurves);
            Mesh mesh = ((Mesh3)((BuilderShell)agentSystem.ModelElements[0]).mesh).Convert();
            List<int> exclusionIndices = new List<int>();
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                foreach (Curve exclCurve in exclusionCurves)
                {
                    if (exclCurve.Contains(mesh.Vertices[i], Rhino.Geometry.Plane.WorldXY, 0.01) == PointContainment.Inside)
                    {
                        exclusionIndices.Add(i);
                    }
                }
            }
            foreach (int i in exclusionIndices)
            {
                noPoints.Add(mesh.Vertices[i]);
            }
            DA.SetData(2, exclusionCurves); 
            DA.SetData(3, noPoints);

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
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

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
            get { return new Guid("{4341E5C4-FC5F-43C0-BE44-30437B39035C}"); }
        }
    }
}