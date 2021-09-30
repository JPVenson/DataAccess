//using System.CodeDom;
//using System.Collections.Generic;
//using System.IO;
//using JPB.DataAccess.Contacts;
//using JPB.DataAccess.EntityCreator.Core.Contracts;
//using JPB.DataAccess.Helper;
//using JPB.DataAccess.ModelsAnotations;
//using JPB.DataAccess.QueryFactory;

//namespace JPB.DataAccess.EntityCreator.Core.Compiler
//{
//	public class ProcedureCompiler : ClassCompiler
//	{
//		public ProcedureCompiler(string targetDir, string targetCsName)
//			: base(targetDir, targetCsName)
//		{

//		}

//		/// <inheritdoc />
//		public override string Type { get; set; } = "StoredProcedure";

//		public override void Compile(IEnumerable<IColumInfoModel> columnInfos, bool splitByType, Stream to = null)
//		{
//			if (string.IsNullOrEmpty(TableName))
//			{
//				TableName = TargetCsName;
//			}

//			var spAttribute = new CodeAttributeDeclaration(typeof(StoredProcedureAttribute).Name);
//			_base.CustomAttributes.Add(spAttribute);

//			if (_base.TypeParameters.Count == 0)
//			{
//				//_base.TypeParameters.Add(new CodeTypeParameter(typeof ().FullName));
//			}

//			//Create Caller
//			var createFactoryMethod = new CodeMemberMethod();
//			createFactoryMethod.Name = "Invoke" + TableName;
//			createFactoryMethod.ReturnType = new CodeTypeReference(typeof(QueryFactoryResult));
//			createFactoryMethod.CustomAttributes.Add(
//				new CodeAttributeDeclaration(typeof(SelectFactoryMethodAttribute).FullName));

//			//Create the Params
//			string query = "EXEC " + TableName;

//			var nameOfListOfParamater = "paramaters";
//			var listOfParams = new CodeObjectCreateExpression(typeof(List<IQueryParameter>));
//			var listOfParamscreator = new CodeVariableDeclarationStatement(typeof(List<IQueryParameter>), nameOfListOfParamater, listOfParams);
//			createFactoryMethod.Statements.Add(listOfParamscreator);
//			int i = 0;
//			foreach (var item in _base.Members)
//			{
//				if (item is CodeMemberProperty)
//				{
//					var variable = item as CodeMemberProperty;
//					var paramName = "param" + i++;
//					query += " @" + paramName + " ";
//					var createParams = new CodeObjectCreateExpression(typeof(QueryParameter),
//						new CodePrimitiveExpression(paramName),
//						new CodeVariableReferenceExpression(variable.Name));
//					var addToList =
//						new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater),
//							"Add", createParams);

//					createFactoryMethod.Statements.Add(addToList);
//				}
//			}

//			//Finaly create the instance
//			var createFactory = new CodeObjectCreateExpression(typeof(QueryFactoryResult),
//				new CodePrimitiveExpression(query),
//				new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater), "ToArray"));
//			var queryFactoryVariable = new CodeMethodReturnStatement(createFactory);

//			createFactoryMethod.Statements.Add(queryFactoryVariable);
//			_base.Members.Add(createFactoryMethod);
//		}
//	}
//}