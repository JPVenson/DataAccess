//using System;
//using System.Collections.Generic;
//using System.CodeDom.Compiler;
//using System.Linq;
//using System.Data;
//using JPB.DataAccess.ModelsAnotations;
//using JPB.DataAccess.DbInfoConfig;
//using JPB.DataAccess.AdoWrapper;

//namespace OcKarat.NextGen.Database.Entities.Pocos
//{

//	//<author>
//	//<created-by>BORGCUBE\User</created-by>
//	//<created-on> 2019 March 29<created-on>
//	//</author>

//	[GeneratedCodeAttribute(tool: "JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator", version: "2.0.0.0")]
//	public partial class Address
//	{
//		public Address(EagarDataRecord reader)
//		{
//			this.AddressId = (Int32)reader["AddressId"];
//			this.Gender = (Boolean)reader["Gender"];
//			this.SalutationContactPerson = (String)reader["SalutationContactPerson"];
//			this.FirstNameContactPerson = (String)reader["FirstNameContactPerson"];
//			this.LastNameContactPerson = (String)reader["LastNameContactPerson"];
//			this.Street = (String)reader["Street"];
//			this.StreetNo = (String)reader["StreetNo"];
//			this.ZipCode = (String)reader["ZipCode"];
//			this.City = (String)reader["City"];
//			this.FederalCountry = (String)reader["FederalCountry"];
//			this.CountryISO = (String)reader["CountryISO"];
//			this.PostBoxCode = (String)reader["PostBoxCode"];
//			this.PostBoxZipCode = (String)reader["PostBoxZipCode"];
//			this.EMailAddress = (String)reader["EMailAddress"];
//			this.PhoneNumber = (String)reader["PhoneNumber"];
//			this.Office = (String)reader["Office"];
//			this.Funktion = (String)reader["Funktion"];
//			this.DateOfBirth = (DateTime?)reader["DateOfBirth"];
//			this.CompanyName = (String)reader["CompanyName"];
//		}
//		public Int32 AddressId { get; set; }
//		public Boolean Gender { get; set; }
//		public String SalutationContactPerson { get; set; }
//		public String FirstNameContactPerson { get; set; }
//		public String LastNameContactPerson { get; set; }
//		public String Street { get; set; }
//		public String StreetNo { get; set; }
//		public String ZipCode { get; set; }
//		public String City { get; set; }
//		public String FederalCountry { get; set; }
//		public String CountryISO { get; set; }
//		public String PostBoxCode { get; set; }
//		public String PostBoxZipCode { get; set; }
//		public String EMailAddress { get; set; }
//		public String PhoneNumber { get; set; }
//		public String Office { get; set; }
//		public String Funktion { get; set; }
//		public DateTime? DateOfBirth { get; set; }
//		public String CompanyName { get; set; }
//		static partial void BeforeConfig();
//		static partial void AfterConfig();
//		static partial void BeforeConfig(ConfigurationResolver<Address> config);
//		static partial void AfterConfig(ConfigurationResolver<Address> config);
//		[ConfigMehtodAttribute]
//		public static void Configuration(ConfigurationResolver<Address> config)
//		{
//			BeforeConfig();
//			BeforeConfig(config);
//			config.SetPropertyAttribute(s => s.AddressId, new ForModelAttribute(alternatingName: "Address_Id"));
//			config.SetPropertyAttribute(s => s.AddressId, new PrimaryKeyAttribute());
//			AfterConfig(config);
//			AfterConfig();
//		}
//	}
//}
