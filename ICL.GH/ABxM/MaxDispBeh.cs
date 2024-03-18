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
using System.Windows.Forms;
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
using ICL.Core.AgentBehaviors;
using ICL.Core.AgentSystem;
using ICL.Core.Environment;
using ICL.Core.ICLsolver;
using ICL.Core.StructuralAnalysis;
using ICL.Core.StructuralModelling;
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
            // the environment is set in the custom code

            // get displacement info from the environment
            Dictionary<int, Point3d> originalPointDict = new Dictionary<int, Point3d>();
            Dictionary<int, Point3d> displacedPointDict = new Dictionary<int, Point3d>();

            foreach (KeyValuePair<string, object> kvp in Environment.CustomData)
            {
                int.TryParse(kvp.Key, out int key);
                var nodeData = (Tuple<Point3d, Point3d>)kvp.Value;
                originalPointDict[key] = nodeData.Item1;
                displacedPointDict[key] = nodeData.Item2;
            }

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

            // Create a dictionary to hold the neighbors and their displacements
            Dictionary<string, object> neighborTuple = new Dictionary<string, object>();
            Dictionary<int, List<Point3d>> neighborNodes = new Dictionary<int, List<Point3d>>();
            Dictionary<int, List<Point3d>> displacedNeighborNodes = new Dictionary<int, List<Point3d>>();
            List<Point3d> meshNeighbors = new List<Point3d>();
            int[] meshNeighborIndexes = EnvMesh.TopologyVertices.ConnectedTopologyVertices(agentPositionIndex);
            foreach (int index in meshNeighborIndexes)
            {
                meshNeighbors.Add(EnvMesh.Vertices.Point3dAt(index));
            }
            foreach (Point3d meshPoint in meshNeighbors)
            {
                foreach (KeyValuePair<string, object> kvp in Environment.CustomData)
                {
                    var nodeData = (Tuple<Point3d, Point3d>)kvp.Value;
                    if (meshPoint == nodeData.Item1)
                    {
                        neighborTuple.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            // Get vector to move towards max displacement
            Point3d targetVertex = Point3d.Unset;
            double maxZ = double.MinValue;

            foreach (KeyValuePair<string, object> kvp in neighborTuple)
            {
                var nodeData = (Tuple<Point3d, Point3d>)kvp.Value;
                if (Math.Abs(nodeData.Item2.Z) > maxZ)
                {
                    targetVertex = nodeData.Item1;
                    maxZ = Math.Abs(nodeData.Item2.Z);
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