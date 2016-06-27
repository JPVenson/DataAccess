/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	public class MsSqlDataPager<T> : MsSqlUntypedDataPager<T>
	{
		public new ICollection<T> CurrentPageItems
		{
			get { return base.CurrentPageItems; }
		}
	}
}