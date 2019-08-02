using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        /*
         * declare variables
         */

        public List<string[]> display = new List<string[]> { };

        public string mUserInput;

        public string mCountry;
        public string mCity;
        public string mWeather;
        public string mTemp;
        public string mPress;


        public IActionResult Index()
        {
            try
            {
                /*
                * Create new table
                */
                SQLiteConnection sqliteConn = new SQLiteConnection("Data Source=myplaces.db;Version=3;New=False;Compress=True;");
                sqliteConn.Open();
                SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();
                sqliteCmd.CommandText = "CREATE TABLE place (Name varchar, Date varchar)";
                sqliteCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //Error handling
            }

            return View();
        }

        [HttpPost]
        public IActionResult Search()
        {
            string html = "";
            mUserInput = Request.Form["city"];
            mUserInput = mUserInput.Replace(' ', '+');

            //Sending request to api server

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?q="+ mUserInput + "&appid=c2675cc27e2d55338119291273f23807");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
                dynamic data = JObject.Parse(html);

                /*
                *  data from the JSON
                */

                try
                {
                    mCity = data.name;
                    mCountry = data.sys.country;
                    mWeather = data.weather[0].main;
                    mTemp = data.main.temp;
                    mPress = data.main.pressure;

                }catch(Exception e)
                {
                    //handle error
                }

            }
            //assign viewdata
            ViewData["mCity"] = mCity;
            ViewData["mCountry"] = mCountry;
            ViewData["mWeather"] = mWeather;
            ViewData["mTemp"] = mTemp;
            ViewData["mPress"] = mPress;

            return View();
        }

        [HttpPost]
        public ActionResult AddPlace(IFormCollection formCollection)
        {
            /*
             * Setting varibales from layout forms
             */
            string mPlaceName = Request.Form["placename"];
            string mDate = Request.Form["date"];

            SQLiteConnection sqliteConn = new SQLiteConnection("Data Source=myplaces.db;Version=3;New=False;Compress=True;");
            sqliteConn.Open();
            SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();

            /*
             * Executing insert query
             */
            sqliteCmd.CommandText = "INSERT INTO place (Name, Date) VALUES (" + "'" + mPlaceName + "'," + "'" + mDate + "'" + ")";
            sqliteCmd.ExecuteNonQuery();

            return View();

        }

        [HttpPost]
        public ActionResult DeletePlace(IFormCollection formCollection)
        {
            string mPlaceName = Request.Form["placename"];

            SQLiteConnection sqliteConn = new SQLiteConnection("Data Source=myplaces.db;Version=3;New=False;Compress=True;");
            sqliteConn.Open();
            SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();

            /*
             * Executing delete
             */
            sqliteCmd.CommandText = "DELETE FROM place WHERE Name = " + "'" + mPlaceName + "'";
            sqliteCmd.ExecuteNonQuery();

            return View();

        }

        public IActionResult ViewPlace()
        {
            SQLiteConnection sqliteConn = new SQLiteConnection("Data Source=myplaces.db;Version=3;New=False;Compress=True;");
            sqliteConn.Open();
            SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();

            /*
             * Executing select query
             */
            sqliteCmd.CommandText = "SELECT * FROM place";

            SQLiteDataReader sqliteReader = sqliteCmd.ExecuteReader();

            while (sqliteReader.Read())
            {

                /*
                * Array of string of length 2
                */
                String[] dat = new string[2];

                /*
                * Adding to list
                */

                dat[0] = sqliteReader.GetString(0);
                dat[1] = sqliteReader.GetString(1);

                display.Add(dat);
            }

            ViewData["Display"] = display;

            return View();
        }

        //method for updating the database

        [HttpPost]
        public IActionResult UpdatePlace()
        {
            string mPlaceName = Request.Form["placename"];
            string mDate = Request.Form["date"];
            mPlaceName = mPlaceName.Replace(' ', '+');

            SQLiteConnection connection = new SQLiteConnection("Data Source=myplaces.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "UPDATE place SET Date=" + "'" + mDate + "'" + " WHERE Name = " + "'" + mPlaceName + "'";
            command.ExecuteNonQuery();

            return View();
        }

        public IActionResult About()
        {
            //About page
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
