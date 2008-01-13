namespace Rhino.Security
{
    using Castle.ActiveRecord;

    /// <summary>
    /// Mapping an entity in the domain so the security 
    /// system can work with it.
    /// </summary>
    /// <remarks>
    /// We are purposefully using the FullName of a type here,
    /// instead of assembly qualified name. This is to avoid
    /// versioning issues with the assembly version.
    /// </remarks>
    [ActiveRecord]
    public class EntityType : NamedEntity<EntityType>
    {
    }
}