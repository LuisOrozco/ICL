using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Karamba;
using Karamba.Geometry;
using KarambaCommon;
using Karamba.Supports;
using Karamba.CrossSections;
using Karamba.Utilities;
using Karamba.Loads;
using Karamba.Models;
using Karamba.Results;
using Karamba.Nodes;
using Karamba.Elements;
using Rhino;
/// <summary>
/// do the FEM parrt like you used to
/// </summary>
namespace ICL.Core.StructuralModelling
{
    public class BeamFEM
    {
        ///public attributes
        public double divisionLength = 0.5;//in mm
        public List<Point3d> BeamLinePoints = new List<Point3d>();
        public List<Point3d> columnPositions = new List<Point3d>();
        public List<string> BeamLoadTypes = new List<string>();
        public Dictionary<int, Point3d> NodalDisplacement = new Dictionary<int, Point3d>();
        public string Material;

        public BeamFEM(List<Point3d> beamLinePoints, List<Point3d> columnPositions, List<string> beamLoadTypes, string material)
        {
            this.BeamLinePoints = beamLinePoints;
            this.columnPositions = columnPositions;
            this.BeamLoadTypes = beamLoadTypes;
            this.Material = material;
        }
        ///create Beam element 

        public Model ComputeFEM()
        {
            ///KarambaLine==========================================================================
            List<Line3> beamLineList = new List<Line3>();
            List<Line3> testBeamLineList = ComputeBeamLineSegments();
            Point3 pStart = new Point3(this.BeamLinePoints[0][0], this.BeamLinePoints[0][1], this.BeamLinePoints[0][2]);
            Point3 pEnd = new Point3(this.BeamLinePoints[1][0], this.BeamLinePoints[1][1], this.BeamLinePoints[1][2]);
            Line3 beamLine = new Line3(pStart, pEnd);
            beamLineList.Add(beamLine);

            double[] tParams = FindBeamCurveParameters();

            ///Material definition==================================================================
            Karamba.Materials.FemMaterial_Isotrop materials = new Karamba.Materials.FemMaterial_Isotrop(
            "family",
            "name",
            1155,
            69,
            57,
            0.0,
            23.5,
            -23.5,
            Karamba.Materials.FemMaterial.FlowHypothesis.mises,
            1e-4,
            null);

            ///CrossSection=========================================================================
            List<CroSec> croSecList = new List<CroSec>();
            CroSec_Trapezoid trapCroSec = new CroSec_Trapezoid(
            "Beam",
            "BeamCrossSection",
            null,
            null,
            materials,
            30,//has to be parameterised
            100,
            100);
            croSecList.Add(trapCroSec);
            //<summary> adding cross section definition for each line segment o fthe beam </summary>
            foreach (Line3 segment in testBeamLineList)
            {
                croSecList.Add(trapCroSec);
            }

            ///Build LineToBeam element=============================================================
            var k3d = new KarambaCommon.Toolkit();
            var logger = new MessageLogger();
            var nodes = new List<Point3>();
            List<BuilderBeam> elems = k3d.Part.LineToBeam(testBeamLineList, new List<string>() { "Be0" }, croSecList, logger, out nodes);

            ///Supports & Loads======================================================================
            List<List<bool>> supportConditions = CreateSupportCondition();
            List<int> TparamIndexOfColumnPos = ComputeTparamIndexOfColumnPos(tParams);
            List<Support> supports = new List<Support>();
            //Line l = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
            for (int i = 0; i < this.columnPositions.Count; i++)
            {
                Point3 nodePt = new Point3(this.columnPositions[i][0], this.columnPositions[i][1], this.columnPositions[i][2]);
                RhinoApp.WriteLine(nodePt + "nodePt");
                Support support = k3d.Support.Support(nodePt, supportConditions[i]);
                supports.Add(support);
            }

            ///Loads==========================================================================
            var gLoad = k3d.Load.GravityLoad(new Vector3(0, 0, -1));
            var testUdl = k3d.Load.ConstantForceLoad(Vector3.UnitZ * -1, 10);
            List<Load> loads = new List<Load>() { gLoad, testUdl };

            ///Model==========================================================================
            double mass;
            Point3 cog;
            bool flag;
            string info;
            Model model = k3d.Model.AssembleModel(
            elems,
            supports,
            loads,
            out info,
            out mass,
            out cog,
            out info,
            out flag);

            ///Analyse==========================================================================
            //List<double> max_disp;
            //List<double> out_g;
            //List<double> out_comp;
            //string message;
            //model = k3d.Algorithms.AnalyzeThI(model, out max_disp, out out_g, out out_comp, out message);

            /////NodalDisplacements
            //model = model.Clone();
            //model.cloneElements();
            //string lc_ind = "0";
            //List<int> ids = ComputeNodeIDs(tParams);
            //var trans = new List<List<Vector3>>();
            //var rotat = new List<List<Vector3>>();
            //NodalDisp.solve(model, lc_ind, ids, out trans, out rotat);

            return model;

        }
        /// <summary>
        /// Method for computing the line segments that make up a beam (affected by column positions)
        /// </summary>
        /// <returns> List<Line3> a list of karamba Line3 </Line3></returns>
        public List<Line3> ComputeBeamLineSegments()
        {
            /// <summary>
            /// sort all list of points
            /// </summary>
            List<Point3d> sortedColumnPositions = SortListOfPoint3d(this.columnPositions);
            List<Point3d> sortedBeamLinePoints = SortListOfPoint3d(this.BeamLinePoints);
            bool pointAtStart = false;
            bool pointAtEnd = false;

            /// <summary>
            /// check for columns at the start and end of the beam
            /// </summary>
            foreach (Point3d point in sortedColumnPositions)
            {
                if (sortedBeamLinePoints[0] == point)
                {
                    pointAtStart = true;
                }
                else if (sortedBeamLinePoints[1] == point)
                {
                    pointAtEnd = true;
                }
            }

            /// <summary>
            /// create list of beam line elements as a Karamba Line3 element
            /// </summary>
            List<Line3> lineSegments = new List<Line3>();
            if (pointAtStart == true && pointAtEnd == true)
            {
                List<List<Point3d>> pointGroups = slidingWindowIterator(sortedColumnPositions);
                List<Line3> lines = LinesFromNestedPointPairs(pointGroups);
                foreach (Line3 l in lines)
                {
                    lineSegments.Add(l);
                }
            }
            else if (pointAtStart == false && pointAtEnd == true)
            {
                List<Point3d> sortedColumnPosDup = new List<Point3d>(sortedColumnPositions);
                sortedColumnPosDup.Insert(0, sortedBeamLinePoints[0]);
                List<List<Point3d>> pointGroups = slidingWindowIterator(sortedColumnPosDup);

                List<Line3> lines = LinesFromNestedPointPairs(pointGroups);
                foreach (Line3 l in lines)
                {
                    lineSegments.Add(l);
                }
            }

            else if (pointAtStart == true && pointAtEnd == false)
            {
                List<Point3d> sortedColumnPosDup = new List<Point3d>(sortedColumnPositions);
                sortedColumnPosDup.Insert(sortedColumnPosDup.Count, sortedBeamLinePoints[1]);
                List<List<Point3d>> pointGroups = slidingWindowIterator(sortedColumnPosDup);

                List<Line3> lines = LinesFromNestedPointPairs(pointGroups);
                foreach (Line3 l in lines)
                {
                    lineSegments.Add(l);
                }
            }
            else if (pointAtStart == false && pointAtEnd == false)
            {
                List<Point3d> sortedColumnPosDup = new List<Point3d>(sortedColumnPositions);
                sortedColumnPosDup.Insert(0, sortedBeamLinePoints[0]);
                sortedColumnPosDup.Insert(sortedColumnPosDup.Count, sortedBeamLinePoints[1]);
                List<List<Point3d>> pointGroups = slidingWindowIterator(sortedColumnPosDup);

                List<Line3> lines = LinesFromNestedPointPairs(pointGroups);
                foreach (Line3 l in lines)
                {
                    lineSegments.Add(l);
                }
            }

            return lineSegments;

        }

        /// <summary>
        /// Method for creating a Karamba Line3 element form a nested list of point3d elements
        /// </summary>
        /// <param> List of List of Point3d elements: groups of points </param>
        /// <returns> List<Line3> a list of karamba Line3 </returns>
        public List<Line3> LinesFromNestedPointPairs(List<List<Point3d>> pointGroups)
        {
            List<Line3> lineSegments = new List<Line3>();
            foreach (var ptPairs in pointGroups)
            {
                Line3 line = new Line3(new Point3(ptPairs[0][0], ptPairs[0][1], ptPairs[0][2]), new Point3(ptPairs[1][0], ptPairs[1][1], ptPairs[1][2]));
                lineSegments.Add(line);
            }
            return lineSegments;
        }

        /// <summary>
        /// Method for sorting list of point3d elements in assending order
        /// </summary>
        /// <param> List of Point3d elements: groups of points </param>
        /// <returns> List<Point3d> </returns>
        public List<Point3d> SortListOfPoint3d(List<Point3d> pointsToSort)
        {
            List<Point3d> sortedPoints = new List<Point3d>();
            Point3d[] point3dArray = pointsToSort.ToArray();
            Array.Sort(point3dArray);
            foreach (Point3d pt in point3dArray)
            {
                sortedPoints.Add(pt);
            }
            return sortedPoints;
        }

        /// <summary>
        /// Method creating a sliding window iterator for list of point3d
        /// </summary>
        /// <param> List of Point3d elements: groups of points </param>
        /// <returns> List<List<Point3d>> </returns>
        public List<List<Point3d>> slidingWindowIterator(List<Point3d> pointsToIterate)
        {
            List<List<Point3d>> iterationGroups = new List<List<Point3d>>();

            for (int i = 0; i < pointsToIterate.Count - 1; i++)
            {
                List<Point3d> inputNest = new List<Point3d>();
                inputNest.Add(pointsToIterate[i]);
                inputNest.Add(pointsToIterate[i + 1]);
                iterationGroups.Add(inputNest);

            }
            return iterationGroups;
        }

        /// <summary>
        /// Method to compute node IDs
        /// </summary>
        /// <param> double[]: array of doubles </param>
        /// <returns> List<int> </int> </returns>
        public List<int> ComputeNodeIDs(double[] nodeParams)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < nodeParams.Length; i++)
            {
                ids.Add(i);
            }
            return ids;
        }

        /// <summary>
        /// Methos to compute the parameter of a Point3d on a Curve
        /// </summary>
        /// <param> double[]: array of double </param>
        /// <returns> List<int> </returns>
        public List<int> ComputeTparamIndexOfColumnPos(double[] beamTparamList)
        {
            List<double> columnPosParam = new List<double>();
            foreach (Point3d pt in this.columnPositions)
            {
                double t;
                Line l = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
                bool curveParam = l.ToNurbsCurve().ClosestPoint(pt, out t);
                if (curveParam == true)
                {
                    columnPosParam.Add(t);
                }
                else
                {
                    RhinoApp.WriteLine("Column not connected to beam");
                }
            }

            List<int> columnPosParamLocation = new List<int>();
            foreach (double ct in columnPosParam)
            {
                List<double> distances = new List<double>();
                foreach (double bt in beamTparamList)
                {
                    Line l = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
                    Point3d pt1 = l.PointAt(bt);
                    Point3d pt2 = l.PointAt(ct);
                    double dist = pt1.DistanceTo(pt2);
                    distances.Add(dist);
                }
                double minDistance = distances.Min();

                for (var i = 0; i < beamTparamList.Length; i++)
                {
                    Line l = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
                    Point3d pt1 = l.PointAt(beamTparamList[i]);
                    Point3d pt2 = l.PointAt(ct);
                    double dist = pt1.DistanceTo(pt2);
                    if (dist == minDistance)
                    {
                        columnPosParamLocation.Add(i);
                    }
                }
            }
            return columnPosParamLocation;
        }

        /// <summary>
        /// Method creating Kasramba support conditions
        /// </summary>
        /// <returns> List<List<bool>> </returns>
        public List<List<bool>> CreateSupportCondition()
        {
            List<List<bool>> conditions = new List<List<bool>>();
            for (int i = 0; i < this.columnPositions.Count; i++)
            {
                List<bool> cond = new List<bool>() { false, true, true, false, false, false };
                conditions.Add(cond);
            }
            return conditions;
        }

        /// <summary>
        /// Method to compute beamline divisions and their respective paramters
        /// </summary>
        /// <returns> double[]: an array of doubles containing beam curve paramters at node points </returns>
        public double[] FindBeamCurveParameters()
        {
            Line line = new Line(this.BeamLinePoints[0], this.BeamLinePoints[1]);
            double lineLength = line.Length;
            int nCount = Convert.ToInt32(Math.Round(lineLength / divisionLength));
            double[] tParams = line.ToNurbsCurve().DivideByCount(nCount, true);
            return tParams;
        }
        ///material definition
        ///load definition
        ///support definition
        ///support definition
        ///create model 
        ///return model
    }
}
