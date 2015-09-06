using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using NgFlowSample.Models;
using NgFlowSample.Services;
using UIBuildIt.Models;
using System.Drawing;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using UIBuildIt.Common.Base;
using NLog;
using Newtonsoft.Json.Linq;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Security;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Tasks;
using System.Data.Entity;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    [RoutePrefix("api")]
    public class FileUploadController : ApiController
    {
        protected FileUploadController()
        {
            Log = LogManager.GetLogger(GetType().FullName);
        }

        protected Logger Log { get; private set; }

        private readonly string path =  "~/App_Data/Tmp/FileUploads";
        //[Route("Upload"), HttpPost]
        [AuthorizeToken]
        public async System.Threading.Tasks.Task<IHttpActionResult> Post()
        {
            var user = (User)Request.Properties["user"];
            if (user == null)
            {
                return ResponseMessage( this.Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else if (!Request.Content.IsMimeMultipartContent())
            {
                return ResponseMessage(this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            } 

            var uploadProcessor = new FlowUploadProcessor(path);
            await uploadProcessor.ProcessUploadChunkRequest(Request);

            if (uploadProcessor.IsComplete)
            {
                // UIBuildItContext db = new UIBuildItContext();
                string output;
                List<Item> res = null;
                var root = HttpContext.Current.Server.MapPath(path);
                var fullPath = root + "/" + uploadProcessor.MetaData.FlowFilename;
                using (var bigStream = new GZipStream(File.Open(fullPath, FileMode.Open), CompressionMode.Decompress))
                using (var bigStreamOut = new MemoryStream())
                {
                    bigStream.CopyTo(bigStreamOut);
                    output = Encoding.UTF8.GetString(bigStreamOut.ToArray());
                }
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.Objects;
                    res = JsonConvert.DeserializeObject<List<Item>>(output, settings);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                ProjectFromJson(user, res);
                return Ok<string>(uploadProcessor.MetaData.FlowFilename);
            }
           
            return Ok();
        }

        private void ProjectFromJson(User user, List<Item> res)
        {
            try
            {
                UIBuildItContext db = new UIBuildItContext();
                var groups = from t in res
                             group t by t.GetEntityFinalType() into g
                             select g;
                var entityByType = new Dictionary<string, List<Item>>();
                foreach (var g in groups)
                {
                    entityByType[g.Key] = g.ToList();
                }
                var project = entityByType["Project"].FirstOrDefault();
                if (project == null)
                {
                    return;
                }
                var oldNewItems = new Dictionary<Item, int>();
                var oldProjectId = project.Id;
                ((Project)project).Owner = user;
                SetUnchanged(((Project)project).Owner, db);
                project = db.Projects.Add((Project)project);
                db.SaveChanges();
                oldNewItems[project] = oldProjectId;
                

                ICollection<Item> tmp = res;

                UpdateRootNotesAndTasks(res, db, oldProjectId, project, oldNewItems, entityByType);
                UpdateEntitiesRecursive(res, db, project, oldProjectId, oldNewItems);

                foreach (var item in res)
                {
                    if (item is Predecessor)
                    {
                        var pred = (Predecessor)item;
                        var newPredList = new List<int>();
                        foreach (var id in pred.PredecessorIds)
                        {
                            var predeccessor = oldNewItems.Keys.FirstOrDefault(e => oldNewItems[e] == id && e.GetEntityFinalType() == item.GetEntityFinalType());
                            if (predeccessor != null)
                            {
                                newPredList.Add(predeccessor.Id);
                            }
                        }
                        pred.PredecessorIds = newPredList;
                    }

                    if (item is Task)
                    {
                        var task = (Task)item;
                        var newReqList = new List<int>();
                        foreach (var id in task.RequirementIds)
                        {
                            var requirement = oldNewItems.Keys.FirstOrDefault(e => oldNewItems[e] == id && e.GetEntityFinalType() == "Requirement");
                            if (requirement != null)
                            {
                                newReqList.Add(requirement.Id);
                            }
                        }
                        task.RequirementIds = newReqList;
                    }
                }
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void UpdateEntitiesRecursive(List<Item> res, UIBuildItContext db, Item newParent, int oldParentId, Dictionary<Item, int> oldNewItems)
        {
            var tmp = UpdateEntities(res, db, oldParentId, newParent, oldNewItems);
            if (tmp != null)
            {
                res = tmp.ToList();
                foreach(var i in res)
                {
                    UpdateEntitiesRecursive(res, db, i, oldNewItems[i], oldNewItems);
                }
            }
        }

        private void UpdateRootNotesAndTasks(List<Item> res, UIBuildItContext db, int oldProjectId, Item project, Dictionary<Item, int> oldNewItems, IDictionary<string, List<Item>> entityByType)
        {
            if (entityByType.ContainsKey("Task"))
            {
                var topLevel = (from t in entityByType["Task"]
                                where ((IParentType)t).ParentType == "Project" && t.ParentId == oldProjectId
                                select t).ToList();
                foreach (var r in topLevel)
                {
                    r.ParentId = project.Id;
                    r.ProjectId = project.Id;
                    int i = r.Id;
                    var item = db.Tasks.Add((Task)r);
                    db.SaveChanges();
                    oldNewItems[item] = i;
                }

            }

            if (entityByType.ContainsKey("Note"))
            {
                var topLevel = (from t in entityByType["Note"]
                            where ((IParentType)t).ParentType == "Project" && t.ParentId == oldProjectId
                            select t).ToList();
                foreach (var r in topLevel)
                {
                    r.ParentId = project.Id;
                    r.ProjectId = project.Id;
                    int i = r.Id;
                    var item = db.Notes.Add((Note)r);
                    db.SaveChanges();
                    oldNewItems[item] = i;
                }
            }

        }

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

        /// <summary>
        /// Returns the updated list or null of ni changes were made
        /// </summary>
        /// <param name="res">List of entities</param>
        /// <param name="db">Database reference</param>
        /// <param name="oldParent">Parent in old project</param>
        /// <param name="newParent">Parent in new project</param>
        /// <returns></returns>
        private static ICollection<Item> UpdateEntities(List<Item> res, UIBuildItContext db, int oldParentId, Item newParent, IDictionary<Item, int> oldNewIds)
        {
            var newList = new List<Item>();
            var changed = false;
            foreach (var entity in res)
            {
                if (entity.ParentId == oldParentId && entity.GetParentType(db).Name == newParent.GetEntityFinalType())
                {
                    var oldId = entity.Id;
                    entity.ProjectId = newParent.ProjectId;
                    entity.ParentId = newParent.Id;
                    var newEntity = (Item)(db.Set(entity.GetType()).Add(entity));
                    newList.Add(newEntity);
                    changed = true;
                    db.SaveChanges();
                    oldNewIds.Add(entity, oldId);
                }
                else
                {
                    newList.Add(entity);
                }
                
            }
            if (changed)
            {
                return newList;
            }
            return null;
            
        }

        private static ICollection<T> CreateTopItems<T>(UIBuildItContext db, Dictionary<string, List<Item>> entityByType, Item project) where T: Item
        {
            var result = new List<T>();
            var topLevel = (from r in entityByType[typeof(T).Name]
                                where ((IParentType)r).ParentType == "Project"
                                select r).ToList();
            foreach (var r in topLevel)
            {
                r.ParentId = project.Id;
                r.ProjectId = project.Id;
                var connected = db.Set<T>().Add((T)r);
                db.SaveChanges();
                result.Add(connected);
                SetMetadata(r, connected, db, entityByType, project);
                if(r is IParentType)
                {
                    SetSubItems<T>(db, entityByType, project, result, r);
                }
            }
            return result;
        }

        private static void SetMetadata<T>(T origin, T updated, UIBuildItContext db, Dictionary<string, List<Item>> entityByType, Item project) where T: Item
        {
            var topLevel = (from t in entityByType["Task"]
                            where ((IParentType)t).ParentType == typeof(T).Name && t.ParentId == origin.Id
                            select t).ToList();
            foreach (var r in topLevel)
            {
                r.ParentId = updated.Id;
                r.ProjectId = updated.ProjectId;
                var connected = db.Tasks.Add((UIBuildIt.Common.Tasks.Task)r);
                db.SaveChanges();
                SetMetadata(r, connected, db, entityByType, project);
                if (r is IParentType)
                {
                    SetSubItems<T>(db, entityByType, project, null, r);
                }
            }

            topLevel = (from t in entityByType["Note"]
                            where ((IParentType)t).ParentType == typeof(T).Name && t.ParentId == origin.Id
                            select t).ToList();
            foreach (var r in topLevel)
            {
                r.ParentId = updated.Id;
                r.ProjectId = updated.ProjectId;
                var connected = db.Notes.Add((Note)r);
                db.SaveChanges();
                SetMetadata(r, connected, db, entityByType, project);
                if (r is IParentType)
                {
                    SetSubItems<T>(db, entityByType, project, null, r);
                }
            }
        }

        private static void SetSubItems<T>(UIBuildItContext db, Dictionary<string, List<Item>> entityByType, Item project, List<T> result, Item parent) where T : Item
        {
            var subItems = (from s in entityByType[typeof(T).Name]
                            where ((IParentType)s).ParentType == typeof(T).Name && s.ParentId == parent.Id
                            select s).ToList();

            foreach (var sub in subItems)
            {
                sub.ParentId = parent.Id;
                sub.ProjectId = project.Id;
                var subItem = db.Set<T>().Add((T)sub);
                db.SaveChanges();
                if (result != null)
                {
                    result.Add(subItem);
                }
                
                SetSubItems(db, entityByType, project, result, sub);
            }
        }
     


        [Route("Get"), HttpGet]
        public virtual IHttpActionResult Get(string fileId)
        {
            var root = HttpContext.Current.Server.MapPath(path);
            var fullPath = string.Format("{0}/{1}", root, fileId);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(fullPath, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");
            return ResponseMessage(result);
        }

        [Route("FileUpload"), HttpGet]
        public virtual IHttpActionResult Get(int flowChunkNumber,
            int flowChunkSize,
            int flowCurrentChunkSize,
            int flowTotalSize,
            string flowIdentifier,
            string flowFilename,
            string flowRelativePath,
            int flowTotalChunks,
            string requestVerificationToken,
            int blueElephant)
        {
            var flowMeta = new FlowMetaData()
            {
                BlueElephant = blueElephant,
                FlowChunkNumber = flowChunkNumber,
                FlowChunkSize = flowChunkSize,
                FlowCurrentChunkSize = flowCurrentChunkSize,
                FlowFilename = flowFilename,
                FlowIdentifier = flowIdentifier,
                FlowRelativePath = flowRelativePath,
                FlowTotalChunks = flowTotalChunks,
                FlowTotalSize = flowTotalSize,
                RequestVerificationToken = requestVerificationToken
            };
            if (FlowUploadProcessor.HasRecievedChunk(flowMeta))
            {
                return Ok();
            }
            
            return NotFound();
        }

        
    }
}