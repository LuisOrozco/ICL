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
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Geometry.Delaunay;
using Grasshopper.Kernel.Geometry.Voronoi;
using GH_IO;
using GH_IO.Serialization;

using ABxM.Core;
using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ABxM.Core.Utilities;

using ICL.Core;
using ICL.Core.AgentSystem;
using ICL.Core.Behavior;

using Karamba;
using Karamba.CrossSections;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Models;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;
using Karamba.Results;
using Karamba.GHopper;
using Karamba.GHopper.Geometry;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Visualize : GH_ScriptInstance
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
    private void RunScript(object x, object y, ref object A, ref object B, ref object C, ref object D, ref object E)
    {

        ICLSlabAgentSystem agentSystem = x as ICLSlabAgentSystem;
        Model karambaModel = agentSystem.KarambaModel;
        A = new Karamba.GHopper.Models.GH_Model(karambaModel);

        List<Point3d> positions = new List<Point3d>();
        foreach (CartesianAgent agent in agentSystem.Agents)
        {
            positions.Add(agent.Position);
        }
        B = positions;

        List<Point3d> noPoints = new List<Point3d>();
        List<Curve> exclusionCurves = new List<Curve>(agentSystem.ExclusionCurves);
        D = exclusionCurves;
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
        C = noPoints;

        List<Line> allEdges = new List<Line>();
        foreach (CartesianAgent agent in agentSystem.Agents)
        {
            List<CartesianAgent> neighbors = agentSystem.FindTopologicalNeighbors(agent);
            foreach (CartesianAgent neighbor in neighbors)
            {
                allEdges.Add(new Line(agent.Position, neighbor.Position));
            }
        }
        List<Line> uniqueEdges = new List<Line>();
        for (int i = 0; i < allEdges.Count; i++)
        {
            bool isDuplicate = false;
            if(uniqueEdges.Count < 1)
            {
                uniqueEdges.Add(allEdges[i]);
                continue;
            }
            for (int j = 0; j < uniqueEdges.Count; j++)
            {
                if (IsDuplicateLine(allEdges[i], uniqueEdges[j], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance))
                {
                    isDuplicate = true; 
                    break;
                }
                if(!isDuplicate) uniqueEdges.Add(allEdges[i]);
            }
        }

        List<int> neighborCount = new List<int>();
        for (int i = 0; i < positions.Count; i++)
        {
            List<int> neighborIndices = agentSystem.diagram.GetConnections(i);
            neighborCount.Add(neighborIndices.Count);
        }
        E = neighborCount;

    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }

    // <Custom additional code> 
    private bool IsDuplicateLine(Line line1, Line line2, double tolerance)
    {
        // Check if the start and end points of the lines are within the tolerance
        if (line1.From.DistanceTo(line2.From) < tolerance && line1.To.DistanceTo(line2.To) < tolerance)
            return true;
        if (line1.From.DistanceTo(line2.To) < tolerance && line1.To.DistanceTo(line2.From) < tolerance)
            return true;
        return false;
    }
    // </Custom additional code> 
}