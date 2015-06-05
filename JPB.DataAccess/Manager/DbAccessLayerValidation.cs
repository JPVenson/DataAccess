using JPB.DataAccess.ModelsAnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        /// <summary>
        /// Validates a Entity
        /// 
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        /// <param name="instance"></param>
        public static void ValidateEntity([NotNull]object instance)
        {
            var context = new ValidationContext(instance);
            Validator.ValidateObject(instance, context, false);
        }

        /// <summary>
        /// Validates a Entity
        /// 
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        /// <param name="instance"></param>
        public static void ValidateEntityPk([NotNull]object instance)
        {
            var pkProperty = instance.GetType().GetPKPropertyName();
            var context = new ValidationContext(instance);
            context.MemberName = pkProperty;
            Validator.ValidateProperty(instance, context);
        }

        /// <summary>
        /// Validates a Entity
        /// 
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        /// <param name="instance"></param>
        public static Tuple<bool, ICollection<ValidationResult>> TryValidateEntity([NotNull]object instance)
        {
            var context = new ValidationContext(instance);
            var result = new Collection<ValidationResult>();
            var success = Validator.TryValidateObject(instance, context, result);
            return new Tuple<bool, ICollection<ValidationResult>>(success, result);
        }
    }
}
