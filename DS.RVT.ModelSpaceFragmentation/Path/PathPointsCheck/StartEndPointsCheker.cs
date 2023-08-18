using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class StartEndPointsCheker
    {

        readonly InputData data;
        public StartEndPointsCheker(InputData inputData)
        {
            data = inputData;
        }

        int Ax = InputData.Ax;
        int Bx = InputData.Bx;
        int Ay = InputData.Ay;
        int By = InputData.By;


        public bool IsStartCellEmpty()
        {
            PointsCheker pointsCheker = new PointsCheker(data);

            bool emptyCell = pointsCheker.IsCellEmpty(Ax, Ay);

            if (emptyCell == true)
                return true;

            //Try to move start point
            bool pointMoved = false;
            StartEndPointsMover startEndPointsMover = new StartEndPointsMover(pointsCheker);
            if (Math.Abs(Ax - Bx) >= Math.Abs(Ay - By))
            {
                //Get side for move
                if (Ay <= By)
                    pointMoved = startEndPointsMover.MovePointUp(Ax, ref Ay);
                else if (Ay > By)
                    pointMoved = startEndPointsMover.MovePointDown(Ax, ref Ay);
            }
            else
            {
                if (Ax < Bx)
                    pointMoved = startEndPointsMover.MovePointRight(ref Ax, Ay);
                else if (Ax > Bx)
                    pointMoved = startEndPointsMover.MovePointLeft(ref Ax, Ay);
            }

            //Check if moved
            if (pointMoved == false)
            {
                TaskDialog.Show("Revit", "Process aborted! \nStart point is busy. Try to move it to another location.");
                return false;
            }
            else
            {
                TaskDialog.Show("Revit", "Start point is busy but it have been moved successfully!");
                return true;
            }
        }

        public bool IsEndCellEmpty()
        {
            PointsCheker pointsCheker = new PointsCheker(data);

            bool emptyCell = pointsCheker.IsCellEmpty(Bx, By);

            if (emptyCell == true)
                return true;

            StartEndPointsMover startEndPointsMover = new StartEndPointsMover(pointsCheker);

            //Try to move end point
            bool pointMoved = startEndPointsMover.MoveEndPointToStart(Bx, Ay, ref By, ref Ay);

            if (Math.Abs(Ax - Bx) >= Math.Abs(Ay - By))
            {
                //Get side for move
                if (By <= Ay)
                    pointMoved = startEndPointsMover.MovePointUp(Bx, ref By);
                else if (By > Ay)
                    pointMoved = startEndPointsMover.MovePointDown(Bx, ref By);
            }
            else
            {
                if (Bx <= Ax)
                    pointMoved = startEndPointsMover.MovePointRight(ref Bx, By);
                else if (Bx > Ax)
                    pointMoved = startEndPointsMover.MovePointLeft(ref Bx, By);
            }

            //Check if moved
            if (pointMoved == false)
            {
                TaskDialog.Show("Revit", "Process aborted! \nStart point is busy. Try to move it to another location.");
                return false;
            }
            else
            {
                TaskDialog.Show("Revit", "Start point is busy but it have been moved successfully!");
                return true;
            }
        }
    }
}
