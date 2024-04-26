using ABxM.Core.Agent;
using ABxM.Core.AgentSystem;
using ABxM.Core.Behavior;
using ABxM.Core.Environments;
using ICL.Core.AgentSystem;
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
        /// The field that defines the minimum distance between two agents, within which agents are removed.
        /// </summary>
        public double Distance;
        
        /// <summary>
        /// The field that defines the maximum displacement between agents before another is created.
        /// </summary>
        public double Displacement;

        /// <summary>
        /// The probability that a new agent will be created.
        /// </summary>
        public double Probability;

        /// <summary>
        /// Random item that allows for agents to be created gradually.
        /// </summary>
        private static Random random = new Random();
        
        /// <summary>
        /// Consructs a new instance of the remove agent behavior.
        /// </summary>
        /// <param name="weight">The behavior's weight.</param>
        /// <param name="displacement">The maximum allowable displacement.</param>
        /// <param name="probability">The probability that a new agent will be created</param>
        public AddAgentBehavior(double weight, double displacement, double probability, double distance)
        {
            Weight = weight;
            Displacement = displacement;
            Probability = probability;
            Distance = distance;
        }

        /// <summary>
        /// Method for executing the behavior's rule.
        /// </summary>
        /// <param name="agent">The agent that executes the behavior.</param>
        public override void Execute(AgentBase agent)
        {
            // defintions
            CartesianAgent cartesianAgent = agent as CartesianAgent;
            ICLSlabAgentSystem agentSystem = (ICLSlabAgentSystem)(cartesianAgent.AgentSystem);
            Mesh agentDelaunay = agentSystem.DelaunayMesh;
            Mesh envMesh = ((Mesh3)((BuilderShell)agentSystem.ModelElements[0]).mesh).Convert();
            CartesianEnvironment cartesianEnvironment = (CartesianEnvironment)agentSystem.CartesianEnvironment;
            Dictionary<int, double> displacements = cartesianEnvironment.CustomData.ToDictionary(kvp => int.Parse(kvp.Key), kvp => (double)kvp.Value);
            double c = Math.Pow(10, 7);

            // find the agent delaunay vertex Id of my agent
            int agentId = -1;
            for (int i = 0; i < agentDelaunay.Vertices.Count; i++)
            {
                Point3f pointF = agentDelaunay.Vertices[i];
                Point3d pointD = new Point3d(pointF);
                if (cartesianAgent.Position.DistanceTo(pointD) < 0.001)
                {
                    agentId = i;
                    break;
                }
            }
            if (agentId == -1) { return; }
            // find topological neighbors
            int[] neighborIndices = agentDelaunay.TopologyVertices.ConnectedTopologyVertices(agentId);
            foreach (int neighborIndex in neighborIndices)
            {
                // Randomly decide (with given probability) whether to create a new agent, effectively creates new agents gradually rather than all at once at the very first iteration
                // if (random.NextDouble() > Probability) continue;

                // find intersections between agent delaunay and evironment mesh edges
                LineCurve lineCurve = new LineCurve(cartesianAgent.Position, agentDelaunay.Vertices[neighborIndex]);
                int[] faceIds = new int[0];
                Point3d[] intersections = Rhino.Geometry.Intersect.Intersection.MeshLine(envMesh, lineCurve.Line, out faceIds);

                // if there are intersections,find the endpoints of the intersecting edges
                if (!(intersections.Length > 0)) continue;
                List<int> nearVertexIds = new List<int>();
                foreach (int faceId in faceIds.Distinct())
                {
                    // look at the faces it intersected with
                    MeshFace face = envMesh.Faces[faceId];
                    int[] faceVertices = face.IsQuad ? new[] { face.A, face.B, face.C, face.D } : new[] { face.A, face.B, face.C };

                    // Check each edge of the face for an intersection
                    for (int j = 0; j < faceVertices.Length; j++)
                    {
                        int next = (j + 1) % faceVertices.Length;
                        Line edgeLine = envMesh.TopologyEdges.EdgeLine(envMesh.TopologyEdges.GetEdgeIndex(faceVertices[j], faceVertices[next]));

                        foreach (Point3d intersectionPoint in intersections)
                        {
                            if (edgeLine.ClosestPoint(intersectionPoint, true).DistanceTo(intersectionPoint) < 0.001)
                            {
                                // If the intersection point is on the edge, add the vertex indices
                                nearVertexIds.Add(faceVertices[j]);
                                nearVertexIds.Add(faceVertices[next]);
                            }
                        }
                    }
                }
                // remove duplicates from list of intersecting edge endpoints
                List<int> cleanVertexIds = nearVertexIds.Distinct().ToList();
                // do any of these vertices have a high enough displacement?
                if (cleanVertexIds.Any(vertexId => displacements[vertexId] * c > Displacement))
                {
                    //// Find indexes of vertices inside the no column zones
                    //List<Curve> exclusonCurves = new List<Curve>(agentSystem.ExclusionCurves);
                    //List<int> exclusionIndices = new List<int>();
                    //for (int i = 0; i < envMesh.Vertices.Count; i++)
                    //{
                    //    foreach (Curve exclCurve in exclusonCurves)
                    //    {
                    //        if (exclCurve.Contains(envMesh.Vertices[i], Plane.WorldXY, 0.01) == PointContainment.Inside)
                    //        {
                    //            exclusionIndices.Add(i);
                    //        }
                    //    }
                    //}
                    // midpoint on agent delaunay
                    Point3d newAgentPosition = lineCurve.PointAtNormalizedLength(0.5);
                    double minDist = double.MaxValue;
                    Point3d newMeshAgentPosition = Point3d.Unset;
                    double maxDist = 0.8;
                    double maxDistSquared = maxDist * maxDist;
                    foreach (int vertexId in cleanVertexIds)
                    {
                        // only make agents outside of exclusion zones
                        if (agentSystem.ExclusionIndices.Contains(vertexId)) { continue; }
                        double verDist = newAgentPosition.DistanceToSquared(envMesh.Vertices[vertexId]);
                        if (verDist < minDist && verDist <= maxDistSquared)
                        {
                            // environment mesh vertex closest to midpoint and within 0.8m
                            minDist = verDist;
                            newMeshAgentPosition = envMesh.Vertices[vertexId];
                        }
                    }
                    // make sure new agent is close enough to agent delaunay midpoint
                    if (newMeshAgentPosition == Point3d.Unset) continue;

                    // only make newAgent if its not too close to an existing agent
                    bool isCloseToAnyAgent = false;
                    foreach (CartesianAgent otherAgent in agentSystem.Agents)
                    {
                        if (newMeshAgentPosition.DistanceTo(cartesianAgent.Position) < Distance)
                        {
                            isCloseToAnyAgent = true;
                            break;
                        }
                    }
                    if (!isCloseToAnyAgent)
                    {
                        List<BehaviorBase> newAgentBehaviors = cartesianAgent.Behaviors;
                        CartesianAgent newAgent = new CartesianAgent(newMeshAgentPosition, newAgentBehaviors);
                        agentSystem.AddAgentList.Add(newAgent);
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
