using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;


using System.Linq;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;

using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper.Geometry;

using ICL.Core.AgentSystem;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class AddAgentTest : GH_ScriptInstance
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
    private void RunScript(object iAgentSystem, int iAgentID, int iNeighborID, double iDisplacement, ref object A, ref object B)
    {

        Random random = new Random();

        // defintions
        ICLSlabAgentSystem agentSystem = iAgentSystem as ICLSlabAgentSystem;
        CartesianAgent cartesianAgent = (CartesianAgent)agentSystem.Agents[iAgentID];
        Mesh mesh = ((Mesh3)((BuilderShell)agentSystem.ModelElements[0]).mesh).Convert();
        Mesh delMesh = agentSystem.DelaunayMesh;
        Dictionary<int, double> displacements = agentSystem.CartesianEnvironment.CustomData.ToDictionary(kvp => int.Parse(kvp.Key), kvp => (double)kvp.Value);
        double c = Math.Pow(10, 7);

        // testing variables
        List<Point3d> allIntersections = new List<Point3d>();
        List<Line> allIntLines = new List<Line>();
        DataTree<object> testTree = new DataTree<object>();
        int pathNum = 0;
        List<bool> tooMuch = new List<bool>();
        List<Point3d> newAgents = new List<Point3d>();

        // find topological neighbors
        int[] neighborIndices = delMesh.TopologyVertices.ConnectedTopologyVertices(iAgentID);

        foreach (int neighborIndex in neighborIndices)
        {
            //if (random.NextDouble() > Probability) return;

            LineCurve lineCurve = new LineCurve(cartesianAgent.Position, agentSystem.DelaunayMesh.Vertices[neighborIndex]);
            int[] faceIds = new int[0];
            Point3d[] intersections = Rhino.Geometry.Intersect.Intersection.MeshLine(mesh, lineCurve.Line, out faceIds);
            List<int> nearVertexIds = new List<int>();
            if (intersections.Length > 0)
            {
                foreach (int faceId in faceIds.Distinct())
                {
                    MeshFace face = mesh.Faces[faceId];
                    int[] faceVertices = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };

                    for (int j = 0; j < faceVertices.Length; j++)
                    {
                        int next = (j + 1) % faceVertices.Length;
                        Line edgeLine = mesh.TopologyEdges.EdgeLine(mesh.TopologyEdges.GetEdgeIndex(faceVertices[j], faceVertices[next]));
                        foreach (Point3d intersectionPoint in intersections)
                        {
                            if (edgeLine.ClosestPoint(intersectionPoint, true).DistanceTo(intersectionPoint) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                            {
                                nearVertexIds.Add(faceVertices[j]);
                                nearVertexIds.Add(faceVertices[next]);
                            }
                        }
                    }
                }
            }
            List<int> cleanVertexIds = nearVertexIds.Distinct().ToList();
            List<double> displs = new List<double>();
            foreach (int vertexId in cleanVertexIds)
            {
                double displ = displacements[vertexId] * c;
                displs.Add(displ);
            }
            testTree.AddRange(displs.Cast<object>(), new GH_Path(pathNum));
            pathNum++;

            if (nearVertexIds.Any(vertexId => displacements[vertexId] * c > iDisplacement))
            {
                tooMuch.Add(true);
            }
            else
            {
                tooMuch.Add(false);
            }
        }
        A = testTree;
        B = tooMuch;


        /*
            Point3d newAgentPosition = lineCurve.PointAtNormalizedLength(0.5);

            double minDist = double.MaxValue;
            Point3d newMeshAgentPosition = Point3d.Unset;
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
            double verDist = newAgentPosition.DistanceToSquared(mesh.Vertices[i]);
            if (verDist < minDist)
            {
            minDist = verDist;
            newMeshAgentPosition = mesh.Vertices[i];
            }

            }

            List<BehaviorBase> newAgentBehaviors = cartesianAgent.Behaviors; // the new agent has the same behaviors as other original agent
            CartesianAgent newAgent = new CartesianAgent(newMeshAgentPosition, newAgentBehaviors);
            */



    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }

    // <Custom additional code> 




    // </Custom additional code> 
}