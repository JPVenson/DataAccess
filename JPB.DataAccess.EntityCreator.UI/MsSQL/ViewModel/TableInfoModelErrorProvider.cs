using System.Collections.Generic;
using System.Windows.Documents;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.DataAccess.EntityCreator.UI.MsSQL.ViewModel
{
	public class TableInfoModelErrorProvider : ErrorCollection<TableInfoViewModel>
	{
		public TableInfoModelErrorProvider()
		{
			Add(new Error<TableInfoViewModel>("The database must be contain only Letters from (a - Z)", "Database", s => CheckLetters(s.Database)));
		}

		public bool CheckLetters(string test)
		{
			const char startLetter = 'a';
			const char endLetter = 'z';

			var listOfGoodChars = new List<char>();
			listOfGoodChars.Add('_');

			for (int i = startLetter; i <= endLetter; i++)
			{
				listOfGoodChars.Add((char)i);
			}

			foreach (var letter in test)
			{
				if (!listOfGoodChars.Contains(letter))
					return false;
			}

			return true;
		}
	}
}