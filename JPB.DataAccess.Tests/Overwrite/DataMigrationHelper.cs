#region

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

#endregion

namespace JPB.DataAccess.Tests
{
	public static class DataMigrationHelper
	{
		public static void ClearDb(DbAccessLayer mgr)
		{
			mgr.Config.Dispose();
			DbConfig.Clear();
			if (mgr.DbAccessType == DbAccessType.MsSql)
			{
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", ImageMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", BookMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", ImageMeta.TableName), null);
			}

			if (mgr.DbAccessType == DbAccessType.SqLite)
			{
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
				mgr.ExecuteGenericCommand("VACUUM", null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", BookMeta.TableName), null);
				mgr.ExecuteGenericCommand("VACUUM", null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", ImageMeta.TableName), null);
				mgr.ExecuteGenericCommand("VACUUM", null);
			}
		}

		public static TPk[] AddEntity<T, TPk>(DbAccessLayer mgr, int number, Action<T> defaulting = null)
			where T : class
		{
			var typeCache = mgr.GetClassInfo(typeof(T));

			if (typeCache.PrimaryKeyProperty.PropertyType != typeof(TPk))
				throw new NotSupportedException(
					$"Wrong Generic for that Entity. Expected was {typeof(TPk)} but was {typeCache.PrimaryKeyProperty.PropertyType}");

			return AddEntity(mgr, number, typeof(T), o =>
			{
				if (defaulting != null)
					defaulting(o as T);
			}).Select(f => (TPk) f).ToArray();
		}

		public static object[] AddEntity(DbAccessLayer mgr, int number, Type poco, Action<object> defaulting = null)
		{
			var typeCache = mgr.GetClassInfo(poco);
			if (typeCache.PrimaryKeyProperty == null)
				throw new NotSupportedException("Please provide a PK for that Entity");

			var users = new List<object>();
			for (var i = 0; i < number; i++)
			{
				dynamic user = typeCache.New();
				if (defaulting != null)
					defaulting(user);
				users.Add(mgr.InsertWithSelect(poco, user));
			}

			return users.Select(f => typeCache.PrimaryKeyProperty.Getter.Invoke(f)).ToArray();
		}

		public static long[] AddUsers(int number, DbAccessLayer mgr)
		{
			var users = new List<Users>();
			for (var i = 0; i < number; i++)
			{
				var user = new Users();
				user.UserName = Guid.NewGuid().ToString();
				users.Add(mgr.InsertWithSelect(user));
			}
			return users.Select(f => f.User_ID).ToArray();
		}

		public static int[] AddBooks(int number, DbAccessLayer mgr)
		{
			var books = new List<Book>();
			for (var i = 0; i < number; i++)
			{
				var book = new Book();
				book.BookName = Guid.NewGuid().ToString();
				books.Add(mgr.InsertWithSelect(book));
			}
			return books.Select(f => f.BookId).ToArray();
		}

		public static int[] AddBooksWithImage(int number, int imagesPerBook, DbAccessLayer mgr)
		{
			var books = new List<Book>();
			for (var i = 0; i < number; i++)
			{
				var book = new Book();
				book.BookName = Guid.NewGuid().ToString();
				books.Add(book = mgr.InsertWithSelect(book));

				for (var j = 0; j < imagesPerBook; j++)
					mgr.Insert(new Image
					{
						Text = Guid.NewGuid().ToString(),
						IdBook = book.BookId
					});
			}
			return books.Select(f => f.BookId).ToArray();
		}

		public static long[] AddImages(int number, DbAccessLayer mgr)
		{
			var images = new List<ImageNullable>();
			for (var i = 0; i < number; i++)
			{
				var image = new ImageNullable();
				image.Text = Guid.NewGuid().ToString();
				image.IdBook = null;
				images.Add(mgr.InsertWithSelect(image));
			}
			return images.Select(f => f.ImageId).ToArray();
		}
	}
}