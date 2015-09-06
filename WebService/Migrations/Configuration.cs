namespace UIBuildIt.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using UIBuildIt.Common;
    using UIBuildIt.Common.Authentication;
    using UIBuildIt.Common.Base;
    using UIBuildIt.Common.UseCases;
    using UIBuildIt.Models;
    using UIBuildIt.WebService.Controllers.API;

    internal sealed class Configuration : DbMigrationsConfiguration<UIBuildIt.Models.UIBuildItContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(UIBuildIt.Models.UIBuildItContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var deleted = false;
            if (deleted)
            {
                SeedUsers(context);
                SeedTests(context);
                SeedTasks(context);
            }
        }

        private void SeedTasks(UIBuildItContext context)
        {
            context.Database.ExecuteSqlCommand("delete from Sequences");
            context.Database.ExecuteSqlCommand("delete from Methods");
            context.Database.ExecuteSqlCommand("delete from Components");
            context.Database.ExecuteSqlCommand("delete from Modules");
            context.Database.ExecuteSqlCommand("delete from Tasks");
            context.Database.ExecuteSqlCommand("delete from Notes");
        }

        private void SeedTests(UIBuildItContext context)
        {
            context.Database.ExecuteSqlCommand("delete from TestParameters");
            context.Database.ExecuteSqlCommand("delete from UseCaseActions");
            context.Database.ExecuteSqlCommand("delete from Parameters");
            context.Database.ExecuteSqlCommand("delete from UseCases");
            context.Database.ExecuteSqlCommand("delete from Tests");
        }


        private void SeedUsers(UIBuildItContext context)
        {
            context.Database.ExecuteSqlCommand("delete from Users");
            context.Database.ExecuteSqlCommand("delete from Tokens");
            context.Set<User>().AddOrUpdate(
                new User()
                {
                    Name = "Default user",
                    Description = "Really Default user",
                    Email = "shy@vrigar.com",
                    Password = "a5512c3541eb5d4e4a3c414f149ff2f7" // The hash code of qwerty
                }
                );
        }
    }
}
