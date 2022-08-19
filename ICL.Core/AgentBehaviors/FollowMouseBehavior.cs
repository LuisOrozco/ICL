using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using ICD.AbmFramework.Core.Agent;
using ICD.AbmFramework.Core.Behavior;
using Rhino;

namespace ICL.Core.AgentBehaviors
{
    public class FollowMouseBehavior : BehaviorBase
    {
        public static double Range;
        public static AgentBase NearestAgent;
        public static double MinDistance;
        public static Line MouseLine;
        public static bool FindNearestAgent = true;
        public double Weight;
        public bool Enabled = true;
        public double SteppingFactor;

        //static MouseResponseBehaviorCallback mouseCallback = new MouseResponseBehaviorCallback(); // testing: does commenting out resolve issue with mouse not being tracked in other component?

        public FollowMouseBehavior(double weight, double range)
        {
            Weight = weight;
            Range = range;
            MinDistance = Range = range;
        }


        public override void Execute(AgentBase agent)
        {
            if (!Enabled)
            {
                NearestAgent = null;
            }

            Boid locomotionAgent = agent as Boid;

            if (locomotionAgent == null)
                throw new Exception("Mouse-response behavior only works with locomotion agents");


            if (FindNearestAgent)
            {
                double distance = locomotionAgent.Position.DistanceTo(MouseLine.ClosestPoint(locomotionAgent.Position, false));
                if (distance < MinDistance)
                {
                    NearestAgent = locomotionAgent;
                    MinDistance = distance;
                }
                else
                {
                    locomotionAgent.IsBeingDragged = false;
                }
            }
            else
            {
                if (locomotionAgent == NearestAgent)
                {
                    locomotionAgent.IsFixed = false;
                    locomotionAgent.IsBeingDragged = true;

                }

            }
        }

        public void AddMoves(Point3d neighbour, Point3d node, CartesianAgent agent)
        {
            Vector3d vec = neighbour - node;
            vec.Unitize();
            Vector3d moveVec = vec * this.SteppingFactor;
            agent.Moves.Add(moveVec);
            double weight = 2; //make it parametric
            agent.Weights.Add(weight);
            /// <summary>
            /// print check 
            /// </summary>
            RhinoApp.WriteLine(moveVec + "moveVec");
            RhinoApp.WriteLine(agent.Moves + "columnAgent.Moves");
        }
    }
}






