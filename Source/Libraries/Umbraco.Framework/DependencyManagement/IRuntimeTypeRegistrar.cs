namespace Umbraco.Framework.DependencyManagement
{
	public interface IRuntimeTypeRegistrar<TContract>
	{
		IRuntimeTypeRegistrarModifier<TContract> Register();
		IRuntimeTypeRegistrarModifier<TContract> RegisterNamed(string name);
	}
}