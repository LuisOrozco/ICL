using Grasshopper.Kernel;
using Rhino;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class InitialiseAgents : GH_ScriptInstance
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
    private void RunScript(int iColumnCount, Curve iEnvBoundary, Mesh iEnvMesh, ref object oColumnStartPos)
    {
        // Initialize lists to store column start positions and their indices
        List<Point3d> columnStartPositions = new List<Point3d>();
        List<int> columnStartPosIndices = new List<int>();

        // Divide the boundary curve into equal segments based on the column count
        double[] tParams = iEnvBoundary.DivideByCount(iColumnCount, true);

        // Iterate over each parameter on the boundary curve
        foreach (var t in tParams)
        {
            // Get the point on the boundary at parameter t
            Point3d boundaryPoint = iEnvBoundary.PointAt(t);

            // Create a list to store mesh vertex points
            List<Point3d> meshVertices = new List<Point3d>(iEnvMesh.Vertices.ToPoint3dArray());

            // Find the index of the closest mesh vertex to the boundary point
            int closestVertexIndex = meshVertices.FindIndex(vertex => vertex.DistanceTo(boundaryPoint) == meshVertices.Min(v => v.DistanceTo(boundaryPoint)));
            columnStartPosIndices.Add(closestVertexIndex);

            // Add the closest mesh vertex to the column start positions list
            columnStartPositions.Add(meshVertices[closestVertexIndex]);
        }

        // Output the column start positions
        oColumnStartPos = columnStartPositions;
    }

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        throw new NotImplementedException();
    }

    // <Custom additional code> 

    // </Custom additional code> 
}