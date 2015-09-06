using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UIBuildIt.Common.Base;

namespace UIBuildIt.WebService.Controllers.API
{
    public abstract class ControllerForBase<T, U> : APIControllerBase<T, U> where T : Item, new()
        where U : ItemData<T, T>, new()
    {
        protected override System.Web.Http.IHttpActionResult Validate(T entity)
        {
            throw new NotImplementedException();
        }

        protected override ICollection<U> GetEntities(Common.Authentication.User user)
        {
            throw new NotImplementedException();
        }

        protected override void ClearEntity(T source)
        {
            throw new NotImplementedException();
        }
    }
}