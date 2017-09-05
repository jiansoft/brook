using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Mvc;
using Brook;
using Brook.Configuration;
using Dapper;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                var sw = new Stopwatch();
                long t1 = 0;
                long t2 = 0;
                long t3 = 0;
                var count = 100;
                DataSet ds;
                using (var db = new MsSql("Pingball"))
                {
                    var connection = new SqlConnection(db.ConnectionSource);
                    connection.Open();
                    var petaPoco = new PetaPoco.Database("DefaultConnection");
                    var user = db.Table("SELECT UserId FROM[dbo].[User] order by newId()");
                    //var customers = db.GetSqlConnection().Query<int>("SELECT COUNT(*) FROM [dbo].[User]");
                    //Response.Write($"會員數 :{customers}<br />");
                    for (var ii = 0; ii < 10; ii++)
                    {
                        for (var i = 1; i <= count; i = i + 3)
                        {
                            sw.Start();

                            var player1 =
                                connection.QueryFirst<Player>(
                                    $"SELECT * FROM [dbo].[User] where [UserId] = {user.Rows[i][0]}");

                            sw.Stop();
                            t1 += sw.ElapsedMilliseconds;
                            sw.Reset();
                            sw.Start();

                            var player2 =
                                db.First<Player>($"SELECT * FROM [dbo].[User] where [UserId] = {user.Rows[i + 1][0]}", new DbParameter[] { });
                            sw.Stop();
                            t2 += sw.ElapsedMilliseconds;
                            sw.Reset();
                            sw.Start();
                           
                            var article =
                                petaPoco.SingleOrDefault<Player>(
                                    $"SELECT * FROM [dbo].[User] where [UserId] = {user.Rows[i + 2][0]}");
                        
                            sw.Stop();
                            t3 += sw.ElapsedMilliseconds;
                            sw.Reset();

                            //sw.Start();

                            //var player1 = db.Query<Player>(CommandType.Text, $"SELECT MachineKey,UserName FROM [dbo].[User]");
                            //var player2 = db.First<Player>($"SELECT * FROM [dbo].[User] where [UserId] = {user.Rows[i][0]}");
                            //var players = db.Table($"SELECT MachineKey,UserName FROM [dbo].[User]");
                            //var ds = db.DataSet(
                            //    CommandType.Text,
                            //    $"SELECT MachineKey,UserName FROM [dbo].[User];SELECT TOP (1000) [MessageId],[Content],[Status],[CreateTime]  FROM [Pingball].[dbo].[Message];");

                            //sw.Stop();
                            //t2 += sw.ElapsedMilliseconds;
                            //sw.Reset();
                        }


                    }
                    Response.Write($"Dapper = {t1},Brook = {t2},PetaPoco = {t3}<br><br><br>");

                     ds = db.DataSet(
                        CommandType.Text,
                        $"SELECT MachineKey,UserName FROM [dbo].[User];SELECT TOP (1000) [MessageId],[Content],[Status],[CreateTime]  FROM [Pingball].[dbo].[Message];");

                }
                Response.Write($"ds.Tables.Count = {ds.Tables.Count}<br><br><br>");

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult AsyncPerformance()
        {
            var sw = new Stopwatch();
            long t1 = 0;
            long t2 = 0;
            long t3 = 0;
            var count = 1;
            using (var dba = new MsSql("Member"))
            {
                using (var dbb = new MsSql("Member"))
                {

                    //return;
                    // object tt1 = t1;
                    var a = new Dictionary<int, object>() { { 1, dbb } };
                    object aa = a;



                    var userCount = dbb.One<long>(15, CommandType.Text,
                        "SELECT COUNT(*) FROM [KoocoMember].[dbo].[tblMember]", new DbParameter[]
                        {
                            new SqlParameter
                            {
                                Value = DateTime.Now.Ticks,
                                SqlDbType = SqlDbType.BigInt,
                                ParameterName = "@argIntIdentityKey",
                                Direction = ParameterDirection.Input
                            }
                        });
                    Response.Write($"會員數 :{userCount}<br />");
                   

                    for (var ii = 0; ii < 1; ii++)
                    {
                        for (var i = 1; i <= count; i++)
                        {
                            sw.Start();
                           
                            sw.Stop();
                            t1 += sw.ElapsedMilliseconds;

                            sw.Reset();
                            
                         
                            sw.Stop();
                            t3 += sw.ElapsedMilliseconds;

                            sw.Reset();
                        }

                        Response.Write($"t1 Async.First = {t1},t2 Async.GetFirst= {t2},t3 First= {t3}<br>");

                    }
                    Response.Write($"t1 Async.First = {t1 / count * 10},t2 Async.GetFirst= {t2 / count * 10},t3 First= {t3 / count * 10}<br>");
                }

             
            }
            return View();
        }
    }
    public class Player
    {
        //手機作業系統
        public string MachineKey;
        //	 帳號是否停權中
        private string UserName { get; set; }
        
        
    }
}