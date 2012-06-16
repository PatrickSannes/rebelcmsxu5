namespace Umbraco.Framework.DependencyManagement
{
	public interface IInstanceRegistrar<TContract>
	{
		IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance) where TImplementation : class, TContract;
		IInstanceRegistrarModifier<TContract> Register<TImplementation>(TImplementation instance, string name) where TImplementation : class, TContract;
	}
}