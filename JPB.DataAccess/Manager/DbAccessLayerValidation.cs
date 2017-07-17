#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

#endregion

namespace JPB.DataAccess.Manager
{
	public partial class DbAccessLayer
	{
		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public void ValidateEntity(object instance)
		{
			var context = new ValidationContext(instance);
			Validator.ValidateObject(instance, context, false);
		}

		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public void ValidateEntityPk(object instance)
		{
			var pkProperty = GetClassInfo(instance.GetType()).PrimaryKeyProperty.PropertyName;
			var context = new ValidationContext(instance);
			context.MemberName = pkProperty;
			Validator.ValidateProperty(instance, context);
		}

		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public Tuple<bool, ICollection<ValidationResult>> TryValidateEntity(object instance)
		{
			var context = new ValidationContext(instance);
			var result = new Collection<ValidationResult>();
			var success = Validator.TryValidateObject(instance, context, result);
			return new Tuple<bool, ICollection<ValidationResult>>(success, result);
		}
	}
}