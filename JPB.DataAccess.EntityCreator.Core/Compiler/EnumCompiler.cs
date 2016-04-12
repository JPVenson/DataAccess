namespace JPB.DataAccess.EntityCreator.Core.Compiler
{
	public class EnumCompiler : ElementCompiler
	{
		public EnumCompiler(string targetDir, string targetCsName)
			: base(targetDir, targetCsName)
		{

		}
		public override void PreCompile()
		{
			_base.IsEnum = true;
		}
	}
}