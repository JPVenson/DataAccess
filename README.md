[![Build status](https://ci.appveyor.com/api/projects/status/vatab1g9oyo6sriq/branch/master?svg=true)](https://ci.appveyor.com/project/JPVenson/dataaccess/branch/master)

## YAORM now supports folloring Frameworks: netstandard2.0;netcoreapp2.2;netcoreapp2.1;netcoreapp2.0;net47;net471;net472

### if you got trouble with the SqLite Adapter please see the wiki "SqLite"

This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.

You can also contact me on gitter at https://gitter.im/JPVenson/DataAccess.

## Introduction
This will be a short article about my multi strategy ADO.NET wrapper that uses FactoryMethods,
Reflection or a combination of both. It is simple to use,
but a complex and powerful solution for simple and fast (fast in Development and usage) database access.

To be clear, this is designed to be a helper for very simple work. It is not created to be an EF alternative!

## Background
Well, the background of this project was that most of my colleagues worked with a very old and oversized solution that needed a lot of maintenance and changes when we started with a new project and even for simple statements like:

```SQL
SELECT * FROM Foo
```
I was forced to manually open a connection, run the statement and parse the IDataReader. I thought this is absolutely not necessary because: Most of the time, the POCOs are designed like the database with properties that are named like Column names and so on. So, this was a task I’d tried to automate.

I'd like to present my solution and I hope to get some nice ideas from you.

# Using the Code
The main parts are the IDatabase, IDatabaseStrategy and for the Main Reflection and loading the DbAccessLayer.

IDatabase defines a ínterface that maintains a Connection, this means to open a Connection, keep it open as long as it is necessary and then close it. In IDatabase, there is an IDatabaseStrategy that is used to connect to certain databases like MySQL, MsSql, OleDB and so on. The lib supports MsSQL, Obdc, OleDb from the hood, but in others, in the project included Assemblies, there are also implementations for MySQL and SqLite.

As I mentioned, there are multiple ways to load or submit data from and to a database.

For example: the simple Select from a database. We expect to be a database that is called Northwind and a Table Foo.

Create an Object that is called like your Table (Foo)
Define properties that are named and of the same type like a Column
Create a new Object of DbAccessLayer with a proper connection string Call
```C#
Select<Foo>();
```
In these 4 steps, you will execute a complete select to the database and then the result will be mapped with Reflection to the Object.

```C#
public class FooTest
{
	public class Foo
	{
		public long Id_Foo { get; set; }
		public string FooName { get; set; }
	}

	public FooTest()
	{
	var accessLayer = new DbAccessLayer(DbTypes.MsSql, "Data Source=(localdb)\\Projects;Initial Catalog=Northwind;Integrated Security=True;");
	var @select = accessLayer.Select<Foo>();
	}
}
```
There are A LOT of overloads of Select, SelectNative, SelectWhere and RunPrimetivSelect. Almost all methods with a Generic Parameter have a corresponding method that accepts a Type instance.

In all examples, when an instance of DbAccessLayer is needed, it will be represented by the variable.

accessLayer
and in the testing, an MsSQL Db is used and its syntax.

# Creating and Customizing a POCO
This is primarily an Object Relationship Mapper. That means that this lib always tries to map the output that is returned by a Query into an Object that has multiple properties. You have some attributes that define certain parts and functions of that object.

As seen in the example, you can skip all extra configuration when you follow some rules. To "bypass" these rules like the Rule that a Class must be named the same, then the Table you can set an Attribute.

## ForModel

```C#
[ForModel("Foo")]
public class NotFooButSomeStrangeNameYouDoNotLike
{
	public long Id_Foo { get; set; }
	[ForModel("FooName")]
	public string Metallica4tw { get; set; }
}
```
The ForModel attribute is allowed on Class | Table and on Property | Column level. It gives the Processor the information that the name that is used in the POCO must be mapped to the Table.

## PrimaryKey

```C#
public class Foo
{
	[PrimaryKey]
	public long Id_Foo { get; set; }
	public string FooName { get; set; }
}
```
The PrimaryKey attribute marks a Property ... what a wonder, to be an PrimaryKey on the database. With this function, you can call:
```C#
accessLayer.Select<Foo>
(155151 /*This is the PrimaryKey we are looking for*/);
```
## InsertIgnore

Marks a Property to be not Automatically included into a InsertStatement. Per default, the PrimaryKey inherits from this attribute.

## ForeignKey

With Foreign keys you can load NavigationProperties from another entity where a relation exists. To Mark an property as an NavigationProperty you have to annoatate them with the `[ForeignKey(foreignKey, referenceKey)]` Attribute where the `foreignKey` is the name of the column on your current entity and the `referenceKey` is the name of the column that should be mapped to. The type of the Property will be used to Join the tables.

```C#
public class FooTest
{
	public class Foo
	{
		[PrimaryKey]
		public long Id_Foo { get; set; }
		public string FooName { get; set; }

		public long Image_Id { get; set; }

		/// <summary>
		/// 	A Property that is of the type that is referred to
		/// 	1 TO 1 relation
		</summary>
		[ForeignKey("Image_Id", "Image_Id")]
		public virtual Image img { get; set; }

		/// <summary>
		/// 	A Property that is a List of the type that is referred to
		/// 	1 TO Many relation
		</summary>
		[ForeignKey("Image_Id", "Image_Id")]
		public virtual DbCollection<Image> imgs { get; set; }
	}

	public class Image
	{
		[PrimaryKey]
		public long Id_Image { get; set; }
		public byte[] ImageData { get; set; }
	}
}
```

There are some restraints you have to take care of. The Navigation Property must be virtual and if its a 1-n relation it must be ether directly an `DbCollection` or assignable from it, that means it could be an ICollection<T>. 



## LoadNotImplimentedDynamic

When the Select statement returns more information than build in the POCO, this property
(must have this signature):

```C#
[LoadNotImplimentedDynamic]
public IDictionary<string, object> UnresolvedObjects { set; get; }
```
(Property Name does not matter) it will be filled with the data (see FactoryMethods).

## IgnoreReflection

Simple: as the XmlIgnore attribute, it marks a Property to not be indexed and accessed by any function of the Mapper. Even if the result contains a Column that matches this property, the property will not be used.

## RowVersion

Defines a RowVersion attribute. When defined, all calls of `accessLayer.Update()` and `accessLayer.Refresh()` will use this Property to check for changes.

### Loading Strategies
There are 2 ways of loading with factory methods defined inside the POCO or automatically with customization over attributes. The 2nd way will be the fallback when there are no or not the right Factory available.

## Constructor and Method Injection

The manager can detect a method to pull statements from it. For example, how you define a method that creates a Select statement without parameter:

```C#
public class Foo
{
	public long Id_Foo { get; set; }
	public string FooName { get; set; }

	[SelectFactoryMehtod]
	public static string CreateSelectStatement()
	{
		return "SELECT * FROM Foo";
	}
}
```
When some method is defined, the manager will always use this method to create a Select statement and he will skip any other reflection based creation.

For Selects, this is also possible on Class level:

```C#
[SelectFactory("SELECT * FROM Foo")]
public class Foo
```

But only Selects must be Public and Static. Update, Insert and Delete Factory’s must be Not static. You can return a string OR an instance of IQueryFactoryResult. To prevent SqlInjection, this is the HEAVILY recommended way when you work with parameters.

An example that uses IQueryFactoryResult for Update and Delete and a String for Select:

```C#
[SelectFactory("SELECT * FROM Foo")]
public class Foo
{
	public long Id_Foo { get; set; }
	public string FooName { get; set; }

	[DeleteFactoryMethod]
	public IQueryFactoryResult CreateDeleteStatement()
	{
		var result = new QueryFactoryResult("DELETE FROM Foo WHERE Id_Foo = @1",
		new QueryParameter()
		{
		Name = "@1", Value = Id_Foo
	});
	return result;
}

[UpdateFactoryMethod]
public IQueryFactoryResult CreateSomeKindOfUpdate()
{
	var result = new QueryFactoryResult("Update Foo SET FooName = @param WHERE Id_Foo = @1",
	new QueryParameter()
	{
		Name = "@1",
		Value = Id_Foo
	},
	new QueryParameter()
	{
		Name = "@param",
		Value = FooName
	});
	return result;    
}
```
It is possible to transfer parameters from the caller to the function. When the caller provides you parameters, they will be given to the function that has the same signature then the parameter. This idea is more or less shamelessly stolen from the ASP.NET MVC approach.

After version 2.0.0.14 you can also use an QueryBuilder or QueryBuilderX on a void Method to create your Statements.

```C#
public class FooTest
{
	public class Foo
	{
		public long Id_Foo { get; set; }
		public string FooName { get; set; }

		[UpdateFactoryMethod]
		public static IQueryFactoryResult CreateSomeKindOfUpdate(string someExternalInfos)
		{
			if (string.IsNullOrEmpty(someExternalInfos))
			return null; //Noting to do here, use the Automatic loading

			var result = new QueryFactoryResult
			("SELECT * FROM Foo f WHERE f.FooName = @info", 
			new QueryParameter()
			{
				Value = someExternalInfos,
				Name = "@info"
			});
			return result;
	}


	public FooTest()
	{
		var access = new DbAccessLayer(DbTypes.MsSql, "Data Source=(localdb)\\Projects;Initial Catalog=Northwind;Integrated Security=True;");
		var @select = access.Select<Foo>("SomeName");
	}
}
```
The string that we provided to...

```C#
access.Select<Foo>("SomeName");
```
...will be given to the Select function to create a statement and this statement will be executed.

It is also possible to control the Loading from a DataRecord to your class by using a Constructor that accepts these parameters:

```C#
public class Foo
{
	[ObjectFactoryMethod]
	public Foo(IDataRecord record)
	{
		Id_Foo = (long)record["Id_Foo"];
		FooName = (string)record["FooName"];
	}

	public long Id_Foo { get; set; }
	public string FooName { get; set; }
}
```
When it is necessary to create a new Instance of that Poco, there is always a IDataRecord to load it from so via Constructor injection, we find this one and provide him the data.

## XML Field Loading
There is a new attribute:

## FromXmlAttribute
It allows a simple loading of Objects from an XML Serialized Column. The attribute contains two parameters:

FieldName [Required]
LoadStrategy [Optional]
The first Param has the same effect as the ForModel one.

The last Param defines the usage of this Property.

Should it be included into a Select Statement => Column exists

Should it be excluded from Select Statement => Column does not exist but will be added by Statement

In both cases, if the Column exists in the result stream, it will be tried to deserialized into the type that the Property defines. If this is an implementation or IEnumerable<T>
	, the result should also be formatted as list.

	# Attributeless Configuration
	As suggested from user Paulo Zemek, I modified the Reflection only MetaData API to support runtime manipulation of the Metadata.

	To configurate any object, you have to instantiate a Config class. It acts as an Fassade to the internal API.

	To extend the reflection based behavior, you have to call the SetConfig method on any Config instance. In the given callback, you have access to several methods that will add the attribute information like ForModel and so on. All helper methods are using the 3 base methods:

	```C#
	public void SetPropertyAttribute<TProp>
		(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
{
	var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
	var info = ConfigHelper.GetPropertyInfoFromLabda(exp);
	var fod = classInfo.GetOrCreatePropertyCache(info);
	fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
}
```

```C#
public void SetMethodAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
{
	var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
	var info = ConfigHelper.GetMehtodInfoFromLabda(exp);
	var fod = classInfo.MethodInfoCaches.First(s => s.MethodName == info);
	fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
}
```

```C#
public void SetClassAttribute(DataAccessAttribute attribute)
{
	var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
	classInfo.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
}
```
You could use these methods directly to add data to the internal ConfigStore or the helper one:

```C#
public void SetForModelKey<TProp>(Expression<Func<T, TProp>> exp, string value)
{
	SetPropertyAttribute(exp, new ForModel(value));
}
```

In one of the next releases, I will provide you a way for loading and store all these data in XML. All type information can be accessed by using the static methods in the Config class. That would allow you to reuse the type information.

All type access parts as ThreadSave.

There are two ways in managing configs:

- From Outside

You can call anywhere in your code:

```C#
new Config().SetConfig<T>(s => { ... })
```
This allows you to configurate a well known POCO in all ways. The generated information will be added to the LocalConfig Store.

- From Inside

Hurray! A new Attribute is there! The ConfigMehtodAttribute. You can decorate a static method with its attribute that will take a Config instance and then it allows you to configurate yourself inside the class itself.

### Speed Test
Lately, I was evaluating YAORM against other ORM's with Frans Bouma's RawBencher. I recognize that the current version has some extremely critical problems with some ... let's call it "Non optimal POCO" usage. As YAORM depends heavily on a ADO.NET conform constructor and only uses Reflection as some kind of fallback method, this way was extremely slow. In its test, it took about 6,000 ms to enumerate all 31465 entries. That was darn slow compared to EntityFramework, and don't even mention Dapper ;-).

So I made some major improvements to these POCOs that are not self containing and ADO.NET Constructor.

> ADO.NET Constructor:
>
> I was talking about an Ado.net conform Ctor. This kind of Constructor is defined by an POCO and takes an instance of IDataReader | IDataRecord and reads all necessary fields from the result set and then sets and/or converts these values to its properties.

After I made the changes to the existing code, including auto code creation due Runtime and the usage of compiled lambdas instead of the heavy usage of the reflection API, I was extremely surprised. From 6,000 ms down to 320 ms. With this test, I also made some improvements and changes to the new Config API like:

### Static Factory setting
Multibe pre-defined setter for Attributes on Properties
Control over the InMemory ADO.NET Ctor creation

# Internal Reflection
The ORM uses an Internal Reflection/IL/Expressions/CodeDom provider to generate most of the needed code due runtime.

There is a mixture of these technologys because some parts where just to timeconsuming to be implimented in IL. That is true for the CodeDOM part which are used to generate an Constructor due Runtime to load entitys. This was first used only by the EntityCreator but then also modifyed to be called due runtime. All reflection based work is located inside the MetaAPI and derived for the ORM.

> The MetaAPI uses IL and Expressions to compile accessors for Propertys and Methods. Methods are wrapped into an IL DynamicMethod and propertys are wrapped in Expressions

In future the basic Reflection API (MetaAPI) will may be moved to an very own Assambly because it is desgined to be generic. The most basic store to access everything is the 

```C#
public class MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> : 
	IDisposable
	where TClass : class, IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>, new()
	where TProp  : class, IPropertyInfoCache<TAttr>, new()
	where TAttr  : class, IAttributeInfoCache, new()
	where TMeth  : class, IMethodInfoCache<TAttr, TArg>, new()
	where TCtor  : class, IConstructorInfoCache<TAttr, TArg>, new() 
	where TArg   : class, IMethodArgsInfoCache<TAttr>, new()
```
As is said it is desgined to be generic and reusable. It contains a class to convert an Type instance to an instance of TClass by using the GetOrCreateClassInfoCache method. This method is of course also Recusiv and aware of that, it will ether give you an instance from the local store or enumerates all "Most used Infos". That means it will enumerate throu all Propertys, Methods, Arguments, Constructors and Attributes on each of them and store them. This class is optional ThreadSave by using the EnableThreadSafety property. This optional property was introduced to ensure a maximum of Performance.

This class can be ether Global or InstanceLocal. By using the constructor

```C#
public MetaInfoStore(bool isGlobal)
```
You can spezify that. To ensure a maximum of Performance you can also Impliment for example the IPropertyInfoCache and override the Init mehtod to define new Attributes that are common accessed. This brings a huge performance advance because otherwise you have to loop through the collection of all Attributes to find the desired one what, of course is timeconsuming. Take a look into the DbPropertyInfoCache to see examples.

An other good reason to use this, is the advantage of adding "fake" propertys and Attributes due Runtime by simply adding them to the collections. This feature is used by the ConfigAttribute to extend POCOs. Each part of the YAORM is using this Store and if you add a new Property to it, it will find it. For example the MethodInfoCache is implimenting an Constructor:

```C#
internal MethodInfoCache(Func<object, object[], object> fakeMehtod, string name = null, params TAtt[] attributes)
```
This allows you to add each method you want to each class without using .net Tricks such as dynamic's.

# LocalDbRepository
Its an Collection that will enforce ForginKeyDeclarationsAttributes in future also ForginKeyAttributes. With this class you can define local Databases inside a scope. All "tables" inside this scope will be validates if you add any object to it and if you try to add an Entity to it which would violate ForeignKey's an exception is thrown.

First you have to setup an DatabaseScope

```C#
using (new DatabaseScope())
{

}
```
This scope will be an Container and validates multibe Tables that are defined inside the Scope. This syntax was takes from the TransactionScope that exists within the .netFramework. Then you have to define tables by creating them inside the scope

```C#
using (new DatabaseScope())
{
	_books = new LocalDbReposetory<Book>();
	_images = new LocalDbReposetory<Image>();
}
```
The defintion for Book and Image is folloring:

```C#
public class Image
{
	[PrimaryKey]
	public long ImageId { get; set; }

	public string Text { get; set; }

	[ForeignKeyDeclaration("BookId", typeof(Book))]
	public int IdBook { get; set; }
}
```

```C#
public class Book
{
	[PrimaryKey]
	public int BookId { get; set; }

	public string BookName { get; set; }
}
```

It is important to decorate an PrimaryKeyAttribute and also an ForeignKeyDeclarationAttribute to define valid connections between both Tables. You can ether use Attributes or an Config method (s.a). The first argument on the ForgeinKeyDeclarationAttribute will be soon obsolete. You can use the Constructor of the LocalDbReposetory to define an PrimaryKey generator if you use PrimaryKeys that are not of type of Long, Int, Guid or if you want to define other Autoincriment by 1 and starting with 1.

## Version 2.0.180


### Trigger
In the latest version you can hook all DDL triggers (MsSQL) on a local collection. Supported are:
- WITH REPLICATION
- WITHOUT REPLICATION

Then on both

- AFTER
- BEFORE
- INSTEAD OF

Then on all

- INSERT
- UPDATE
- DELETE

From LocalDbTriggerTestNotInReplication:

```C#
LocalDbRepository<Users> repro;
using (var db = new DatabaseScope())
{
	repro = new LocalDbRepository<Users>(new DbConfig());
}
var orderFlag = false;
repro.Triggers.NotForReplication.For.Insert += (sender, token) =>
{
	Assert.That(orderFlag, Is.False);
	orderFlag = true;
};
repro.Triggers.NotForReplication.After.Insert += (sender, token) =>
{
	token.Cancel("AFTER");
};
Assert.That(orderFlag, Is.False);
Assert.That(() =>
{
	repro.Add(new Users());
}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("AFTER"));
Assert.That(orderFlag, Is.True);
Assert.That(repro.Count, Is.EqualTo(0));
```

## Constraints

As there are trigger there are also Constraints you can add to any Table.

All Contraints have to be added due creation of an database. That means you must use the example code in the DatabaseScope. This restriction was made to enforce that all Entities always match the given constrains.

There is support for:

- Check
  - Adds a check that will be enforced when an Item is Inserted or Updated
- Unique
  - Adds a check that will be enforced when an Item is Inserted or Updated
- Defaults
  - Adds a check that will be enforced when an Item is Inserted or Updated

From LocalDbWithConstraintsTest:

```C#
public LocalDbRepository<Image> TestInit(IEnumerable<ILocalDbCheckConstraint<Image>> checks,
			IEnumerable<ILocalDbUniqueConstraint<Image>> unique,
			IEnumerable<ILocalDbDefaultConstraint<Image>> defaults)
{
	LocalDbRepository<Image> images;
	using (new DatabaseScope())
	{
		images = new LocalDbRepository<Image>(new DbConfig());
		if (checks != null)
			foreach (var localDbCheckConstraint in checks)
			{
				images.Constraints.Check.Add(localDbCheckConstraint);
			}
		if (unique != null)
			foreach (var localDbCheckConstraint in unique)
			{
				images.Constraints.Unique.Add(localDbCheckConstraint);
			}
		if (defaults != null)
			foreach (var localDbCheckConstraint in defaults)
			{
				images.Constraints.Default.Add(localDbCheckConstraint);
			}
	}
	return images;
}

[Test]
public void AddCheckConstraint()
{
	var images = TestInit(new[]{new LocalDbCheckConstraint<Image>("TestConstraint", s =>
	{
		var item = s;
		return item.IdBook > 0 && item.IdBook < 10;
	})}, null, null);

	var image = new Image();
	image.IdBook = 20;
	Assert.That(() => images.Add(image), Throws.Exception.TypeOf<ConstraintException>());
	image.IdBook = 9;
	Assert.That(() => images.Add(image), Throws.Nothing);
	Assert.That(images.Count, Is.EqualTo(1));
}
```

Note:
There are implementations for all 3 constraints. For the Default constraint there are 2:
- LocalDbDefaultConstraint<TEntity, TValue>
- LocalDbDefaultConstraintEx<TEntity, TValue>

LocalDbDefaultConstraint: Will always update the value no matter what value was inside the entitie

LocalDbDefaultConstraintEx: Will check the value for its Default(by using C# default() operator) and if its not equals the default value it will set an predefined value
	

# Entity Creator
The lib now contains a Console Application that will be possible to create Entities based on a database. At the Current state (01.Nov.2014), only MsSql databases are supported and the testing is very basic.

The usage is simple in its basic component but has a lot of potential. And also, the idea here is to re-write the current CommandoLine tool to support complete parameterised works.

After you start the program, it will ask you for a Target directory (where the generated files will be stored) and a connection string.



After that, you will see some information from that database including Tables, StoredProcedures and Views. Views are handled the same as Tables are because the calling syntax is pretty much the same.

With typing a Number of a Table, Sp or View, you can alter the settings of that object. Other commands are:

\compile
\autoGenNames
\add
You start the process:

Starts the compiling of all Tables, SPs and views that are not excluded
Starts a simple renaming process that will Save Remove all '_',' ' chars from the database names and replacing them with C# Conform names
Not implemented (In future, it will be possible to add static loader constructors. This will dramatically increase the selecting performance. But due to the newest feature (XML based loading), this is not completely implemented).
Changes in Version 2.0

More Unit tests (yeeea)
- Mapping from DB fields to Class properties is now stored inside the ClassInfoCache and is persisted
- The Reflection API now uses HashSets instead of lists
- DataConverterExtentions are reduced
- PropertyInfoCache is now used to access Properties directly by using dynamic compiled Lambdas
- A Static factory method Delegate on ClassInfoCache level is now taking care of the creation of POCOs
- Some methods from the EntityCreator are moved from the EXE to the DataAccess.dll
- A new class "FactoryHelper" is now capable of creating ADO.NET Ctor due Runtime by using the improved methods of the EntityCreator
Major improvements in ctor creation the EntityCreator and the Runtime creator are now capable of constructors for:
  - (Single)XML
  - (List)XML
  - (Single)ForginKey
  - (List)ForginKey
  - ValueConverter
  - Null Values
  - (Possible)Null Values
  - ForModel
  - Added Multibe Comments
  - Removed the Linq Provider completely
  - Replaced the ReposetoryCollection with the DbCollection
  - Bug fixing
 
# Points of Interest
This project has brought me a lot of fun and one or two sleepless nights and I guess they will not be the last I had because of this. The lib contains a small Linq Provider that is marked as obsolete because, due to the implementation and development, I was ... let's say I was annoyed by Linq.

I expect from this project to have some ideas and more to improve my work.

Thanks to everyone that took the time to read this. Thanks also to my trainer Christian H. for his impressions and help.
