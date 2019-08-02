using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Adds a Converter that is used to convert from an DB object to an C# object
	///     The Converter must inhert from
	///     ModelAnotations.IValueConverter
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ValueConverterAttribute : DataAccessAttribute
	{
		private static readonly Dictionary<object, IValueConverter> ConverterInstance;
		internal readonly Type Converter;

		static ValueConverterAttribute()
		{
			ConverterInstance = new Dictionary<object, IValueConverter>();
		}

		internal ValueConverterAttribute(IValueConverter runtimeSupport, object para)
		{
			ConverterInstance.Add(para, runtimeSupport);
		}

		/// <summary>
		/// </summary>
		/// <param name="converter" />
		public ValueConverterAttribute(Type converter)
		{
			Converter = converter;

			if (!typeof(IValueConverter).IsAssignableFrom(converter))
			{
				throw new ArgumentException("converter must be Inhert from IValueConverter", nameof(converter));
			}

			Parameter = string.Empty;
		}

		/// <summary>
		/// </summary>
		/// <param name="converter" />
		/// <param name="parameter" />
		public ValueConverterAttribute(Type converter, object parameter)
				: this(converter)
		{
			Parameter = parameter;
		}

		/// <summary>
		///     A static object that will be given to the Paramether
		/// </summary>
		public object Parameter { get; private set; }

		internal IValueConverter CreateConverter()
		{
			//TODO Cache converter results
			return (IValueConverter) Activator.CreateInstance(Converter);
		}
	}
}