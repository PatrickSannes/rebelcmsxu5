using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.EntityProviderFederation;

namespace Prototyping.Umbraco.Framework.Playground
{
    public class FakeController
    {
        public void IndexAction(IMappedIdentifier entityId)
        {
            // Assume an ID is provided
            // From this, we need to request the relevant IContentResolver for the given entityId.MappedProvider
            // We then need to ask that IContentResolver for an entity with the given entityId.Value
            // (Ideally the Mediator would do all this for us by being given the entityId)
            // When asking for the entity, we decide whether to ask just for that entity in which
            // case we get an IContentEntity back, or we ask for a certain depth traversal in which
            // case we get an IContentVertex back (slower operation).
            // This is the DTO layer of the Synonym tier.
            // Then we map this to an IContentViewModel which is basically dynamic but includes the AttributeSchema for IntelliSense support

            //var mediator = new Mediator();
            //var contentEntityRepo = mediator.GetEntityRepositoryReader(entityId.MappedProvider);
            //var test = contentEntityRepo[a => a.WithId(new RepositoryGeneratedIdentifier())];
            //var test2 = contentEntityRepo.Filter(a => a.WithId(new RepositoryGeneratedIdentifier())).ToList();

            //var test3 = new List<IMappedIdentifier>();
            //test3.Where(a => a.ValueAsString = "test").ToList();
        }
    }
}
