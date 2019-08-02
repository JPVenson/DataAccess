namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Defines how an Constructor should be created
	/// </summary>
	public enum CtorGeneratorMode
	{
		/// <summary>
		///     Use and inherted class and set Propertys in its super Constructor. A Proxy will be created
		/// </summary>
		Inhert,

		/// <summary>
		///     Should be used when the Constructor is private or class is sealed. No Proxy created
		/// </summary>
		FactoryMethod
	}
}