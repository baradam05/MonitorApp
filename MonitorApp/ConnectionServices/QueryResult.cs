using System.Collections.Generic;

namespace MonitorApp.ConnectionServices
{
    public class QueryResult
    {
        public bool HasResults { get; set; }
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
    }
}
