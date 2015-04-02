This lib is for an EF like DataAccess.

Usage and Annotating is Attribute based.

To work with an POCO just name the class like the Table you want to operate Or place the [ForModel(name)] attribute on  it and specify the DB Column name. This works on Class|Table level as on Propertys|Columns.

USAGE:

class with different name then the Table
	
	    [ForModel("TableNameOfFooTable")]
	    public class Foo { }
	
class with same name than Table
	
	    public class TableNameOfFooTable { }
	  
	  
To use the Complete helper Function you need to annotate the:

Primary Key property with the [PrimaryKey] attribute
RowVersion property with the [RowVersion] attribute

USAGE:

	[PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [RowVersion]
        [ForModel("RowState")]
        public byte[] RowBla { get; set; }

for property’s you want to exclude from the Automatic loading mark them with the [InsertIgnore] *( primary keys are ignored by default )*
If you do so this property will not include in the INSERT statement but it will be Auto Updated and Selected.

It is possible to load a POCO complete automatic without Annotations or additional info’s.
In that case it is only necessary that the Property’s are named as the Corresponding Column names.

The Library does support a number of factory Injectors.
This feature is still in work and incomplete.

Constructor Injection.

When the lib needs to create a new POCO it will search for a Ctor that has the [ObjectFactoryMethod] attribute and 
Takes only one Parameter of type IDataRecord. if it does not find any with the Attribute, it will search for one that fits when it find only one this one will used.
IDataRecord will maybe an instance of my own Implementation: EgarDataRecord.

USAGE:
  Ctor with attribute:
  
    [ObjectFactoryMethod]
    public Foo(IDataRecord record)
    {
      //TODO load propertys from record
      //At this point the connection is already closed and the data are stored into the Record if you do not change the LoadCompleteResultBeforeMapping property to false
    }  
    
    public Foo()
    {
      //other ctors
    }  
    
    public Foo(Northwind nord)
    {
    
    }  
    
  Ctor without attribute:
  
    public Foo(IDataRecord record)
    {
      //TODO load property’s from record
      //At this point the connection is allready closed and the data are stored into the Record
      //must be the only ctor
    }

Method Injection.

To use custom generated statements define a function and mark it with the attribute.
USAGE:

  SELECT:
  
    [SelectFactoryMethod()]
    public static [string | IQueryFactoryResult] fooName();
    
  UPDATE :
  
    [UpdateFactoryMethod()]
    public [string | IQueryFactoryResult] fooName();

More will follow.

The Lib does also look for public virtual property’s to inject a FK dependency. ( WIP )

if a property is Defined as Public Virtual and contains a ForeignKeyAttribute it will be loaded when you select that class.


USAGE
	1 to 1 dependency
	
		public long? ID_OFFOO { get; set; }
		
		[ForeignKeyAttribute("ID_OFFOO")]
		public virtual Foo FoosProperty { get; set; }
		
		
	1 to many dependency
	
		public long? ID_OFFOO { get; set; }
		
		[ForeignKeyAttribute("ID_OFFOO")]
		public virtual ICollection<Foo> FoosProperty { get; set; }
		
		
As this is still a WIP feature, items will only loaded recursive into the list but new items are not tracked.

Linq

The lib contains a small Linq Provider ( WIP )
this feature is very basic and will not be complete implemented.





DbAccessLayer

This is the Main class that is used to calculate everything.

USAGE:

		var layer = new DbAccessLayer(DbTypes.MsSql, "ConnectionString");
		List<Foo> foos = layer.Select<Foo>();
		
This will Create a DB connection, Enumerate all propertys or call a factory, to create a Select Statement, then execute this statement and load the propertys back into the object via reflection or Factory.

Performace.

Performace is also very importend. The Solution contains a Console application that will access your localdb to create test tables and insert and select from and to it. Then it will generate a small report with the times. 
Compare it to Dapper or EF.

Extend it.

You can add any Database as long as you impliemnt the IDatabaseStrategy for it. There are default implimentations for MsSql, MsSql and Untested for Odbc and OleDb. The ORM is desgined to word on top of this Ado.net implimentations. You can use the Ado wrapper as Usual by using the Database property of DbAccessLayer or simply by creating the IDatabase implimentation.

QueryBuilder.

There is a implimentation that is not using IQueryable but IEnumerable. There is also an Unsupported IQueryable implimentation but its constructor is marked as Obsolete. The working implimentation uses Extention mehtods to write SQL confrom strings into a list. At the end the string are parsed with parameter optional and then the statement can be executed syncronly or asyncronly by enumerating the collection.

	Example:
	var withParamerters = new DbAccessLayer(DbAccessType.MsSql, "")
                .Query()
                //Create a new Query object that handles all calls
                .Select<Foo>()
                //Create a select query for type
                .As("s")
                //add the "AS (alias)" query part
                .Join<Foo, Foo>(TJoinMode.Left)
                //add a Join on a table by using the TJoin for left joins
                .Where("", new { param = 1 })
                //For allmost all mehtods we can overhand a dynamic object for paramethers it is not
                //Importend where we do that they are handled globaly
                .InBracket(s =>
                {
                    //This is for a better readability everything inside this action will be wrapped into "( )"
                    s.QueryD("s.Id < @param");
                    //We can also add anything with QueryD QueryQ and Query
                    s.Or("s.Name CONTAINS @param2");
                    //Create an Or statement
                })
                .WithParamerters(new { param2 = "test" });
        //We can also add the paramthers at the end or when every we would like to

        var compileFlat = withParamerters.CompileFlat();
        //This will return as a local compiled statement from this query without executing on server
        Console.WriteLine(compileFlat.Item1);
        
	Output:
	SELECT ID, Name FROM Foo AS s 
		LEFT JOIN Foo ON Foo.ID = Foo.FK_Self 
		WHERE ( s.Id < @param OR s.Name CONTAINS @param2 ) 
		
	We could also call ToArray() or To List or loop throu it inside an ForEach.
	
            foreach (object withParamerter in withParamerters)
            {
                Console.WriteLine(withParamerter /*this is still an object*/);
            }
        To create a strongly typed Query just call 
            foreach (Foo withParamerter in withParamerters.ForResult<Foo>();)
            {
                Console.WriteLine(withParamerter /*this is now casted to Foo*/);
            }
        at the end just before executing.
        
        
        Future:
        
        I am currently working on some TCP intirgration for Realtime state changeing. The Project will be partial OpenSource 	because the TCP wrapper isn't for now.


[![Join the chat at https://gitter.im/JPVenson/DataAccess](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JPVenson/DataAccess?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)