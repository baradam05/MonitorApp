using System.Collections.Generic;

namespace MonitorApp.ConnectionServices
{
    /// <summary>
    /// Represents the result of an executed query.
    /// </summary>
    public class QueryResult
    {
        public bool HasResults { get; set; }
        /// <summary>
        /// The list of data rows returned by the query.
        /// </summary>
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
    }
}
