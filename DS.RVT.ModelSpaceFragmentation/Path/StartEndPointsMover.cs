using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class StartEndPointsMover
    {
        PointsCheker startEndPointsCheker;

        public StartEndPointsMover(PointsCheker sEPointsCheker) 
        {
            startEndPointsCheker = sEPointsCheker;
        }

        public bool MovePointUp(int px, ref int py)
        {
            for (int y = py; y <= InputData.H; y++)
            {
                bool emptyCell = startEndPointsCheker.IsCellEmpty(px, y);
                if (emptyCell == true)
                {
                    py = y;
                    return true;
                }
            }
            return false;
        }

        public bool MovePointDown(int px, ref int py)
        {
            for (int y = py; y >= 0; y--)
            {
                bool emptyCell = startEndPointsCheker.IsCellEmpty(px, y);
                if (emptyCell == true)
                {
                    py = y;
                    return true;
                }
            }
            return false;
        }

        public bool MovePointRight(ref int px, int py)
        {
            for (int x = px; x <= InputData.Xcount; x++)
            {
                bool emptyCell = startEndPointsCheker.IsCellEmpty(x, py);
                if (emptyCell == true)
                {
                    px = x;
                    return true;
                }
            }
            return false;
        }

        public bool MovePointLeft(ref int px, int py)
        {
            for (int x = px; x >= 0; x--)
            {
                bool emptyCell = startEndPointsCheker.IsCellEmpty(x, py);
                if (emptyCell == true)
                {
                    px = x;
                    return true;
                }
            }
            return false;
        }

        public bool MoveEndPointToStart(int bx, int ay, ref int By, ref int Ay)
        {
            bool emptyCell = startEndPointsCheker.IsCellEmpty(bx, ay);

            if (emptyCell == true)
            {
                By = Ay;
                return true;
            }

            return false;
        }
    }
}
