using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Documents;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;

namespace UIBuildIt.WebService.Controllers.API
{
    public class ProjectData : ItemData<Project, Project>
    {
        public ProjectData()
            : base()
        {
            SetEnums();
        }

        public ProjectData(Project project, UIBuildItContext db , bool setMetaData = true)
            : this()
        {
            Entity = project;
            if(setMetaData)
            {
                SetMetadata(db);
            }
        }

        public ProjectData(UIBuildItContext db, Project project, User currentUser, int id)
        {
            var user = project == null ? currentUser : db.Users.FirstOrDefault(u => u.Id == project.CreatorId);
            Creator = user == null ? "Unknown" : user.Name;
            user = project == null ? currentUser : db.Users.FirstOrDefault(u => u.Id == project.ModifierId);
            LastModifier = user == null ? "Unknown" : user.Name;
            var userRole = ProjectUserType.Owner;
            // The only case we can expect -1 for a new project without a parent
            if (project == null) // return dummy project
            {
                if (id == -1)
                {
                    project = APIControllerBase<Project, ProjectData>.CreateSeedProject(currentUser, db);
                }
            }

            if (project.Owner.Id != currentUser.Id)
            {
                var sharedUser = (from pu in db.ProjectUsers
                                  where pu.ProjectId == project.Id && pu.UserMail == currentUser.Email
                                  select pu).FirstOrDefault();

                if (sharedUser == null)
                {
                    throw new AuthenticationException(string.Format("Project {0} is not available for user {1}", project.Id, currentUser.Email));
                }
                userRole = sharedUser.UserType;
            }
            using (MiniProfiler.Current.Step(string.Format("Potential Users {0}: {1}", project.GetEntityFinalType(), project.Name)))
            {
                SetEnums();
                user = project.Owner;
                Entity = project;
                PotentialUsers = (from u in db.Users
                                  where u.Organization == user.Organization
                                  select u.Email).ToList();
            }
            using (MiniProfiler.Current.Step("Project Modules"))
            {
                Children.Add("Modules", (from m in (from m in db.Modules where m.ParentId == project.Id select m).ToList().OrderBy(m => m.Process) select (ItemDataPrimitive)(new ModuleData(db, m, project, 1))).ToList());
            }
            using (MiniProfiler.Current.Step("Project Users"))
            {
                Users = (from pu in db.ProjectUsers where pu.ProjectId == project.Id select pu).ToList();
            }
            using (MiniProfiler.Current.Step("Project Milestones"))
            {
                Children.Add("Milestones", GetMilestones(project, db));
            }
            using (MiniProfiler.Current.Step("Project Requirements"))
            {
                Children.Add("Requirements", GetRequirements(project, db));
            }
            
            UserRole = userRole;
            
            
            SetMetadata(db);
            ProjectId = project.Id;
        }

        private List<ItemDataPrimitive> GetMilestones(Project project, UIBuildItContext db)
        {
            var list = (from m in db.Milestones where m.ParentId == project.Id select m).ToList().OrderBy(m => m.HashIndex()).ToList();
            return list == null ? new List<ItemDataPrimitive>() : (from m in list select (ItemDataPrimitive)(new MilestoneData(db, project, m, 1))).ToList();
        }

        private List<ItemDataPrimitive> GetRequirements(Project project, UIBuildItContext db)
        {
            var list = (from m in db.Requirements where m.ParentId == project.Id select m).ToList().OrderBy(m => m.HashIndex()).ToList();
            return list == null ? new List<ItemDataPrimitive>() : (from r in list select (ItemDataPrimitive)(new RequirementData(db, r, project, 1))).ToList();
        }

        private void SetEnums()
        {
            DocumentTypes = Enum.GetNames(typeof(DocumentType));
            //UserTypes = from ProjectUserType d in Enum.GetValues(typeof(ProjectUserType))
            //             select new
            //             {
            //                 Id = d,
            //                 Name = Enum.GetName(typeof(ProjectUserType), d)
            //             };
        }
        public ICollection<string> DocumentTypes { get; set; }

        public int Effort { get; set; }

        // public IList<ModuleData> Modules { get; set; }

        public IList<ProjectUser> Users { get; set; }

        public ProjectUserType UserRole { get; set; }

        //public object UserTypes { get; set; }

        public List<string> PotentialUsers { get; set; }
    }

    public class ProjectUserData : ItemData<ProjectUser, Project>
    {

    }

    public class ProjectDocumentData : ItemData<Project, Project>
    {
        public ICollection<MilestoneData> Milestones { get; set; }

        public ICollection<DocumentType> DocumentTypes { get; set; }
    }
 

    public class MilestoneData : PredecessorItemData<Milestone, Project>
    {
        public MilestoneData() { }

        public MilestoneData(UIBuildItContext db, Project project, Milestone milestone, int generations = 2)
            : base(db, milestone)
        {
            Entity = milestone;
            ParentItem = project != null ? project : (from p in db.Projects.AsNoTracking() where p.Id == Entity.ParentId select p).FirstOrDefault();
            Metadata.Add("DirectTasks", (from r in (from r in db.Tasks.AsNoTracking() where r.ContainerID == Entity.Id && r.ContainerType == ContainerType.Milestone select r).ToList() select (ItemDataPrimitive)new TaskData(db, r, null, 0)).ToList().OrderBy(m => ((TaskData)m).Entity.HashIndex()).ToList());
            Metadata.Add("SprintTasks", GetSprintTasks(db).OrderBy(m => ((TaskData)m).Entity.HashIndex()).ToList());

            Children.Add("Sprints", (from s in (from s in db.Sprints.AsNoTracking() where s.ParentId == Entity.Id select s).ToList() select (ItemDataPrimitive)new SprintData(db, s, milestone, generations - 1)).ToList().OrderBy(m => ((SprintData)m).Entity.HashIndex()).ToList());
            Effort = GetEffort(db);
            if (generations > 1)
            {
                SetMetadata(db);
            }
            
            PotentialPredecessors = GetPotentialPredecessors(db);
        }

        private List<ItemDataPrimitive> GetSprintTasks(UIBuildItContext db)
        {
            return (from r in
                        (from r in db.Tasks.AsNoTracking()
                         join s in db.Sprints on r.ContainerID equals s.Id
                         where s.ParentId == Entity.Id && r.ContainerType == ContainerType.Sprint
                         select r).ToList()
                    select (ItemDataPrimitive)new TaskData(db, r, null, 0)).ToList();
        }

        public int GetEffort(UIBuildItContext db)
        {
            if (!Children.ContainsKey("Tasks"))
            {
                var efforts = (from t in db.Tasks
                               join r in db.Requirements on t.ParentId equals r.Id
                               join m in db.Milestones on r.ParentId equals m.Id
                               where m.Id == Entity.Id
                               select t.Effort).ToList();
                return efforts != null ? efforts.Sum() : 0;
            }
            else
            {
                Effort = 0;
                foreach (var t in Children["Tasks"])
                {
                    Effort += ((TaskData)t).Entity.Effort;
                }
            }
            return Effort;
        }

        public int Effort { get; set; }
    }


    public class SprintData : PredecessorItemData<Sprint, Milestone>
    {
        public SprintData() { }

        public SprintData(UIBuildItContext db, Sprint sprint, Milestone milestone = null, int generations = 2)
            : base(db, sprint)
        {
            Entity = sprint;
            ParentItem = milestone != null ? milestone : (from m in db.Milestones.AsNoTracking() where m.Id == Entity.ParentId select m).FirstOrDefault();
            Metadata.Add("Tasks", (from r in (from r in db.Tasks.AsNoTracking() where r.ContainerID == Entity.Id && r.ContainerType == ContainerType.Sprint select r).ToList() select (ItemDataPrimitive)new TaskData(db, r, null, 0)).ToList().OrderBy(m => ((TaskData)m).Entity.HashIndex()).ToList());
            PotentialPredecessors = GetPotentialPredecessors(db);
        }
    }

    public class IssueData : ItemData<Issue, Task>
    {
        public IssueData() : base() { }
        public IssueData(UIBuildItContext db, Issue s, Task task = null, int generations = 2)
            : base(db, s)
        {
            Entity = s;
            ParentItem = task != null ? task : db.Tasks.FirstOrDefault(t => t.Id == s.ParentId);
            if(generations > 0)
            {
                var testsBrr = new List<Test>();
                foreach (var requirementId in ParentItem.RequirementIds)
                {
                    var tests = (from t in db.Tests
                                 join u in db.UseCases on t.ParentId equals u.Id
                                 join r in db.Requirements on u.ParentId equals r.Id
                                 where r.Id == requirementId
                                 select t).ToList();
                    if(tests != null)
                    {
                        testsBrr.AddRange(tests);
                    }
                }
                Tests = new HashSet<Test>(testsBrr);
            }
        }

        public string[] IssueStatuses
        {
            get
            {
                return Enum.GetNames(typeof(IssueStatus));
            }
        }

        public ICollection<Test> Tests { get; set; }
    }

    public class ModuleData : ItemData<Module, Project>
    {
        public ModuleData() { }

        public ModuleData(UIBuildItContext db, Module m, Project project = null, int generations = 2)
            : base(db, m)
        {
            Entity = m;
            ParentItem = project != null ? project : (from p in db.Projects.AsNoTracking() where p.Id == m.ParentId select p).FirstOrDefault();
            if (generations > 0)
            {
                Children["Components"] = (from c in (from c in db.Components.AsNoTracking() where c.ParentId == m.Id && c.ParentType != "Component" select c).ToList() select (ItemDataPrimitive)new ComponentData(db, c, Entity, generations - 1)).ToList().OrderBy(mm => ((ComponentData)mm).Entity.HashIndex()).ToList();
            }
            if(generations > 1)
            {
                //MissingMethods = Entity.GetMissingMethods(db);
                SetMetadata(db);
            }
            Processes = (from p in (from mo in db.Modules.AsNoTracking() where mo.ParentId == ParentItem.Id select mo.Process).ToList() where !String.IsNullOrEmpty(p) select p).Distinct();
        }

        public IEnumerable<string> Processes { get; set; }
    }

    public class ComponentData : ItemData<Component, Item>
    {
        public ComponentData() { }
        public ComponentData(UIBuildItContext db, Component c, Item parent = null, int generations = 2)
            : base(db, c)
        {
            Entity = c;
            // Bug correction
            if(String.IsNullOrEmpty( Entity.ParentType))
            {
                Entity.ParentType = "Module";
            }
            ParentItem = parent != null ? parent :
                c.ParentType == "Component" ?   (Item)(from m in db.Components where m.Id == c.ParentId select m).FirstOrDefault() :
                                                (Item)(from m in db.Modules where m.Id == c.ParentId select m).FirstOrDefault();
            Children["Methods"] = (from m in (from m in db.Methods.AsNoTracking() where c.Id == m.ParentId select m).ToList() select (ItemDataPrimitive) new MethodData(db, m, Entity, generations - 1)).ToList();
            if (generations > 0)
            {
                Children["Components"] = (from m in (from m in db.Components.AsNoTracking() where c.Id == m.ParentId && m.ParentType == "Component" select m).ToList() select (ItemDataPrimitive)new ComponentData(db, m, Entity, 0)).ToList().OrderBy(m => ((ComponentData)m).Entity.HashIndex()).ToList();
            }
            if(generations > 1)
            {
                var module = c.ParentModule(db);
                //MissingMethods = module.GetMissingMethods(db);
                SetMetadata(db);
            }
            ParentType = Entity.ParentType;
        }

        public string[] RiskLevels
        {
            get
            {
                return Enum.GetNames(typeof(RiskLevel));
            }
        }

        public string[] RiskStatuses
        {
            get
            {
                return Enum.GetNames(typeof(RiskStatus));
            }
        }

        public string[] ReturnTypes
        {
            get
            {
                return Enum.GetNames(typeof(ReturnType));
            }
        }

        public string[] CallTypes
        {
            get
            {
                return Enum.GetNames(typeof(CallType));
            }
        }
    }

    public class MethodData : ItemData<Method, Component>
    {
        public MethodData() { }

        public MethodData(UIBuildItContext db, Method m, Component component = null, int generations = 2)
            : base(db, m)
        {
            Entity = m;
            ParentItem = component != null ? component : (from c in db.Components.AsNoTracking() where c.Id == m.ParentId select c).FirstOrDefault();
            var module = (from o in db.Modules.AsNoTracking() where o.Id == ParentItem.ParentId select o).FirstOrDefault();
            Module = module.Name;
            if(generations > 1)
            {
                SetMetadata(db);
            }
        }

        public string Module { get; set; }
    }

    public class RequirementData : PredecessorItemData<Requirement, Item>
    {
        public RequirementData() { }

        public RequirementData(UIBuildItContext db, Requirement r, Item parent = null, int generations = 2)
            : base(db, r)
        {
            Entity = r;
            if (String.IsNullOrEmpty(Entity.ParentType))
            {
                Entity.ParentType = "Project";
            }
            ParentItem = parent != null ? parent :
                Entity.ParentType == "Requirement" ? (Item)(from m in db.Requirements where m.Id == Entity.ParentId select m).FirstOrDefault() :
                                                (Item)(from m in db.Projects where m.Id == Entity.ParentId select m).FirstOrDefault();
            if(generations > 0)
            {
                Children["Requirements"] = (from m in (from m in db.Requirements.AsNoTracking() where Entity.Id == m.ParentId && m.ParentType == "Requirement" select m).ToList() select (ItemDataPrimitive)new RequirementData(db, m, Entity, 0)).ToList().OrderBy(m => ((RequirementData)m).Entity.HashIndex()).ToList();
                Children["UseCases"] = (from u in (from u in db.UseCases.AsNoTracking() where u.ParentId == r.Id select u).ToList() select (ItemDataPrimitive)new UseCaseData(db, u, r, generations - 1)).ToList().OrderBy(m => ((UseCaseData)m).Entity.HashIndex()).ToList();
                // Tasks = (from t in (from t in db.Tasks.AsNoTracking() where t.ParentId == r.Id select t).ToList() select new TaskData(db, t, Parent, generations - 1)).ToList();
                ContainerTypes = Enum.GetNames(typeof(ContainerType));
                PotentialPredecessors = GetPotentialPredecessors(db);
            }
            if (generations > 1)
            {
                SetMetadata(db);
            }
        }

        protected override ICollection<ItemData<Requirement, Item>> GetPotentialPredecessors(UIBuildItContext db)
        {
            var result = new List<ItemData<Requirement, Item>>();
            var sibs = (from s in db.Requirements.AsNoTracking()
                       where s.ProjectId == Entity.ProjectId
                       select s).ToList();
            if (sibs != null && sibs.Count() > 0)
            {
                var siblings = from s in sibs
                               where !s.IsAncestor(Entity, db)
                               select new ItemData<Requirement, Item>() { Entity = s, ParentItem = ParentItem };
                AddSiblings(db, result, siblings);
            }

            return result;
        }


        public ICollection<string> ContainerTypes { get; set; }
    }

    public class UseCaseData : PredecessorItemData<UseCase, Requirement>
    {
        public UseCaseData() { }

        public UseCaseData(UIBuildItContext db, UseCase u, Requirement requirement = null, int generations = 2) 
            : base(db, u)
        {
            ParentItem = requirement != null ? requirement : db.Requirements.AsNoTracking().FirstOrDefault(p => p.Id == u.ParentId);
            Project = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == u.ProjectId);
            // Deal with ProjectId bug
            u = FixProjectId(db, u);
            Entity = u;
            int topRisk = 0;
            if (generations > 0)
            {
                Methods = new List<MethodData>();
                int markercounter = 0;
                foreach (int methodId in u.MethodIds)
                {
                    if (methodId < 0)
                    {
                        markercounter -= 2;
                        if (methodId % 2 == -1) // a return marker
                        {
                            Methods.Add(new MethodData() { Entity = new Method { Id = markercounter - 1, Name = "Return Marker" } });
                        }
                        else if (methodId % 2 == 0) // a return marker
                        {
                            Methods.Add(new MethodData() { Entity = new Method { Id = markercounter - 2, Name = "No Return Marker" } });
                        }
                    }
                    else
                    {
                        var method = db.Methods.AsNoTracking().FirstOrDefault(m => m.Id == methodId);
                        if (method != null)
                        {
                            var methodData = new MethodData(db, method, null, generations - 1);
                            Methods.Add(methodData);
                            topRisk = Math.Max(topRisk, (int)methodData.Entity.Status);
                        }
                    }
                }
                u.MethodIds = (from m in Methods select m.Entity.Id).ToList();
                // Include methods
                Modules = (from m in
                               (from m in db.Modules.AsNoTracking()
                                where m.ParentId == Project.Id
                                select m).ToList()
                           select new ModuleData(db, m, Project, 1)).ToList();
            }
            else
            {
                foreach (int methodId in u.MethodIds)
                {
                    if (methodId > -1)
                    {
                        var method = db.Methods.AsNoTracking().FirstOrDefault(m => m.Id == methodId);
                        if (method != null)
                        {
                            topRisk = Math.Max(topRisk, (int)method.Status);
                        }
                    }
                }
            }
            if (generations > 1)
            {
                SetMetadata(db);
            }

            TopRisk = ((RiskStatus)topRisk).ToString();
        }

        private UseCase FixProjectId(UIBuildItContext db, UseCase u)
        {
            if (Project == null)
            {
                Project = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == ParentItem.ProjectId);
                if (Project != null)
                {
                    u = SetProjectId(db, u, Project.Id);
                }
            }
            return u;
        }

        
        public string TopRisk { get; set; }

        public ICollection<MethodData> Methods { get; set; }

        public ICollection<ModuleData> Modules { get; set; }

        public Project Project { get; set; }

        public string[] ExecutionStatuses
        {
            get
            {
                return Enum.GetNames(typeof(ExecutionStatus));
            }
        }

        protected override UseCase CreateDuplicateEntity(User user, int parentId, UIBuildItContext db)
        {
            var entity = base.CreateDuplicateEntity(user, parentId, db);
            // Use the strings to create a new collection
            entity.MethodIdsString = Entity.MethodIdsString;
            // entity.Initiator = Entity.Initiator;
            entity.ExecutionStatus = Entity.ExecutionStatus;
            return entity;
        }

        public override ItemDataPrimitive Duplicate(UIBuildItContext db, User user, ItemDataPrimitive scaffold, int generations = 0, int parentId = -1)
        {
            var m = CreateDuplicateEntity(user, parentId, db);
            if (scaffold == null)
            {
                scaffold = new UseCaseData();
            }
            var data = (UseCaseData)scaffold;

            try
            {
                db.UseCases.Add(m);
                db.SaveChanges();

                //if (generations > 0)
                //{
                //    data.Children["Tests"] = new List<ItemDataPrimitive>();
                //    foreach (var child in Children["Tests"])
                //    {
                //        var newTest = child.Duplicate(db, user, null, generations - 1, m.Id);
                //        data.Children["Tests"].Add(newTest);
                //    }
                //}

                data.Entity = m;
                data.ParentItem = ParentItem;
                data.CanEdit = CanEdit;
                return data;

            }
            catch (DbEntityValidationException dbex)
            {
                Console.WriteLine(dbex);
                throw;
            }
        }
    }

    public class TaskData : PredecessorItemData<Task, Item>
    {
        public TaskData() { }

        // TODO: This need serious optimization
        public TaskData(UIBuildItContext db, Task t, Item parent = null, int generations = 2, bool document = false)
            : base(db, t)
        {
            Entity = t;
            if(parent != null)
            { 
                ParentItem = parent;
            }
            else
            {
                SetParent(db, t.ParentType, this);
            }

            // Get all project tasks ONCE

            // Tasks dictionary
            
            if (t.ParentId > 0)
            {
                if (generations > 0)
                {
                    var tasks = (from task in db.Tasks.AsNoTracking()
                                 where task.ProjectId == Entity.ProjectId    //GetPotentialPredecessors(db);
                                 select task).ToDictionary(t2 => t2.Id, t2 => t2);

                    PotentialPredecessors = (from task in tasks.Values
                                             select (ItemData<Task, Item>)new TaskData() { Entity = task }).ToList();
                    RemoveSubTasks(tasks);
                    ICollection<Task> siblings = GetSiblings(db);
                    var issues = (from s in
                                      (from s in db.Issues.AsNoTracking()
                                       where s.ParentId == Entity.Id
                                       select s).ToList()
                                  select (ItemDataPrimitive)new IssueData(db, s, Entity, generations - 1)).ToList();
                    if (document)
                    {
                        Children["Issues"] = new List<ItemDataPrimitive>();
                        foreach (var issue in issues)
                        {
                            var i = (IssueData)issue;
                            if (!Children.ContainsKey(i.Entity.Status + "Issues"))
                            {
                                Children[i.Entity.Status + "Issues"] = new List<ItemDataPrimitive>();
                            }
                            Children[i.Entity.Status + "Issues"].Add(i);
                        }
                    }
                    else
                    {
                        Children["Issues"] = issues;

                    }
                    var issueEffort = (from i in issues
                                       select ((IssueData)i).Entity.Effort).Sum();
                    TotalEffort = Entity.Effort + issueEffort;
                    var issueEstimatedEffort = (from i in issues
                                       select ((IssueData)i).Entity.EffortEstimation).Sum();
                    TotalEffortEstimation = Entity.EffortEstimation + issueEstimatedEffort;

                    var subTasks = GetSubTasks(tasks);
                    foreach(var subTask in subTasks)
                    {
                        TotalEffort += subTask.Effort;
                        TotalEffortEstimation += subTask.EffortEstimation;
                    }

                    if (generations > 1)
                    {
                        Owners = new HashSet<string>();
                        if (siblings.Any())
                        {
                            foreach (var s in siblings)
                            {
                                if (!String.IsNullOrEmpty(s.Owner))
                                {
                                    Owners.Add(s.Owner);
                                }
                            }
                        }
                        
                        AvailableRequirements = (from r in db.Requirements.AsNoTracking() 
                                                 where r.ParentId == t.ProjectId select r).ToList();
                        RiskTypes = Enum.GetNames(typeof(RiskLevel)).ToList();
                        IssueStatuses = Enum.GetNames(typeof(IssueStatus));
                        RiskStatuses = Enum.GetNames(typeof(RiskStatus));
                    }

                    if (document)
                    {
                        SetChildRequirements(db);
                    }

                    if (t.ContainerID > 0)
                    {
                        if (t.ContainerType == ContainerType.Milestone)
                        {
                            Container = new MilestoneData(db, null, db.Milestones.FirstOrDefault(m => m.Id == t.ContainerID), 0);
                        }
                        else if (t.ContainerType == ContainerType.Sprint)
                        {
                            Container = new SprintData(db, db.Sprints.FirstOrDefault(m => m.Id == t.ContainerID), null, 0);
                        }
                    }
                    

                    Milestones = (from m in db.Milestones
                                  where m.ParentId == Entity.ProjectId
                                  select new TreeNode() { Id = m.Id, Name = m.Name }).ToList();

                    foreach (TreeNode m in Milestones)
                    {
                        m.Children = (from s in db.Sprints
                                      where s.ParentId == m.Id
                                      select new TreeNode() { Id = s.Id, Name = s.Name }).ToList();
                    }
                }
                if (generations > 1)
                {
                    SetMetadata(db);
                }
            }
            ParentType = t.ParentType;
        }

        private void RemoveSubTasks(Dictionary<int, Task> tasks)
        {
            var rootId = GetRootId(Entity, tasks);
            var filtered = (from task in PotentialPredecessors
                            where !ShareRoot(task.Entity, rootId, tasks)
                            select task).ToList();
            PotentialPredecessors = filtered;
        }

        private IList<Task> GetSubTasks(Dictionary<int, Task> tasks)
        {
            var rootId = GetRootId(Entity, tasks);
            var subTasks = (from task in tasks.Values
                            where IsDescendant(task, (Task)Entity, tasks)
                            select task).ToList();
            return subTasks;
        }

        private bool ShareRoot(Task task, int root, Dictionary<int, Task> tasks)
        {
            int rootId1 = GetRootId(task, tasks);
            return rootId1 == root;
        }

        private int GetRootId(Task task, Dictionary<int, Task> tasks)
        {
            if (task.ParentType.IndexOf("task", StringComparison.InvariantCultureIgnoreCase) < 0 || !tasks.ContainsKey(task.ParentId))
            {
                return task.Id; // this is the root
            }
            var directParent = (Task)tasks[task.ParentId];
            return GetRootId(directParent, tasks);
        }

        /// <summary>
        /// checks whether the task is a sub task of the parent
        /// </summary>
        /// <param name="task">The task to test</param>
        /// <param name="parent">The master task</param>
        /// <returns></returns>
        private bool IsDescendant(Task task, Task parent, UIBuildItContext db)
        {
           // GetTaskParent(task, db);
            if(task.ParentType.IndexOf("task", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                return false; // The parent is not even a task
            }
            if(task.ParentId == parent.Id)
            {
                return true; // It is the parent
            }
            var directParent = (Task)GetTaskParent(task, db);
            return IsDescendant(directParent, parent, db);
        }

        private bool IsDescendant(Task task, Task parent, Dictionary<int, Task> tasks)
        {
            if (task.ParentId == parent.Id || task.ParentType.IndexOf("task", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                return false; // The parent is not even a task
            }
            if (task.ParentId == parent.Id)
            {
                return true; // It is the parent
            }
            var directParent = (Task)tasks[task.ParentId];
            return IsDescendant(directParent, parent, tasks);
        }



        private void SetChildRequirements(UIBuildItContext db)
        {
            Children["Related Requirements"] = (from r in
                                      (from r in db.Requirements.AsNoTracking()
                                       where Entity.RequirementIds.Contains(r.Id)
                                       select r).ToList()
                                  select (ItemDataPrimitive)new RequirementData(db, r, null, 0)).ToList();
        }

        private ICollection<Task> GetSiblings(UIBuildItContext db)
        {
            // var project = GetProject(Parent, db);
            var result = (from task in db.Tasks.AsNoTracking()
                          where task.ProjectId == ParentItem.ProjectId
                          select task).ToList();
            return result;
        }

        public ICollection<string> Owners { get; set; }

        public ICollection<Requirement> AvailableRequirements { get; set; }

        public ICollection<string> RiskTypes { get; set; }

        internal static void SetParent(UIBuildItContext db, string parentType, TaskData data)
        {
            Item parent = data.Entity.GetParent(db);
            if(parent == null)
            {
                //todo: return the exception
                return;
                throw new ArgumentException("No parent for " + data.Entity.Name);
            }
            data.Entity.ParentType = parent.GetType().Name;
            data.Entity.ParentId = parent.Id;
            data.ParentItem = parent;
        }

        public static Item GetTaskParent(Task source, UIBuildItContext db)
        {
            var data = new TaskData() { Entity = source };
            SetParent(db, source.ParentType, data);
            return data.ParentItem;
        }

        public string[] IssueStatuses { get; set; }

        public string[] RiskStatuses { get; set; }

        public int TotalEffort { get; set; }

        public int TotalEffortEstimation { get; set; }

        public ItemDataPrimitive Container { get; set; }


        public List<TreeNode> Milestones { get; set; }
    }

    public class TestData : PredecessorItemData<Test, UseCase>
    {
        public TestData() : base() { }
        public TestData(UIBuildItContext db, Test t, UseCase u = null, int generations = 2)
            : base(db, t)
        {
            Entity = t;
            ParentItem = u != null ? u : db.UseCases.First(uc => uc.Id == t.ParentId);
            var requirment = db.Requirements.First(r => r.Id == ParentItem.ParentId);
            if(generations > 0)
            {
                var actions = (from us in db.UseCases
                               join ac in db.Actions on us.Id equals ac.ParentId
                               where us.Id == ParentItem.Id
                               select ac).ToList();
                var parameters = (from ac in actions
                                  join pa in db.Parameters.AsNoTracking() on ac.Id equals pa.ParentId
                                  select pa).ToList();
                Children["TestParameters"] = (from s in
                                                  (from s in db.TestParameters.AsNoTracking()
                                                   where s.ParentId == Entity.Id
                                                   select s).ToList()
                                              select (ItemDataPrimitive)new TestParameterData() { Entity = s, ParentItem = t, Parameter = db.Parameters.AsNoTracking().FirstOrDefault(p => p.Id == s.ParameterId) }).ToList();
                PotentialParameters = (from s in
                                           (from par in parameters
                                            where Children["TestParameters"].FirstOrDefault(tpar => ((TestParameterData)tpar).Entity.ParameterId == par.Id) == null
                                            select new TestParameter() { ParameterId = par.Id, Name = par.Name, ParentId = t.Id }).ToList()
                                       select (ItemDataPrimitive)new TestParameterData() { Entity = s, ParentItem = t, Parameter = db.Parameters.AsNoTracking().FirstOrDefault(p => p.Id == s.ParameterId) }).ToList();
                Entity = t;
                PotentialPredecessors = GetPotentialPredecessors(db);
            }
            if (generations > 1)
            {
                SetMetadata(db);
            }
        }
        public ICollection<ItemDataPrimitive> PotentialParameters { get; set; }

        protected override Test CreateDuplicateEntity(User user, int parentId, UIBuildItContext db)
        {
            var entity = base.CreateDuplicateEntity(user, parentId, db);
            entity.PredecessorIds = Entity.PredecessorIds;
            entity.Result = Entity.Result;
            return entity;
        }

    }

    public class UseCaseActionData : PredecessorItemData<UseCaseAction, UseCase>
    {
        public UseCaseActionData() : base() { }
        public UseCaseActionData(UIBuildItContext db, UseCaseAction u, UseCase uc = null, int generations = 2)
            : base(db, u)
        {
            Entity = u;
            ParentItem = uc != null ? uc : db.UseCases.First(uca => uca.Id == u.ParentId);
            var requirment = db.Requirements.First(r => r.Id == ParentItem.ParentId);
            Milestone = db.Milestones.First(m => m.Id == requirment.ParentId);
            if (generations > 0)
            {
                var actions = (from re in db.Requirements
                               join us in db.UseCases on re.Id equals us.ParentId
                               join ac in db.Actions on us.Id equals ac.ParentId
                               where re.Id == requirment.Id
                               select ac).ToList();

                Children["Parameters"] = (from pa in
                                             (from pa in db.Parameters
                                              join ac in db.Actions on pa.ParentId equals ac.Id
                                              join us in db.UseCases on ac.ParentId equals us.Id
                                              where us.Id == ParentItem.Id
                                              select pa).ToList()
                                                select (ItemDataPrimitive) new ParameterData(){ Entity = pa, ParentItem = Entity}).ToList();
                var parameterNames = new HashSet<string>();
                var parameterTypes = GetTypes();

                foreach (var par in Children["Parameters"])
                {
                    if (!string.IsNullOrEmpty(((ParameterData)par).Entity.Name))
                    {
                        parameterNames.Add(((ParameterData)par).Entity.Name);
                    }
                    if (!string.IsNullOrEmpty(((ParameterData)par).Type))
                    {
                        parameterTypes.Add(((ParameterData)par).Type);
                    }
                }

                Children["Parameters"] = (from pa in(from pa in db.Parameters
                                          where pa.ParentId == Entity.Id
                                          select pa ).ToList() select (ItemDataPrimitive) new ParameterData(){ Entity = pa, ParentItem = Entity}).ToList();

                Modules = new HashSet<string>((from m in db.Modules.AsNoTracking() where m.ParentId == Milestone.ParentId select m.Name).ToList());
                Verbs = new HashSet<string>((from me in db.Methods.AsNoTracking()
                                             join co in db.Components.AsNoTracking() on me.ParentId equals co.Id
                                             join mo in db.Modules.AsNoTracking() on co.ParentId equals mo.Id
                                             where mo.ParentId == Milestone.ParentId
                                             select me.Name).ToList());
                ParameterNames = parameterNames;
                ParameterTypes = parameterTypes;

                PotentialPredecessors = GetPotentialPredecessors(db);
            }
        }

        protected HashSet<string> GetTypes()
        {
            return new HashSet<string>()
            {
                "int", "float", "text", "date", "time"
            };
        }

        public ICollection<string> Modules { get; set; }
        public ICollection<string> Verbs { get; set; }

        public Milestone Milestone { get; set; }

        public ICollection<string> ParameterNames { get; set; }

        public ICollection<string> ParameterTypes { get; set; }

        protected override UseCaseAction CreateDuplicateEntity(User user, int parentId, UIBuildItContext db)
        {
            var result = base.CreateDuplicateEntity(user, parentId, db);
            // Name and description should be identical
            result.Name = Entity.Name;
            result.Object = Entity.Object;
            result.Subject = Entity.Subject;
            result.Verb = Entity.Verb;
            return result;
        }
    }

    public class UserData : ItemData<User, Item>
    {
        public UserData() { }

        public UserData(UIBuildItContext db, User entity) : base(db, entity)
        {
            Entity = new User() 
            {
                Name = entity.Name,
                Id = entity.Id,
                Email = entity.Email,
                Organization = entity.Organization,
                 Created = entity.Created,
                 Modified = entity.Modified,
                 Password = "wouldn't you like to know"
            };


            var tasks = (from t in
                                     (from t in db.Tasks
                                      where t.Owner == Entity.Email
                                      orderby t.Priority descending
                                      select t).ToList()
                                 select (ItemDataPrimitive)new TaskData(db, t, null, 0) 
                                 {
                                     Container = t.ContainerID < 1 ? null : t.ContainerType == ContainerType.Milestone ? 
                                            (ItemDataPrimitive)new MilestoneData(db, null, db.Milestones.FirstOrDefault(m => m.Id == t.ContainerID), 0) : 
                                            t.ContainerType == ContainerType.Sprint ?
                                            (ItemDataPrimitive)new SprintData(db, db.Sprints.FirstOrDefault(m => m.Id == t.ContainerID), null, 0) : null
                                 }).ToList();
            var groups = from t in tasks
                         group t by ((ItemDataBase<Task>)t).Entity.Status into g
                         select g;

            foreach(var g in groups)
            {
                Metadata[g.Key + " Tasks"] = g.ToList();
            }

            // Metadata["Tasks"] = groups;

            Metadata["Tags"] = GetAllTags(db);
        }

    }

    public class ParameterData : ItemData<Parameter, UseCaseAction>
    {
        protected override Parameter CreateDuplicateEntity(User user, int parentId, UIBuildItContext db)
        {
            var result =  base.CreateDuplicateEntity(user, parentId, db);
            // Name and description should be identical
            result.Name = Entity.Name;
            result.Type = Entity.Type;
            return result;
        }
    }

    public class NoteData : ItemData<Note, Item>
    {
        public NoteData() { }
        public NoteData(UIBuildItContext db, Note t, Item parent = null, int generations = 2, bool document = false)
            : base(db, t)
        {

            Entity = t;
            if (parent != null)
            {
                ParentItem = parent;
            }
            else
            {
                SetParent(db, t.ParentType, this);
            }

            ParentType = t.ParentType;

            if (generations > 1)
            {
                SetMetadata(db);
            }

        }


        internal static void SetParent(UIBuildItContext db, string parentType, NoteData data)
        {
            var typeName = String.Format("UIBuildIt.Common.Tasks.{0},UIBuildIt.Common", parentType);
            SetParent(typeName, db, data);
            if (data.ParentItem == null)
            {
                typeName = String.Format("UIBuildIt.Common.UseCases.{0},UIBuildIt.Common", parentType);
                SetParent(typeName, db, data);
            }
        }


        // TODO: Generalize this code
        private static void SetParent(string typeName, UIBuildItContext db, NoteData data)
        {
            using (MiniProfiler.Current.Step("Set Parent for " + data.Entity.Name))
            {
                var type = System.Type.GetType(typeName);
                if (type != null)
                {
                    Item parent = null;
                    using (MiniProfiler.Current.Step("Find Parent for " + data.Entity.Name)) 
                    {
                        parent = (Item)db.Set(type).Find(data.Entity.ParentId);
                    }
                    if (parent != null)
                    {
                        var proxyCreationEnabled = db.Configuration.ProxyCreationEnabled;
                        try
                        {
                            using (MiniProfiler.Current.Step("Create Parent for " + data.Entity.Name))
                            {
                                db.Configuration.ProxyCreationEnabled = false;
                                data.ParentItem = db.Entry(parent).CurrentValues.ToObject() as Item;
                                data.Entity.ParentType = data.ParentItem.GetType().Name;
                                data.Entity.ParentId = parent.Id;
                            }
                        }
                        finally
                        {
                            db.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
                        }
                    }
                }
            }
        }

        public static Item GetNoteParent(Note source, UIBuildItContext db)
        {
            var data = new NoteData() { Entity = source };
            SetParent(db, source.ParentType, data);
            return data.ParentItem;
        }
    }

    public class TestParameterData : ItemData<TestParameter, Test>
    {
        public Parameter Parameter { get; set; }

        protected override TestParameter CreateDuplicateEntity(User user, int parentId, UIBuildItContext db) 
        {
            var entity = base.CreateDuplicateEntity(user, parentId, db);
            entity.ParameterId = GetParameter(Entity.ParameterId, parentId, db);
            entity.Value = Entity.Value;
            entity.Name = Entity.Name;
            return entity;
        }

        /// <summary>
        /// Parameters were possibly blindly switched
        /// </summary>
        /// <param name="currentParameter"></param>
        /// <param name="parentId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private int GetParameter(int currentParameter, int parentId, UIBuildItContext db)
        {          
            var test = db.Tests.AsNoTracking().FirstOrDefault(p => p.Id == parentId);
            var originalParameter = db.Parameters.AsNoTracking().FirstOrDefault(p => p.Id == currentParameter);
            var useCase = db.UseCases.AsNoTracking().FirstOrDefault(p => p.Id == test.ParentId); // thats the new one with the actions
            var newName = String.Format("{0} (Copy)", originalParameter.Name);
            var parameters = (from ac in db.Actions.AsNoTracking()
                              join pa in db.Parameters.AsNoTracking() on ac.Id equals pa.ParentId
                              // where pa.Name == newName
                              where ac.ParentId == useCase.Id
                              select pa).ToList();
            if(parameters != null && parameters.Count > 0)
            {
                return parameters.FirstOrDefault(pa => pa.Name.StartsWith(originalParameter.Name, StringComparison.InvariantCultureIgnoreCase)).Id;
            }
            return currentParameter;
        }

        //private void ReplaceParameter(ItemDataPrimitive newTest, ICollection<ItemDataPrimitive> collection, UIBuildItContext db)
        //{
        //    var nt = (ItemData<Test, UseCase>)newTest;
        //    foreach (var testparameter in nt.Children["TestParameters"])
        //    {
        //        var tp = (ItemData<TestParameter, Test>))testparameter;
        //        //var found = db.TestParameters.AsNoTracking().FirstOrDefault(tp => tp.Id ==  ((TestParameter)tp.Entity).ParameterId);

        //        var originalParameter = db.Parameters.AsNoTracking().FirstOrDefault(p => p.Id == tp.Entity.ParameterId);
        //        if(originalParameter != null)
        //        {
        //            var originalName = String.Format("{0} (Copy)", originalParameter.Name);
        //            var newParameter = (from p in collection
        //                               where ((ItemDataBase<UseCaseAction>)p).Entity.Name == originalName //&& p.ParentId == parentId
        //                               select p).FirstOrDefault();
        //            if(newParameter != null)
        //            {
        //                tp.Entity.ParameterId = ((ItemDataBase<UseCaseAction>)newParameter).Entity.Id;
        //            }
        //        }
        //    }
        //}
    }
}