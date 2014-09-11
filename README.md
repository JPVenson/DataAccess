This lib is for an EF like DataAccess.

Usage and Anotatin is Attribute based.

To work with an POCO just name the class like the Table you want to operate Or place the [ForModel(name)] attribute on  it and specify the DB Column name. This works on Class|Table level as on Propertys|Columns.

USAGE:

class with different name then the Table
	
		[ForModel("TableNameOfFooTable")]
	    public class Foo
	
class with same name than Table
	
	    public class TableNameOfFooTable
	  
	  
To use the Complete helper Funktion you need to annotate the:

Primary Key property with the [PrimaryKey] attribute
RowVersion property with the [RowVersion] attribute

USAGE:

	    [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [RowVersion]
        [ForModel("RowState")]
        public byte[] RowBla { get; set; }

for propertys you want to exclude from the Automatik loading mark them with the [InsertIgnore] *( primary keys are ignored by default )*
If you do so this property will not included in the INSERT statement but it will be Auto Updated and Selected.

It is possible to load a POCO complete automatik without Annotations or additional infos.
In that case it is only nessasary that the Propertys are named as the Corresponding Column names.

The Liberay does support a number of factory Injectors.
This feature is still in work and incomplete.

Constructor Injection.

When the lib needs to create a new POCO it will search for a Ctor that has the [ObjectFactoryMethod] attribute and 
takes only one Parameter of type IDataRecord. if it does not find any with the Attribute, it will search for one that fits when it find only one this one will used.
IDataRecord will maybe an instance of my own Implimentaion: EgarDataRecord.

USAGE:
  Ctor with attribute:
  
    [ObjectFactoryMethod]
    public Foo(IDataRecord record)
    {
      //TODO load propertys from record
      //At this point the connection is allready closed and the data are stored into the Record
    }  
    
    public Foo()
    {
      //other ctors
    }  
    
    public Foo(Fooa foo)
    {
    
    }  
    
  Ctor without attribute:
  
    public Foo(IDataRecord record)
    {
      //TODO load propertys from record
      //At this point the connection is allready closed and the data are stored into the Record
      //must be the only ctor
    }

Method Injection.

To use costome generated statements define a function and mark it with the attribute.
USAGE:

  SELECT :
  
    [SelectFactoryMethod()]
    public static [string | IQueryFactoryResult] fooName();
    
  UPDATE :
  
    [UpdateFactoryMethod()]
    public [string | IQueryFactoryResult] fooName();

more will follow.

The Lib does also look for public virtual propertys to inject a FK dependency. ( WIP )

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
this feature is very basic and will not be complete implimented.

