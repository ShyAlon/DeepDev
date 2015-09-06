using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;
using UIBuildIt.WebService.Models;

namespace UIBuildIt.WebService.Controllers.API
{
    public class GanttRow
    {
        public GanttRow() { }

        public GanttRow(Task c)
        {
            this.id = (int)c.HashIndex();
            this.start_date = c.StartTime.ToString("dd-MM-yyyy");
            this.text = c.Name;
            this.duration = (int)(c.EndTime - c.StartTime).TotalHours / hoursPerDay;
            this.progress = c.EffortEstimation == 0 ? 0 : c.Effort / c.EffortEstimation;
        }

        public GanttRow(Sprint c)
        {
            this.id = (int)c.HashIndex();
            this.text = c.Name;
        }

        public string text { get; set; }

        public int id { get; set; }

        public string start_date {get;set;}

        public float progress {get;set;}

        public int duration {get;set;} // in hours?

        public int parent {get;set;}

        #region static members

        private static ICollection<GanttRow> _rows;

        internal static ICollection<GanttRow> GenerateRows(Project project, UIBuildItContext db)
        {
            _rows = new List<GanttRow>();
            var milestones = (from m in db.Milestones.AsNoTracking()
                             where m.ParentId == project.Id
                             select m).ToList();
            foreach(var m in milestones)
            {
                var row = new GanttRow() { id = (int)m.HashIndex(), parent = 0, progress = 0, duration = 0, text = m.Name };
                _rows.Add(row);

                var sprintSpans = AddSprintRows(m, db);
                var taskSpans = AddExecutionUnitTasks(m, db);
                var start = (sprintSpans.Item1 < taskSpans.Item1) ? sprintSpans.Item1: taskSpans.Item1;
                row.start_date = start.ToString("dd-MM-yyyy");
                var end = (sprintSpans.Item2 > taskSpans.Item2) ? sprintSpans.Item2 : taskSpans.Item2;
                row.duration = 0;// (int)(end - start).TotalHours / hoursPerDay;
            }


            return _rows;
        }


        private static Tuple<DateTime, DateTime> AddExecutionUnitTasks(Item i, UIBuildItContext db)
        {
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;
            var tasks = i is Milestone ?
                (from s in db.Tasks.AsNoTracking()
                 where s.ContainerType == ContainerType.Milestone && s.ContainerID == i.Id
                 select s).ToList()
                           : (from s in db.Tasks.AsNoTracking()
                              where s.ContainerType == ContainerType.Sprint && s.ContainerID == i.Id
                              select s).ToList();
            var topLevel = (from t in tasks where t.ParentType != "Task" select t).ToList();
            foreach (var s in topLevel)
            {
                var times = AddTaskTasks(s, tasks, db);
                if (times.Item1 > new DateTime(1950, 1, 1) && times.Item1 != DateTime.MaxValue && times.Item2 < new DateTime(2222, 1, 1) && times.Item2 != DateTime.MinValue)
                {
                    if (s.StartTime > new DateTime(1950, 1, 1) && s.EndTime < new DateTime(2222, 1, 1))
                    {
                        var row = new GanttRow(s);
                        row.parent = (int)i.HashIndex();
                        _rows.Add(row);
                        if (s.StartTime < start)
                        {
                            start = s.StartTime;
                        }
                        if (s.EndTime > end)
                        {
                            end = s.EndTime;
                        }
                        if (start > times.Item1)
                        {
                            start = times.Item1;
                        }
                        if (end < times.Item2)
                        {
                            end = times.Item2;
                        }
                    }
                }
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }

        private static Tuple<DateTime, DateTime> AddTaskTasks(Task s, List<Task> tasks, UIBuildItContext db)
        {
            var result = new List<string>();
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;
            var children = (from t in tasks where t.ParentType == "Task" && t.ParentId == s.Id select t).ToList();
            if (children.Count == 0)
            {
                return new Tuple<DateTime, DateTime>(s.StartTime, s.EndTime);
            }
            else
            {
                foreach (var c in children)
                {
                    if (c.StartTime > new DateTime(1950, 1, 1) && c.EndTime < new DateTime(2222, 1, 1))
                    {
                        var row = new GanttRow(c);
                        _rows.Add(row);
                        row.parent = (int)s.HashIndex();
                        if (start < c.StartTime)
                        {
                            start = c.StartTime;
                        }
                        if (end < c.EndTime)
                        {
                            end = c.EndTime;
                        }
                        var times = AddTaskTasks(c, tasks, db);
                        if (start > times.Item1)
                        {
                            start = times.Item1;
                        }
                        if (end < times.Item2)
                        {
                            end = times.Item2;
                        }
                    }
                }
            }
            
            return new Tuple<DateTime,DateTime>(start, end);
        }

        private static int hoursPerDay = 8;

        private static Tuple<DateTime, DateTime> AddSprintRows(Milestone m, UIBuildItContext db)
        {
            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;
            var sprints = (from s in db.Sprints.AsNoTracking()
                              where s.ParentId == m.Id
                              select s).ToList();
            foreach(var s in sprints)
            {
                var times = AddExecutionUnitTasks(s, db);
                if (times.Item1 > new DateTime(1950, 1, 1) && times.Item1 != DateTime.MaxValue && times.Item2 < new DateTime(2222, 1, 1) && times.Item2 != DateTime.MinValue)
                {
                    var row = new GanttRow(s);
                    _rows.Add(row);
                    row.start_date = times.Item1.ToString("dd-MM-yyyy");
                    row.duration = 0;// (int)(times.Item2 - times.Item1).TotalHours / hoursPerDay;
                    row.parent = (int)m.HashIndex();
                    if (start > times.Item1)
                    {
                        start = times.Item1;
                    }
                    if (end < times.Item2)
                    {
                        end = times.Item2;
                    }
                }
            }
            return new Tuple<DateTime, DateTime>(start, end);
        }



        #endregion

        
    }
}
