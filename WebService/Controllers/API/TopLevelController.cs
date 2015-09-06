using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using UIBuildIt.Models;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using StackExchange.Profiling;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class TopItemData : ItemData<Node, Node>
    {
    }

    public class Node : Item
    {
        public static UIBuildItContext db { get; set; }
        public Dictionary<string, List<Node>> Children { get; private set; }

        public Item Item { get; set; }

        public override bool IsIndexed()
        {
            return false;
        }

        public Node() { }
        public Node(Item i, ICollection<int> index = null, int counter = 1)
        {
            Children = new Dictionary<string, List<Node>>();
            Item = i;


            counter = 1;

            using (MiniProfiler.Current.Step(string.Format("Getting tasks for {0}: {1}", i.GetEntityFinalType(), i.Name)))
            {
                var tasks = GetTasks(i);
                if (tasks.Count > 0)
                {
                    int internalIndex = 1;
                    Children["Tasks"] = (from t in tasks
                                         select new Node(t, Item.Index.Concat(new List<int> { counter }).ToList(), internalIndex++)).ToList();
                    counter++;
                }
            }

            using (MiniProfiler.Current.Step(string.Format("Getting notes for {0}: {1}", i.GetEntityFinalType(), i.Name)))
            {
                var notes = i.GetDirectNotesMinimal(db);
                if (notes.Count > 0)
                {
                    int internalIndex = 1;
                    Children["Notes"] = (from t in notes
                                         select new Node(t, Item.Index.Concat(new List<int> { counter }).ToList(), internalIndex++)).ToList();
                    counter++;
                }
            }

            using (MiniProfiler.Current.Step(string.Format("Getting children for {0}: {1}", i.GetEntityFinalType(), i.Name)))
            {
                foreach (var item in i.GetChildrenMinimal(db))
                {
                    int internalIndex = 1;
                    if (item.Value.Count > 0)
                    {
                        Children[item.Key] = (from d in item.Value select (new Node(d, Item.Index.Concat(new List<int> { counter }).ToList(), internalIndex++))).ToList();
                        counter++;
                    }
                }
            }
        }

        public  ICollection<Item> GetTasks(Item entity)
        {
            var type = entity.GetEntityFinalType();
            var tasks = (from t in (from t in db.Tasks
                         where t.ParentId == entity.Id && t.ParentType == type
                         select new 
                         {
                             Name = t.Name,
                             Id = t.Id,
                             IndexString = t.IndexString,
                             ParentType = t.ParentType,
                             ParentId = t.ParentId
                         }
                         ).ToList()
                         select (Item)new Task() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList();

            if(entity is Milestone)
            {
                var more = (from t in (from t in db.Tasks
                            where t.ContainerType == ContainerType.Milestone && t.ContainerID == entity.Id
                            select new 
                         {
                             Name = t.Name,
                             Id = t.Id,
                             IndexString = t.IndexString,
                             ParentType = t.ParentType,
                             ParentId = t.ParentId
                         }).ToList()
                            select (Item)new Task() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList();
                tasks = tasks == null ? more.ToList() : tasks.Concat(more).ToList();
            }

            if (entity is Sprint)
            {
                var more = (from t in (from t in db.Tasks
                            where t.ContainerType == ContainerType.Sprint && t.ContainerID == entity.Id
                            select new 
                         {
                             Name = t.Name,
                             Id = t.Id,
                             IndexString = t.IndexString,
                             ParentType = t.ParentType,
                             ParentId = t.ParentId
                         }).ToList()
                            select (Item)new Task() { Name = t.Name, Id = t.Id, IndexString = t.IndexString, ParentType = t.ParentType, ParentId = t.ParentId }).ToList();
                tasks = tasks == null ? more.ToList() : tasks.Concat(more).ToList();
            }
            return tasks.OrderBy(m => m.HashIndex()).ToList();
        }

        
    }

    public class TopLevelController : APIControllerBase<Node, TopItemData>
    {
        protected override ICollection<TopItemData> GetEntities(User user)
        {
            var data = new TopItemData();
            Node.db = db;
            var result = (from p in
                              (from p in db.Projects
                               where p.Owner.Id == user.Id
                               select p).ToList()
                          select new TopItemData() { Entity = new Node(p) }).ToList();

            return result;
        }

        protected override TopItemData CreateData(Node source, int id)
        {
            throw new NotImplementedException();
        }

        protected override void ClearEntity(Node source)
        {
            throw new NotImplementedException();
        }

        protected override IHttpActionResult Validate(Node entity)
        {
            throw new NotImplementedException();
        }

        protected override Project GetProject(Node entity)
        {
            throw new NotImplementedException();
        }
    }
}