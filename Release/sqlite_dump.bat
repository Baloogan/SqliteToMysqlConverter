set input=%1
set output=%input:.db3=.sql%
set zipoutput=%input:.db3=.zip%
set schemaoutput=%input:.db3=.schema.sql%
sqlite3.exe %input% .schema | "sqlite3_mysql.exe" > %schemaoutput%

sqlite3.exe %input% .dump | "sqlite3_mysql.exe" > %output%



rem "C:\Program Files\7-Zip\7z.exe" a -tzip %zipoutput% %output%
rem
rem




