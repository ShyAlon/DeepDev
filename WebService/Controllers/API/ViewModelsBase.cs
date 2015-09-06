using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Documents;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;
using UIBuildIt.WebService.Models;

namespace UIBuildIt.WebService.Controllers.API
{
    public class ItemDataPrimitive
    {
        private Dictionary<string, ICollection<ItemDataPrimitive>> _children = new Dictionary<string, ICollection<ItemDataPrimitive>>();

        public Dictionary<string, ICollection<ItemDataPrimitive>> Children { get { return _children; } }

        private Dictionary<string, ICollection<ItemDataPrimitive>> _metadata = new Dictionary<string, ICollection<ItemDataPrimitive>>();

        public Dictionary<string, ICollection<ItemDataPrimitive>> Metadata { get { return _metadata; } }

        private Dictionary<string, ICollection<IItemTree>> _trees = new Dictionary<string, ICollection<IItemTree>>();
        public Dictionary<string, ICollection<IItemTree>> Trees { get { return _trees; } }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public Dictionary<string, object> Properties { get { return _properties; } }

        public bool CanEdit { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ItemDataPrimitive Duplicate(UIBuildItContext db, User user, ItemDataPrimitive scaffold, int generations = 0, int parentId = -1)
        {
            throw new NotImplementedException();
        }

        internal virtual ICollection<Task> GetTasks(UIBuildItContext db)
        {
            return null;
        }

        internal virtual ICollection<Note> GetNotes(UIBuildItContext db)
        {
            return null;
        }
    }

    public class ItemDataBase<T> : ItemDataPrimitive where T : Item
    {
        public T Entity { get; set; }

        public string Type { get; set; }

        public ItemDataBase()
            : base()
        {
            Type = typeof(T).Name;
        }

        /// <summary>
        /// Gets all the tasks related to the entity
        /// </summary>
        /// <param name="db">The database to query</param>
        /// <returns>The tasks</returns>
        internal override ICollection<Task> GetTasks(UIBuildItContext db)
        {
            var type = GetEntityFinalType();
            var tasks = (from t in db.Tasks
                            where t.ParentId == Entity.Id && t.ParentType == type
                            select t).ToList();
            return tasks;
        }

        protected string GetEntityFinalType()
        {
            var entityType = Entity.GetType();
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.BaseType;
            }
            var type = entityType.Name;
            return type;
        }

        internal override ICollection<Note> GetNotes(UIBuildItContext db)
        {
            var type = GetEntityFinalType();
            var tasks = (from t in db.Notes
                         where t.ParentId == Entity.Id && t.ParentType == type
                         select t).ToList();
            return tasks;
        }

        public int Id { get; set; }
    }

    public class ParentContainer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public ParentContainer(Item item)
        {
            Id = item.Id;
            Index = item.Index;
            Name = item.Name;
        }

        public ICollection<int> Index { get; set; }
    }

    public class ItemData<T, U> : ItemDataBase<T>
        where T : Item, new()
        where U : Item
    {
        private U _parent;
        public U ParentItem
        {
            get
            {
                return _parent;
            }
            set 
            {
                _parent = value;
                Parent = new ParentContainer(value);
            }
        }

        public bool ShouldSerializeParentItem()
        {
            return false;
        }

        public ParentContainer Parent { get; set; }

        public string ParentType { get; set; }

        public ItemData() : base()
        {
            ParentType = typeof(U).Name;
        }

        /// <summary>
        /// Constructor with metadata and audit
        /// </summary>
        /// <param name="db"></param>
        /// <param name="source"></param>
        public ItemData(UIBuildItContext db, Item source)  : this()
        {
            var user = db.Users.FirstOrDefault(u => u.Id == source.CreatorId);
            Creator = user == null ? "Unknown" : user.Name;
            user = db.Users.FirstOrDefault(u => u.Id == source.ModifierId);
            LastModifier = user == null ? "Unknown" : user.Name;
            if (source.GetEntityFinalType() != "User")
            {
                SetProject(db, source);
            }
            
            ProjectId = source.ProjectId;
            Entity = (T)source;
        }

        private static void SetProject(UIBuildItContext db, Item source)
        {
            if (source.ProjectId < 1)
            {
                if(source is Project)
                {
                    source.ProjectId = source.Id;
                    // don't resave projects. The user gets recreated.
                }
                else
                {
                    var parent = source.GetParent(db);
                    if (parent != null)
                    {
                        SetProject(db, parent);
                        source = db.Set<T>().FirstOrDefault(t => t.Id == source.Id);
                        source.ProjectId = parent.ProjectId;
                        //var entry = db.Entry(source);
                        //entry.State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                }
            }
        }

        public override ItemDataPrimitive Duplicate(UIBuildItContext db, User user, ItemDataPrimitive scaffold, int generations = 0, int parentId = -1)
        {
            var m = CreateDuplicateEntity(user, parentId, db);
            if (scaffold == null)
            {
                scaffold = new ItemData<T, U>();
            }
            var data = (ItemData<T, U>)scaffold;

            try
            {
                db.Set<T>().Add(m);
                db.SaveChanges();

                if (generations > 0)
                {
                    foreach (var name in Children.Keys)
                    {
                        if (Children[name] != null && Children[name].Count > 0)
                        {
                            data.Children[name] = Children[name].ToList();
                            data.Children[name].Clear();
                            foreach (var child in Children[name])
                            {
                                data.Children[name].Add(child.Duplicate(db, user, null, generations - 1, m.Id));
                            }
                        }

                    }
                }

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
        protected virtual T CreateDuplicateEntity(User user, int parentId, UIBuildItContext db)
        {
            return Entity.CreateDuplicateEntity(user, parentId);
        }

        protected T SetProjectId(UIBuildItContext db, T u, int projectId)
        {
            var uc = db.Set<T>().FirstOrDefault(uca => uca.Id == u.Id);
            uc.ProjectId = projectId;
            var entry = db.Entry(uc);
            entry.State = EntityState.Modified;
            db.SaveChanges();
            u = uc;
            return u;
        }

        /// <summary>
        /// Make sue it gets calculated only once
        /// </summary>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static Item GetProject(Item item, UIBuildItContext db)
        {
            Item result = null;
            if (item.ParentId < 0 || typeof(Project) == item.GetType())
            {
                return item;
            }
            else if(item.ProjectId > 0)
            {
                result = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == item.ProjectId);
                return result;
            }
            else if (typeof(Milestone) == item.GetType() || typeof(Module) == item.GetType())
            {
                result = db.Projects.FirstOrDefault(p => p.Id == item.ParentId);
            }
            else if (typeof(Sprint) == item.GetType())
            {
                var parent = db.Milestones.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(Component) == item.GetType())
            {
                var parent = db.Modules.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(Method) == item.GetType())
            {
                var parent = db.Components.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(UseCase) == item.GetType())
            {
                var parent = db.Requirements.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(Test) == item.GetType())
            {
                var parent = db.UseCases.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(Requirement) == item.GetType())
            {
                var parent = db.Projects.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            else if (typeof(Task) == item.GetType())
            {   
                var parent = TaskData.GetTaskParent((Task)item, db);
                result = GetProject(parent, db);
            }
            else if (typeof(Note) == item.GetType())
            {
                var parent = NoteData.GetNoteParent((Note)item, db);
                result = GetProject(parent, db);
            }
            else if (typeof(Issue) == item.GetType())
            {
                var parent = db.Tasks.FirstOrDefault(p => p.Id == item.ParentId);
                result = GetProject(parent, db);
            }
            if (result != null)
            {
                //throw new NotImplementedException("Didn't implement GetParent for " + item.GetType());
                item.ProjectId = result.Id;
                db.SaveChanges();
            }
            return result;
        }

        /// <summary>
        /// Get the trees for the tasks and the notes
        /// </summary>
        /// <param name="db"></param>
        protected void SetMetadata(UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step("Getting Tasks for " + Entity.Name))
            {
                Trees["Tasks"] = Entity.GetTasks(db);
            }
            using (MiniProfiler.Current.Step("Getting Notes for " + Entity.Name))
            {
                Trees["Notes"] = Entity.GetNotes(db);// GetAllNotes(db);
            }
            using (MiniProfiler.Current.Step("Getting Tags for " + Entity.Name))
            {
                Metadata["Tags"] = GetAllTags(db);
            }
        }

        protected ICollection<ItemDataPrimitive>  GetAllTags(UIBuildItContext db)
        {
            var type = GetEntityFinalType();
            var tags = (from te in
                           (from te in db.TagEntities.AsNoTracking()
                            where te.EntityId == Entity.Id && te.EntityType == type
                            select te).ToList()
                       select (ItemDataPrimitive) new ItemDataBase<TagEntity>() { Name = te.Name, Id = te.Id}).ToList();
            return tags;
        }

        public ICollection<ItemDataPrimitive> GetAllTasks(UIBuildItContext db)
        {
            //TODO: Use a better data structure
            var list = new List<ItemDataPrimitive>();
            list.AddRange(
                (from t in GetTasks(db)
                 select new ItemDataBase<Task>()
                 {
                     Entity = t,
                     CanEdit = false
                 }).ToList());
            foreach (var brood in Children.Values)
            {
                foreach (var item in brood)
                {
                    var ll = from t in item.GetTasks(db)
                             select new ItemDataBase<Task>()
                             {
                                 Entity = t,
                                 CanEdit = false
                             };
                    list.AddRange(ll);
                }
            }
            return list.OrderBy(m => ((ItemDataBase<Task>)m).Entity.HashIndex()).ToList();
        }

        public ICollection<ItemDataPrimitive> GetAllNotes(UIBuildItContext db)
        {
            //TODO: Use a better data structure
            var list = new List<ItemDataPrimitive>();
            list.AddRange(
                (from t in GetNotes(db)
                 select new ItemDataBase<Note>()
                 {
                     Entity = t,
                     CanEdit = false
                 }).ToList());
            foreach (var brood in Children.Values)
            {
                foreach (var item in brood)
                {
                    var ll = from t in item.GetNotes(db)
                             select new ItemDataBase<Note>()
                             {
                                 Entity = t,
                                 CanEdit = false
                             };
                    list.AddRange(ll);
                }
            }
            return list.OrderBy(m => ((ItemDataBase<Note>)m).Entity.HashIndex()).ToList();
        }

        public string Creator { get; set; }

        public string LastModifier { get; set; }

        public int ProjectId { get; set; }
    }

    public class PredecessorItemData<T, U> : ItemData<T, U>
        where T : Predecessor, new()
        where U : Item
    {
        public PredecessorItemData()
            : base()
        { }

        public PredecessorItemData(UIBuildItContext db, T source) : base(db, source)
        {}

        /// <summary>
        /// Returns the parallel children of the parent
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        protected virtual ICollection<ItemData<T, U>> GetPotentialPredecessors(UIBuildItContext db)
        {
            var result = new List<ItemData<T, U>>();
            var sibs = from s in db.Set<T>().AsNoTracking()
                       where s.ParentId == ParentItem.Id
                       select s;
            if (sibs != null && sibs.Count() > 0)
            {
                var siblings = from s in sibs.ToList()
                               select new ItemData<T, U>() { Entity = s, ParentItem = ParentItem };
                AddSiblings(db, result, siblings);
            }

            return result;
        }

        protected void AddSiblings(UIBuildItContext db, List<ItemData<T, U>> result, IEnumerable<ItemData<T, U>> siblings)
        {
            if (siblings.Any())
            {
                foreach (var s in siblings)
                {
                    if (!APIControllerBase<T, ItemData<T, U>>.Precedes<T>(Entity, (Predecessor)s.Entity, db))
                    {
                        result.Add(s);
                    }
                }
            }
        }

        public ICollection<ItemData<T, U>> PotentialPredecessors { get; set; }
    }

    public class TreeNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TreeNode> Children { get; set; }
        // public TreeNode Parent { get; set; }
    }
}