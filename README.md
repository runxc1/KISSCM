# KISSCM
KISS Database Change Management with DDL rollback for SQL Server
# KISS Database Change Management

Project Description
KISS is so simple that you can get your database versioning set in place in less than 15 minutes.
KISS can be integrated into any automated build as its a simple dotnet core console app.
Convention-

1. Create numbered Database Version scripts that are either hand written or created by a tool like DBDiff or RedGate or Whatever.
Examples
```
001-Creating some tables.sql
002-Creating-some-more-tables.sql
003-Insertingsomedataandcreating_views.sql
```
___


2. Place all of the Database change scripts in the same folder.

3. Thats it. What you thought it should be more difficult?
---
### Execution
KISS is a dotnet core 2.1 console app but the code is so simple you could easily refactor for your own uses.
```
dotnet .\KISSCM.dll /f:Kissprops.xml
```

Kissprops.xml is a simple xml file that specifies the connection string and DataBase Provider that is going to be used.
ExampleFile
```
<?xml version="1.0" encoding="utf-16"?>
<Properties xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<Verbose>true</Verbose>
<Wait>false</Wait>
<Provider>SQLServer</Provider>
<ConnectionString>server=.\SQLEXPRESS;database=KISS;User Id=KissUser;Password=$donate$;</ConnectionString>
<VersionTable>db_schema</VersionTable>
<VersionScriptsFolder>C:\dev\Schema\SqlServer</VersionScriptsFolder>
</Properties>
```
Behind the scenes
KISS will first discover what version the database is at by reading from the Version Table you specified (If the table doesn't exist KISS will create it so you don't need to worry about it).
KISS will then run each script that has a version above that of the database.
KISS will execute batch scripts so you can create multiple tables, views etc in the same script.
KISS executes each script as a Transaction so if the script fails the Transaction is rolled back the error message is displayed and execution is stopped. It returns an error code of 400 so that if you use it with MsBuild or Nant your build will Fail.

Currently Supported DataBases
Sql Server

KISS uses the "SMO" SQL SERVER Management Objects so that script execution is handled in the same manner as if it was run through SQL SERVER Management Studio which allows GO statements and whole scripts to be rolled back that include DDL and DML statements.