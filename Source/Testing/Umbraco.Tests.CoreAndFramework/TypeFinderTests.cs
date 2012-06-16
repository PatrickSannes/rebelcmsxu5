using System;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Examine;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class TypeFinderFixture
    {
        [Test]
        public void DelegatePropertyGetter_CanGetProperty()
        {
            var guid = Guid.NewGuid();
            var blah = "hello";
            var tryTuple = new Tuple<Guid, string>(guid, blah);

            var netAccessor = ExpressionHelper.GetPropertyInfo<Tuple<Guid, string>, Guid>(x => x.Item1);
            var netValue = netAccessor.GetValue(tryTuple, null);

            var customAccessor = TypeFinder.DynamicMemberAccess.GetViaDelegate(netAccessor, tryTuple);
            Assert.That(netValue, Is.EqualTo(guid));
            Assert.That(customAccessor, Is.EqualTo(guid));
        }
    }


    [PropertyEditor("87B8E882-38A1-4EE2-9F2E-8FE267B8F258", "benchmarkPropEd", "Benchmark Prop Ed")]
    public class BenchmarkTestPropertyEditor : PropertyEditor
    {

    }
    
    /// <summary>
    /// Summary description for TypeFinderTests
    /// </summary>
    [TestFixture]
    [Ignore("This is a benchark test")]
    public class TypeFinderTests
    {
        [Test]
        public void Benchmark_Finding_First_Type_In_Assemblies()
        {
            var timer = new Stopwatch();
            var assemblies = new[]
                {
                    //both contain the type
                    this.GetType().Assembly, 
                    typeof (MandatoryPropertyEditor).Assembly,
                    //these dont contain the type
                    typeof(StandardAnalyzer).Assembly,
                    typeof(NSubstitute.Substitute).Assembly,
                    typeof(Remotion.Linq.DefaultQueryProvider).Assembly,
                    typeof(NHibernate.IdentityEqualityComparer).Assembly,
                    typeof(System.Guid).Assembly,
                    typeof(NUnit.Framework.Assert).Assembly,
                    typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
                    typeof(System.Xml.NameTable).Assembly,
                    typeof(System.Configuration.GenericEnumConverter).Assembly,
                    typeof(System.Web.SiteMap).Assembly,
                    typeof(System.Data.SQLite.CollationSequence).Assembly,
                    typeof(System.Web.Mvc.ActionResult).Assembly,
                    typeof(Umbraco.Hive.LazyRelation<>).Assembly,
                    typeof(Umbraco.Framework.DependencyManagement.AbstractContainerBuilder).Assembly,
                    typeof(FixedIndexedFields).Assembly,
                    typeof(Umbraco.Framework.Persistence.DefaultAttributeTypeRegistry).Assembly,
                    typeof(Umbraco.Framework.Security.FixedPermissionTypes).Assembly
                };

            //we'll use PropertyEditors for this tests since there are some int he text Extensions project

            var finder = new TypeFinder();

            timer.Start();
            var found1 = finder.FindClassesOfType<PropertyEditor, AssemblyContainsPluginsAttribute>(assemblies);
            timer.Stop();

            Console.WriteLine("Total time to find propery editors (" + found1.Count() + ") in " + assemblies.Count() + " assemblies using AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

            timer.Start();
            var found2 = finder.FindClassesOfType<PropertyEditor>(assemblies);
            timer.Stop();

            Console.WriteLine("Total time to find propery editors (" + found2.Count() + ") in " + assemblies.Count() + " assemblies without AssemblyContainsPluginsAttribute: " + timer.ElapsedMilliseconds);

        }
    }
}