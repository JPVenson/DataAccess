This lib is for an EF like DataAccess.

Usage and Anotatin is Attribute based.

To work with an POCO just name the class like the Table you want to operate Or place the [ForModel(name)] attribute on it
and specify the DB Column name. This works on Class|Table level as on Propertys|Columns.

To use the Complete helper Funktion you need to annotate the:

Primary Key property with the [PrimaryKey] attribute
RowVersion property with the [RowVersion] attribute

for propertys you want to exclude from the Automatik loading mark them with the [InsertIgnore]. 
If you do so this property will not included in the INSERT statement but it will be Auto Updated and Selected.

It is possible to load a POCO complete automatik without Annotations or additional infos.
In that case it is only nessasary that the Propertys are named as the Corresponding Column names.

The Liberay does support a number of factory Injectors.
This feature is still in work and incomplete.

Constructor Injection.

When the lib needs to create a new POCO it will search for a Ctor that has the [ObjectFactoryMethod] attribute and 
takes only one attribute 
