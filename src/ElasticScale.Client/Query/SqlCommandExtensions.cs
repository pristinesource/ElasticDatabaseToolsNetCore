using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SqlDatabase.ElasticScaleNetCore.Query {
  public static class SqlCommandExtensions {

    public static SqlCommand Clone(this SqlCommand from) {

      SqlCommand newcmd = new SqlCommand(from.CommandText, from.Connection, from.Transaction);

      newcmd.CommandTimeout = from.CommandTimeout;
      newcmd.CommandType = from.CommandType;
      newcmd.DesignTimeVisible = from.DesignTimeVisible;
      newcmd.UpdatedRowSource = from.UpdatedRowSource;
      if(from.Parameters.Count > 0) {
        SqlParameter[] p = new SqlParameter[from.Parameters.Count];
        from.Parameters.CopyTo(p, 0);
        foreach(object current in p) {
          newcmd.Parameters.Add(current);
        }
      }
      return newcmd;
    }
    
  }
}
