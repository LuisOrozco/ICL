using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICD.AbmFramework.Core.Agent;
using Rhino.Geometry;
using ICL.Core.AgentBehaviors;


namespace ICL.Core.Agent
{
    public class ICLcartesianAgent : ICLagentBase
    {
        /// <summary>
        /// The position of the agent.
        /// </summary>
        public Point3d Position;

        /// <summary>
        /// The start position of the agent.
        /// </summary>
        public Point3d StartPosition;

        /// <summary>
        /// The List of vectors that add to the displacement.
        /// </summary>
        public List<Vector3d> Moves = new List<Vector3d>();

        public List<double> Weights = new List<double>();

        /// <summary>
        /// Constructs a new instance of a Cartesian, i.e., position-based agent.
        /// </summary>
        /// <param> startPosition: rhino.geometry.Point3d </param>
        /// <param> behaviors: The list of ICLbehaviorBase</param>
        public ICLcartesianAgent(Point3d startPosition, List<ICLbehaviorBase> behaviors)
        {
            this.StartPosition = this.Position = startPosition;
            this.ICLbehaviors = behaviors;
        }

        /// <summary>
        /// Method for resetting the agent.
        /// </summary>
        public override void Reset()
        {
            this.Position = this.StartPosition;
            Moves.Clear();
            Weights.Clear();
        }

        /// <summary>
        /// Method for running code that should be pre-executed.
        /// </summary>
        public override void PreExecute()
        {
            Moves.Clear();
            Weights.Clear();
        }

        /// <summary>
        /// Method for updating the agent's state.
        /// </summary>
        public override void Execute()
        {
            foreach (ICLbehaviorBase behavior in this.ICLbehaviors)
            {
                behavior.Execute(this);
            }
        }

        /// <summary>
        /// Method for running code that should be post-executed.
        /// </summary>
        public override void PostExecute()
        {
            if (Moves.Count == 0) return;

            Vector3d totalWeightedMove = Vector3d.Zero;
            double totalWeight = 0.0;

            for (int i = 0; i < Moves.Count; ++i)
            {
                totalWeightedMove += Weights[i] * Moves[i];
                totalWeight += Weights[i];
            }

            if (totalWeight > 0.0)
                Position += totalWeightedMove / totalWeight;
        }

        /// <summary>
        /// Method for collecting the geometry that should be displayed.
        /// </summary>
        /// <returns>Returns a list containing each agent's position.</returns>
        public override List<object> GetDisplayGeometries()
        {
            return new List<object> { Position };
        }

    }
}
