using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;

using Rhino.Collections;
using Rhino.DocObjects;
using GH_IO;
using GH_IO.Serialization;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;

using ICL.Core;
using ICL.Core.Behavior;
using ICL.Core.AgentSystem;
using ICL.Core.Environments;
using ICL.Core;
using ICL.Core.Utilities;
using ICL.Core.Utilities;
using ICL.Core.Utilities;

using Karamba;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper;
using Karamba.GHopper.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class MaxDispBeh : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(Mesh iEnvMesh, object iCartEnv, object iMeshEnv, ref object oBehavior)
    {

        behavior.EnvMesh = iEnvMesh;
        behavior.Environment = iCartEnv as CartesianEnvironment;
        oBehavior = behavior;


    }

    // <Custom additional code> 


    MaxDispBehavior behavior = new MaxDispBehavior(new Mesh());

    public class MaxDispBehavior : BehaviorBase
    {
        public Mesh EnvMesh;
        public CartesianEnvironment Environment;


        public MaxDispBehavior(Mesh envMesh)
        {
            EnvMesh = envMesh;
            Weight = 1.0;
        }

        public override void Execute(AgentBase agent)
        {
            if (!(agent is CartesianAgent cartesianAgent)) return;
            if (!(cartesianAgent.AgentSystem is CartesianAgentSystem system)) return;
            if (!(system.CartesianEnvironment is CartesianEnvironment environment)) return;

            // get displacement info from the environment
            Dictionary<int, double> nodalDisplacements = environment.CustomData.ToDictionary(kvp => int.Parse(kvp.Key), kvp => (double)kvp.Value);

            //// Find the index of the agent's position in the mesh's topology vertices
            // Convert Point3d to Point3f
            Point3f agentPositionF = new Point3f((float)cartesianAgent.Position.X, (float)cartesianAgent.Position.Y, (float)cartesianAgent.Position.Z);
            // Define a tolerance for comparing points
            float tolerance = 0.01f;
            // Find the index of the agent's position in the mesh's topology vertices
            int agentPositionIndex = -1;

            for (int i = 0; i < EnvMesh.TopologyVertices.Count; i++)
            {
                Point3f vertex = EnvMesh.TopologyVertices[i];
                if (vertex.DistanceTo(agentPositionF) < tolerance)
                {
                    agentPositionIndex = i;
                    break;
                }
            }

            if (agentPositionIndex == -1) return;

            // find mesh neighbors in topology
            int[] meshNeighborIndexes = EnvMesh.TopologyVertices.ConnectedTopologyVertices(agentPositionIndex);

            // Get vector to move towards max displacement
            Point3d targetVertex = Point3d.Unset;
            double maxZ = double.MinValue;

            foreach (int meshNeighborIndex in meshNeighborIndexes)
            {
                double thisDisplacement = Math.Abs(nodalDisplacements[meshNeighborIndex]);
                if (thisDisplacement > maxZ)
                {
                    maxZ = thisDisplacement;
                    targetVertex = EnvMesh.Vertices[meshNeighborIndex];
                }
            }

            if (targetVertex != Point3d.Unset)
            {
                // Create a vector that points from the agent's current position to the target vertex
                Vector3d moveVec = targetVertex - cartesianAgent.Position;
                // Apply the moveVec to the agent's position
                cartesianAgent.Moves.Add(moveVec);
                cartesianAgent.Weights.Add(Weight);
            }

        }
    }


    // </Custom additional code> 
    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }


}