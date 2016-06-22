using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;

namespace JPB.DataAccess.Tests.Overwrite
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
				mgr.ExecuteGenericCommand(string.Format("VACUUM"), null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", BookMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("VACUUM"), null);
				mgr.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", ImageMeta.TableName), null);
				mgr.ExecuteGenericCommand(string.Format("VACUUM"), null);
			}
		}

		public static long[] AddUsers(int number, DbAccessLayer mgr)
		{
			var users = new List<Users>();
			for (int i = 0; i < number; i++)
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
			for (int i = 0; i < number; i++)
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
			for (int i = 0; i < number; i++)
			{
				var book = new Book();
				book.BookName = Guid.NewGuid().ToString();
				books.Add(book = mgr.InsertWithSelect(book));

				for (int j = 0; j < imagesPerBook; j++)
				{
					mgr.Insert(new Image()
					{
						Text = Guid.NewGuid().ToString(),
						IdBook = book.BookId
					});
				}
			}
			return books.Select(f => f.BookId).ToArray();
		}

		public static long[] AddImages(int number, DbAccessLayer mgr)
		{
			var images = new List<Image>();
			for (int i = 0; i < number; i++)
			{
				var image = new Image();
				image.Text = Guid.NewGuid().ToString();
				images.Add(mgr.InsertWithSelect(image));
			}
			return images.Select(f => f.ImageId).ToArray();
		}
	}
}
