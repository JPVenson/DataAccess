﻿/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using JPB.DataAccess.DbInfoConfig;

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
			var pkProperty = this.GetClassInfo(instance.GetType()).PrimaryKeyProperty.PropertyName;
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