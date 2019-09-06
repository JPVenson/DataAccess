/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Collections.Generic;

namespace JPB.DataAccess.MySql
{
    public class MySqlDataPager<T> : MySqlUntypedDataPager<T>
    {
	    /// <summary>
	    ///     Gets the current page items.
	    /// </summary>
	    /// <value>
	    ///     The current page items.
	    /// </value>
	    public new ICollection<T> CurrentPageItems
	    {
		    get { return base.CurrentPageItems; }
	    }
    }
}