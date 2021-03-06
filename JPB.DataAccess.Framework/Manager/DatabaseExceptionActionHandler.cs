﻿using System;
using JPB.DataAccess.DbEventArgs;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A database operation is done with an exception
	/// </summary>
	public delegate void DatabaseExceptionActionHandler(object sender, DatabaseActionEvent e, Exception ex);
}