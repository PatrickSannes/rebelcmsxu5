using System;
using System.Globalization;
using System.Linq;
using System.Web.Security;
using Examine;
using NUnit.Framework;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive.Providers.Membership.Mapping;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    [TestFixture]
    public class MembershipTypeMappingTests
    {
        private MembershipWrapperModelMapper _mapper;
        private IAttributeTypeRegistry _attributeTypeRegistry = new CmsAttributeTypeRegistry();

        private readonly AttributeType _stringAttType = new AttributeType("text", "Text", "Text field", new StringSerializationType());

        [TestFixtureSetUp]
        public void Setup()
        {
            var frameworkContext = new FakeFrameworkContext();
            var frameworkModelMapper = new FrameworkModelMapper(frameworkContext);
            _mapper = new MembershipWrapperModelMapper(_attributeTypeRegistry, frameworkContext);

            frameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] {frameworkModelMapper, _mapper}));
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
        }

        //[Test]
        //public void From_MembershipUser_To_Typed_Entity()
        //{

        //    var input = new MembershipUser("test", "Shannon", Guid.NewGuid(), "test@test.com", "what is my name?", "this is a member", true, false, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-9), DateTime.MinValue);
        //    var props = TypeFinder.CachedDiscoverableProperties(input.GetType(), mustWrite: false)
        //                    .Where(x => !x.GetIndexParameters().Any()).ToArray();

        //    var output = _mapper.Map<MembershipUser, TypedEntity>(input);

        //    Assert.AreEqual(props.Count(), output.Attributes.Count());
        //    Assert.AreEqual(input.UserName, output.Attributes["UserName".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.ProviderUserKey, output.Attributes["ProviderUserKey".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.Email, output.Attributes["Email".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.PasswordQuestion, output.Attributes["PasswordQuestion".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.Comment, output.Attributes["Comment".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.IsApproved, output.Attributes["IsApproved".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.IsLockedOut, output.Attributes["IsLockedOut".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.LastLockoutDate, output.Attributes["LastLockoutDate".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.CreationDate, output.Attributes["CreationDate".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.LastLoginDate, output.Attributes["LastLoginDate".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.LastActivityDate, output.Attributes["LastActivityDate".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.LastPasswordChangedDate, output.Attributes["LastPasswordChangedDate".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.IsOnline, output.Attributes["IsOnline".ToUmbracoAlias()].DynamicValue);
        //    Assert.AreEqual(input.ProviderName, output.Attributes["ProviderName".ToUmbracoAlias()].DynamicValue);
        //}

        [Test]
        public void From_Typed_Entity_To_Existing_MembershipUser()
        {
            var input = new Member()
                {
                    Comments = "some comments",
                    Email = "test@test.com",
                    Id = new HiveId(Guid.NewGuid()),
                    IsApproved = true,
                    IsLockedOut = false,
                    IsOnline = true,
                    LastLoginDate = DateTimeOffset.UtcNow.AddDays(-10),
                    LastActivityDate = DateTimeOffset.UtcNow.AddDays(-9),
                    LastLockoutDate = DateTimeOffset.MinValue,
                    Name = "Test",
                    LastPasswordChangeDate = DateTimeOffset.Now.AddDays(-20),
                    Password = "blah",
                    PasswordQuestion = "My question is?",
                    Username = "test",
                    UtcCreated = DateTimeOffset.UtcNow.AddDays(-100),
                    UtcModified = DateTimeOffset.UtcNow,
                    UtcStatusChanged = DateTimeOffset.UtcNow
                };
            
            //create a member object to map to with data that will not match the above so we can verify the mapping worked.
            var output = new MembershipUser("test", "dontmatch", "dontmatch", "dont@match.com", "dontmatch", "dontmatch", false, true, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);

            _mapper.Map(input, output);

            //Assert.AreEqual(input.Username, output.UserName);
            //Assert.AreEqual(input.ProviderUserKey, output.ProviderUserKey);
            Assert.AreEqual(input.Email, output.Email);
            //Assert.AreEqual(input.PasswordQuestion, output.PasswordQuestion);
            Assert.AreEqual(input.Comments, output.Comment);
            Assert.AreEqual(input.IsApproved, output.IsApproved);
            //Assert.AreEqual(input.IsLockedOut, output.IsLockedOut);
            //Assert.AreEqual(input.LastLockoutDate.UtcDateTime, output.LastLockoutDate.ToUniversalTime());
            //Assert.AreEqual(input.UtcCreated.UtcDateTime, output.CreationDate.ToUniversalTime());
            Assert.AreEqual(input.LastLoginDate.UtcDateTime, output.LastLoginDate.ToUniversalTime());
            Assert.AreEqual(input.LastActivityDate.UtcDateTime, output.LastActivityDate.ToUniversalTime());
            //Assert.AreEqual(input.LastPasswordChangeDate.UtcDateTime, output.LastPasswordChangedDate.ToUniversalTime());
            //Assert.AreEqual(input.IsOnline, output.IsOnline);
        }

        [Test]
        public void From_MembershipUser_To_Typed_Entity()
        {

            var input = new MembershipUser("test", "Shannon", Guid.NewGuid(), "test@test.com", "what is my name?", "this is a member", true, false, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-9), DateTime.MinValue);

            var output = _mapper.Map<MembershipUser, TypedEntity>(input);

            Assert.AreEqual(input.UserName, output.Attribute<string>(MemberSchema.UsernameAlias));
            Assert.AreEqual(input.ProviderUserKey, output.Id.Value.Value);
            Assert.AreEqual(input.Email, output.Attribute<string>(MemberSchema.EmailAlias));
            Assert.AreEqual(input.PasswordQuestion, output.Attribute<string>(MemberSchema.PasswordQuestionAlias));
            Assert.AreEqual(input.Comment, output.Attribute<string>(MemberSchema.CommentsAlias));
            Assert.AreEqual(input.IsApproved, output.Attribute<bool>(MemberSchema.IsApprovedAlias));
            Assert.AreEqual(input.IsLockedOut, output.Attribute<bool>(MemberSchema.IsLockedOutAlias));
            Assert.AreEqual(input.LastLockoutDate.ToUniversalTime(), output.Attribute<DateTimeOffset>(MemberSchema.LastLockoutDateAlias).UtcDateTime);
            Assert.AreEqual(input.CreationDate.ToUniversalTime(), output.UtcCreated.UtcDateTime);
            Assert.AreEqual(input.LastLoginDate.ToUniversalTime(), output.Attribute<DateTimeOffset>(MemberSchema.LastLoginDateAlias).UtcDateTime);
            Assert.AreEqual(input.LastActivityDate.ToUniversalTime(), output.Attribute<DateTimeOffset>(MemberSchema.LastActivityDateAlias).UtcDateTime);
            Assert.AreEqual(input.LastPasswordChangedDate.ToUniversalTime(), output.Attribute<DateTimeOffset>(MemberSchema.LastPasswordChangeDateAlias).UtcDateTime);
            Assert.AreEqual(input.IsOnline, output.Attribute<bool>(MemberSchema.IsOnlineAlias));
            //Assert.AreEqual(input.ProviderName, output.ProviderName);
        }

        [Test]
        public void From_MembershipUser_To_Member()
        {

            var input = new MembershipUser("test", "Shannon", Guid.NewGuid(), "test@test.com", "what is my name?", "this is a member", true, false, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-9), DateTime.MinValue);
            
            var output = _mapper.Map<MembershipUser, Member>(input);

            Assert.AreEqual(input.UserName, output.Username);
            Assert.AreEqual(input.ProviderUserKey, output.ProviderUserKey);
            Assert.AreEqual(input.Email, output.Email);
            Assert.AreEqual(input.PasswordQuestion, output.PasswordQuestion);           
            Assert.AreEqual(input.Comment, output.Comments);
            Assert.AreEqual(input.IsApproved, output.IsApproved);
            Assert.AreEqual(input.IsLockedOut, output.IsLockedOut);
            Assert.AreEqual(input.LastLockoutDate.ToUniversalTime(), output.LastLockoutDate.UtcDateTime);
            Assert.AreEqual(input.CreationDate.ToUniversalTime(), output.UtcCreated.UtcDateTime);
            Assert.AreEqual(input.LastLoginDate.ToUniversalTime(), output.LastLoginDate.UtcDateTime);
            Assert.AreEqual(input.LastActivityDate.ToUniversalTime(), output.LastActivityDate.UtcDateTime);
            Assert.AreEqual(input.LastPasswordChangedDate.ToUniversalTime(), output.LastPasswordChangeDate.UtcDateTime);
            Assert.AreEqual(input.IsOnline, output.IsOnline);
            //Assert.AreEqual(input.ProviderName, output.ProviderName);
        }

    }
}
