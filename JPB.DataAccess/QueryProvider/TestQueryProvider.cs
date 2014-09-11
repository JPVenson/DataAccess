using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.QueryProvider
{
    public class TestQueryProvider : QueryProvider
    {
        public DbAccessLayer DbAccessLayer { get; set; }

        public TestQueryProvider(DbAccessLayer dbAccessLayer)
        {
            DbAccessLayer = dbAccessLayer;
        }

        #region Overrides of QueryProvider

        public override string GetQueryText(Expression expression)
        {
            return null;
        }

        private Type type;

        private List<Tuple<MethodInfo, Expression>> _expressionTree = new List<Tuple<MethodInfo, Expression>>();

        private List<IQueryParameter> _parameters = new List<IQueryParameter>();

        private void SplitArguments(MethodCallExpression parent)
        {
            if (!parent.Arguments.Any())
            {
                _expressionTree.Add(new Tuple<MethodInfo, Expression>(parent.Method, parent));
                return;
            }

            var expression = parent.Arguments.Last();
            _expressionTree.Add(new Tuple<MethodInfo, Expression>(parent.Method, expression));
            SplitArguments(parent.Arguments.FirstOrDefault() as MethodCallExpression);
        }

        public override object Execute(Expression expression)
        {
            _expressionTree.Clear();
            _parameters.Clear();

            MethodCallExpression expessions = null;
            expessions = expression as MethodCallExpression;

            //http://referencesource.microsoft.com/#System.Core/System/Linq/IQueryable.cs
            //http://referencesource.microsoft.com/#System.Core/Microsoft/Scripting/Ast/ConstantExpression.cs
            //http://msdn.microsoft.com/en-us/library/bb397951.aspx

            var queryBuilder = new StringBuilder();
            SplitArguments(expression as MethodCallExpression);

            _expressionTree.Reverse();

            type = expessions.Method.GetGenericArguments().FirstOrDefault();

            foreach (var exp in _expressionTree)
            {
                queryBuilder.Append(processParameter(exp.Item1, exp.Item2));
            }

            return DbAccessLayer.RunSelect(type, DbAccessLayer.Database, queryBuilder.ToString(), _parameters);
        }


        //var exp = (MethodCallExpression)expression;
        //var queryBuilder = new StringBuilder();
        //foreach (var argument in exp.Arguments)
        //{
        //    queryBuilder.Append(processParameter(argument));
        //}

        private string ReplaceExpressionWithTableName(BinaryExpression argument)
        {
            var exp = argument as BinaryExpression;
            var getLeftHandExp = exp.Left as MemberExpression;
            var expression = getLeftHandExp.Expression;
            var expressionAsString = exp.ToString();
            var leftHandExp = expression.ToString();
            //replace alias with Table name
            var indexOfDot = expressionAsString.IndexOf(".");
            var indexOfExpression = expressionAsString.IndexOf(leftHandExp, 0, indexOfDot);
            expressionAsString = expressionAsString.Remove(indexOfExpression, indexOfDot);
            expressionAsString = expressionAsString.Insert(indexOfExpression, type.GetTableName() + ".");
            //replace column name with mapped name
            indexOfDot = expressionAsString.IndexOf(".") + 1;
            var indexOfOperator = expressionAsString.IndexOf(' ', indexOfDot);


            var maybeNotRealColumnName = expressionAsString.Substring(indexOfDot, indexOfOperator - indexOfDot);
            expressionAsString = expressionAsString.Remove(indexOfDot, indexOfOperator - indexOfDot);
            expressionAsString = expressionAsString.Insert(indexOfDot,
                type.MapEntiysPropToSchema(maybeNotRealColumnName));
            expressionAsString = expressionAsString.Replace('(', ' ');
            expressionAsString = expressionAsString.Replace(')', ' ');

            var parama = new QueryParameter();
            parama.Name = "@" + _parameters.Count;
            parama.Value = getValueFromExp(exp.Right);
            _parameters.Add(parama);

            //TODO remove everything from the right 
            expressionAsString = expressionAsString.Replace(exp.Right.ToString(), parama.Name);
            return expressionAsString;
        }

        private object getValueFromExp(Expression exp)
        {
            if ((exp is ConstantExpression))
            {
                var constantExpression = (exp as ConstantExpression);
                return constantExpression.Value;
            }
            if (exp is MemberExpression)
            {
                return GetValue(exp as MemberExpression);
            }

            throw new NotSupportedException("This expression is not suported") { Data = { { "Expression", exp.GetType() } } };
        }

        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        private string processParameter(MethodInfo item1, Expression argument)
        {
            var query = "";
            //Sql operator Syntax cleanup

            //Sql Query Syntax cleanup
            if (argument.NodeType == ExpressionType.Equal)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression).Replace("==", "=");
            }
            if (argument.NodeType == ExpressionType.LessThan)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.GreaterThan)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.LessThanOrEqual)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument is UnaryExpression)
            {
                return processParameter(item1, (argument as UnaryExpression).Operand);
            }
            else if (argument is LambdaExpression)
            {
                return processParameter(item1, (argument as LambdaExpression).Body);
            }

            else if (argument is MethodCallExpression)
            {
                //TODO test for CRUD

                var methodCall = argument as MethodCallExpression;
                var upper = methodCall.Method.Name.ToUpper();
                if (upper.Contains("SELECT"))
                {
                    return DbAccessLayer.CreateSelect(type) + " ";
                }
                //if (upper.Contains("DELETE"))
                //{
                //    return DbAccessLayer.CreateSelectQueryFactory(targetType);
                //}
                //if (upper.Contains("INSERT"))
                //{
                //    return DbAccessLayer.CreateSelectQueryFactory(targetType);
                //}
            }

            return query;
        }

        #endregion
    }
}
