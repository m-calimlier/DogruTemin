namespace DogruTemin.Wpf.Features;
/// <summary>A feature owns its editor state and behavior; dependencies are explicit in Context.</summary>
internal abstract class FeatureModule
{
    protected WorkspaceContext Context { get; }

    protected FeatureModule(WorkspaceContext context) => Context = context;
    protected T View<T>(string name)
        where T : class => Context.View<T>(name);
}
