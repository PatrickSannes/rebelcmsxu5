using System;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class HiveIdTest
    {
        [TestCase]
        public void IsSystem_WhenCreatedFromNegativeInt()
        {
            // Arrange
            var id = HiveId.ConvertIntToGuid(-100);

            // Assert
            Assert.IsTrue(id.IsSystem());
        }

        [TestCase]
        public void IsSystem_WhenCreatedFromSystemGuid()
        {
            // Arrange
            var id = new HiveId(Guid.Parse("10000000000000000000000000000100"));

            // Assert
            Assert.IsTrue(id.IsSystem());
        }

        [TestCase("storage://provider/string/value")]
        [TestCase("guid$_587060bad94448c9a25cc6ef305b2bb1")]
        public void ExtensionMethods_GetHtmlId_ThenTryParseFromHtmlId_Success(string input)
        {
            var id = HiveId.TryParse(input).Result;
            var htmlId = id.GetHtmlId() + ".TemplateId";
            var fromHtmlId = HiveIdExtensions.TryParseFromHtmlId(htmlId).Result;
            Assert.AreEqual(id, fromHtmlId, "HtmlId was: " + htmlId);
        }

        [TestCase("storage://provider/string/value")]
        public void SerializedCommonValues_ContainNoInvalidPathChars(string input)
        {
            CheckInvalidPathChars(input);
            var hiveId = new HiveId(input);
            CheckInvalidPathChars(hiveId.ToString(HiveIdFormatStyle.AsUri));
            CheckInvalidPathChars(hiveId.ToString(HiveIdFormatStyle.UriSafe));
        }

        [TestCase("storage://provider/v__string/value", "storage://provider/", null, "value", "storage://provider/v__string/value")]
        [TestCase("storage://p__provider/v__string/value", "storage:///", "provider", "value", "storage://p__provider/v__string/value")]
        [TestCase("storage://provider/", "storage://provider/", null, "/", "storage://provider/v__string/$$")]
        [TestCase("storage://provider", "storage://provider/", null, "/", "storage://provider/v__string/$$")]
        public void CreateHiveIdFromRootValue(string input, string rootShouldBe, string providerShouldBe, string valueShouldBe, string toStringShouldBe)
        {
            var id = new HiveId(input);
            Assert.AreEqual(rootShouldBe, id.ProviderGroupRoot == null ? null : id.ProviderGroupRoot.ToString());
            Assert.AreEqual(providerShouldBe, id.ProviderId);
            Assert.AreEqual(valueShouldBe, id.Value.ToString());
            Assert.AreEqual(toStringShouldBe, id.ToUri().ToString());
            Assert.AreEqual(id.ToString(), new HiveId(id.ToString()).ToString());
        }

        /// <summary>
        /// Copied from System.Path in order to check against invalid path chars
        /// </summary>
        /// <param name="path"></param>
        internal static void CheckInvalidPathChars(string path)
        {
            for (int index = 0; index < path.Length; ++index)
            {
                int num = (int)path[index];
                switch (num)
                {
                    case 34:
                    case 60:
                    case 62:
                    case 124:
                        Assert.Fail("Invalid char " + path[index] + " found");
                        return;
                    default:
                        if (num >= 32)
                            continue;
                        else
                            goto case 34;
                }
            }
        }

        [TestCase]
        public void ExtensionMethod_IsNullOrEmpty()
        {
            // Arrange
            var nullValue = new HiveId((string)null);
            var nullValue2 = new HiveId((Uri)null);
            var validValue = new HiveId(Guid.NewGuid());

            // Assert
            Assert.IsTrue(default(HiveId).IsNullValueOrEmpty());
            Assert.IsTrue(new HiveId().IsNullValueOrEmpty());
            Assert.IsTrue(HiveId.Empty.IsNullValueOrEmpty());
            Assert.IsTrue(nullValue.IsNullValueOrEmpty());
            Assert.IsTrue(nullValue2.IsNullValueOrEmpty());
            Assert.IsFalse(validValue.IsNullValueOrEmpty());
        }

        [TestCase]
        public void ExtensionMethod_ConvertToNullIfEmpty()
        {
            // Arrange
            var nullValue = new HiveId((string)null);
            var nullValue2 = new HiveId((Uri)null);
            var validValue = new HiveId(Guid.NewGuid());

            // Act
            var result = nullValue.ConvertToEmptyIfNullValue();
            var result2 = nullValue2.ConvertToEmptyIfNullValue();
            var nonEmptyResult = validValue.ConvertToEmptyIfNullValue();

            // Assert
            Assert.AreEqual(HiveId.Empty, result);
            Assert.AreEqual(HiveId.Empty, result2);
            Assert.AreNotEqual(HiveId.Empty, nonEmptyResult);
        }

        [TestCase]
        public void NullConversion_EqualTo_Empty()
        {
            HiveId myItem = new HiveId((string)null);
            string serialized = myItem.ToString();
            var myDeserializedItem = HiveId.Parse(serialized);
            Assert.AreEqual(myItem, HiveId.Empty);
            Assert.AreEqual(myDeserializedItem, HiveId.Empty);
            Assert.IsTrue(myItem.IsNullValueOrEmpty());
        }

        [TestCase]
        public void ValidValue_NotEqualTo_Empty()
        {
            Assert.IsFalse(new HiveId(1).IsNullValueOrEmpty());
        }

        [TestCase]
        public void EmptyHiveId_RepresentedAsString()
        {
            Assert.AreEqual("string$_(null)", HiveId.Empty.ToString());
        }

        [TestCase]
        public void CreateFromValues_WithShortProviderGroup()
        {
            // Arrange
            var val1 = new HiveId(new Uri("storage://"), "provider-id", new HiveIdValue(1));

            // Assert
            Assert.AreEqual("storage://p__provider-id/v__int32/1", val1.ToString(HiveIdFormatStyle.AsUri));
        }

        [TestCase]
        public void CreateFromValue_ToUriIsRelative()
        {
            // Arrange
            var val1 = new HiveId("my-string-id");

            // Assert
            var toCompare = new Uri("/string/my-string-id", UriKind.Relative);
            Assert.IsNull(val1.ProviderId);
            Assert.IsNull(val1.ProviderGroupRoot);
            Assert.AreEqual(toCompare, val1.ToUri());
        }

        [TestCase]
        public void CreateFully_WithStringValue_ToUri()
        {
            // Arrange
            var val1 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue("my-filename.aspx"));
            var val2 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue("/my-filename.aspx"));
            var val3 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue("/my-filename.aspx"));
            var val4 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue("my-filename.aspx"));


            // Assert
            const string comparisonUri = "storage://stylesheets/p__io-provider/v__string/my-filename.aspx";
            const string comparisonUriWithFileSlash = "storage://stylesheets/p__io-provider/v__string/$$my-filename.aspx";
            Assert.AreEqual(new Uri(comparisonUri), val1.ToUri());
            Assert.AreEqual(new Uri(comparisonUriWithFileSlash), val2.ToUri());
            Assert.AreEqual(new Uri(comparisonUriWithFileSlash), val3.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val4.ToUri());
        }

        [TestCase]
        public void CreateFully_WithGuidValue_ToUri()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var val1 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue(guid));
            var val2 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue(guid));
            var val3 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue(guid));
            var val4 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue(guid));


            // Assert
            string comparisonUri = "storage://stylesheets/p__io-provider/v__guid/" + guid.ToString("N");
            Assert.AreEqual(new Uri(comparisonUri), val1.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val2.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val3.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val4.ToUri());
        }

        [TestCase]
        public void CreateFully_WithIntValue_ToUri()
        {
            // Arrange
            var val1 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue(5));
            var val2 = new HiveId(new Uri("storage://stylesheets/"), "io-provider", new HiveIdValue(5));
            var val3 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue(5));
            var val4 = new HiveId(new Uri("storage://stylesheets"), "io-provider", new HiveIdValue(5));


            // Assert
            const string comparisonUri = "storage://stylesheets/p__io-provider/v__int32/5";
            Assert.AreEqual(new Uri(comparisonUri), val1.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val2.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val3.ToUri());
            Assert.AreEqual(new Uri(comparisonUri), val4.ToUri());
        }

        [TestCase]
        public void ToString_AsUriFormat_FromPartialValue()
        {
            // Arrange
            var val1 = new HiveId("my-string-value");
            var uri = "/string/my-string-value";

            // Assert
            Assert.AreEqual("string$_my-string-value", val1.ToString());
            Assert.AreEqual("string$_my-string-value", val1.ToString(HiveIdFormatStyle.UriSafe));
            Assert.AreEqual("/string/my-string-value", val1.ToString(HiveIdFormatStyle.AsUri));
        }

        [TestCase]
        public void ExplicitOperator_FromGuidToHiveId()
        {
            var val1 = Guid.NewGuid();
            var val2 = (HiveId)val1;

            Assert.AreEqual(HiveIdValueTypes.Guid, val2.Value.Type);
            Assert.IsTrue(val1 == val2.Value);
        }

        [TestCase]
        public void EqualsImpl_WhenPassedGuid()
        {
            var val1 = Guid.NewGuid();
            var val2 = new HiveId(val1);

            Assert.AreEqual(HiveIdValueTypes.Guid, val2.Value.Type);
            Assert.IsTrue(val1 == val2.Value);
            Assert.IsTrue(val2.Equals((object)val1)); // Test for passing into Equals(obj) method
        }

        [TestCase]
        public void ToString_AsUriSafeFormat_FromPartialValue()
        {
            // Arrange
            var val1 = new HiveId("my-string-value");
            var uriSafe = "string$_my-string-value";

            // Assert
            Assert.AreNotEqual(uriSafe, val1.ToString(HiveIdFormatStyle.AsUri));
            Assert.AreEqual(uriSafe, val1.ToString(HiveIdFormatStyle.UriSafe));
        }

        [TestCase]
        public void ToString_AsUriFormat_FromFullValue()
        {
            // Arrange
            var val1 = new HiveId(new Uri("storage://stylesheets/"), "test-provider", new HiveIdValue("my-string-value"));
            var uri = "storage://stylesheets/p__test-provider/v__string/my-string-value";

            // Assert
            Assert.AreNotEqual(uri, val1.ToString());
            Assert.AreEqual(uri, val1.ToString(HiveIdFormatStyle.AsUri));
        }

        [TestCase("/string/my-string-value", null, null, "my-string-value")]
        [TestCase("storage://stylesheets/v__string/my-string-value", "storage://stylesheets/", null, "my-string-value")]
        [TestCase("storage://stylesheets/p__test-provider/v__string/my-string-value", "storage://stylesheets/", "test-provider", "my-string-value")]
        [TestCase("p__test-provider/v__string/my-string-value", null, "test-provider", "my-string-value")]
        public void ToUri(string parseFrom, string uriPart, string providerPart, object valuePart)
        {
            // Arrange
            var hiveId = HiveId.Parse(parseFrom);
            var urlToCheck = uriPart != null ? new Uri(uriPart, UriKind.Absolute) : null;

            // Act
            var asUri = hiveId.ToUri();

            // Assert
            Assert.AreEqual(hiveId.ProviderGroupRoot, urlToCheck);
            Assert.AreEqual(providerPart, hiveId.ProviderId);
            Assert.AreEqual(valuePart.ToString(), hiveId.Value.ToString());
            Assert.AreEqual(parseFrom, asUri.ToString());
        }

        [TestCase]
        public void ToString_AsUriSafeFormat_FromFullValue()
        {
            // Arrange
            var val1 = new HiveId(new Uri("storage://stylesheets/"), "test-provider", new HiveIdValue("my-string-value"));
            var uriSafe = "storage$net_root$stylesheets$_p__test-provider$_v__string$_my-string-value";

            // Assert
            Assert.AreEqual(uriSafe, val1.ToString());
            Assert.AreEqual(uriSafe, val1.ToString(HiveIdFormatStyle.UriSafe));
            Assert.AreNotEqual(uriSafe, val1.ToString(HiveIdFormatStyle.AsUri));
        }

        // Note: ensure to literalise incoming path strings; the compiler appears not to mind
        // you putting in backslashes in attribute values but they are treated as if they are unescaped
        // i.e. the \b in c:\blah\ is treated as \b the character, i.e. a backslash
        [TestCase("/string/AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "string$_AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "/string/AC505936-F83B-47A5-B8B6-ECC18FD43DF2", HiveIdValueTypes.String, false)]
        [TestCase(@"c:\blah\blah2", "string$_c$path_fileroot$blah$!blah2", "/string/c$path_fileroot$blah$!blah2", HiveIdValueTypes.String, false)]
        [TestCase("string$_AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "string$_AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "/string/AC505936-F83B-47A5-B8B6-ECC18FD43DF2", HiveIdValueTypes.String, false)]
        [TestCase("guid$_AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "guid$_ac505936f83b47a5b8b6ecc18fd43df2", "/guid/ac505936f83b47a5b8b6ecc18fd43df2", HiveIdValueTypes.Guid, false)]
        [TestCase("/guid/AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "guid$_ac505936f83b47a5b8b6ecc18fd43df2", "/guid/ac505936f83b47a5b8b6ecc18fd43df2", HiveIdValueTypes.Guid, false)]
        [TestCase("storage://stylesheets/p__test-provider/v__string/my-string-value", "storage$net_root$stylesheets$_p__test-provider$_v__string$_my-string-value", "storage://stylesheets/p__test-provider/v__string/my-string-value", HiveIdValueTypes.String, false)]
        [TestCase("storage://stylesheets/p__test-provider/v__guid/AC505936-F83B-47A5-B8B6-ECC18FD43DF2", "storage$net_root$stylesheets$_p__test-provider$_v__guid$_ac505936f83b47a5b8b6ecc18fd43df2", "storage://stylesheets/p__test-provider/v__guid/ac505936f83b47a5b8b6ecc18fd43df2", HiveIdValueTypes.Guid, false)]
        [TestCase("storage://file-uploader/v__string/0d3601f3e89c401f8b330dbbda20af69\\me_220.jpg", "storage$net_root$file-uploader$_v__string$_0d3601f3e89c401f8b330dbbda20af69$!me_220.jpg", "storage://file-uploader/v__string/0d3601f3e89c401f8b330dbbda20af69$!me_220.jpg", HiveIdValueTypes.String, true)]
        public void Ctor_AsString_FullValue(string input, string expectedUriSafe, string expectedAsUri, HiveIdValueTypes expectedType, bool shouldThrow)
        {
            HiveId hiveId = HiveId.Empty;

            if (shouldThrow)
            {
                Assert.Throws<FormatException>(() => hiveId = new HiveId(input));
                return;
            }
            else
                hiveId = new HiveId(input);

            Assert.AreEqual(expectedType, hiveId.Value.Type);
            Assert.AreEqual(expectedUriSafe, hiveId.ToString(HiveIdFormatStyle.UriSafe));
            Assert.AreEqual(expectedAsUri, hiveId.ToString(HiveIdFormatStyle.AsUri));
        }

        [TestCase(@"c:\this\is-my-file.dll", false)]
        [TestCase(@"file://c/absolutely/this/is-my-file.dll", false)]
        [TestCase(@"storage://file-uploader/string/0d3601f3e89c401f8b330dbbda20af69\\me_220.jpg", false)] // Should not throw because it's passed first into a Uri which parses the \ fine
        public void Ctor_FromUri_ToString_ThenTryParse_AreEqual(string inputUri, bool shouldThrow)
        {
            HiveId hiveId = HiveId.Empty;

            if (shouldThrow)
            {
                Assert.Throws<FormatException>(() => hiveId = new HiveId(new Uri(inputUri)), inputUri + " should have thrown");
                return;
            }
            else
                hiveId = new HiveId(new Uri(inputUri));

            var toStringAsUri = hiveId.ToString(HiveIdFormatStyle.AsUri);
            var toStringUriSafe = hiveId.ToString(HiveIdFormatStyle.UriSafe);

            var newHiveIdFromAsUri_ctor = new HiveId(toStringAsUri);
            AssertCompareHiveIds(hiveId, newHiveIdFromAsUri_ctor);
            var newHiveIdFromUriSafe_ctor = new HiveId(toStringUriSafe);
            AssertCompareHiveIds(hiveId, newHiveIdFromUriSafe_ctor);
            var newHiveIdFromAsUri_Parse = HiveId.Parse(toStringAsUri);
            AssertCompareHiveIds(hiveId, newHiveIdFromAsUri_Parse);
            var newHiveIdFromUriSafe_Parse = HiveId.Parse(toStringUriSafe);
            AssertCompareHiveIds(hiveId, newHiveIdFromUriSafe_Parse);
        }

        [TestCase(@"/string/asdasdaasd", false)]
        [TestCase(@"/int32/4", false)]
        [TestCase(@"/guid/aeec082125eb446d8d67d176124b9f54", false)]
        [TestCase(@"storage://stylesheets/v__string/asdasdaasd", false)]
        [TestCase(@"storage://stylesheets/v__int32/4", false)]
        [TestCase(@"storage://stylesheets/v__guid/aeec082125eb446d8d67d176124b9f54", false)]
        [TestCase(@"storage://file-uploader/v__string/0d3601f3e89c401f8b330dbbda20af69\\me_220.jpg", true)]
        [TestCase(@"storage://stylesheets/p__file-uploader/v__string/0d3601f3e89c401f8b330dbbda20af69\\me_220.jpg", true)]
        [TestCase("p__nhibernate-02$_v__guid$_10000000000000000000000000000005", false)]
        public void Ctor_FromString_ToString_ThenTryParse_AreEqual(string inputUri, bool shouldThrow)
        {
            HiveId hiveId = HiveId.Empty;

            if (shouldThrow)
            {
                Assert.Throws<FormatException>(() => hiveId = new HiveId(inputUri));
            }
            else
                hiveId = new HiveId(inputUri);

            var toStringAsUri = hiveId.ToString(HiveIdFormatStyle.AsUri);
            var toStringUriSafe = hiveId.ToString(HiveIdFormatStyle.UriSafe);

            var newHiveIdFromAsUri_ctor = new HiveId(toStringAsUri);
            AssertCompareHiveIds(hiveId, newHiveIdFromAsUri_ctor);
            var newHiveIdFromUriSafe_ctor = new HiveId(toStringUriSafe);
            AssertCompareHiveIds(hiveId, newHiveIdFromUriSafe_ctor);
            var newHiveIdFromAsUri_Parse = HiveId.Parse(toStringAsUri);
            AssertCompareHiveIds(hiveId, newHiveIdFromAsUri_Parse);
            var newHiveIdFromUriSafe_Parse = HiveId.Parse(toStringUriSafe);
            AssertCompareHiveIds(hiveId, newHiveIdFromUriSafe_Parse);
        }

        private static void AssertCompareHiveIds(HiveId hiveId, HiveId compareTo)
        {
            Assert.AreEqual(hiveId.ProviderGroupRoot, compareTo.ProviderGroupRoot);
            Assert.AreEqual(hiveId.ProviderId, compareTo.ProviderId);
            Assert.AreEqual(hiveId.Value, compareTo.Value);
            Assert.AreEqual(hiveId.Value.Type, compareTo.Value.Type);
            Assert.AreEqual(hiveId, compareTo);
            Assert.AreEqual(hiveId.ToString(), compareTo.ToString());
            Assert.AreEqual(hiveId.ToUri(), compareTo.ToUri());
            Assert.AreEqual(hiveId.ToString(HiveIdFormatStyle.AsUri), compareTo.ToString(HiveIdFormatStyle.AsUri));
            Assert.AreEqual(hiveId.ToString(HiveIdFormatStyle.UriSafe), compareTo.ToString(HiveIdFormatStyle.UriSafe));
            Assert.AreEqual(hiveId.ToString(HiveIdFormatStyle.AutoSingleValue), compareTo.ToString(HiveIdFormatStyle.AutoSingleValue));
        }

        [TestCase]
        public void FromString_DetectStyle()
        {
            // Assert
            var item = "storage://stylesheets/provider-key/string/value";
            var itemUriSafe = "storage$_stylesheets$_provider-key$_string$_value";
            var autoItemSafeIntAsString = "1";
            var shortStyle = "/string/blahblah";
            var shortStyleInt = "/int32/1234";

            // Act
            var tryDetect = HiveId.DetectFormatStyleFromString(item);
            var tryDetectUriSafe = HiveId.DetectFormatStyleFromString(itemUriSafe);
            var tryDetectAutoIntAsString = HiveId.DetectFormatStyleFromString(autoItemSafeIntAsString);
            var tryDetectShort = HiveId.DetectFormatStyleFromString(shortStyle);
            var tryDetectShortAsInt = HiveId.DetectFormatStyleFromString(shortStyleInt);

            // Assert
            Assert.IsTrue(tryDetect.Success);
            Assert.AreEqual(HiveIdFormatStyle.AsUri, tryDetect.Result);
            Assert.IsTrue(tryDetectUriSafe.Success);
            Assert.AreEqual(HiveIdFormatStyle.UriSafe, tryDetectUriSafe.Result);
            Assert.IsTrue(tryDetectAutoIntAsString.Success);
            Assert.AreEqual(HiveIdFormatStyle.AutoSingleValue, tryDetectAutoIntAsString.Result);
            Assert.IsTrue(tryDetectShort.Success);
            Assert.AreEqual(HiveIdFormatStyle.AsUri, tryDetectShort.Result);
            Assert.IsTrue(tryDetectShortAsInt.Success);
            Assert.AreEqual(HiveIdFormatStyle.AsUri, tryDetectShortAsInt.Result);
        }

        [Test]
        public void FromString_RootCharacterOnly_ParsesAsRoot()
        {
            // Arrange
            var input = "/";

            // Act
            var hiveId = HiveId.Parse(input);

            // Assert
            Assert.AreEqual("/", (string)hiveId.Value);
            Assert.AreEqual(HiveIdValueTypes.String, hiveId.Value.Type);
        }

        [TestCase]
        public void FromString_ParseAsAutoValue()
        {
            // Assert
            var intAsString = "1";
            var guidAsString = "B7C8A690-D721-42E6-B571-79EC46DEDFC8";
            var blahAsString = "ssdfljsdlfjsdfjdkfl";

            // Act
            var parsedAsInt = HiveId.TryParse(intAsString);
            var parsedAsGuid = HiveId.TryParse(guidAsString);
            var parsedAsString = HiveId.TryParse(blahAsString);

            // Assert
            Assert.IsTrue(parsedAsInt.Success);
            HiveId hiveIdAsInt = parsedAsInt.Result;
            Assert.IsNull(hiveIdAsInt.ProviderId);
            Assert.AreEqual(hiveIdAsInt.Value.Type, HiveIdValueTypes.Int32);
            Assert.AreEqual((int)hiveIdAsInt.Value, 1);
            Assert.AreEqual(hiveIdAsInt.Value, new HiveIdValue(1));

            Assert.IsTrue(parsedAsGuid.Success);
            HiveId hiveIdAsGuid = parsedAsGuid.Result;
            Assert.IsNull(hiveIdAsGuid.ProviderId);
            Assert.AreEqual(hiveIdAsGuid.Value.Type, HiveIdValueTypes.Guid);
            Assert.AreEqual((Guid)hiveIdAsGuid.Value, Guid.Parse(guidAsString));
            Assert.AreEqual(hiveIdAsGuid.Value, new HiveIdValue(Guid.Parse(guidAsString)));

            Assert.IsTrue(parsedAsString.Success);
            HiveId hiveIdAsString = parsedAsString.Result;
            Assert.IsNull(hiveIdAsString.ProviderId);
            Assert.AreEqual(hiveIdAsString.Value.Type, HiveIdValueTypes.String);
            Assert.AreEqual((string)hiveIdAsString.Value, blahAsString);
            Assert.AreEqual(hiveIdAsString.Value, new HiveIdValue(blahAsString));
        }

        [TestCase]
        public void FromString_ParseAsUri()
        {
            // Assert
            var item = "storage://stylesheets/p__provider-key/v__string/value";

            // Act
            var itemParsed = HiveId.TryParse(item);

            // Assert
            Assert.IsTrue(itemParsed.Success);
            HiveId hiveId = itemParsed.Result;
            Assert.AreEqual(hiveId.ProviderId, "provider-key");
            Assert.AreEqual(hiveId.Value.Type, HiveIdValueTypes.String);
            Assert.AreEqual((string)hiveId.Value, "value");
            Assert.AreEqual(hiveId.Value, new HiveIdValue("value"));
            Assert.AreEqual(new Uri("storage://stylesheets/"), hiveId.ProviderGroupRoot);
        }

        [TestCase]
        public void FromString_ParseAsUriSafe()
        {
            // Assert
            var item = "storage$net_root$stylesheets$_p__provider-key$_v__string$_value";

            // Act
            var itemParsed = HiveId.TryParse(item);

            // Assert
            Assert.IsTrue(itemParsed.Success);
            HiveId hiveId = itemParsed.Result;
            Assert.AreEqual(hiveId.ProviderId, "provider-key");
            Assert.AreEqual(hiveId.Value.Type, HiveIdValueTypes.String);
            Assert.AreEqual((string)hiveId.Value, "value");
            Assert.AreEqual(hiveId.Value, new HiveIdValue("value"));
            Assert.AreEqual(new Uri("storage://stylesheets/"), hiveId.ProviderGroupRoot);
        }
    }
}
