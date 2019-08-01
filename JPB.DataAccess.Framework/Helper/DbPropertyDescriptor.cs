using System;
using System.ComponentModel;

namespace JPB.DataAccess.Framework.Helper
{
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