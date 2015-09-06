using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Base
{
    /// <summary>
    /// Represents a persisted data item
    /// </summary>
    public abstract class Item
    {
        [Required]
        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The Id of the item
        /// </summary>
        public int  Id { get; set; }
        /// <summary>
        /// The time the item was created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// The ID of the creating user
        /// </summary>
        public int CreatorId { get; set; }
        /// <summary>
        /// The time the item was created
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// The ID of the modifying user
        /// </summary>
        public int ModifierId { get; set; }

        /// <summary>
        /// The ID of the parent
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// An optimization for finding the project
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Serialization control when serializing projects
        /// </summary>
        /// <param name="detailed"></param>
        public static void SetProjectDetails(bool detailed)
        {
            _detailedProject = detailed;
        }

        public Item()
        {
            CreatorId = -1;
            ModifierId = -1;
            Created = new DateTime(1900, 1, 1);
            Modified = new DateTime(1900, 1, 1);
            ParentId = -1;
            Name = String.Format("{0} Name", this.GetType().Name);
            Description = String.Format("{0} Description", this.GetType().Name);
            ProjectId = -1;
        }

        public abstract bool IsIndexed();

        protected static bool _detailedProject;

        public ICollection<Item> GetItemMembers()
        {
            var properties = new List<Item>();
            var currentProperties = this.GetType().GetProperties();
            foreach (PropertyInfo pi in currentProperties)
            {
                if (IsNamespaceNotAllowed(pi.PropertyType.Namespace) && !pi.PropertyType.IsEnum)
                {
                    properties.Add((Item)pi.GetValue(this));
                }
            }
            return properties;
        }

        public ICollection<IEnumerable> GetCollectionMembers()
        {
            var properties = new List<IEnumerable>();
            var currentProperties = this.GetType().GetProperties();
            foreach (PropertyInfo pi in currentProperties)
            {
                // If you have more than a single generic that's your problem
                if (IsCollection(pi.PropertyType.Namespace) && IsNamespaceNotAllowed(pi.PropertyType.GetGenericArguments()[0].Namespace))
                {
                    var item = (IEnumerable)pi.GetValue(this);
                    if(item != null)
                    {
                        properties.Add(item);
                    }
                    else
                    {
                        Console.WriteLine("{0} is null collection", pi.Name);
                    }
                }
            }
            return properties;
        }

        protected ICollection<int> GetIntCollection(string source, bool single = true)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<int>();
            }
            // Allow dots instead of spaces
            source = source.Replace('.', ' ');
            var ids = source.Split(' ');
            var list = new List<int>();
            foreach (var id in ids)
            {
                if (!String.IsNullOrEmpty(id))
                {
                    int i = 0;
                    if(single)
                    {
                        if (Int32.TryParse(id, out i) && !list.Contains(i))
                        {
                            list.Add(Int32.Parse(id));
                        }
                    }
                    else
                    {
                        if (Int32.TryParse(id, out i))
                        {
                            list.Add(Int32.Parse(id));
                        }
                    }
                }
            }
            return list;
        }

        protected string GetIntString(ICollection<int> value)
        {
            if (value == null || value.Count() == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var val in value)
            {
                sb.Append(val);
                sb.Append(' ');
            }
            var result = sb.ToString();
            return result;
        }

        protected IDictionary<int, string> GetStringCollection(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new Dictionary<int, string>();
            }
            var ids = source.Split(new string[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries);
            var dick = new Dictionary<int, string>();
            for (int i = 0; i < ids.Length; i+=2 )
            {
                dick[Int32.Parse(ids[i])] = ids[i + 1];
            }
            return dick;
        }

        protected string GetStringsString(IDictionary<int, string> value)
        {
            if (value == null || value.Count() == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var val in value)
            {
                sb.Append(val.Key);
                sb.Append(";;;");
                sb.Append(val.Value);
                sb.Append(";;;");
            }
            var result = sb.ToString();
            return result;
        }


        /*
        /// <summary>
        /// Clone the item before serializing - in order not to serialize the entire DB
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual Item Clone(Type t)
        {
            var currentProperties = this.GetType().GetProperties();
            var newProperties     = t.GetProperties();
            var p                 = Activator.CreateInstance(t);

            foreach (PropertyInfo pi in newProperties)
            {
                if (pi.CanWrite)
                {
					if(IsNamespaceNotAllowed(pi.PropertyType.Namespace) && 	!pi.PropertyType.IsEnum)
                    {
                        pi.SetValue(p, null, null);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
                    {
                        var collection = (IEnumerable)pi.GetValue(this);
                        var newCollection = Activator.CreateInstance(collection.GetType());
                        foreach(object o in collection)
                        {
                            // Leave only IDs
                            newCollection
                        }
                    }
					else
					{
					    var c        = (from cp in currentProperties where cp.Name == pi.Name select cp).FirstOrDefault();
                        var newValue = c == null ? (pi.PropertyType.IsValueType ? Activator.CreateInstance(pi.PropertyType) : null) : c.GetValue(this, null);

                    	pi.SetValue(p, newValue, null);
					}
                }
            }
            return (Item)p;
        }

        public virtual Item Clone()
        {
            return Clone(GetType());
        }
        */

        private bool IsNamespaceNotAllowed(string name)
        {
            return name.StartsWith("UIBuildIt.Common");//|| name.StartsWith("System.Collections.Generic");
        }

        private bool IsCollection(string name)
        {
            return name.StartsWith("System.Collections.Generic");
        }

        /// <summary>
        /// The type of the user which own this type of entity
        /// </summary>
        public virtual ProjectUserType GetOwnerType()
        {
            return ProjectUserType.Owner;
        }

        public string GetEntityFinalType()
        {
            var entityType = this.GetType();
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
            {
                entityType = entityType.BaseType;
            }
            var type = entityType.Name;
            return type;
        }


        #region Index

        public virtual ICollection<int> Index { get; set; }

        public virtual string IndexString
        {
            get
            {
                return GetIntString(Index);
            }
            set
            {
                Index = GetIntCollection(value, false);
            }
        }

        public bool ShouldSerializeIndex()
        {
            return true;
        }

        public bool ShouldSerializeIndexString()
        {
            return false;
        }

        #endregion
    }
}
