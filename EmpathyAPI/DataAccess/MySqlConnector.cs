using System;
using System.Collections.Generic;
using System.Text;
using EmpathyLibrary.Models;
using System.Data;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using EmpathyAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmpathyLibrary.DataAccess
{
    public class MySqlConnector
    {

        public static List<SignalLineBeacons> GetSignalLineBeacons_All()
        {
            List<SignalLineBeacons> output = new List<SignalLineBeacons>();
            List<SignalLineBeacons> enters = new List<SignalLineBeacons>();
            List<SignalLineBeacons> leaves = new List<SignalLineBeacons>();

            using (MySqlConnection connection = new MySqlConnection(GlobalConfig.Config["Data:ConnectionString"]))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(GlobalConfig.Config["Procedure:LineBeaconCurrentDate_GetAll"], connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var json = JsonConvert.DeserializeObject<LineBeacon>(reader[2].ToString());

                            if (json.Type.ToString().ToLower() == "beacon")
                            {
                                if (json.Beacon.type.ToString().ToLower() == "enter")
                                {
                                    var signalLineBeacons = new SignalLineBeacons()
                                    {
                                        Timestamp = DateTime.Parse(reader[0].ToString()),
                                        Topic = reader[1].ToString(),
                                        LineBeacon = json
                                    };
                                    enters.Add(signalLineBeacons);
                                }
                                else if (json.Beacon.type.ToString().ToLower() == "leave")
                                {
                                    var signalLineBeacons = new SignalLineBeacons()
                                    {
                                        Timestamp = DateTime.Parse(reader[0].ToString()),
                                        Topic = reader[1].ToString(),
                                        LineBeacon = json
                                    };
                                    leaves.Add(signalLineBeacons);
                                }
                            }
                        }
                    }
                }
            }

            var dataEnters = enters
                .GroupBy(e => e.LineBeacon.Beacon.hwid)
                .Select(grp => new
                {
                    grp.Key,
                    LastVisit = grp.OrderByDescending(
                        x => x.Timestamp).FirstOrDefault()
                }).ToList();


            var dataLeves = leaves
                .GroupBy(e => e.LineBeacon.Beacon.hwid)
                .Select(grp => new
                {
                    grp.Key,
                    LastVisit = grp.OrderByDescending(
                        x => x.Timestamp).FirstOrDefault()
                }).ToList();

            foreach (var dataE in dataEnters)
            {
                var dataL = dataLeves.Any(a => a.Key == dataE.Key) ? dataLeves.Find(a => a.Key == dataE.Key).LastVisit : new SignalLineBeacons();

                if (dataE.LastVisit.Timestamp > dataL.Timestamp)
                {
                    output.Add(dataE.LastVisit);
                }

            }

            output = output.OrderByDescending(x => x.Timestamp).ToList();

            return output;
        }

        public static async Task<FileStreamResult> GetLineImageByUserId(string userId, int w, int h)
        {
            byte[] imageBytes = Convert.FromBase64String(GlobalConfig.Config["Image:NoImageFound"]);
            using (var conn = new MySqlConnection(GlobalConfig.Config["Data:ConnectionString"]))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(GlobalConfig.Config["Procedure:GetLineImageByUserId"], conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@usrId", userId);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var data = reader["pictureBase64"].ToString();

                                if (string.IsNullOrEmpty(data))
                                {
                                    return await Helper.ByteArrayToImage(imageBytes, w, h);
                                }
                                else
                                {
                                    var stringBase64 = Encoding.ASCII.GetString((byte[])(reader["pictureBase64"]));
                                    imageBytes = Convert.FromBase64String(stringBase64);

                                    return await Helper.ByteArrayToImage(imageBytes, w, h);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return await Helper.ByteArrayToImage(imageBytes, w, h);
                    }

                }
            }

            return await Helper.ByteArrayToImage(imageBytes, w, h);
        }
    }
}
