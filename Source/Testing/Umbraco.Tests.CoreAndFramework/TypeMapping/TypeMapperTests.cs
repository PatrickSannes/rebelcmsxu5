using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.TypeMapping;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.TypeMapping
{
    [TestFixture]
    public class TypeMapperTests : AbstractPartialTrustFixture<TypeMapperTests>
    {
        private class Foo
        {
            public string Name { get; set; }
            public int Xyz { get; set; }
            public string Props { get; set; }
            public Foo Child { get; set; }
            public IEnumerable<Foo> Foos { get; set; }
        }

        private class SuperFoo : Foo
        {
        }

        private class Bar
        {
            public string Name { get; set; }
            public string NoConvention { get; set; }
            public Bar Child { get; set; }
            public ICollection<Bar> Foos { get; set; }
        }

        private class SuperBar : Bar
        {
        }

        private class FooBar
        {
            public Bar SomeBar { get; set; }
            public Foo SomeFoo { get; set; }
        }

        private class FooBarMapper : TypeMapper<Foo, Bar>
        {
            public FooBarMapper(AbstractFluentMappingEngine engine)
                : base(engine)
            {
            }

            protected override void PerformMap(Foo source, Bar target, MappingExecutionScope scope)
            {
                base.PerformMap(source, target, scope);
                target.NoConvention = source.Name + source.Xyz + source.Props;
            }
        }

        private readonly FakeMappingEngine _mappingEngine;

        public TypeMapperTests()
        {
            var frameworkContext = new FakeFrameworkContext();
            _mappingEngine = new FakeMappingEngine(frameworkContext);
            frameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new[] {_mappingEngine}));
        }

        private class BarMemberMapper : MemberMapper<FooBar, Bar>
        {
            public override Bar GetValue(FooBar source)
            {
                return new Bar() { Name = source.SomeFoo.Name + "test" };
            }
        }

        [Test]
        public void Map_To_Type_From_Super_Type()
        {
            _mappingEngine.CreateMap<Foo, Bar>(true);

            var inheritedMap = _mappingEngine.GetMapperDefinition(typeof (SuperFoo), typeof (Bar), false);
            Assert.IsNotNull(inheritedMap);
            Assert.DoesNotThrow(() => _mappingEngine.Map<Bar>(new SuperFoo()));

            _mappingEngine.ClearMappers();

            _mappingEngine.CreateMap<Foo, Bar>(false);
            Assert.Throws<NotSupportedException>(() => _mappingEngine.GetMapperDefinition(typeof (SuperFoo), typeof (Bar), false));
        }

        [Test]
        public void Map_To_Super_Type_From_Type()
        {
            _mappingEngine.CreateMap<Foo, Bar>(true);

            var inheritedMap = _mappingEngine.GetMapperDefinition(typeof(Foo), typeof(SuperBar), false);
            Assert.IsNotNull(inheritedMap);
            Assert.DoesNotThrow(() => _mappingEngine.Map<SuperBar>(new Foo()));

            _mappingEngine.ClearMappers();

            _mappingEngine.CreateMap<Foo, Bar>(false);
            Assert.Throws<NotSupportedException>(() => _mappingEngine.GetMapperDefinition(typeof(Foo), typeof(SuperBar), false));

        }

        [Test]
        public void Map_To_Super_Type_From_Super_Type()
        {
            _mappingEngine.CreateMap<Foo, Bar>(true);

            var inheritedMap = _mappingEngine.GetMapperDefinition(typeof(SuperFoo), typeof(SuperBar), false);
            Assert.IsNotNull(inheritedMap);
            Assert.DoesNotThrow(() => _mappingEngine.Map<SuperBar>(new SuperFoo()));
           
            _mappingEngine.ClearMappers();

            _mappingEngine.CreateMap<Foo, Bar>(false);
            Assert.Throws<NotSupportedException>(() => _mappingEngine.GetMapperDefinition(typeof(SuperFoo), typeof(SuperBar), false));

        }

        [Test]
        public void MapUsing()
        {
            _mappingEngine.CreateMap<FooBar, FooBar>()
                .ForMember(x => x.SomeBar, opt => opt.MapUsing<BarMemberMapper>());
            var output = _mappingEngine.Map<FooBar>(new FooBar() { SomeFoo = new Foo() { Name = "hello" } });
            Assert.AreEqual("hellotest", output.SomeBar.Name);
        }

        [Test]
        public void Implicit_Mapping_MapFrom()
        {
            _mappingEngine.CreateMap<Foo, Bar>();
            //maps SomeBar from SomeFoo implicitly using the Foo -> Bar mapping,
            //and because both share Child and Foos properties, the above implicit mapping should work for them too
            _mappingEngine.CreateMap<FooBar, FooBar>()
                .ForMember(x => x.SomeBar, opt => opt.MapFrom(x => x.SomeFoo));

            var input = new FooBar()
                {
                    SomeFoo = new Foo()
                        {
                            Child = new Foo() { Name = "child" },
                            Foos = new[] { new Foo() { Name = "blah1" }, new Foo() { Name = "blah2" } },
                            Name = "test"
                        }
                };

            var output = _mappingEngine.Map<FooBar>(input);

            Assert.AreEqual(input.SomeFoo.Name, output.SomeBar.Name);
            Assert.AreEqual(input.SomeFoo.Child.Name, output.SomeBar.Child.Name);
            Assert.AreEqual(input.SomeFoo.Foos.Count(), output.SomeBar.Foos.Count());
        }

        [Test]
        public void WhenMappingEnumerable_ContainingMultipleReferencesToSameInstance_MapsToSingleOutgoingReference()
        {
            _mappingEngine.CreateMap<Foo, Bar>();

            var singleInstance = new Foo() { Name = "single-instance" };
            var input = new Foo() { Foos = Enumerable.Repeat(singleInstance, 2) };
            var output = _mappingEngine.Map<Bar>(input);

            Assert.That(input.Foos.ToArray()[0], Is.SameAs(input.Foos.ToArray()[1]));
            Assert.That(output.Foos.ToArray()[0], Is.SameAs(output.Foos.ToArray()[1]), "Output created multiple instances when input was same instance");
        }

        protected override void FixtureSetup()
        {
            base.FixtureSetup();
            TestHelper.SetupLog4NetForTests();
        }

        //[Test]
        //[Category("Performance")]
        //[TestOnlyInFullTrust] // So logging to console works
        //public void ReflectionSpeedVsExpressionSpeed()
        //{
        //    using (DisposableTimer.TraceDuration<TypeMapperTests>("Start using reflection", "End"))
        //    {
        //        for (int i = 0; i < 15; i++)
        //        {
        //            WhenMappingGraph_ContainingVaryingDeepReferencesToSameInstance_OutputContainsSingleDestinationReference();
        //            _mappingEngine.ClearMappers();
        //        }
        //    }
        //    AbstractMappingEngine.UseExpressions = true;
        //    using (DisposableTimer.TraceDuration<TypeMapperTests>("Start using expressions", "End"))
        //    {
        //        for (int i = 0; i < 15; i++)
        //        {
        //            WhenMappingGraph_ContainingVaryingDeepReferencesToSameInstance_OutputContainsSingleDestinationReference();
        //            _mappingEngine.ClearMappers();
        //        }
        //    }
        //}

        [Test]
        public void WhenMappingGraph_ContainingMultipleDeepReferencesToSameInstance_OutputContainsSingleDestinationReference()
        {
            // Arrange
            _mappingEngine.CreateMap<Foo, Bar>();

            var singleInstance = new Foo()
                {
                    Name = "repeated-instance"
                };

            var input = new Foo()
                {
                    Name = "top-level",
                    Child = new Foo()
                        {
                            Name = "second-level",
                            Foos = new[]
                                {
                                    new Foo()
                                        {
                                            Name = "has-repeated-child-1",
                                            Child = singleInstance
                                        },
                                    new Foo()
                                        {
                                            Name = "has-repeated-child-2",
                                            Child = singleInstance
                                        }
                                }
                        }
                };

            // Act
            var output = _mappingEngine.Map<Bar>(input);

            // Assert
            Assert.That(input.Child.Foos.ElementAt(0).Child, Is.SameAs(input.Child.Foos.ElementAt(1).Child));
            Assert.That(output.Child.Foos.ElementAt(0).Child, Is.SameAs(output.Child.Foos.ElementAt(1).Child), "Output created multiple instances when input was same instance");
            Assert.That(output.Child, Is.Not.SameAs(output.Child.Foos.ElementAt(1).Child), "Output used same instance for wrong map destination");
        }

        [Test]
        public void WhenMappingGraph_ContainingVaryingDeepReferencesToSameInstance_OutputContainsSingleDestinationReference()
        {
            // Arrange
            _mappingEngine.CreateMap<Foo, Bar>();

            var singleInstance = new Foo()
            {
                Name = "repeated-instance"
            };

            var input = new Foo()
            {
                Name = "top-level",
                Child = new Foo()
                {
                    Name = "second-level",
                    Foos = new[]
                                {
                                    new Foo()
                                        {
                                            Name = "does not have repeated child"
                                        },
                                    new Foo()
                                        {
                                            Name = "has-repeated-child-2",
                                            Child = singleInstance
                                        }
                                }
                },
                Foos = new[]
                    {
                        new Foo()
                            {
                                Name = "has repeated child in different place in the graph",
                                Child = singleInstance
                            }
                    }
            };

            // Act
            var output = _mappingEngine.Map<Bar>(input);
            var secondOutput = _mappingEngine.Map<Bar>(input);

            // Assert
            Assert.That(input.Child.Foos.ElementAt(1).Child, Is.SameAs(input.Foos.ElementAt(0).Child));
            Assert.That(output.Child.Foos.ElementAt(1).Child, Is.SameAs(output.Foos.ElementAt(0).Child), "Output created multiple instances when input was same instance");
            Assert.That(output.Child, Is.Not.SameAs(output.Foos.ElementAt(0).Child), "Output used same instance for wrong map destination");
            Assert.That(output.Child, Is.Not.SameAs(secondOutput.Child), "Scope survived beyond one mapping operation?");
            Assert.That(output.Child.Foos.ElementAt(1).Child, Is.Not.SameAs(secondOutput.Foos.ElementAt(0).Child), "Scope survived beyond one mapping operation?");
        }

        [Test]
        public void Not_Creating_Value_Types()
        {
            _mappingEngine.CreateMap<Bar, string>();
            _mappingEngine.CreateMap<Bar, Decimal>();

            Assert.Throws<ArgumentException>(
                () =>
                {
                    _mappingEngine.Map<Bar, string>(new Bar());
                    _mappingEngine.Map<Bar, Decimal>(new Bar());
                });
        }

        [Test]
        public void Creating_Value_Types()
        {
            _mappingEngine.CreateMap<Bar, string>().CreateUsing(x => x.Name);
            _mappingEngine.CreateMap<Bar, Decimal>().CreateUsing(x => 22);

            Assert.AreEqual("hello", _mappingEngine.Map<Bar, string>(new Bar() { Name = "hello" }));
            Assert.AreEqual(22, _mappingEngine.Map<Bar, Decimal>(new Bar()));

        }

        //[Test]
        //[Ignore]
        //public void AutoMapper_Vs_ValueInjector()
        //{

        //    var watch = new Stopwatch();

        //    //test simple map

        //    Mapper.CreateMap<Foo, Bar>();            
        //    _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine));
        //    DoMap(watch);

        //    //now test with aftermap event

        //    _mappingEngine.ClearMappers();
        //    Mapper.Reset();
        //    Mapper.CreateMap<Foo, Bar>().AfterMap((s, t) =>
        //        {
        //            if (t != null)
        //                t.Name += s.Xyz;
        //        });
        //    _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine)).AfterMap((s, t) =>
        //        {
        //            t.Name += s.Xyz;
        //        });
        //    DoMap(watch);

        //}

        //private void DoMap(Stopwatch watch)
        //{            

        //    watch.Reset();
        //    watch.Start();
        //    for (var i = 0; i < 10000; i++)
        //    {
        //        var viBar = _mappingEngine.Map<Foo, Bar>(new Foo { Xyz = i, Name = "Shannon" + Guid.NewGuid(), Props = "Blah" });
        //    }
        //    watch.Stop();
        //    double viTotal = watch.ElapsedMilliseconds;

        //    watch.Reset();
        //    watch.Start();
        //    for (int i = 0; i < 10000; i++)
        //    {
        //        var amBar = Mapper.Map<Foo, Bar>(new Foo { Xyz = i, Name = "Shannon" + Guid.NewGuid(), Props = "Blah" });
        //    }
        //    watch.Stop();
        //    double amTotal = watch.ElapsedMilliseconds;

        //    Console.WriteLine("ValueInjector total = " + viTotal);
        //    Console.WriteLine("AutoMapper total = " + amTotal);
        //}


        [Test]
        public void Use_Custom_Create_Using()
        {
            var customConstructed = false;
            var f = new Foo { Xyz = 43 };
            _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine))
                .CreateUsing(x =>
                    {
                        customConstructed = true;
                        return new Bar();
                    });

            var b = _mappingEngine.Map<Foo, Bar>(f);

            Assert.IsTrue(customConstructed);
        }

        [Test]
        public void TypeMapDefinition_Equals_Override()
        {
            var t1 = new TypeMapDefinition(typeof(FakeMappingEngine), typeof(FakeMappingEngine));
            var t2 = new TypeMapDefinition(typeof(FakeMappingEngine), typeof(FakeMappingEngine));

            Assert.IsTrue(t1.Equals(t2));
            Assert.IsTrue(t1.GetHashCode() == t2.GetHashCode());

            var d = new ConcurrentDictionary<TypeMapDefinition, string>();
            d.TryAdd(t1, "hello");
            Assert.IsFalse(d.TryAdd(t2, "goodbye"));

        }

        [Test]
        public void Engine_Deals_With_Duplicate_Mappings()
        {
            var frameworkContext = new FakeFrameworkContext();
            var engine = new FakeMappingEngine(frameworkContext);

            //create duplicate mappings...though only the first will be added and no exception thrown
            engine.CreateMap<FakeMappingEngine, FakeMappingEngine>();

            Assert.Throws<InvalidOperationException>(() => engine.CreateMap<FakeMappingEngine, FakeMappingEngine>());
        }

        [Test]
        public void Use_Custom_After_Map()
        {
            var f = new Foo { Xyz = 43 };
            _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine))
                .AfterMap((s, d) =>
                    {
                        d.Name = "AfterMapped!";
                        d.Child = new Bar { Name = "Blah" };
                    });

            var b = _mappingEngine.Map<Foo, Bar>(f);

            Assert.AreEqual("AfterMapped!", b.Name);
            Assert.AreEqual("Blah", b.Child.Name);
        }

        [Test]
        public void Use_For_Member_Ignore()
        {
            var f = new Foo
                {
                    Xyz = 43,
                    Name = "Shannon"
                };

            _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine))
                .ForMember(x => x.Name, opt => opt.Ignore());

            var b = _mappingEngine.Map<Foo, Bar>(f);

            Assert.IsTrue(string.IsNullOrEmpty(b.Name));
        }

        [Test]
        public void Use_For_Member_MapFrom()
        {
            var f = new Foo
            {
                Xyz = 43,
                Name = "Shannon"
            };

            _mappingEngine.CreateMap(new TypeMapper<Foo, Bar>(_mappingEngine))
                .ForMember(x => x.Name, opt => opt.MapFrom(val => val.Name + "-blah"));

            var b = _mappingEngine.Map<Foo, Bar>(f);

            Assert.AreEqual("Shannon-blah", b.Name);
        }

        [Test]
        public void MapShouldMapToExistingObject()
        {
            var f2 = new Foo { Xyz = 43 };
            _mappingEngine.Map(new { Name = "hi" }, f2);

            Assert.AreEqual("hi", f2.Name);
            Assert.AreEqual(43, f2.Xyz);
        }

        [Test]
        public void MapShouldCreateNewMappedObject()
        {
            var foo = new Foo { Name = "f1", Props = "p", Xyz = 123 };
            var foo2 = _mappingEngine.Map<Foo, Foo>(foo);

            Assert.AreEqual(foo.Name, foo2.Name);
            Assert.AreEqual(foo.Props, foo2.Props);
            Assert.AreEqual(foo.Xyz, foo2.Xyz);
        }

        [Test]
        public void ShouldMapChildPropertiesFooToBar()
        {
            var foo = new Foo { Child = new Foo { Name = "aaa" } };
            var bar = new Bar();

            _mappingEngine.Map(foo, bar);

            Assert.AreEqual("aaa", bar.Child.Name);
        }

        [Test]
        public void MapShouldMapCollections()
        {
            var foos = new List<Foo>
                           {
                               new Foo {Name = "f1"},
                               new Foo {Name = "f2"},
                               new Foo {Name = "f3"},
                           };

            var foos2 = _mappingEngine.Map<IEnumerable<Foo>, IList<Foo>>(foos);
            Assert.AreEqual(3, foos2.Count());
            Assert.AreEqual("f1", foos2.First().Name);
            Assert.AreEqual("f2", foos2.Skip(1).First().Name);
            Assert.AreEqual("f3", foos2.Last().Name);
        }

        [Test]
        public void MapShouldUseFooBarTypeMapperForMapping()
        {
            _mappingEngine.CreateMap(new FooBarMapper(_mappingEngine));
            var foo = new Foo { Name = "a", Props = "b", Xyz = 123 };
            var bar = new Bar();

            _mappingEngine.Map(foo, bar);

            Assert.AreEqual("a123b", bar.NoConvention);
            Assert.AreEqual(foo.Name, bar.Name);
        }

        [Test]
        public void MapShouldMapCollectionTypeProperties()
        {
            var foo = new Foo
            {
                Foos = new List<Foo>
                           {
                               new Foo{Name = "f1"},
                               new Foo{Name = "f2"},
                               new Foo{Name = "f3"},
                           }
            };

            var bar = _mappingEngine.Map<Foo, Bar>(foo);

            Assert.AreEqual(foo.Foos.Count(), bar.Foos.Count());
            Assert.AreEqual("f1", bar.Foos.First().Name);
            Assert.AreEqual("f3", bar.Foos.Last().Name);
        }

        [Test]
        public void MapShouldMapCollectionPropertiesAndUseFooBarTypeMapper()
        {
            _mappingEngine.CreateMap(new FooBarMapper(_mappingEngine));
            var foo = new Foo
            {
                Foos = new List<Foo>
                           {
                               new Foo{Name = "f1",Props = "v",Xyz = 19},
                               new Foo{Name = "f2",Props = "i",Xyz = 7},
                               new Foo{Name = "f3",Props = "v",Xyz = 3},
                           }
            };

            var bar = _mappingEngine.Map<Foo, Bar>(foo);

            Assert.AreEqual(foo.Foos.Count(), bar.Foos.Count());

            var ffoos = foo.Foos.ToArray();
            var bfoos = bar.Foos.ToArray();

            for (var i = 0; i < ffoos.Count(); i++)
            {
                Assert.AreEqual(ffoos[i].Name, bfoos[i].Name);
                Assert.AreEqual(ffoos[i].Name + ffoos[i].Xyz + ffoos[i].Props, bfoos[i].NoConvention);
            }
        }

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        public override void TestSetup()
        {
            return;
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        public override void TestTearDown()
        {
            _mappingEngine.ClearMappers();
        }
    }
}
