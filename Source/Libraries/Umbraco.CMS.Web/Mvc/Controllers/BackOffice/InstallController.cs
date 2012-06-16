using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Install;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NuGet;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.Install;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Framework;

using Umbraco.Framework.Configuration;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Framework.Tasks;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    [UmbracoInstallAuthorizeAttribute]
    [HandleError(View = "Exception")]
    [HandleAuthorizationErrors(false)]
    public class InstallController : Controller
    {
        private readonly IBackOfficeRequestContext _requestContext;
        private readonly PackageInstallUtility _packageInstallUtility;

        public InstallController(IBackOfficeRequestContext requestContext)
        {
            _requestContext = requestContext;
            //set the custom installer action invoker
            ActionInvoker = new UmbracoInstallActionInvoker(requestContext);
            _packageInstallUtility = new PackageInstallUtility(requestContext);
        }


        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //public ActionResult License()
        //{
        //    return View();
        //}

        /// <summary>
        /// Shows the UI to input the database connection details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Database()
        {

            if (!_requestContext.Application.AllProvidersInstalled())
            {
                return View();
            }

            //if they are all installed, then we can show the User action
            return RedirectToAction("StarterKit");


        }

        /// <summary>
        /// Creates the configurations files based on the posted values, then redirects the action to initiate the installation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Database")]
        public ActionResult DatabaseForm(DatabaseInstallModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //create a new web.config file in /plugins for the providers to write to
            var configFile = new FileInfo(
                Path.Combine(HttpContext.Server.MapPath("~/App_Data/Umbraco/HiveConfig"), "web.config"));
            Directory.CreateDirectory(configFile.Directory.FullName);
            var configXml = DeepConfigManager.CreateNewConfigFile(configFile, true);

            //for each hive provider, get them to serialize their config to our xml file
            foreach (var hive in _requestContext.Application.Hive.GetAllReadWriteProviders())
            {
                var installStatus = hive.Bootstrapper.GetInstallStatus();

                //even if it's not completed, then overwrite
                if (installStatus.StatusType == InstallStatusType.RequiresConfiguration
                    || installStatus.StatusType == InstallStatusType.Pending
                    || installStatus.StatusType == InstallStatusType.TriedAndFailed)
                {
                    //TODO: This will currently only work for the nhibernate provider since we're just passing in the DatabaseInstallmodel as the install params
                    hive.Bootstrapper.ConfigureApplication(hive.ProviderMetadata.Alias, configXml, new BendyObject(model));
                }
            }

            // Test connection strings
            if (model.DatabaseType != DatabaseServerType.SQLCE)
            {
                // We don't test SQL CE connection string as we will just create the DB if it doesn't exist
                try
                {
                    foreach (
                        var connStringElement in
                            configXml.Element("configuration").Element("connectionStrings").Descendants("add"))
                    {
                        var connString = connStringElement.Attribute("connectionString").Value;
                        var providerName = connStringElement.Attribute("providerName").Value;

                        var factory = DbProviderFactories.GetFactory(providerName);
                        using (var conn = factory.CreateConnection())
                        {
                            conn.ConnectionString = connString;
                            conn.Open();

                            if (model.DatabaseType == DatabaseServerType.Custom || model.DatabaseType == DatabaseServerType.MSSQL)
                            {
                                var cmd = conn.CreateCommand();
                                cmd.CommandText = "SELECT SERVERPROPERTY('productversion')";

                                var reader = cmd.ExecuteReader();
                                while(reader.Read())
                                {
                                    var versionString = reader[0].ToString();
                                    var version = Convert.ToInt32(versionString.Replace(".", "").PadRight(10, '0'));
                                    if(version < 1000160000) // SQL 2008 RTM
                                    {
                                        throw new ApplicationException(string.Format("Unsupported version of SQL Server. SQL 2008 RTM (10.00.1600) or greater is required, but found {0}.", versionString));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (global::System.Exception ex)
                {
                    ModelState.AddModelError("", "An error occured while testing connection string: " + ex.Message);

                    return View(model);
                }
            }

            //now save the new config file
            configXml.Save(configFile.FullName);

            return RedirectToAction("DatabaseInstall");
        }

        /// <summary>
        /// Displays a simple view that makes an ajax call to restart the app and install the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DatabaseInstall()
        {
            //lets verify that the config file exists
            var configFile = new FileInfo(
                Path.Combine(HttpContext.Server.MapPath("~/App_Data/Umbraco/HiveConfig"), "web.config"));
            if (!configFile.Exists)
            {
                throw new FileNotFoundException("Could not find the web.config file containing Hive configuration at location: " + configFile.FullName);
            }

            return View();
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CreateUser()
        {
            var model = new CreateNewUserModel
            {
                Name = "Administrator",
                Email = "admin@domain.com",
                Username = "admin",
                SignUpForNewsletter = true
            };

            return View(model);
        }

        /// <summary>
        /// Creates the user form.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateUser")]
        public ActionResult CreateUserForm(CreateNewUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Create user entity
            var admin = new User()
            {
                Name = model.Name,
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                StartContentHiveId = FixedHiveIds.ContentVirtualRoot,
                StartMediaHiveId = FixedHiveIds.MediaVirtualRoot,
                Applications = new List<string>(new[] { "content", "media", "settings", "developer", "users" }),
                IsApproved = true,
                SessionTimeout = 60
            };

            using (var uow = _requestContext.Application.Hive.OpenWriter<IContentStore>())
            {
                // Find admin usergroup
                var adminUserGroup = uow.Repositories.GetEntityByRelationType<UserGroup>(
                                    FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                                    .Where(y => y.Name == "Administrator")
                                    .FirstOrDefault();

                // Add user to admin role
                if (adminUserGroup != null)
                    admin.RelationProxies.EnlistParent(adminUserGroup, FixedRelationTypes.UserGroupRelationType);

                // Save user
                uow.Repositories.AddOrUpdate(admin);

                // Comit user
                uow.Complete();
            }

            //TODO: Subscribe user to newsletter
            if(model.SignUpForNewsletter)
            {
                try
                {
                    new WebClient()
                        .UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", new NameValueCollection()
                    {
                        {
                            "name",
                            model.Name
                        },
                        {
                            "email",
                            model.Email
                        }
                    });
                }
                catch(Exception ex)
                {
                    LogHelper.TraceIfEnabled<InstallController>("Unable to subscribe user to newsletter: {0}", () => ex.Message);
                }
            }

            return RedirectToAction("StarterKit");
        }

        /// <summary>
        /// Displays the UI to install a starter kit
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [InstalledFilter]
        public ActionResult StarterKit()
        {
            var packages = GetLocalPackages().Where(x => !x.IsPackageInstalled);
            if (packages.Any())
                return View(packages);
            else
                return RedirectToAction("Finish");
        }

        /// <summary>
        /// Handles the post to install a starter kit and presents a display that makes an ajax call to restart the app and install the kit
        /// </summary>
        /// <param name="starterKitId"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("StarterKit")]
        [InstalledFilter]
        public ActionResult StarterKitForm(string starterKitId)
        {
            Mandate.ParameterNotNull(starterKitId, "starterKitName");
            
            var package = _requestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(starterKitId);
            _requestContext.PackageContext.LocalPackageManager.InstallPackage(package, false);

            SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

            return RedirectToAction("PostStarterKitInstall", new { id = package.Id });
        }

        /// <summary>
        /// An action to recycle app domain after the starter kit is intalled
        /// </summary>
        /// <param name="id">The Hive id of the package</param>
        /// <returns></returns>
        [HttpGet]
        [InstalledFilter]
        public ActionResult PostStarterKitInstall(string id)
        {
            if (id == null) throw new ArgumentNullException("id");
            return View((object)id);
        }

        [HttpGet]
        [InstalledFilter]
        public ActionResult Finish()
        {
            this.HttpContext.UmbracoLogout();

            return View();
        }

        /// <summary>
        /// Actually does the shutting down of the app which is called by ajax, if onlyCheck is true, this will just
        /// return the status
        /// </summary>
        /// <param name="onlyCheck"></param> 
        /// <returns></returns>
        [HttpPost]
        public JsonResult PerformRecycleApplication(bool onlyCheck)
        {
            if (!onlyCheck)
            {
                UmbracoApplicationContext.RestartApplicationPool(HttpContext);
                return Json(new { status = "restarting" });
            }

            return Json(new { status = "restarted" });
        }


        /// <summary>
        /// Does the installation of Hive providers based on the db connection info
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PerformInstall(bool onlyCheck)
        {

            if (!onlyCheck)
            {



                var statuses = new List<InstallStatus>();
                var context = new TaskExecutionContext(this, new TaskEventArgs(_requestContext.Application.FrameworkContext, null));
                //for each hive provider, get them to serialize their config to our xml file
                foreach (var hive in _requestContext.Application.Hive.GetAllReadWriteProviders())
                {
                    var installStatus = hive.Bootstrapper.GetInstallStatus();

                    //it needs to be configured by now, 
                    if (installStatus.StatusType == InstallStatusType.Pending
                        || installStatus.StatusType == InstallStatusType.TriedAndFailed)
                    {
                        var status = hive.Bootstrapper.TryInstall();
                        statuses.Add(status);
                        //now check if was successful and see if there are some tasks to run
                        if (status.StatusType == InstallStatusType.Completed)
                        {
                            foreach (var t in status.TasksToRun)
                            {
                                t.Execute(context);
                            }
                        }
                    }
                }

                //TODO: Handle this
                ////if for some reason there's nothing been installed
                //if (statuses.Count == 0)
                //{                    
                //    if (!_requestContext.Application.AnyProvidersHaveStatus(InstallStatusType.Pending))
                //    {
                //        //go back to step one
                //        return RedirectToAction("Index");
                //    }    
                //}
                

                //now, if there were no errors, then we can run the post hive install tasks
                if (!statuses.Any(x => x.StatusType == InstallStatusType.TriedAndFailed))
                {
                    context = _requestContext.Application.FrameworkContext.TaskManager.ExecuteInContext(
                        TaskTriggers.Hive.PostInstall,
                        this,
                        new TaskEventArgs(_requestContext.Application.FrameworkContext, new EventArgs()));

                }

                if (!context.Exceptions.Any())
                {
                    return Json(new
                    {
                        status = "complete",
                        message = "Installation completed!",
                        percentage = 100
                    });
                }
                else
                {
                    context.Exceptions.AddRange(statuses.Where(x => x.StatusType == InstallStatusType.TriedAndFailed).SelectMany(x => x.Errors).ToArray());

                    var errorMsg = ControllerContext.RenderPartialViewToString("Error", context.Exceptions);
                    return Json(new
                    {
                        status = "failure",
                        message = errorMsg,
                        percentage = 100,
                        fail = true
                    });

                }

            }

            return Json(new
            {
                percentage = 40
            });
        }

        /// <summary>
        /// Runs the actions for a package, this is generally run after the app pool is recycled
        /// </summary>
        /// <param name="id">the nuget id of the package</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RunPackageActions(string id)
        {
            bool onlyCheck;
            if (!bool.TryParse(ValueProvider.GetValue("onlyCheck").AttemptedValue, out onlyCheck))
            {
                throw new InvalidCastException("The required onlyCheck parameter is not found or was not recognized as boolean");
            }

            if (!onlyCheck)
            {                
                var nugetPackage = _requestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(id);
                
                //Run the configuration taks                
                //get the package with the name and ensure it exists
                
                if (nugetPackage != null)
                {
                    var packageFolderName = _requestContext.PackageContext.LocalPathResolver.GetPackageDirectory(nugetPackage);

                    //Run the post pacakge install tasks
                    var taskExeContext = _packageInstallUtility.GetTaskExecutionContext(nugetPackage, packageFolderName, PackageInstallationState.Installing , this);
                    _packageInstallUtility.RunPostPackageInstallActions(PackageInstallationState.Installing, taskExeContext, packageFolderName);
                }

                return Json(new
                {
                    status = "complete",
                    message = "Installation completed!",
                    percentage = 100
                });

            }

            return Json(new
            {
                percentage = 40
            });


        }

        //[HttpPost]
        //public JsonResult CheckAppRestarted()
        //{
        //    //TODO: Check install status

        //    return Json(new
        //        {
        //            status = "complete",
        //            message = "Installation completed!",
        //            percentage = 100
        //        });
        //}


        private IEnumerable<PackageModel> GetLocalPackages()
        {
            var packages = _requestContext.PackageContext.LocalPackageManager.SourceRepository
                .GetPackages()
                .ToArray();

            return packages.Select(x => new PackageModel
            {
                Metadata = x,
                IsPackageInstalled = x.IsInstalled(_requestContext.PackageContext.LocalPackageManager.LocalRepository)
            }).ToArray();

        }





    }
}