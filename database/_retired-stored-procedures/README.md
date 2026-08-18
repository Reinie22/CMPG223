# Retired stored procedures

These files are **no longer used** and are not run by anything. They are kept
only so that the earlier approach is not lost.

## What happened

The project originally called stored procedures from the repository classes.
The group changed to writing the SQL inline in the repositories instead, as
ordinary `SELECT` / `INSERT` / `UPDATE` statements with parameters.

The reason is that this is a database module and the marks are in the SQL. With
stored procedures, the SQL lives in a file a marker has to go and find. Inline,
it sits directly next to the C# that runs it, so the query and its purpose are
read together.

**Safety did not change.** Stored procedures are not what makes a query safe -
*parameters* are. Every query in the repositories passes its values as
`SqlParameter` objects and never glues user input into the SQL string, which is
exactly what the procedures did internally. See the long comment at the top of
`Data/Db.cs` for the full explanation, with examples of the attack it prevents.

## Do not run these

Running them creates procedures the application never calls. They will drift out
of step with the schema and confuse the next person who reads them.

## What replaced each file

| Retired file | Replaced by |
|---|---|
| `Attraction.sql` | `Data/AttractionRepository.cs` |
| `Lookup.sql` | `Data/LookupRepository.cs` |
| `User.sql` | `Data/UserRepository.cs` |

Once the project is in git and everyone is comfortable that nothing was lost,
this whole folder can be deleted.
