#if UNITY_EDITOR
namespace ArtyECS.Core
{
    public enum QueryType
    {
        GetEntitiesWith1,
        GetEntitiesWith2,
        GetEntitiesWith3,
        GetComponents,
        GetEntitiesWithout1,
        GetEntitiesWithout2,
        GetEntitiesWithout3,
        GetComponent,
        GetModifiableComponent,
        GetModifiableComponents,
        QueryBuilderExecute
    }
}
#endif

