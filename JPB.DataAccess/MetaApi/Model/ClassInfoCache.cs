#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     for internal use only
	/// </summary>
	[DebuggerDisplay("{Type.Name}")]
	[Serializable]
	public abstract class ClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>
			: IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg> where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new()
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		/// <summary>
		/// The MsCoreLib Assembly used for checking of an Framework Type
		/// </summary>
		public static readonly Assembly MsCoreLibAssembly = typeof(string).Assembly;
		/// <summary>
		/// The MsCoreLib Assembly used for checking of an Framework Type
		/// </summary>
		public static readonly Assembly CollectionAssembly = typeof(ICollection<>).Assembly;

		internal ClassInfoCache(Type type, bool anon = false)
		{
			//this is ok.
			//init is used to be called from an other MetaApi part after the instanciation
			//so as this constructor is internal we know what we do ;-)
			Init(type, anon);
		}

		/// <summary>
		///     For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ClassInfoCache()
		{
		}

		private Type _type;

		/// <summary>
		/// Makes on demand check in the domain for all system prefixed assemblies
		/// </summary>
		/// <returns></returns>
		public bool IsFrameworkType()
		{
			if (IsMsCoreFrameworkType)
			{
				return true;
			}

			if (Type.Assembly == CollectionAssembly)
			{
				return true;
			}

			return AppDomain.CurrentDomain.GetAssemblies().Where(f => f.FullName.StartsWith("System"))
			                .FirstOrDefault(e => e == Type.Assembly) != null;
		}

		/// <summary>
		/// Is this type Located in the MsCoreLib Assembly
		/// </summary>
		public bool IsMsCoreFrameworkType { get; private set; }

		/// <summary>
		///     The default constructor that takes no arguments if known
		/// </summary>
		public dynamic DefaultFactory { get; protected internal set; }

		/// <summary>
		///     For interal use Only
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg> Init(Type type, bool anon = false)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			if (!String.IsNullOrEmpty(Name))
			{
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			}

			Name = type.FullName;
			Type = type;

			if (Type.IsInterface)
			{
				anon = true;
			}

			Attributes = new HashSet<TAttr>(type
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new TAttr().Init(s as Attribute) as TAttr));
			Propertys = new Dictionary<string, TProp>(type
					.GetProperties(BindingFlags.Public | BindingFlags.Static |
					               BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(e => !e.GetIndexParameters().Any())
					.Select(s => new TProp().Init(s, anon) as TProp)
					.ToDictionary(s => s.PropertyName, s => s));
			Mehtods = new HashSet<TMeth>(type
					.GetMethods(BindingFlags.Public | BindingFlags.Static |
					            BindingFlags.NonPublic | BindingFlags.Instance)
					.Select(s => new TMeth().Init(s) as TMeth));
			Constructors = new HashSet<TCtor>(type
					.GetConstructors(BindingFlags.Public | BindingFlags.Static |
					                 BindingFlags.NonPublic | BindingFlags.Instance)
					.Select(s => new TCtor().Init(s) as TCtor));
			var defaultConstructor = Constructors.FirstOrDefault(f => !f.Arguments.Any());

			IsMsCoreFrameworkType = type.Assembly == MsCoreLibAssembly;

			//if (type.IsValueType)
			//{
			//	var dm = new DynamicMethod("InvokeDefaultCtorFor" + ClassName, Type, System.Type.EmptyTypes, Type, true);
			//	var il = dm.GetILGenerator();
			//	il.Emit(OpCodes.Initobj, type);
			//	var ctorDelegate = dm.CreateDelegate(typeof(Func<>).MakeGenericType(type));
			//	DefaultFactory = new TMeth().Init(ctorDelegate.Method, type) as TMeth;
			//}
			//else if (defaultConstructor != null)
			//{
			//	var dm = new DynamicMethod("InvokeDefaultCtorFor" + ClassName, Type, System.Type.EmptyTypes, Type, true);
			//	var il = dm.GetILGenerator();
			//	il.Emit(OpCodes.Newobj);
			//	var ctorDelegate = dm.CreateDelegate(typeof(Func<>).MakeGenericType(type));
			//	DefaultFactory = new TMeth().Init(ctorDelegate.Method, type) as TMeth;
			//}

			if (type.IsValueType || defaultConstructor != null)
			{
				Expression defaultExpression = null;

				if (type.IsValueType)
				{
					defaultExpression = Expression.Default(type);
				}
				else if (defaultConstructor != null)
				{
					defaultExpression = Expression.New(defaultConstructor.MethodInfo as ConstructorInfo);
				}

				var dynamicAccess = typeof(Expression)
						.GetMethods()
						.First(s => s.Name == "Lambda")
						.MakeGenericMethod(
						typeof(Func<>)
								.MakeGenericType(type)
						)
						.Invoke(null, new object[]
						{
							defaultExpression, null
						});
				var expressionBuilder = dynamicAccess.GetType().GetMethods().First(s => s.Name == "Compile");

				DefaultFactory = expressionBuilder.Invoke(dynamicAccess, null);
			}

			return this;
		}

		/// <summary>
		///     The full .net ClassName with namespace
		/// </summary>
		public string Name { get; protected internal set; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		public Type Type
		{
			get { return _type; }
			protected internal set { _type = value; }
		}

		/// <summary>
		///     All Propertys
		/// </summary>
		public Dictionary<string, TProp> Propertys { get; protected internal set; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		public HashSet<TAttr> Attributes { get; protected internal set; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		public HashSet<TMeth> Mehtods { get; protected internal set; }

		/// <summary>
		///     All Constructors
		/// </summary>
		public HashSet<TCtor> Constructors { get; protected internal set; }

		/// <summary>
		///     Creates a new Object or a Default value
		/// </summary>
		/// <returns></returns>
		public object New()
		{
			if (DefaultFactory != null)
			{
				return DefaultFactory();
			}
			return Expression.Lambda(Expression.Default(Type)).Compile().DynamicInvoke();
		}

		/// <summary>
		/// Comparers IClassInfoCache to type and to IClassInfoCache
		/// </summary>
		public static readonly ClassInfoEquatableComparer Comparer = new ClassInfoEquatableComparer();

		/// <inheritdoc />
		public bool Equals(IClassInfoCache other)
		{
			return Comparer.Equals(this, other);
		}

		/// <inheritdoc />
		public int CompareTo(IClassInfoCache other)
		{
			return Comparer.Compare(this, other);
		}

		/// <inheritdoc />
		public bool Equals(Type other)
		{
			return Comparer.Equals(Type, other);
		}
	}
}