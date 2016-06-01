using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;
using UIBuildIt.WebService.Models;

namespace UIBuildIt.WebService.Controllers.API
{
    public static class Utility
    {
        public static T CreateEmpty<T>() where T : Item, new()
        {
            return CreateEmpty<T>(-1);
        }

        public static T CreateEmpty<T>(int id) where T : Item, new()
        {
            return new T()
            {
                Description = "I Don't Care " + id,
                Name = "Whatever",
                Id = id,
            };
        }

        /// <summary>
        /// Maps the containment heirarchy
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] ChildrenTypes(this Type type)
        {
            if (typeof(Project) == type)
            {
                return new Type[] { typeof(Milestone), typeof(Module) };
            }
            if (typeof(Milestone) == type)
            {
                return new Type[] { typeof(Requirement) };
            }
            if (typeof(Requirement) == type)
            {
                return new Type[] { typeof(UseCase) };
            }
            if (typeof(Module) == type)
            {
                return new Type[] { typeof(Component) };
            }
            if (typeof(Component) == type)
            {
                return new Type[] { typeof(Method) };
            }
            throw new NotImplementedException("Didn't implement ChildrenTypes for " + type.Name);
        }

        public static Type GetParentType(this Item item, UIBuildItContext db)
        {
            if (item is Project)
            {
                return null;
            }
            if (item is Milestone)
            {
                return typeof(Project);
            }
            if (item is Sprint)
            {
                return typeof(Milestone);
            }
            if (item is Requirement)
            {
                return ((IParentType)item).ParentType == "Requirement" ? typeof(Requirement) : typeof(Project) ;
            }
            if (item is Module)
            {
                return typeof(Project);
            }
            if (item is Component)
            {
                return ((IParentType)item).ParentType == "Component" ? typeof(Component) : typeof(Module) ;
            }
            if (item is UseCase)
            {
                return typeof(Requirement);
            }
            if (item is Method)
            {
                return typeof(Component);
            }
            if (item is Task || item is Note)
            {
                var parent = item.GetParent(db);
                return parent.GetType();
            }
            if (item is Issue)
            {
                return typeof(Task);
            }
            throw new NotImplementedException("Didn't implement GetParentType for " + item.GetType().Name);
        }

        public static Item GetParent(this Item item, UIBuildItContext db)
        {
            if (item is Project)
            {
                return null;
            }
            if (item is Milestone)
            {
                return db.Projects.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Sprint)
            {
                return db.Milestones.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Requirement)
            {
                return ((IParentType)item).ParentType == "Requirement" ? (Item)db.Requirements.FirstOrDefault(p => p.Id == item.ParentId) : (Item)db.Projects.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Module)
            {
                return db.Projects.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Component)
            {
                return ((IParentType)item).ParentType == "Component" ? (Item)db.Components.FirstOrDefault(p => p.Id == item.ParentId) : (Item)db.Modules.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is UseCase)
            {
                return db.Requirements.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Method)
            {
                return db.Components.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if (item is Task || item is Note)
            {
                var t = (IParentType)item;
                var parent = item.GetParentRef(item.ParentId, t.ParentType, db);
                return parent;
            }
            if (item is Issue)
            {
                return db.Tasks.FirstOrDefault(p => p.Id == item.ParentId);
            }
            if(item is TagEntity)
            {
                var tag = (TagEntity)item;
                item.GetParentRef(tag.EntityId, tag.EntityType,  db);
            }
            throw new NotImplementedException("Didn't implement GetParentType for " + item.GetType().Name);
        }

        public static Item GetParentRef(this Item item, int parentId, string sourceTypeName, UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step("Get parent for " + item.Name))
            {
                var typeName = String.Format("UIBuildIt.Common.Tasks.{0},UIBuildIt.Common", sourceTypeName);
                Item parent = GetParentInternal(typeName, db, parentId);
                if (parent == null)
                {
                    typeName = String.Format("UIBuildIt.Common.UseCases.{0},UIBuildIt.Common", sourceTypeName);
                    parent = GetParentInternal(typeName, db, parentId);
                }
                if (parent == null)
                {
                    typeName = String.Format("UIBuildIt.Common.Authentication.{0},UIBuildIt.Common", sourceTypeName);
                    parent = GetParentInternal(typeName, db, parentId);
                }
                return parent;
            }
        }

        internal static Item GetParentInternal(string typeName, UIBuildItContext db, int parentId)
        {
            var type = System.Type.GetType(typeName);
            if (type != null)
            {
                var parent = (Item)db.Set(type).Find(parentId);
                if (parent != null)
                {
                    var proxyCreationEnabled = db.Configuration.ProxyCreationEnabled;
                    try
                    {
                        db.Configuration.ProxyCreationEnabled = false;
                        return db.Entry(parent).CurrentValues.ToObject() as Item;

                    }
                    finally
                    {
                        db.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
                    }

                }
            }
            return null;
        }


        public static ICollection<TagEntity> GetTags(this Item item, UIBuildItContext db)
        {
            var type = item.GetEntityFinalType();
            var tags = (from t in db.TagEntities.AsNoTracking()
                         where t.EntityId == item.Id && t.EntityType == type
                         select t).ToList();
            return tags;
        }

        public class TagComparer : IEqualityComparer<TagEntity>
        {

            #region IEqualityComparer<TagEntity> Members

            public bool Equals(TagEntity x, TagEntity y)
            {
                return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(TagEntity obj)
            {
                return obj.Name.GetHashCode();
            }

            #endregion
        }

        public static int Closer(this Item item, ICollection<TagEntity> tags, UIBuildItContext db)
        {
            var result =  item.GetTags(db).Intersect(tags, new TagComparer()).Count();
            return result;
        }

        /// <summary>
        /// TODO: Break this up to the classes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="parentId"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static T CreateDuplicateEntity<T>(this T source, UIBuildIt.Common.Authentication.User user, int parentId, string parentType = null) where T : Item, new()
        {
            var m = new T()
            {
                Name = String.Format(source.Name),
                Description = String.Format("Copy: {0}", source.Description),
                CreatorId = user.Id,
                Created = DateTime.UtcNow,
                ParentId = parentId < 0 ? source.ParentId : parentId,
                ProjectId = source.ProjectId
            };
            if (m is IParentType)
            {
                ((IParentType)m).ParentType = parentType;
            }
            if(m is IDeadline)
            {
                ((IDeadline)m).Deadline = ((IDeadline)source).Deadline;
            }
            if(m is IOwner)
            {
                ((IOwner)m).Owner = ((IOwner)source).Owner;
            }
            return m;
        }

        public static T SetIndex<T>(this T item, Type parentType, UIBuildItContext db, bool resetIndice = false)  where T : Item
        {
            using (MiniProfiler.Current.Step(string.Format("Set index for {0}: {1}", item.GetEntityFinalType(), item.Name)))
            {
                if (typeof(T) == typeof(Project))
                {
                    item.Index = new List<int>();
                    return item;
                }
                var parent = (Item)(db.Set(parentType).Find(item.ParentId));
                if (parent.Index == null)
                {
                    return item;
                }

                int counter = 1;
                var type = item.GetType().Name;
                foreach (var child in parent.GetChildren(db, true, true))
                {
                    if (child.Key.IndexOf(type, StringComparison.InvariantCultureIgnoreCase) > -1) // correct collection
                    {
                        int guess = resetIndice ? 1 : Math.Max(child.Value.Count, 1);
                        var unique = false;
                        while (!unique)
                        {
                            item.Index = parent.Index.Concat(new List<int>() { counter, guess }).ToList();
                            unique = true;
                            for (int i = 0; i < child.Value.Count; i++)
                            {
                                if (item.Id == child.Value[i].Id) // same Item
                                {
                                    continue;
                                }
                                if (IndiceEqual(item.Index, child.Value[i].Index))
                                {
                                    guess++;
                                    unique = false;
                                    break;
                                }
                            }
                        }
                    }
                    counter++;
                }

                return item;
            }
        }

        private static bool IndiceEqual(ICollection<int> c1, ICollection<int> c2)
        {
            if(c1.Count != c2.Count)
            {
                return false;
            }
            for(int i = 0; i < c1.Count; i++)
            {
                if(c1.ElementAt(i) != c2.ElementAt(i))
                {
                    return false;
                }
            }
            return true;
        }

        public static Module ParentModule(this Component item, UIBuildItContext db)
        {
            while(item.ParentType == "Component")
            {
                item = db.Components.FirstOrDefault(c => c.Id == item.ParentId);
            }
            return db.Modules.FirstOrDefault(c => c.Id == item.ParentId);
        }

        /// <summary>
        /// A requirement is an ancestor of himself
        /// </summary>
        /// <param name="item"></param>
        /// <param name="child"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool IsAncestor(this Requirement item, Requirement child, UIBuildItContext db)
        {
            if(child == null)
            {
                return false;
            }
            if(child.Id == item.Id)
            {
                return true;
            }
            if (child.ParentType != "Requirement")
            {
                return false;
            }
            return item.IsAncestor(db.Requirements.FirstOrDefault(c => c.Id == child.ParentId), db);
        }

        public static ICollection<Component> SubComponents(this Item parent, UIBuildItContext db)
        {
            var result = new List<Component>();
            if(parent is Module)
            {
                var components = (from c in db.Components
                                  where c.ParentType != "Component" && c.ParentId == parent.Id
                                  select c).ToList();
                result = (result.Concat(components)).ToList();

            }
            else if (parent is Component)
            {
                var components = (from c in db.Components
                                  where c.ParentType == "Component" && c.ParentId == parent.Id
                                  select c).ToList();
                result = (result.Concat(components)).ToList();

            }
            var tmp = result.ToList();
            for (int i = 0; i < result.Count; i++ )
            {
                tmp = result.Concat(result[i].SubComponents(db)).ToList();
            }
            return tmp;
        }


        public static ICollection<UseCaseAction> GetMissingMethods(this Module m, UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step(string.Format("Get missing methods for {0}: {1}", m.GetEntityFinalType(), m.Name)))
            {
                var components = from c in m.SubComponents(db)
                                 select new ComponentData(db, c, null, 0);
                var actionsList = (from a in db.Actions.AsNoTracking() where a.Subject == m.Name select a).ToList();
                var actions = new Dictionary<string, UseCaseAction>();
                foreach (var act in actionsList)
                {
                    // Last one takes hold
                    actions[act.Name] = act;
                }
                var methods = (from component in components
                               where ((ComponentData)component).Children.ContainsKey("Methods")
                               select ((ComponentData)component).Children["Methods"]).ToList();

                foreach (var pop in methods)
                {
                    var mNames = (from met in pop select ((MethodData)met).Entity.Name).ToList();
                    foreach (var name in mNames)
                    {
                        if (actions.ContainsKey(name))
                        {
                            actions.Remove(name);
                        }
                    }
                }
                return actions.Values;
            }
        }

        public static ICollection<Note> GetDirectNotes(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type
                         select t).ToList().OrderBy(n => n.HashIndex()).ToList(); ;
            return notes;
        }

        public static ICollection<IItemTree> GetNotes(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type
                         select t).ToList();
            var results = (from note in notes
                           orderby note.HashIndex()
                           select (IItemTree)new ItemTree("Note", note.ParentType) { Entity = note, Children = note.GetNotes(db) }).ToList();

            return results;
        }

        public static ICollection<IItemTree> GetNotesForTaskDocument(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type && t.DisplayInTasks
                         select t).ToList();
            var results = (from note in notes
                           orderby note.HashIndex()
                           select (IItemTree)new ItemTree("Note", note.ParentType) { Entity = note, Children = note.GetNotesForTaskDocument(db) }).ToList();

            return results;
        }

        public static ICollection<IItemTree> GetNotesForDesignDocument(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type && t.DisplayInDesign
                         select t).ToList();
            var results = (from note in notes
                           orderby note.HashIndex()
                           select (IItemTree)new ItemTree("Note", note.ParentType) { Entity = note, Children = note.GetNotesForDesignDocument(db) }).ToList();

            return results;
        }

        public static ICollection<IItemTree> GetNotesForRequirementsDocument(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type && t.DisplayInRequirements
                         select t).ToList();
            var results = (from note in notes
                           orderby note.HashIndex()
                           select (IItemTree)new ItemTree("Note", note.ParentType) { Entity = note, Children = note.GetNotesForRequirementsDocument(db) }).ToList();

            return results;
        }

        public static ICollection<Note> GetDirectNotesMinimal(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in
                            (from t in db.Notes
                             where t.ParentId == entity.Id && t.ParentType == type
                             select new
                             {
                                 Name = t.Name,
                                 Id = t.Id,
                                 IndexString = t.IndexString,
                                 ParentType = t.ParentType,
                                 ParentId = t.ParentId
                             }).ToList()
                         select new Note() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).OrderBy(n => n.HashIndex()).ToList();
            return notes;
        }

        public static ICollection<IItemTree> GetNotesMinimal(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var notes = (from t in (from t in db.Notes
                         where t.ParentId == entity.Id && t.ParentType == type
                         select new 
                         {
                             Name = t.Name,
                             Id = t.Id,
                             IndexString = t.IndexString,
                             ParentType = t.ParentType,
                             ParentId = t.ParentId,
                             DisplayInTasks = t.DisplayInTasks,
                             DisplayInRequirements = t.DisplayInRequirements,
                             DisplayInDesign = t.DisplayInDesign
                         }).ToList()
                         select new Note() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId, DisplayInDesign = t.DisplayInDesign, DisplayInRequirements=t.DisplayInRequirements, DisplayInTasks = t.DisplayInTasks }).ToList();
            var results = (from note in notes
                           orderby note.HashIndex()
                           select (IItemTree)new ItemTree("Note", note.ParentType) { Entity = note, Children = note.GetNotes(db) }).ToList();

            return results;
        }

        public static ICollection<IItemTree> GetTasks(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var tasks = (from t in db.Tasks
                         where t.ParentId == entity.Id && t.ParentType == type
                         select t).ToList();
            var results = (from task in tasks
                           orderby task.HashIndex()
                           select (IItemTree)new ItemTree("Task", task.ParentType) { Entity = task, Children = task.GetTasks(db) }).ToList();

            return results;
        }

        public static IItemTree GetTaskDataItemRoot(this Task entity, UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step(string.Format("Get task data for {0}: {1}", entity.GetEntityFinalType(), entity.Name)))
            {
                var result = new TaskItemTree(new TaskData(db, entity, null, 1, true), entity.GetTaskDataItems(db));
                return result;
            }
        }

        public static ICollection<IItemTree> GetTaskDataItems(this Item entity, UIBuildItContext db)
        {
            var type = entity.GetEntityFinalType();
            var tasks = (from t in db.Tasks
                         where t.ParentId == entity.Id && t.ParentType == type
                         select t).ToList();
            var results = (from task in tasks
                           orderby task.HashIndex()
                           select (IItemTree)new TaskItemTree(new TaskData(db, task, null, 1, true), task.GetTaskDataItems(db))).ToList();

            return results;
        }

        public static IDeadline GetContainer(this Task task, UIBuildItContext db)
        {
            if(task.ContainerType == ContainerType.Milestone)
            {
                return db.Milestones.AsNoTracking().FirstOrDefault(m => m.Id == task.ContainerID);
            }
            else if (task.ContainerType == ContainerType.Sprint)
            {
                return db.Sprints.AsNoTracking().FirstOrDefault(m => m.Id == task.ContainerID);
            }
            return null;
        }

        /// <summary>
        /// Get the children of an entity from the database
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Item>> GetChildren(this Item parent, UIBuildItContext db, bool includeNotes = false, bool includeTasks = false)
        {
            using (MiniProfiler.Current.Step(string.Format("Get children for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
            {
                var type = ObjectContext.GetObjectType(parent.GetType());

                var result = new Dictionary<string, List<Item>>();
                if (type == typeof(Project))
                {
                    result["Milestones"] = (from m in
                                                (from m in db.Milestones
                                                 where m.ParentId == parent.Id

                                                 select m).ToList()
                                            select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                    result["Modules"] = (from m in
                                             (from m in db.Modules
                                              where m.ParentId == parent.Id

                                              select m).ToList()
                                         select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                    result["Requirements"] = (from m in
                                                  (from m in db.Requirements
                                                   where m.ParentId == parent.Id && (m.ParentType == "Project" || m.ParentType == null)

                                                   select m).ToList()
                                              select (Item)m).OrderBy(m => m.HashIndex()).ToList();

                }
                else if (type == typeof(Milestone))
                {
                    result["Sprints"] = (from m in
                                             (from m in db.Sprints
                                              where m.ParentId == parent.Id

                                              select m).ToList()
                                         select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                }
                else if (type == typeof(Requirement))
                {
                    result["UseCases"] = (from m in
                                              (from m in db.UseCases
                                               where m.ParentId == parent.Id

                                               select m).ToList()
                                          select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                    result["Sub-Requirements"] = (from m in
                                                      (from m in db.Requirements
                                                       where m.ParentId == parent.Id && m.ParentType == "Requirement"

                                                       select m).ToList()
                                                  select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                }
                else if (type == typeof(UseCase))
                {
                    result["Tests"] = (from m in
                                           (from m in db.Tests
                                            where m.ParentId == parent.Id

                                            select m).ToList()
                                       select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                }
                else if (type == typeof(Module))
                {
                    result["Components"] = (from m in
                                                (from m in db.Components
                                                 where m.ParentId == parent.Id && m.ParentType != "Component"

                                                 select m).ToList()
                                            select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                }
                else if (type == typeof(Component))
                {
                    result["Methods"] = (from m in
                                             (from m in db.Methods
                                              where m.ParentId == parent.Id

                                              select m).ToList()
                                         select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                    result["Sub-Components"] = (from m in
                                                    (from m in db.Components
                                                     where m.ParentId == parent.Id && m.ParentType == "Component"

                                                     select m).ToList()
                                                select (Item)m).OrderBy(m => m.HashIndex()).ToList();
                }
                var typeName = type.Name;
                if (includeNotes)
                {
                    result["Notes"] =
                        (from t in
                             (from m in db.Notes
                              where m.ParentId == parent.Id && m.ParentType == typeName

                              select m).ToList()
                         select (Item)t).ToList();
                }
                if (includeTasks)
                {
                    result["Tasks"] = (from t in
                                           (from m in db.Tasks
                                            where m.ParentId == parent.Id && m.ParentType == typeName

                                            select m).ToList()
                                       select (Item)t).ToList();
                }
                return result;
            }
        }

        /// <summary>
        /// Get the children of an entity from the database - but only the minimal structure 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Item>> GetChildrenMinimal(this Item parent, UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step(string.Format("Get children for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
            {
                var type = ObjectContext.GetObjectType(parent.GetType());

                var result = new Dictionary<string, List<Item>>();
                if (type == typeof(Project))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting milestones for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Milestones"] = (from t in
                                                    (from t in db.Milestones
                                                     where t.ParentId == parent.Id
                                                     select new
                             {
                                 Name = t.Name,
                                 Id = t.Id,
                                 IndexString = t.IndexString,
                                 ParentId = t.ParentId
                             }).ToList()
                                                select (Item)new Milestone() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                    using (MiniProfiler.Current.Step(string.Format("Getting modules for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Modules"] = (from t in
                                                 (from t in db.Modules
                                                  where t.ParentId == parent.Id
                                                  select new
                                                  {
                                                      Name = t.Name,
                                                      Id = t.Id,
                                                      IndexString = t.IndexString,
                                                      ParentId = t.ParentId
                                                  }).ToList()
                                             select (Item)new Module() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                    using (MiniProfiler.Current.Step(string.Format("Getting requirements for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Requirements"] = (from t in
                                                      (from t in db.Requirements
                                                       where t.ParentId == parent.Id && t.ParentType != "Requirement"
                                                       select new
                                                       {
                                                           Name = t.Name,
                                                           Id = t.Id,
                                                           IndexString = t.IndexString,
                                                           ParentType = t.ParentType,
                                                           ParentId = t.ParentId
                                                       }).ToList()
                                                  select (Item)new Requirement() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                }
                else if (type == typeof(Milestone))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting Sprints for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Sprints"] = (from t in
                                                      (from t in db.Sprints
                                                       where t.ParentId == parent.Id
                                                       select new
                                                       {
                                                           Name = t.Name,
                                                           Id = t.Id,
                                                           IndexString = t.IndexString,
                                                           ParentId = t.ParentId
                                                       }).ToList()
                                                  select (Item)new Sprint() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                }
                else if (type == typeof(Requirement))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting use cases for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["UseCases"] = (from t in
                                                      (from t in db.UseCases
                                                       where t.ParentId == parent.Id
                                                       select new
                                                       {
                                                           Name = t.Name,
                                                           Id = t.Id,
                                                           IndexString = t.IndexString,
                                                           ParentId = t.ParentId
                                                       }).ToList()
                                                  select (Item)new UseCase() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }

                    using (MiniProfiler.Current.Step(string.Format("Getting sub-requirements for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Sub-Requirements"] = (from t in
                                                      (from t in db.Requirements
                                                       where t.ParentId == parent.Id && t.ParentType == "Requirement"
                                                       select new
                                                       {
                                                           Name = t.Name,
                                                           Id = t.Id,
                                                           IndexString = t.IndexString,
                                                           ParentType = t.ParentType,
                                                           ParentId = t.ParentId
                                                       }).ToList()
                                                  select (Item)new Requirement() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }

                }
                else if (type == typeof(UseCase))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting tests for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Tests"] = (from t in
                                                          (from t in db.Tests
                                                           where t.ParentId == parent.Id
                                                           select new
                                                           {
                                                               Name = t.Name,
                                                               Id = t.Id,
                                                               IndexString = t.IndexString,
                                                               ParentId = t.ParentId
                                                           }).ToList()
                                                      select (Item)new Requirement() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                }
                else if (type == typeof(Module))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting components for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Components"] = (from t in
                                                    (from t in db.Components
                                                     where t.ParentId == parent.Id && t.ParentType != "Component"
                                                           select new
                                                           {
                                                               Name = t.Name,
                                                               Id = t.Id,
                                                               IndexString = t.IndexString,
                                                               ParentType = t.ParentType,
                                                               ParentId = t.ParentId
                                                           }).ToList()
                                                      select (Item)new Component() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                }
                else if (type == typeof(Component))
                {
                    using (MiniProfiler.Current.Step(string.Format("Getting components for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Methods"] = (from t in
                                                 (from t in db.Methods
                                                     where t.ParentId == parent.Id
                                                     select new
                                                     {
                                                         Name = t.Name,
                                                         Id = t.Id,
                                                         IndexString = t.IndexString,
                                                         ParentId = t.ParentId
                                                     }).ToList()
                                                select (Item)new Method() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }

                    using (MiniProfiler.Current.Step(string.Format("Getting components for {0}: {1}", parent.GetEntityFinalType(), parent.Name)))
                    {
                        result["Sub-Components"] = (from t in
                                                    (from t in db.Components
                                                     where t.ParentId == parent.Id && t.ParentType == "Component"
                                                     select new
                                                     {
                                                         Name = t.Name,
                                                         Id = t.Id,
                                                         IndexString = t.IndexString,
                                                         ParentType = t.ParentType,
                                                         ParentId = t.ParentId
                                                     }).ToList()
                                                select (Item)new Component() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList().OrderBy(m => m.HashIndex()).ToList();
                    }
                }
                return result;
            }
        }


        public static decimal HashIndex(this Item item)
        {
            decimal result = 0;
            for(int i = 0; i < item.Index.Count; i++)
            {
                result = result * 10 + item.Index.ElementAt(i);
            }
            return result;
        }
    }
}