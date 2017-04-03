using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Test
{
    public class TestCitas
    {
        void GetConfigurationInJsonByDate(DateTime dateTime)
        {

            const string sql = @"
                SELECT * 
                FROM   vwdashboard
                WHERE  Fecha = @fecha";

            var parametros = new List<MySqlParameter>
            {
                new MySqlParameter("@fecha", dateTime.Date)
            };

            var result = Db.GetDataTable(sql, parametros);

            var json = JsonConvert.SerializeObject(result);

            Console.WriteLine(json);
            Console.ReadLine();
        }

        public TestCitas()
        {
            GetConfigurationInJsonByDate(DateTime.Today);
            
        }
    }
}
