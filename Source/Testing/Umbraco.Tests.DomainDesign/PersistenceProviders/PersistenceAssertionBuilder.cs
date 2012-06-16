using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Tests.DomainDesign.PersistenceProviders.NHibernate;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders
{
    public class PersistenceAssertionBuilder<T> where T : AbstractEntity
    {
        private readonly List<Tuple<Func<T, object>, Action<T, object>>> _valuesAndAssertions = new List<Tuple<Func<T, object>, Action<T, object>>>();
        private readonly Action _postWriteCallback;

        public PersistenceAssertionBuilder(Action postWriteCallback)
        {
            _postWriteCallback = postWriteCallback;
        }

        public PersistenceAssertionBuilder<T> AddAssert<TReturnType>(Func<T, TReturnType> origValueGetter, Action<T, TReturnType> assertion)
        {
            // This method allows a type parameter specifying the return type of the expression that gets the original value,
            // but internally we have to store Func<T, object> so that the type can vary across the whole list
            // so we have to convert it here
            Func<T, object> castExpression = x => origValueGetter.Invoke(x);
            // Same goes for the delegate responsible for the assertion after writing the mocked object to the mocked db
            Action<T, object> castAction = (y, z) => assertion.Invoke(y, (TReturnType)z);
            // Add the cast expressions to the list
            _valuesAndAssertions.Add(new Tuple<Func<T, object>, Action<T, object>>(castExpression, castAction));
            return this;
        }

        public PersistenceAssertionBuilder<T> AddAssertIsTrue<TReturnType>(Func<T, TReturnType> getExpression, Func<TReturnType, TReturnType, bool> comparisonExpression, string assertionMessage)
        {
            AddAssert(getExpression,
                (x, y) =>
                Assert.IsTrue(comparisonExpression.Invoke(getExpression.Invoke(x), y),
                              "Failed comparing '{0}' original to '{1}' new (trying: {2})",
                              getExpression.Invoke(x), y, assertionMessage));
            return this;
        }

        public PersistenceAssertionBuilder<T> AddAssertIntegerEquals(Func<T, int> getExpression, string assertionMessage)
        {
            AddAssertIsTrue(getExpression, (x, y) => x == y, assertionMessage);
            return this;
        }

        public PersistenceAssertionBuilder<T> AddAssertStringEquals(Func<T, string> getExpression, string assertionMessage)
        {
            AddAssertIsTrue(getExpression, (x, y) => String.Equals(x, y, StringComparison.InvariantCultureIgnoreCase), assertionMessage);
            return this;
        }

        public PersistenceAssertionBuilder<T> AddAssertDateTimeOffset(Func<T, DateTimeOffset> getExpression, string assertionMessage)
        {
            AddAssertIsTrue(getExpression, (x, y) =>
                                         {
                                             var timeSpan = y.Subtract(x);
                                             return timeSpan.TotalSeconds < 1 || timeSpan.TotalSeconds > 1;
                                         }, assertionMessage);
            return this;
        }

        public PersistenceAssertionBuilder<T> RunAssertionsWithQuery(IHiveReadProvider readerProvider, IHiveReadWriteProvider writeProvider, Func<T> mockedEntityGenerator, Func<HiveId, Expression<Func<T, bool>>> expression)
        {
            return RunAssertionsWithQuery(readerProvider, writeProvider, mockedEntityGenerator.Invoke(), expression);
        }

        public PersistenceAssertionBuilder<T> RunAssertionsWithQuery(IHiveReadProvider readerProvider, IHiveReadWriteProvider writeProvider, T mockedEntity, Func<HiveId, Expression<Func<T, bool>>> expression)
        {
            HiveId generatedId;
            object[] checkValuesAfterCommitting = GetCheckValuesAfterCommitting(writeProvider, out generatedId, mockedEntity);

            using (DisposableTimer.TraceDuration<NHibernateHiveTests>("Start read", "End read"))
            {
                using (var unit = readerProvider.CreateReadOnlyUnitOfWork())
                {
                    var result = unit.ReadRepository.QueryContext.Query<T>().Where(expression(generatedId)).FirstOrDefault();

                    Assert.IsNotNull(result,
                                     "No entity was retrieved back from the datastore for query {0} (id was {1})",
                                     expression,
                                     generatedId);

                    Assert.IsTrue(generatedId == result.Id);

                    LogHelper.TraceIfEnabled<PersistenceAssertionBuilder<T>>("Item owned by {0}, retrieved from {1}", () => result.PersistenceMetadata.OwnerProviderAlias, () => result.PersistenceMetadata.ReturnedByProviderAlias);

                    for (int i = 0; i < _valuesAndAssertions.Count; i++)
                    {
                        _valuesAndAssertions[i].Item2.Invoke(result, checkValuesAfterCommitting[i]);
                    }
                }
            }

            return this;
        }

        public PersistenceAssertionBuilder<T> RunAssertions(IHiveReadProvider readerProvider, IHiveReadWriteProvider writeProvider, Func<T> mockedEntityGenerator)
        {
            return RunAssertions(readerProvider, writeProvider, mockedEntityGenerator.Invoke());
        }

        public PersistenceAssertionBuilder<T> RunAssertions(IHiveReadProvider readerProvider, IHiveReadWriteProvider writeProvider, T mockedEntity)
        {
            HiveId generatedId;
            object[] checkValuesAfterCommitting = GetCheckValuesAfterCommitting(writeProvider, out generatedId, mockedEntity);

            using (DisposableTimer.TraceDuration<NHibernateHiveTests>("Start read", "End read"))
            {
                using (var unit = readerProvider.CreateReadOnlyUnitOfWork())
                {
                    var result = unit.ReadRepository.GetEntity<T>(generatedId);

                    Assert.IsNotNull(result, "No entity was retrieved back from the datastore with id {0}", generatedId);

                    Assert.AreEqual(generatedId, result.Id, "Got a different value back from the database for the Id");

                    LogHelper.TraceIfEnabled<PersistenceAssertionBuilder<T>>("Item owned by {0}, retrieved from {1}", () => result.PersistenceMetadata.OwnerProviderAlias, () => result.PersistenceMetadata.ReturnedByProviderAlias);

                    for (int i = 0; i < _valuesAndAssertions.Count; i++)
                    {
                        _valuesAndAssertions[i].Item2.Invoke(result, checkValuesAfterCommitting[i]);
                    }
                }
                _postWriteCallback.Invoke();
            }

            return this;
        }

        private object[] GetCheckValuesAfterCommitting(IHiveReadWriteProvider writeProvider, out HiveId generatedId, T mockedEntity)
        {
            var checkValuesAfterCommitting = new object[_valuesAndAssertions.Count];

            using (DisposableTimer.TraceDuration<NHibernateHiveTests>("Start write", "End write"))
            {
                using (var unit = writeProvider.CreateReadWriteUnitOfWork())
                {
                    unit.ReadWriteRepository.AddOrUpdate(mockedEntity);
                    unit.Commit();

                    generatedId = mockedEntity.Id;

                    for (int i = 0; i < _valuesAndAssertions.Count; i++)
                    {
                        checkValuesAfterCommitting[i] = _valuesAndAssertions[i].Item1.Invoke(mockedEntity);
                    }
                }
                _postWriteCallback.Invoke();
            }

            Assert.IsNotNull(generatedId, "Id was not obtained from adding the entity to the db");
            return checkValuesAfterCommitting;
        }
    }
}