using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Documents;
using UIBuildIt.Common;
using UIBuildIt.Common.Tasks;
using UIBuildIt.WebService.Models;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class ProjectForDocumentController : ProjectController
    {
        protected override ProjectData CreateData(Project source, int id, string parentType)
        {
            var documentType = (DocumentType)Enum.Parse(typeof(DocumentType), parentType);
            return (ProjectData) CreateData(source, id, (int)documentType, null);
        }

        protected override ItemDataBase<Project> CreateData(Project project, int id, int parentId, User user)
        {
            var documentType = (DocumentType)parentId;
            var data = new ProjectData(project, db, false);
            data.ParentType = documentType.ToString();
            AddChildren(documentType, project, data);
            // data.Properties.Add("Version", )
            return data;
        }

        private void AddChildren(DocumentType documentType, Project project, ProjectData data)
        {
            if (documentType==DocumentType.DetailedRequirements)
            {
                data.Trees["Notes"] = project.GetNotesForRequirementsDocument(db);
                data.Children.Add("Requirements", (from m in (from m in db.Requirements where m.ParentId == project.Id select m).ToList() select (ItemDataPrimitive)(new RequirementData(db, m, project, 3))).ToList());
                CreateRequirementsMatrix(data, data.Children["Requirements"]);
            }
            else if (documentType==DocumentType.HighLevelDesign)
            {
                data.Trees["Notes"] = project.GetNotesForDesignDocument(db);
                foreach (var connections in CreateProcesses(project).Children)
                {
                    data.Metadata.Add(connections.Key, connections.Value);
                }
                var useCases = (from s in
                                    (from u in db.UseCases
                                     join r in db.Requirements on u.ParentId equals r.Id
                                     where r.ParentId == project.Id
                                     select u).ToList().OrderBy(m => m.HashIndex())
                                select (ItemDataPrimitive)(new UseCaseData(db, s))).ToList();

                // data.Children.Add("Milestones", (from m in (from m in db.Milestones where m.ParentId == project.Id select m).ToList() select (ItemDataPrimitive)(new MilestoneData(db, m, 4))).ToList());
                data.Children.Add("UseCases", useCases);
                data.Children.Add("Modules", (from m in (from m in db.Modules where m.ParentId == project.Id select m).ToList().OrderBy(m => m.HashIndex()) select (ItemDataPrimitive)(new ModuleData(db, m, project, 2))).ToList());
            }
            else if (documentType==DocumentType.ProjectStatus)
            {
                data.Trees["Notes"] = project.GetNotesForTaskDocument(db);
                data.Trees["Tasks"] = GetTasks(project);
                AddEfforts(data);
            }
            else if (documentType == DocumentType.Gantt)
            {
                data.Properties["Gantt"] = CreateRows(project);
            }
        }

        private ICollection<GanttRow> CreateRows(Project project)
        {
            return GanttRow.GenerateRows(project, db);
        }

        #region RequirementMatrix

        public class RequirementMatrixData : ItemData<Requirement, Project>
        {
            public List<Task> RelatedTasks { get; set; }

            public RequirementMatrixData()
            {
                Type = "RequirementMatrix";
            }
        }

        private void CreateRequirementsMatrix(ProjectData data, ICollection<ItemDataPrimitive> requirements)
        {
            var tasks = (from t in db.Tasks.AsNoTracking()
                         where t.ProjectId == data.Entity.Id
                         select t).ToDictionary(t => t.Id, t => t);
            //var requirements = (   from r in db.Requirements.AsNoTracking()
            //                                where r.ParentId == data.Entity.Id
            //                                select r).ToDictionary(r => r.Id, r => (ItemDataPrimitive)(new RequirementMatrixData(){Entity = r, RelatedTasks = new List<Task>(), Parent=data.Entity}));

            foreach(var t in tasks.Values)
            {
                foreach(var requirementId in t.RequirementIds)
                {
                    var requirementData = requirements.FirstOrDefault(r => ((RequirementData)r).Entity.Id == requirementId);
                    if(requirementData != null)
                    {
                        if (!requirementData.Metadata.ContainsKey("Tasks"))
                        {
                            requirementData.Metadata["Tasks"] = new List<ItemDataPrimitive>();
                        }
                        requirementData.Metadata["Tasks"].Add(new TaskData() { Entity = t });
                    }
                }
            }
        }

        #endregion

        private void AddEfforts(ProjectData data)
        {
            Dictionary<string, int> riskHours = new Dictionary<string, int>();
            Dictionary<string, int> riskEstimatedHours = new Dictionary<string, int>();
            Dictionary<string, int> riskRemainingHours = new Dictionary<string, int>();
            var tasks = (from t in db.Tasks where t.ProjectId == data.Entity.Id select t).ToList();
            foreach (var task in tasks)
            {
                var riskString = task.Risk == RiskLevel.None ? "NoRisk" : task.Risk.ToString() + "Risk";
                if (!riskHours.ContainsKey(riskString))
                {
                     riskHours[riskString] = 0;
                }
                if (!riskEstimatedHours.ContainsKey(riskString))
                {
                    riskEstimatedHours[riskString] = 0;
                }
                if (!riskRemainingHours.ContainsKey(riskString))
                {
                    riskRemainingHours[riskString] = 0;
                }
                riskHours[riskString] += task.Effort;
                riskEstimatedHours[riskString] += task.EffortEstimation;
                riskRemainingHours[riskString] += Math.Max(0, task.EffortEstimation - task.Effort);
            }
            data.Properties["Efforts"] = new Dictionary<string, object>()
            {
                {"CommittedEffortsByRisk", riskHours},
                {"EstimatedEffortsByRisk", riskEstimatedHours},
                {"RemainingEffortsByRisk", riskRemainingHours}
            };
        }

        private ICollection<IItemTree> GetTasks(Project project)
        {
            var tasks = from t in
                            (from t in db.Tasks.AsNoTracking()
                             where t.ProjectId == project.Id && t.ParentType != "Task"
                             select t).ToList()
                        select t.GetTaskDataItemRoot(db);
                        
                        
                        
                        
                        // (ItemDataPrimitive)new TaskData(db, t, null, 1, true);
            return tasks.ToList();
        }

        private class Connection : ItemDataPrimitive
        {
            public Item Source { get; set; }
            public Item Target { get; set; }
            public CallType CallType { get; set; }
            public string CallTypeName
            {
                get
                {
                    return CallType.ToString();
                }

                set
                {
                    CallType val;
                    if(Enum.TryParse<CallType>(value, out val))
                    {
                        CallType = val;
                    }
                }
            }

            public bool Same { get { return String.Equals(Source.Name, Target.Name, StringComparison.InvariantCultureIgnoreCase); } }

            public string Key { get { return String.Format("{0}:{1}:{2}", Source.Name, CallTypeName, Target.Name); } }
        }

        private class InitiatorItem : Item
        {
            public override bool IsIndexed()
            {
                return false;
            }
        }

        private Connection CreateProcesses(Project project)
        {
            Connection result = new Connection();
            var processList = new List<string>();
            var componentList = CreateConnectionList(project, processList);
            var componentDictionary = new Dictionary<string, ItemDataPrimitive>();
            var moduleDictionary = new Dictionary<string, ItemDataPrimitive>();
            var processDictionary = new Dictionary<string, ItemDataPrimitive>();
            var moduleByProcessDictionary = new Dictionary<string, ItemDataPrimitive>();

            foreach (var connection in componentList)
            {
                if (((Connection)connection).Same)
                {
                    continue;
                }
                componentDictionary[((Connection)connection).Key] = connection;
                var targetModuleId = ((Connection)connection).Target.ParentId;
                var targetModule = db.Modules.AsNoTracking().FirstOrDefault(m => m.Id == targetModuleId);
                var sourceModuleId = ((Connection)connection).Source.ParentId;
                var sourceModule = db.Modules.AsNoTracking().FirstOrDefault(m => m.Id == sourceModuleId);
                var moduleConnection = new Connection()
                {
                    Target = targetModule,
                    Source = sourceModule == null ? ((Connection)connection).Source : sourceModule,
                    CallType = ((Connection)connection).CallType
                };
                if (((Connection)moduleConnection).Same)
                {
                    continue;
                }
                moduleDictionary[moduleConnection.Key] = moduleConnection;
                var processConnection = new Connection()
                {
                    Target = new InitiatorItem() { Name = String.IsNullOrEmpty(targetModule.Process) ? "External" : targetModule.Process },
                    Source = new InitiatorItem() 
                    {
                        Name = sourceModule == null ? ((Connection)connection).Source.Name : 
                                String.IsNullOrEmpty(sourceModule.Process) ? "External" : sourceModule.Process 
                    },
                    CallType = ((Connection)connection).CallType
                };
                if (!((Connection)processConnection).Same &&  
                    processList.Contains( ((Connection)processConnection).Target.Name) &&
                    processList.Contains( ((Connection)processConnection).Source.Name))
                {
                    processDictionary[processConnection.Key] = processConnection;
                }
            }

            result.Children["ModuleConnections"] = moduleDictionary.Values;
            result.Children["ComponentConnections"] = componentDictionary.Values;
            result.Children["ProcessConnections"] = processDictionary.Values;
            foreach(var module in moduleDictionary.Values)
            {
                var m = ((Connection)module).Source as Module;
                AddModuleProcess(moduleByProcessDictionary, m);
                m = ((Connection)module).Target as Module;
                AddModuleProcess(moduleByProcessDictionary, m);
            }
            result.Children["ProcessModules"] = moduleByProcessDictionary.Values;
            return result;
        }

        private static void AddModuleProcess(Dictionary<string, ItemDataPrimitive> moduleByProcessDictionary, Module m)
        {
            if (m != null && !String.IsNullOrEmpty(m.Process))
            {
                if (!moduleByProcessDictionary.ContainsKey(m.Process))
                {
                    moduleByProcessDictionary[m.Process] = new ItemDataPrimitive() { Name = m.Process };
                }
                moduleByProcessDictionary[m.Process].Children[m.Name] = null;
            }
        }

        private ICollection<ItemDataPrimitive> CreateConnectionList(Project project, IList<string> processes)
        {
            var sequences = ProjectUseCases(project);
            var componentConnections = new List<Connection>();
            var result = new List<ItemDataPrimitive>();
            for (int i = 0; i < sequences.Count; i++)
            {
                var methodIDs = sequences[i].MethodIds.ToList();
                var actorlist = new List<Component>();
                actorlist.Add(new Component() { Name = sequences[i].Initiator });
                int popcount = 2;
                for (int m = 0; m < methodIDs.Count; m++)
                {
                    var methodId = methodIDs[m];
                    if (methodId < 1)
                    {
                        actorlist.Add(actorlist[actorlist.Count - popcount]);
                        popcount++;
                        continue;
                    }
                    else
                    {
                        popcount = 2;
                    }
                    var method = db.Methods.AsNoTracking().FirstOrDefault(met => met.Id == methodId);
                    if (method != null)
                    {
                        var component = db.Components.AsNoTracking().FirstOrDefault(com => com.Id == method.ParentId);
                        actorlist.Add(component);
                        var module = component.ParentModule(db);
                        if (module != null && !String.IsNullOrEmpty(module.Process))
                        {
                            processes.Add(module.Process);
                        }
                    }
                    else
                    {
                        throw new ApplicationException(String.Format("Method {0} was deleted without being removed from a sequence {1}", methodId, sequences[i].Id ));
                    }
                }
                for (int m = 0; m < methodIDs.Count; m++)
                {
                    var methodId = methodIDs[m];
                    if (methodId < 1)
                    {
                        continue;
                    }
                    var c = new Connection();

                    var method = db.Methods.AsNoTracking().FirstOrDefault(met => met.Id == methodId);
                    var component = actorlist[m + 1];
                    c.Target = component;
                    c.CallType = method.CallType;
                    c.Source = actorlist[m];
                    result.Add(c);
                }
            }
            return result;
        }

        private IList<UseCase> ProjectUseCases(Project project)
        {
            return  (from t in db.Requirements.AsNoTracking()
                     join u in db.UseCases.AsNoTracking() on t.Id equals u.ParentId
                    where project.Id == t.ParentId
                    select u).ToList();
        }
  

        private UseCaseActionData PopulateAction(UseCaseAction action)
        {
            return new UseCaseActionData(db, action);
        }

        private TestData PopulateTest(Test test)
        {
            return new TestData(db, test);
        }
    }
}