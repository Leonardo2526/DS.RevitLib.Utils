using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class StepsPriority
    {

        public List<StepPoint> PriorityList { get; set; }

        public static int CurrentPriority {get; set;}

        /// <summary>
        /// >Set the priority step. 1 - priority by X, 2 - priority by Y, 3 - priority by Z.
        /// </summary>
        public List<StepPoint> GetPointsList(int priority)
        {           
            PriorityList = new List<StepPoint>();
            CurrentPriority = priority;

            if (priority == 1)
            {
                PriorityList.AddRange(StepPointsList.XPoints);
                AddRestPoints();
            }
            else if (priority == 2)
            {
                PriorityList.AddRange(StepPointsList.YPoints);
                AddRestPoints();
            }
            else if (priority == 3)
            {
                PriorityList.AddRange(StepPointsList.ZPoints);
                AddRestPoints();
            }
            else
            {
                AddRestPoints();
            }

            return PriorityList;
        }

        void AddRestPoints()
        {
            foreach (StepPoint stepPoint in StepPointsList.AllPoints)
            {
                if (!PriorityList.Contains(stepPoint))
                    PriorityList.Add(stepPoint);
            }
        }
    }
}
