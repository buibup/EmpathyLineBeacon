using System;
using System.Collections.Generic;
using System.Text;
using EmpathyLibrary.Models;
using System.Data;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;
using Newtonsoft.Json;

namespace EmpathyLibrary.DataAccess
{
    public class MySqlConnector 
    {
       
        public static List<SignalLineBeacons> GetSignalLineBeacons_All()
        {
            List<SignalLineBeacons> output = new List<SignalLineBeacons>();
        
            using (MySqlConnection connection = new MySqlConnection(GlobalConfig.Config["Data:ConnectionString"]))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(GlobalConfig.Config["Procedure:LineBeaconCurrentDate_GetAll"], connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using(var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var json = JsonConvert.DeserializeObject<LineBeacon>(reader[2].ToString());
                           
                            if(json.Type.ToString().ToLower() == "beacon")
                            {
                                if(json.Beacon.type.ToString().ToLower() == "enter")
                                {
                                    var signalLineBeacons = new SignalLineBeacons()
                                    {
                                        Timestamp = DateTime.Parse(reader[0].ToString()),
                                        Topic = reader[1].ToString(),
                                        LineBeacon = json
                                    };
                                    output.Add(signalLineBeacons);
                                }
                            }
                        }
                    }
                }
            }

            output = output.OrderByDescending(x => x.Timestamp).ToList();

            return output;
        }
    }
}
