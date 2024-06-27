using OpenAI_API;
using OpenAI_API.Chat;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relational2Rdf.Converter.Ai
{
	public class AiMagic
	{
		private readonly IInferenceService _ai;

		public AiMagic(IInferenceService ai)
		{
			_ai = ai;
		}

		private string getRelationShipPrompt(string table, string columns) =>
				 $$"""
			Given the following column names of the table "{{table}}", generate clean rdf predicate names which describes the relationship between the table and the columns
			columns: {{columns}}

			Example:
			Table: Teacher
			Columns: [id, age, location]

			respond in the following json format:
			{
				"id": "hasId",
				"age": "hasAge",
				"location": "isLocatedIn"
			}

			dont explain things, just respond in json
			""";

		public async Task<Dictionary<string, string>> GetRdfRelationshipNamesAsync(string table, IEnumerable<string> columns)
		{
			if (columns.Count() == 0)
				return new();

			int failCount = 0;
			Dictionary<string, string> result = null;
			var record = Profiler.CreateTrace(nameof(AiMagic), nameof(GetRdfFriendlyNamesAsync));
			record.Start();
			while ((result == null || columns.Any(x => result.ContainsKey(x) == false)) && failCount++ < 3)
			{
				var colList = string.Join(", ", columns.Select(x => $"\"{x}\""));
				var prompt = getRelationShipPrompt(table, colList);
				var dict = await _ai.RequestJsonModelAsync<Dictionary<string, string>>(prompt);
				if (result == null)
					result = dict;
				else
					result.Merge(dict);
			}

			record.Stop($"Ended with {failCount - 1} Retries");
			if (failCount == 4)
				throw new TimeoutException($"AI failed to generate rdf names 3 times in a row");

			return result;
		}

		public async Task<Dictionary<string, string>> GetRdfFriendlyNamesAsync(IEnumerable<string> names)
		{
			if (names.Count() == 0)
				return new();

			var nameList = string.Join(", ", names);
			var prompt = $$"""
			Given the following database table names, 
			{{nameList}}

			Reply with a json object which maps the table name to a cleaned RDF type name

			Example: 
			{
				"l_masseinheit": "UnitOfMeasurement"
			}
			""";

			return await _ai.RequestJsonModelAsync<Dictionary<string, string>>(prompt);
		}

		public class AiManyToManyMapping
		{
			public string Forward { get; set; }
			public string Backward { get; set; }
		}

		public class AiForeignKey : IForeignKey
		{
			public string Name { get; set; }
			public string FromSchema { get; set; }
			public string FromTable { get; set; }
			public string ReferencedTable { get; set; }
			public string ReferencedSchema { get; set; }
			public AiColumnReference[] References { get; set; }

			[JsonIgnore]
			IEnumerable<IColumnReference> IForeignKey.References => References;
		}

		public class AiColumnReference : IColumnReference
		{
			public string SourceColumn { get; set; }

			public string TargetColumn { get; set; }
		}

		public async Task<IEnumerable<AiForeignKey>> GuessForeignKeysAsync(IRelationalDataSource source)
		{
			var builder = new StringBuilder();
			foreach (var schema in source.Schemas)
			{
				builder.AppendLine($"{schema.Name}:");
				foreach (var table in schema.Tables)
				{
					var primary = table.KeyColumns.Select(x => x.Name).ToFrozenSet();
					var columns = string.Join(", ", table.ColumnNames.Select(x => primary.Contains(x) ? $"*{x}" : x));
					builder.AppendLine($"\t- {table.Name} ({columns})");
				}

				builder.AppendLine();
			}

			var prompt = $$"""
			Given the following Schemas, Tables and Column names, where Primarykey columns are prefixed by an asterisk,
			take an educated guess based off the names which tables are related by a foreign key. 
			Don't explain anything, just respond in a json format defining all keys.
			
			{{builder.ToString()}}

			Example:
			Schema1:
				- Student (*Id, Name, SchoolDistrictId, SchoolId)

			Schema2:
				- School (*DistrictId, *SchoolId, Name)

			```json
			[
				{
					"Name": "fk_student_school",
					"FromSchema": "Schema1",
					"FromTable": "Student"
					"ReferencedTable": "School",
					"ReferencedSchema": "Schema2",
					"References": [
						{
							"SourceColumn": "SchoolDistrictId",
							"TargetColumn": "DistrictId"
						},
						{
							"SourceColumn": "SchoolId",
							"TargetColumn": "SchoolId"
						}
					]
				}
			]
			```
			""";

			return await _ai.RequestJsonModelAsync<AiForeignKey[]>(prompt);
		}

		public async Task<AiManyToManyMapping> GetManyToManyNamesAsync(string table1Name, string table2Name, string middleTable, string fk1, string fk2)
		{
			var prompt = $$"""
			Given the following names of two many to many related tables: {{table1Name}}, {{table2Name}}
			As well as the name of the inbetween table: {{middleTable}} 
			And the foreign key names: {{fk1}}, {{fk2}}

			Reply with a json object containing RDF predicates describing the relationship between the many to many tables

			Example:
			Student, Teacher
			StudentTeacher
			fk_id_student, fk_id_teacher
			Reply:
			{
				"Forward": "hasTeacher",
				"Backward": "hasStudent"
			}
			""";

			return await _ai.RequestJsonModelAsync<AiManyToManyMapping>(prompt);
		}



		public class AiForeignKeyPredicateNaming
		{
			public Dictionary<string, string> Forward { get; set; }
			public Dictionary<string, string> Backward { get; set; }
		}

		public async Task<AiForeignKeyPredicateNaming> GetForeignKeyNamesAsync(string tableName, IEnumerable<(string name, string table)> foreignKeys)
		{
			if (foreignKeys.Count() == 0)
				return new();

			var prompt = $$"""
			Given the following relation ships between two tables denoted as `Table1 --(Relation Name)--> Table2`
			{{string.Join("\n", foreignKeys.Select(x => $"{tableName} --({x.name})--> {x.table}"))}}

			Reply with a json object containing RDF predicates describing the relationship between those tables.
			Forward should contain predicates describing the relation going out from Table1
			Backward should contain prediactes descibing the relation going out from Table2

			Example:
			Teacher --(fk_id_student)--> Student
			School --(fk_id_bezirk)--> District

			Response:
			{
				"Forward": {
					"fk_id_lehrer": "hasStudent",
					"fk_id_bezirk": "isInDistrict"	
				},
				"Backward": {
					"fk_id_lehrer": "hasTeacher",
					"fk_id_bezirk: "hasSchool"
				}
			}
			""";

			return await _ai.RequestJsonModelAsync<AiForeignKeyPredicateNaming>(prompt);
		}
	}
}
