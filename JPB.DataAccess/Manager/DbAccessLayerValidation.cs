using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Manager
{
	public partial class DbAccessLayer
	{
		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public static void ValidateEntity( object instance)
		{
			var context = new ValidationContext(instance);
			Validator.ValidateObject(instance, context, false);
		}

		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public static void ValidateEntityPk( object instance)
		{
			string pkProperty = instance.GetType().GetPKPropertyName();
			var context = new ValidationContext(instance);
			context.MemberName = pkProperty;
			Validator.ValidateProperty(instance, context);
		}

		/// <summary>
		///     Validates a Entity
		/// </summary>
		/// <exception cref="ValidationException"></exception>
		public static Tuple<bool, ICollection<ValidationResult>> TryValidateEntity( object instance)
		{
			var context = new ValidationContext(instance);
			var result = new Collection<ValidationResult>();
			bool success = Validator.TryValidateObject(instance, context, result);
			return new Tuple<bool, ICollection<ValidationResult>>(success, result);
		}
	}
}