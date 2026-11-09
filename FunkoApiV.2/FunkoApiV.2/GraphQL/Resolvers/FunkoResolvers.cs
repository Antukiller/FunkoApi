using FunkoApiV._2.Models;

namespace FunkoApiV._2.GraphQL.Resolvers;

[ExtendObjectType(typeof(Funko))]
public class FunkoResolvers {
    public bool GetEstaDisponible([Parent] Funko funko) {
        return !funko.IsDeleted;
    }
}