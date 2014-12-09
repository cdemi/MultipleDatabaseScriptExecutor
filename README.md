# Introduction #
This tool allows you to run SQL Queries across different MS SQL databases. It may be useful for cases where you need to run some upgrade scripts.

# Requirements #
You need to have the following files in your root folder (relative to the executable)

- `config.json`
- At least one `*.sql` file

## config.json ##
The `config.json` file must be of the format:

    {"ConnectionStrings":["ConnectionString1","ConnectionString2"]}

## *.sql files ##
You can have any amount of *.sql files in the root folder of your application. They will all be grabbed by the application and executed across all databases.

# Usage #
You can either download the solution and build it from source, or you can just download the binary file [MDSE.zip](https://github.com/cdemi/MultipleDatabaseScriptExecutor/raw/master/MDSE.zip).

After you have downloaded (or compiled) these files, you can just execute `MDSE.exe`. The application will read `config.json` and any `*.sql` files located at the root of your application.

# Errors #
If the application encounters any errors, it will not stop execution. Rather it will skip the erroneous SQL file and move on to the next. An error file will be created at the root folder of the application with the name format `<SQLFileName>_<DatabaseName>_<Date>.txt`.

# Updates #
This by far can be improved a lot and is not a complete product in any way. If you want you can fork this or submit pull requests.