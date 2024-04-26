using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.SiardV1
{
	[SiardVersion(SiardVersion.Siard1)]
	public partial class SiardArchiveV1 : ISiardArchive
	{
		public IEnumerable<ISchema> Schemas => this.schemas;

		public string Name => this.Dbname;

		public string LobFolder => "";

		SiardVersion ISiardArchive.Version => SiardVersion.Siard1;

		public IType GetSuperType(IType type) => null;
	}

	public partial class ColumnTypeV1 : ISiardColumn, IAttributeMeta
	{
		public string LobFolder => "";

		public string MimeType => null;

		IEnumerable<IAttributeMeta> IAttributeMeta.Metas => Array.Empty<IAttributeMeta>();
	}
}
