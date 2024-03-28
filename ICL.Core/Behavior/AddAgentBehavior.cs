using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ICL.Core.AgentSystem;
using ICL.Core.Utilities;
using Karamba.Elements;
using Karamba.Geometry;
using Karamba.GHopper.Geometry;
using Rhino.Geometry;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.Delaunay;

namespace ICL.Core.Behavior
{
    public class AddAgentBehavior : BehaviorBase
    {
        /// <summary>
        /// The field that defines the maximum displacement between agents before another is created.
        /// </summary>
        public double Displacement;

        /// <summary>
        /// The probability that a new agent will be created.
        /// </summary>
        public double Probability;

        /// <summary>
        /// The minimum angle, in degrees, between the edges of each face in the mesh.
        /// </summary>
        public double AngleThreshold;

        /// <summary>
        /// Random item that allows for agents to be created gradually.
        /// </summary>
        private static Random random = new Random();
        
        /// <summary>
        /// Consructs a new instance of the remove agent behaviour.
        /// </summary>
        /// <param name="weight">The behaviour's weight.</param>
        /// <param name="displacement">The maximum allowable displacement.</param>
        public AddAgentBehavior(double weight, double displacement, double probability, double angle)
        {
            Weight = weight;
            Displacement = displacement;
            Probability = probability;
            AngleThreshold = angle;
        }

        /// <summary>
        /// Method for executing the behaviour's rule.
        /// </summary>
        /// <param name="agent">The agent that executes the behaviour.</param>
        public override void Execute(AgentBase agent)
        {
            CartesianAgent cartesianAgent = agent as CartesianAgent;
            ICLSlabAgentSystem agentSystem = (ICLSlabAgentSystem)(cartesianAgent.AgentSystem);
            CartesianEnvironment cartesianEnvironment = (CartesianEnvironment)agentSystem.CartesianEnvironment;

            // Randomly decide (with given probability) whether to create a new agent
            // This effectively makes the new agents being created gradually rather than all at once at the very first iteration
            if (random.NextDouble() > Probability)  return;



            // get the postition of all agents in the system
            List<Point3d> agentPositions = agentSystem.Agents.Cast<CartesianAgent>().Select(oneAgent => oneAgent.Position).ToList();

            // convert Point3d to node2 because grasshopper needs a Node2List for Delaunay
            var nodes = new Grasshopper.Kernel.Geometry.Node2List();
            foreach (var p in agentPositions)
            {
                nodes.Append(new Grasshopper.Kernel.Geometry.Node2(p.X, p.Y));
            }
            // Solve Delaunay
            Connectivity diagram = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Connectivity(nodes, 1.0, false);
            List<CartesianAgent> neighbourList = new List<CartesianAgent>();
            List<int> connections = new List<int>();

            Mesh delMesh = new Mesh();
            var faces = new List<Grasshopper.Kernel.Geometry.Delaunay.Face>();
            faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, 1.0);
            delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, 1.0, ref  faces);
            Mesh cleanMesh = CleanMesh(delMesh, AngleThreshold);

            //find this agetn's index in the clean mesh
            Point3f agentPositionF = new Point3f((float)cartesianAgent.Position.X, (float)cartesianAgent.Position.Y, (float)cartesianAgent.Position.Z);
            float tolerance = 0.01f;
            int agentPositionIndex = -1;
            for (int i = 0; i < cleanMesh.TopologyVertices.Count; i++)
            {
                Point3f vertex = cleanMesh.TopologyVertices[i];
                if (vertex.DistanceTo(agentPositionF) < tolerance)
                {
                    agentPositionIndex = i;
                    break;
                }
            }
            if (agentPositionIndex == -1) return;

            // find mesh neighbors in topology
            int[] meshNeighborIndexes = cleanMesh.TopologyVertices.ConnectedTopologyVertices(agentPositionIndex);

            // which neighbours have more than an allowable displacement betwen us
            foreach (int i in meshNeighborIndexes)
            {
                LineCurve lineCurve = new LineCurve(cartesianAgent.Position, new Point3d(cleanMesh.TopologyVertices[i]));
                int[] faceIds = new int[0];
                Point3d[] intersections = Rhino.Geometry.Intersect.Intersection.MeshLine(cleanMesh, lineCurve.Line, out faceIds);
                List<int> intersectingEdgesIndices = new List<int>();
                if (intersections.Length > 0)
                {
                    foreach (int faceId in faceIds.Distinct())
                    {
                        MeshFace face = cleanMesh.Faces[faceId];
                        int[] faceVertices = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };

                        // Check each edge of the face for an intersection
                        for (int j = 0; j < faceVertices.Length; j++)
                        {
                            int next = (j + 1) % faceVertices.Length;
                            Line edgeLine = cleanMesh.TopologyEdges.EdgeLine(cleanMesh.TopologyEdges.GetEdgeIndex(faceVertices[j], faceVertices[next]));

                            foreach (Point3d intersectionPoint in intersections)
                            {
                                if (edgeLine.ClosestPoint(intersectionPoint, true).DistanceTo(intersectionPoint) < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                                {
                                    // If the intersection point is on the edge, add the vertex indices
                                    intersectingEdgesIndices.Add(faceVertices[j]);
                                    intersectingEdgesIndices.Add(faceVertices[next]);
                                }
                            }
                        }
                    }
                }
            }

        }

        public Mesh CleanMesh(Mesh mesh, double angleThreshold)
        {
            if (mesh == null || !mesh.IsValid)
                return null;

            Mesh cleanedMesh = mesh.DuplicateMesh();

            cleanedMesh.Normals.ComputeNormals();
            cleanedMesh.FaceNormals.ComputeFaceNormals();
            cleanedMesh.Compact();

            double angleThresholdRadians = Rhino.RhinoMath.ToRadians(angleThreshold);
            List<int> facesToDelete = new List<int>();
            bool[] nakedPts = mesh.GetNakedEdgePointStatus();

            for (int i = 0; i < cleanedMesh.Faces.Count; i++)
            {
                MeshFace face = cleanedMesh.Faces[i];
                int smallAngleCount = 0;
                bool hasNakedEdge = false;

                int[] vertexIndices = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };
                Point3d[] vertices = new Point3d[face.IsQuad ? 4 : 3];
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j] = cleanedMesh.Vertices[vertexIndices[j]];
                    if (nakedPts[vertexIndices[j]])
                    {
                        hasNakedEdge = true;
                    }
                }

                if (hasNakedEdge)
                {
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        int prev = (j + vertices.Length - 1) % vertices.Length;
                        int next = (j + 1) % vertices.Length;

                        Vector3d vectorA = vertices[j] - vertices[prev];
                        Vector3d vectorB = vertices[next] - vertices[j];

                        double angle = Vector3d.VectorAngle(vectorA, vectorB);
                        if (angle > Math.PI / 2) angle = Math.PI - angle;
                        if (angle < angleThresholdRadians)
                        {
                            smallAngleCount++;
                        }
                    }

                    if (smallAngleCount >= 2)
                    {
                        facesToDelete.Add(i);
                    }
                }
            }

            if (facesToDelete.Count > 0)
            {
                cleanedMesh.Faces.DeleteFaces(facesToDelete.ToArray());
                cleanedMesh.Compact();
            }

            return cleanedMesh;
        }
    }
}
