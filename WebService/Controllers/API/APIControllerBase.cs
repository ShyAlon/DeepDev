using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using UIBuildIt.Common;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;
using UIBuildIt.Security;

namespace UIBuildIt.WebService.Controllers.API
{
    /// <summary>
    /// Used to log in
    /// </summary>
    public abstract class APIControllerBase<T, U> : ApiController   where T : Item, new()
                                                                    where U : ItemDataBase<T>, new()
    {
        protected UIBuildItContext db = new UIBuildItContext();

        // GET api/Project
        [AuthorizeToken]
        public virtual IHttpActionResult Get()
        {
            var user = (User)Request.Properties["user"];
            if (user != null)
            {
                Item.SetProjectDetails(false);
                ICollection<U> entities = GetEntities(user);
                return Content<ICollection<U>>(HttpStatusCode.OK, entities);
            }
            return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get tests "));
        }

        protected abstract ICollection<U> GetEntities(User user);

        [AuthorizeToken]
        public virtual IHttpActionResult Get(int id)
        {
            var user = (User)Request.Properties["user"];
            Item.SetProjectDetails(true);
            if (user != null)
            {

                var entity = db.Set<T>().FirstOrDefault(m => m.Id == id);
                var type = entity == null ? "unknown" : entity.GetEntityFinalType();
                var name = entity == null ? "unknown" : entity.Name;
                using (MiniProfiler.Current.Step(string.Format("Fix index for {0}: {1}", type, name)))
                {
                    if (entity != null && entity.Index.Count == 0 && entity.IsIndexed())
                    {
                        var parentType = entity.GetParentType(db);
                        entity.SetIndex<T>(parentType, db);
                        db.Set<T>().Add(entity);
                        db.SaveChanges();
                    }
                }
                using (MiniProfiler.Current.Step(string.Format("Create Data {0}: {1}", type, name)))
                {
                    var data = CreateData(entity, id);
                    data.CanEdit = (user.GetUserType(GetProject(data.Entity), db) & data.Entity.GetOwnerType()) == data.Entity.GetOwnerType();
                    return Content(HttpStatusCode.OK, data);
                }
            }
            return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get test "));
        }

        /// <summary>
        /// Gets an entity for entity. This might be a project for document or replicate an entity.
        /// </summary>
        /// <param name="id">The entity to get or (if id == -1) the entity to create</param>
        /// <param name="parentId">The parent or model to copy</param>
        /// <returns>The requested / new entity</returns>
        [AuthorizeToken]
        public virtual IHttpActionResult Get(int id, int parentId)
        {
            var user = (User)Request.Properties["user"];
            Item.SetProjectDetails(true);
            if (user != null)
            {
                var entity = db.Set<T>().FirstOrDefault(m => m.Id == id);
                var data = CreateData(entity, id, parentId, user);
                data.CanEdit = (user.GetUserType(GetProject(data.Entity), db) & data.Entity.GetOwnerType()) == data.Entity.GetOwnerType();
                return Content(HttpStatusCode.OK, data);
            }
            return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get test "));
        }

        /// <summary>
        /// Gets an entity for entity. This might be a project for document or replicate an entity.
        /// </summary>
        /// <param name="id">The entity to get or (if id == -1) the entity to create</param>
        /// <param name="parentId">The parent or model to copy</param>
        /// <returns>The requested / new entity</returns>
        [AuthorizeToken]
        public virtual IHttpActionResult Get(int id, int parentId, string parentType)
        {
            var user = (User)Request.Properties["user"];
            Item.SetProjectDetails(true);
            if (user != null)
            {
                var entity = id < 0 ? null : db.Set<T>().FirstOrDefault(m => m.Id == id);
                var data = CreateData(entity, parentId, parentType);
                data.CanEdit = (user.GetUserType(GetProject(data.Entity), db) & data.Entity.GetOwnerType()) == data.Entity.GetOwnerType();
                return Content(HttpStatusCode.OK, data);
            }
            return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get test "));
        }

        protected virtual U CreateData(T source, int id, string parentType)
        {
            return CreateData(source, id);
        }

        /// <summary>
        /// Duplicates the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual ItemDataBase<T> CreateData(T entity, int id, int parentId, User user)
        {
            if (id != -1)
            {
                throw new NotImplementedException();
            }
            var sourceEntity = db.Set<T>().FirstOrDefault(t => t.Id == parentId);
            var target = sourceEntity.CreateDuplicateEntity(user, sourceEntity.ParentId, GetParent(sourceEntity));
            var parentType = target.GetParentType(db);
            target.SetIndex<T>(parentType, db);
            db.Set<T>().Add(target);
            db.SaveChanges();
            return CreateData(target, parentId);
        }

        private string GetParent(T sourceEntity)
        {
            return sourceEntity is IParentType ? ((IParentType)sourceEntity).ParentType : null;
        }

        protected abstract U CreateData(T source, int id);

        protected abstract void ClearEntity(T source);

        protected abstract IHttpActionResult Validate(T entity);
        protected abstract Project GetProject(T entity);

        [AuthorizeToken]
        public virtual IHttpActionResult Put(T entity)
        {
            var user = (User)Request.Properties["user"];
            var existing = db.Set<T>().AsNoTracking().FirstOrDefault(e => e.Id == entity.Id);
            if (existing == null)
            {
                return Content(HttpStatusCode.NotFound, "Doesn't exist");
            }
            if(existing.Created != entity.Created || existing.CreatorId != entity.CreatorId)
            {
                return Content(HttpStatusCode.Forbidden, "Illegal operation for " + user.Name);
            }
            BeforeEntry(entity, existing);
            var entry = db.Entry(entity);
            if ((user.GetUserType(GetProject(entity), db) & entity.GetOwnerType()) != entity.GetOwnerType())
            {
                return Content(HttpStatusCode.Forbidden, "Not allowed to edit");
            }
            
            entry.State = EntityState.Modified;
            try
            {
                if ((entity.Index == null || entity.Index.Count == 0) && entity.IsIndexed())
                {
                    var parentType = entity.GetParentType(db);
                    if (parentType != null)
                    {
                        entity.SetIndex(parentType, db);
                    }
                }
                entity.Modified = DateTime.UtcNow;
                entity.ModifierId = user.Id;
                BeforeAttach(entity);
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Content(HttpStatusCode.NotFound, ex);
            }
            return Content(HttpStatusCode.OK, db.Set<T>().AsNoTracking().FirstOrDefault(e => e.Id == entity.Id));
        }

        /// <summary>
        /// Manage the entity before connecting to the database
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="existing"></param>
        protected virtual void BeforeEntry(T entity, T existing)
        {
            
        }


        protected virtual U CreateDataFor(T m, int id, string prefix = null, string parentType = null)
        {
            var user = (User)Request.Properties["user"];
            if (m != null)
            {
                m = m.CreateDuplicateEntity(user, id, parentType);
                if(prefix != null)
                {
                    m.Name = string.Format("{0}{1}", prefix, m.Name);
                }
            }
            else
            {
                m = new T()
                {
                    Created = DateTime.UtcNow,
                    CreatorId = user.Id,
                    ParentId = id,
                };
                if(m is IParentType)
                {
                    ((IParentType)m).ParentType = parentType;
                }
            }
           
            
            try
            {
                var parent = m.GetParentType(db);
                var parentEntity = m.GetParent(db);
                if(parentEntity != null)
                {
                    m.ProjectId = parentEntity.ProjectId;
                }
                
                m.SetIndex<T>(parent, db);
                db.Set<T>().Add(m);
                db.SaveChanges();
                return new U()
                    {
                        Entity = m,
                    };
            }
            catch (DbEntityValidationException dbex)
            {
                Console.WriteLine(dbex);
                throw;
            }
        }
        

        protected virtual string BeforeAttach(T entity)
        {
            return null;
        }

        protected static V FirstOrWhatever<V>(int id, UIBuildItContext db) where V : Item, new()
        {
            var found = db.Set<V>().FirstOrDefault(t => t.Id == id);
            if(found == null)
            {
                found = db.Set<V>().FirstOrDefault(t => t.Name == "Whatever");
            }
            if (found == null)
            {
                found = new V() { Id = -1 };
            }
            return found;
        }

        // POST api/Project
        [AuthorizeToken]
        public virtual IHttpActionResult Post(T entity)
        {
            var user = (User)Request.Properties["user"];
            if (!(entity is User) && (user.GetUserType(GetProject(entity), db) & entity.GetOwnerType()) != entity.GetOwnerType())
            {
                Content(HttpStatusCode.Forbidden, "Not allowed to post");
            }
            if (entity is User && user.Id != entity.Id)
            {
                Content(HttpStatusCode.Forbidden, "Only the user can edit iteself");
            }
            if (db.Set<T>().AsNoTracking().FirstOrDefault(e => e.Id == entity.Id) != null)
            {
                Content(HttpStatusCode.Conflict, "Already exists");
            }
            var result = Validate(entity);
            if (result != null)
            {
                return result;
            }
            if (ModelState.IsValid)
            {
                var ready = BeforeAttach(entity);
                if (string.IsNullOrEmpty(BeforeAttach(entity)))
                {
                    entity.Created = DateTime.UtcNow;
                    entity.CreatorId = user.Id;
                    var parentType = entity.GetParentType(db);
                    entity.SetIndex<T>(parentType, db);
                    db.Set<T>().Attach(entity);
                    var entry = db.Entry(entity);
                    entry.State = EntityState.Added;
                    db.SaveChanges();
                    return Content(HttpStatusCode.Created, CreateData(entity, 0));
                }
                else
                {
                    return Content(HttpStatusCode.Conflict, ready);
                }
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, entity);
            }
        }


        [AuthorizeToken]
        public virtual IHttpActionResult Delete(T entity)
        {
            entity = db.Set<T>().Find(entity.Id);
            if (entity == null)
            {
                return Content(HttpStatusCode.NotFound, entity);
            }
            ClearEntity(entity);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Content(HttpStatusCode.NotFound, ex);
            }
            return Content(HttpStatusCode.OK, entity);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        /// <summary>
        /// Sets the status of the entity to unchanged to avoid extra creation
        /// </summary>
        /// <param name="item"></param>
        protected static void SetUnchanged(Item item, UIBuildItContext db)
        {
            if (item != null)
            {
                // Recourse
                foreach (var child in item.GetItemMembers())
                {
                    SetUnchanged(child, db);
                }
                foreach (var collection in item.GetCollectionMembers())
                {
                    foreach (var collected in collection)
                    {
                        SetUnchanged((Item)collected, db);
                    }
                }
                if (item.Id < 1)
                {
                    db.Entry(item).State = EntityState.Added;
                }
                else
                {
                    db.Entry(item).State = EntityState.Unchanged;
                }
            }
        }

        protected void ClearProjectUser(ProjectUser pu)
        {
            //var milestones = (from m in db.Milestones
            //                  where m.ProjectId == project.Id
            //                  select m).ToList();

            //foreach (var milestone in milestones)
            //{
            //    ClearMilestone(milestone);
            //}
            db.ProjectUsers.Remove(pu);
        }

        protected void ClearProject(Project project)
        {
            var milestones = (from m in db.Milestones
                             where m.ParentId == project.Id
                             select m).ToList();

            foreach (var milestone in milestones)
            {
                ClearMilestone(milestone);
            }
            var modules = (from m in db.Modules
                              where m.ParentId == project.Id
                              select m).ToList();

            foreach (var module in modules)
            {
                ClearModule(module);
            }
            var requiments = (from m in db.Requirements
                           where m.ParentId == project.Id
                           select m).ToList();

            foreach (var requiment in requiments)
            {
                ClearRequirement(requiment);
            }
            var tasks = (from m in db.Tasks
                              where m.ProjectId == project.Id
                              select m).ToList();

            foreach (var task in tasks)
            {
                ClearTask(task);
            }
            var issues = (from m in db.Issues
                         where m.ProjectId == project.Id
                         select m).ToList();

            foreach (var issue in issues)
            {
                ClearIssue(issue);
            }
            db.Projects.Remove(project);
        }

        protected void ClearMilestone(Milestone milestone)
        {
            var sprints = (from r in db.Sprints
                           where r.ParentId == milestone.Id
                           select r).ToList();
            foreach (var sprint in sprints)
            {
                ClearSprint(sprint);
            }
            db.Milestones.Remove(milestone);
        }

        protected void ClearRequirement(Requirement requirement)
        {
            var usecases = (from u in db.UseCases
                            where u.ParentId == requirement.Id
                            select u).ToList();
            foreach (var usecase in usecases)
            {
                ClearUsecase(usecase);
            }


            db.Requirements.Remove(requirement);
        }

        protected void ClearUsecase(UseCase usecase)
        {
            db.UseCases.Remove(usecase);

            var actions = (from a in db.Actions
                            where a.ParentId == usecase.Id
                            select a).ToList();
            foreach (var action in actions)
            {
                ClearUseCaseAction(action);
            }

            var tests = (from a in db.Tests
                         where a.ParentId == usecase.Id
                             select a).ToList();
            foreach (var s in tests)
            {
                ClearTest(s);
            }
            db.UseCases.Remove(usecase);
        }

        protected void ClearTask(Task task)
        {
            var issues = (from a in db.Issues
                             where a.ParentId == task.Id
                             select a).ToList();
            foreach (var s in issues)
            {
                ClearIssue(s);
            }

            db.Tasks.Remove(task);
        }

        protected void ClearSprint(Sprint sprint)
        {
            db.Sprints.Remove(sprint);
        }

        protected void ClearNote(Note note)
        {
            db.Notes.Remove(note);
        }

        protected void ClearUseCaseAction(UseCaseAction action)
        {
            var parameters = (from p in db.Parameters
                           where p.ParentId == action.Id
                           select p).ToList();
            foreach (var par in parameters)
            {
                ClearParameter(par);
            }
            db.Actions.Remove(action);
        }

        protected void ClearTest(Test test)
        {
            var parameters = (from p in db.TestParameters
                              where p.ParentId == test.Id
                              select p).ToList();
            foreach (var par in parameters)
            {
                ClearTestParameter(par);
            }
            db.Tests.Remove(test);
        }

        protected void ClearTestParameter(TestParameter par)
        {
            db.TestParameters.Remove(par);
        }

        protected void ClearTag(TagEntity tag)
        {
            db.TagEntities.Remove(tag);
        }

        protected void ClearParameter(Parameter parameter)
        {
            db.Parameters.Remove(parameter);
        }

        protected void ClearModule(UIBuildIt.Common.Tasks.Module module)
        {
            var components = (from p in db.Components
                              where p.ParentId == module.Id
                              select p).ToList();
            foreach (var par in components)
            {
                ClearComponent(par);
            }
            db.Modules.Remove(module);
        }

        protected void ClearComponent(Component component)
        {
            var methods = (from p in db.Methods
                           where p.ParentId == component.Id
                              select p).ToList();
            foreach (var par in methods)
            {
                ClearMethod(par);
            }

            var components = (from p in db.Components
                           where p.ParentId == component.Id && p.ParentType == "Component"
                              select p).ToList();
            foreach (var com in components)
            {
                ClearComponent(com);
            }
            db.Components.Remove(component);
        }

        protected void ClearMethod(Method method)
        {
            db.Methods.Remove(method);
            var key = string.Format(" {0} ", method.Id);
            var containingUseCases = from s in db.UseCases
                                      where s.MethodIdsString.Contains(key)
                                      select s;
            foreach (var useCase in containingUseCases)
            {
                useCase.MethodIds.Remove(method.Id);
            }
        }


        protected void ClearIssue (Issue issue)
        {
            db.Issues.Remove(issue);
        }

        internal static Project CreateSeedProject(User owner, UIBuildItContext db)
        {
            var project = new Project()
            {
                Description = 


                "<h1>Project Concept</h1>"+
"<p>This is a very simple example of a minimalist project. It describes the suggested steps you should follow to <b>successfully</b>&nbsp;deliver your project.</p>" +
"<h2>Requirements</h2>"+
"<p>Requirements are descriptors of the system which presents a functional capability which needs to be met or a non-functional constraint which needs to be taken into account. Requirement should be broken down and refined into sub requirements until they become measurable and verifiable.</p>" +
"<h2>Use Cases</h2>"+
"<p>A Use Case is a description of a certain, specific usage which need to be supported successfully by the project's result. Functional requirements have uses cases which define the system's modules and components by following a technical sequence.</p>" +
"<h2>Modules</h2>"+
"<p>A Module is a group of components with shared functionality, objectives and interfaces. Modules are the top level technical entity and grouped inside processes.</p>" +
"<h2>Components</h2>"+
"<p>A Component is an implementation unit with a functionality, objectives and interface as exposed by its methods. Components can (and should) be divided into sub components with specific functions.</p>" +
"<h2>Tasks</h2>"+
"<p>A Task is an actionable item which needs to be performed in the scope of the project. Once the technical description of the project (requirements and design) is understood tasks can be assigned to people. Tasks, like requirements and components should be broken down until they are clear and functional.</p>"
                ,
                Id = -1,
                Name = "Minimal Project",
                Owner = owner,
                Title = "Seed Project Title",
                PriceTag = 65,
                Deadline = DateTime.UtcNow.AddDays(210),
                Created = DateTime.UtcNow, 
                CreatorId = owner.Id,
                Index = new int[] { 1}
            };

            project = db.Projects.Add(project);
            SetUnchanged(project.Owner, db);
            db.SaveChanges();
            project.ProjectId = project.Id;
            AddSeedMilestones(project, db, owner);
            return project;
        }

        protected static void AddSeedMilestones(Project project, UIBuildItContext db, User owner)
        {
            Milestone milestoneA = CreateMilestone(project, DateTime.UtcNow.AddDays(45.0), "Requirements Specification", db);
            Milestone milestoneB = CreateMilestone(project, DateTime.UtcNow.AddDays(105.0), "Architecture and Design", db);
            Milestone milestoneC = CreateMilestone(project, DateTime.UtcNow.AddDays(165.0), "Efforts and Scheduling", db);
            Milestone milestoneD = CreateMilestone(project, DateTime.UtcNow.AddDays(210.0), "Delivery", db);
            db.SaveChanges();

            AddRequirements(project, db, milestoneA, milestoneB, milestoneC, owner);
        }

        private UseCase CreateUseCase(Requirement requirement, string name, string description, int effort, UseCase predecessor = null)
        {
            var u = db.UseCases.Add(new UseCase()
            {
                Description = description,
                ParentId = requirement.Id,
                Name = name,
                Priority = 1,              
            });
            if (predecessor != null)
            {
                u.PredecessorIds.Add(predecessor.Id);
            }

            db.SaveChanges();
            return u;
        }

        private static Requirement CreateRequirement(UIBuildItContext db,Item parent, string name, string description, Requirement predecessor = null)
        {
            var r = db.Requirements.Add(new Requirement()
            {
                Name = name,
                Description = description,
                ParentId = parent.Id,
                ParentType = parent.GetEntityFinalType()
            });
            r.SetIndex(r.GetParentType(db) , db);
            if(predecessor != null)
            {
                r.PredecessorIds.Add(predecessor.Id);
            }
            db.SaveChanges();
            return r;
        }

        private static void AddRequirements(Project project, UIBuildItContext db, Milestone milestoneA, Milestone milestoneB, Milestone milestoneC, User owner)
        {
            var requirement = CreateRequirement(db, project, "Operational Needs", "The system shall be defined to meet the customer's needs.");
            var sub = CreateRequirement(db, requirement, "Operational Requirements", "The system shall acquire requirements from various sources such as customers, existing systems and emerging market needs.");
            sub = CreateRequirement(db, requirement, "Requirements Elicitation", "The system shall enhance the operational requirements by interviewing customers and considering alternatives.");
            AddTask(sub, requirement, milestoneA, db, "Conduct Interviews", "Conduct interviews at the customer site and establish the actual needs at the field level.", owner);
            CreateRequirement(db, requirement, "Operational Concept", "The system shall select an operational concept from several possible contenders.");
            CreateRequirement(db, requirement, "Requirements Specifications Document", "The system shall generate an elaborate requirements specifications document and utilize tasks and use case information to display requirements compliance matrix and components relations to requirements.");

            requirement = CreateRequirement(db, project, "System Design", "The system shall be met with a realistic and efficient design.");
            CreateRequirement(db, requirement, "Use Cases Creation", "The system shall expand and elaborate refined requirements with use cases which will describe the interaction between system components and external systems.");
            CreateRequirement(db, requirement, "Sequence Creation", "The system shall utilize the use cases to generate sequences which will provide a technical description of the system components and define their public APIs.");
            CreateRequirement(db, requirement, "Modules", "The system shall group the components into modules..");
            CreateRequirement(db, requirement, "Components", "The system shall describe the public methods (including risk, synchronicity and other factors) of components and sub components.");
            sub = CreateRequirement(db, requirement, "High Level Design Document", "The system shall generate a high level design document which will describe the processes, modules, components and method comprising the system and their interactions.");
            var task = AddTask(sub, requirement, milestoneB, db, "Review High Level Design", "Review the high level design and verify all item in th escope were covered.", owner);

            requirement = CreateRequirement(db, project, "Tasks Assignment", "The system shall allow assigning tasks and following their statuses.");
            CreateRequirement(db, requirement, "Task Generation", "The system shall allow generating tasks related to any other entity and assign it to a specific user.");
            CreateRequirement(db, requirement, "Issue Generation", "The system shall allow attaching issues to tasks to describe new risks or changes.");
            CreateRequirement(db, requirement, "Project Status Document", "The system shall generate a project status document which wil display tasks completion status according to risk and other parameters.");

            var module = AddModule(project, db, "User Interface", "The user interface of the system.", owner);

            task = AddTask(module, requirement, milestoneC, db, "Implement System User Interface", "Implement the user interface for the system.", owner);
            AddTask(task, requirement, milestoneB, db, "Select User Interface Packages", "Choose the most cost effective user interface packages.", owner);
            AddTask(task, requirement, milestoneB, db, "Create User Interface", "Create the user interface for the entire system (needs refinement).", owner);
            AddTask(task, requirement, milestoneB, db, "Verify User Interface", "Verify that the user interface supports all specified use cases (needs refinement).", owner);

            module = AddModule(project, db, "Server", "The back end of the system.", owner);

            task = AddTask(module, requirement, milestoneC, db, "Implement Server", "Implement the serverfor the system.", owner);
            AddTask(task, requirement, milestoneB, db, "Select Server Infrastructure", "Choose the most cost effective infrastructure for the server including API layer, business layer and data access layer.", owner);
            AddTask(task, requirement, milestoneB, db, "Implement Server", "Create the server which will provide logic and persistence for the entire system (needs refinement).", owner);
            AddTask(task, requirement, milestoneB, db, "Verify User Interface", "Verify that the server supports all specified use cases (needs refinement).", owner);

            db.SaveChanges();
            // AddCreateProjectUseCaseItems(usecase1);
            // AddEditProjectUseCaseItems(usecase2);
        }

        private static Task AddTask(Item parent, Requirement requirement, Milestone milestone, UIBuildItContext db, string name, string description, User owner)
        {
            var task = new Task()
            {
                Description = description,
                ParentId = parent.Id,
                Name = name,
                ParentType = parent.GetEntityFinalType(),
                RequirementIds = new int[] { requirement.Id},
                Owner = owner.Email,
                ContainerType = ContainerType.Milestone,
                ContainerID = milestone.Id,
                ProjectId = parent.ProjectId,
                EffortEstimationMean = 40,
                EffortEstimationPessimistic = 50,
                EffortEstimationOptimistic = 30,
            };
            task.SetIndex(parent.GetType(), db);
            db.Tasks.Add(task);
            db.SaveChanges();
            return task;
        }

        private static Common.Tasks.Module AddModule(Item parent, UIBuildItContext db, string name, string description, User owner)
        {
            var module = new Common.Tasks.Module()
            {
                Description = description,
                ParentId = parent.Id,
                Name = name,
                ProjectId = parent.ProjectId
            };
            module.SetIndex(parent.GetType(), db);
            db.Modules.Add(module);
            db.SaveChanges();
            return module;
        }

        private static Milestone CreateMilestone(Project project, DateTime deadline, string name, UIBuildItContext db)
        {
            var milestone = new Milestone()
            {
                Deadline = deadline,
                Description = "Milestone " + name + " is an example milestone",
                ParentId = project.Id,
                Name = name,
            };
            milestone.SetIndex(typeof(Project), db);
            db.Milestones.Add(milestone);
            db.SaveChanges();
            return milestone;
        }

        private UseCaseAction CreateUseCaseAction(UseCase usecase, string target, string obj, string sub, string verb)
        {
            var action = new UseCaseAction()
            {
                Description = "Set " + target,
                Name = "Edit " + target,
                Object = obj,
                Verb = verb,
                Subject = sub,
                ParentId = usecase.Id
            };
            return action;
        }

        private Parameter CreateParameter(UseCaseAction action, string item, string sub, string type)
        {
            var param1 = new Parameter()
            {
                Name = item + " " + sub,
                Description = item  + " " + sub,
                ParentId = action.Id,
                Type = type
            };
            return param1;
        }

        private Test CreateTest(UseCase useCase, string verb, string sub)
        {
            var test = new Test()
            {
                Name = verb + " " + sub,
                Description = verb + " the " + sub,
                Result = "Success",
                ParentId = useCase.Id
            };
            return test;
        }

        private TestParameter CreateTestParameter(Test test, Parameter par, string val, string decs)
        {
            var testParam1 = new TestParameter()
            {
                Name = "Valid " + decs,
                Description = "A valid " + decs,
                ParameterId = par.Id,
                ParentId = test.Id,
                Value = val
            };
            return testParam1;
        }

        private void AddEditProjectUseCaseItems(UseCase usecase2)
        {
            var action1 = CreateUseCaseAction(usecase2, "Name", "User", "Sets", "Name");
            var action2 = CreateUseCaseAction(usecase2, "Description", "User", "Sets", "Description");


            action1 = db.Actions.Add(action1);
            action2 = db.Actions.Add(action2);
            db.SaveChanges();

            var param1 = CreateParameter(action1, "Project", "Name", "String");
            var param2 = CreateParameter(action1, "Project", "Description", "String");

            param1 = db.Parameters.Add(param1);
            param2 = db.Parameters.Add(param2);

            var test = CreateTest(usecase2, "Edit", "project");

            test = db.Tests.Add(test);
            db.SaveChanges();

            var testParam1 = CreateTestParameter(test, param1, "UIBuildIt", "Name");

            var testParam2 = CreateTestParameter(test, param1, "A project for projects", "Description");

            testParam1 = db.TestParameters.Add(testParam1);
            testParam2 = db.TestParameters.Add(testParam2);
            db.SaveChanges();
        }

        private void AddCreateProjectUseCaseItems(UseCase usecase2)
        {
            var action1 = CreateUseCaseAction(usecase2, "Name", "User", "Creates", "Name");
            var action2 = CreateUseCaseAction(usecase2, "Description", "User", "Creates", "Description");

            action1 = db.Actions.Add(action1);
            action2 = db.Actions.Add(action2);
            db.SaveChanges();

            var param1 = CreateParameter(action1, "Project", "Name", "String");
            var param2 = CreateParameter(action1, "Project", "Description", "String");

            param1 = db.Parameters.Add(param1);
            param2 = db.Parameters.Add(param2);

            var test = CreateTest(usecase2, "Create", "project");

            test = db.Tests.Add(test);
            db.SaveChanges();

            var testParam1 = CreateTestParameter(test, param1, "UIBuildIt", "Name");

            var testParam2 = CreateTestParameter(test, param1, "A project for projects", "Description");

            testParam1 = db.TestParameters.Add(testParam1);
            testParam2 = db.TestParameters.Add(testParam2);
            db.SaveChanges();
        }

        protected bool Precedes<S>(Predecessor m, Predecessor s) where S : Predecessor
        {
            return Precedes<S>(m, s, db);
        }

        public static bool Precedes<S>(Predecessor m, Predecessor s, UIBuildItContext db) where S : Predecessor
        {
            if (m.Id == s.Id)
            { // for this purpose a milestone preceeds itself
                return true;
            }
            foreach (int id in s.PredecessorIds)
            {
                if (m.Id == id)
                {
                    return true;
                }
                var c = db.Set<S>().FirstOrDefault(mi => mi.Id == id);
                if (c != null)
                {
                    if (Precedes<S>(m, c, db))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}