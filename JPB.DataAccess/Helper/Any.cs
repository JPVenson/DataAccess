using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	/// Helper Class to enumerate a Table Directly
	/// </summary>
	internal sealed class Any : DynamicObject, ICustomTypeDescriptor
	{
		internal Any(string tableName)
		{
			Table = tableName;
		}

		internal string Table { get; set; }

		[ConfigMehtod]
		public static void Configurate(ConfigurationResolver<Any> resolver)
		{
			resolver.SetPropertyAttribute(f => f.PropertyBag, new LoadNotImplimentedDynamicAttribute());
			resolver.SetPropertyAttribute(f => f.Table, new IgnoreReflectionAttribute());
		}

		public IDictionary<string, object> PropertyBag { get; set; }

		/// <inheritdoc />
		public AttributeCollection GetAttributes()
		{
			return AttributeCollection.Empty;
		}

		/// <inheritdoc />
		public string GetClassName()
		{
			return Table;
		}

		/// <inheritdoc />
		public string GetComponentName()
		{
			return null;
		}

		/// <inheritdoc />
		public TypeConverter GetConverter()
		{
			return null;
		}

		/// <inheritdoc />
		public EventDescriptor GetDefaultEvent()
		{
			return null;
		}

		/// <inheritdoc />
		public PropertyDescriptor GetDefaultProperty()
		{
			return GetProperties()[0];
		}

		/// <inheritdoc />
		public object GetEditor(Type editorBaseType)
		{
			return null;
		}

		/// <inheritdoc />
		public EventDescriptorCollection GetEvents()
		{
			return null;
		}

		/// <inheritdoc />
		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return null;
		}

		/// <inheritdoc />
		public PropertyDescriptorCollection GetProperties()
		{
			return new PropertyDescriptorCollection(this.PropertyBag.Select(e => new DbPropertyDescriptor(e.Key, this) as PropertyDescriptor).ToArray());
		}
		/// <inheritdoc />

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return GetProperties();
		}

		/// <inheritdoc />
		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
	}

	/// <inheritdoc />
	internal class DbPropertyDescriptor : PropertyDescriptor
	{
		private readonly Any _owner;

		/// <inheritdoc />
		public DbPropertyDescriptor(string name, Any owner) : base(name, new Attribute[0])
		{
			_owner = owner;
		}

		/// <inheritdoc />
		public override bool CanResetValue(object component)
		{
			return false;
		}

		/// <inheritdoc />
		public override object GetValue(object component)
		{
			return _owner.PropertyBag[Name];
		}

		/// <inheritdoc />
		public override void ResetValue(object component)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void SetValue(object component, object value)
		{
			_owner.PropertyBag[Name] = value;
		}

		/// <inheritdoc />
		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		/// <inheritdoc />
		public override Type ComponentType
		{
			get { throw new NotImplementedException(); }
		}

		/// <inheritdoc />
		public override bool IsReadOnly
		{
			get { return false; }
		}

		/// <inheritdoc />
		public override Type PropertyType
		{
			get { return _owner.PropertyBag[Name].GetType(); }
		}
	}

}
