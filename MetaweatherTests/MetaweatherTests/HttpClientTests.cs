using NUnit.Framework;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace MetaweatherTests
{
    public class ApiTests
    {
        public static Match CountryData = null;

        public string County = "Minsk";
        public string CountyLatitude = "53";
        public string CountyLongitude = "27";

        readonly string RequestLocationURL = "https://www.metaweather.com/api/location/search/?query=";
        readonly string RequestWoeIdURL = "https://www.metaweather.com/api/location/";

        public static string JsonFile = null;

        public static int[] TimesOfTheYear = new int[12] {1,2,3,4,5,6,7,8,9,10,11,12};

        public static int AutumnSpringAverageMinTemperature = -5;
        public static int AutumnSpringAverageMaxTemperature = 10;

        [Test, Order(1)]
        public void SearchMinsk()
        {

            string searchBy = "min";

            GetRequest(RequestLocationURL + searchBy);

            string regex = @"\{.title.\:." + County + @".\,.location_type.\:.\w+.\,.woeid.\:.\d+.\,.latt_long.\:.[0-9]+.[0-9]+\,[0-9]+.[0-9]+.\}";

            Regex neededCity = new Regex(regex);
            CountryData = neededCity.Match(JsonFile);

            Assert.IsNotEmpty(CountryData.Value);
        }

        [Test, Order(2)]
        public void CompareGeoData()
        {
            Regex latitudeLongitudeRegex = new Regex(@"[0-9]+.[0-9]+\,[0-9]+.[0-9]+");

            Match latitudeLongitudeValues = latitudeLongitudeRegex.Match(CountryData.Value);

            List<string> latitudeLongitude = new List<string>();
            StringBuilder tempStr = new StringBuilder();

            int i = 0;
            bool flag = false;

            while (!flag)
            {
                while (latitudeLongitudeValues.Value[i] != '.')
                {
                    tempStr.Append(latitudeLongitudeValues.Value[i]);
                    i++;
                }

                latitudeLongitude.Add(tempStr.ToString());
                tempStr.Clear();

                for (int j = 0; j < latitudeLongitudeValues.Value.Length; j++)
                    i = latitudeLongitudeValues.Value.IndexOf(",");

                i++;

                if (latitudeLongitude.Count == 2)
                    flag = true;
            }

            Assert.AreEqual(CountyLatitude + CountyLongitude, latitudeLongitude[0] + latitudeLongitude[1]);
        }

        [Test, Order(3)]
        public void CityWeatherToday()
        {
            Regex woeIdRegex = new Regex(@"\d+");

            Match woeIdValue = woeIdRegex.Match(CountryData.Value);

            DateTime today = DateTime.Today;

            GetRequest(RequestWoeIdURL + woeIdValue + "/" + today.Year.ToString() + "/" + today.Month.ToString() + "/" + today.Day.ToString());

            Assert.NotNull(JsonFile);
        }

        [Test, Order(4)]
        public void CompareTemperature()
        {
            Regex woeIdRegex = new Regex(@"\d+");
            Regex theTempRegex = new Regex(@".the_temp.\:\d+.[0-9]{2}");
            
            Match woeIdValue = woeIdRegex.Match(CountryData.Value);

            GetRequest(RequestWoeIdURL + woeIdValue);

            string timeOfTheYear = null;

            int currMnth = DateTime.Now.Month;

            if (currMnth == 12 || currMnth == 1 || currMnth == 2)
            {
                timeOfTheYear = "Winter";
            }
            else if (currMnth >= 6 && currMnth == 8)
            {
                timeOfTheYear = "Summer";
            }
            else timeOfTheYear = "AutumnSpring";

            MatchCollection theTemps = theTempRegex.Matches(JsonFile);
            bool assertTemperature = true;

            for (int i = 0; i < theTemps.Count; i++)
            {
                int currTemp = theTemps[i].Value.IndexOf(":") + 1;

                if (timeOfTheYear.Equals("Winter") && (theTemps[i].Value[currTemp] - '0') > 0)
                    assertTemperature = false;

                if (timeOfTheYear.Equals("Summer") && (theTemps[i].Value[currTemp] - '0') < 0)
                    assertTemperature = false;

                if (timeOfTheYear.Equals("AutumnSpring") && 
                    ((theTemps[i].Value[currTemp] - '0') < AutumnSpringAverageMinTemperature ||
                    (theTemps[i].Value[currTemp] - '0') > AutumnSpringAverageMaxTemperature))
                    assertTemperature = false;
            }  

            Assert.IsTrue(assertTemperature);
        }

        [Test, Order(5)]
        public void FifeYearsAgoWeatger()
        {
            bool assertFlag = false;

            Regex woeIdRegex = new Regex(@"\d+");
            Regex weatherRegex = new Regex(@".weather_state_name.\:.\D+.created.\:.[0-9]{4}\-[0-9]{2}\-[0-9]{2}");
            Regex weatherStateNameDateRegex = new Regex(@"created.\:.[0-9]{4}\-[0-9]{2}\-[0-9]{2}");

            Match woeIdValue = woeIdRegex.Match(CountryData.Value);

            DateTime today = DateTime.Today;
            today = today.AddYears(-5);

            GetRequest(RequestWoeIdURL + woeIdValue + "/" + today.Year.ToString() + "/" + today.Month.ToString() + "/" + today.Day.ToString());

            MatchCollection allWeatherConditions = weatherRegex.Matches(JsonFile);

            string weatherStateName = null;
            string dateOfWeatherStateName = null;

            for (int i = 0; i < allWeatherConditions.Count; i++)
            {
                weatherStateName = allWeatherConditions[i].Value.Substring(allWeatherConditions[i].Value.IndexOf(":") + 2, allWeatherConditions[i].Value.IndexOf("\",") - (allWeatherConditions[i].Value.IndexOf(":") + 2));

                Match dateOfWeatherCondition = weatherStateNameDateRegex.Match(allWeatherConditions[i].Value);

                dateOfWeatherStateName = dateOfWeatherCondition.Value.Substring(dateOfWeatherCondition.Value.IndexOf(":") + 2, (dateOfWeatherCondition.Value.Length - (dateOfWeatherCondition.Value.IndexOf(":") + 2)));
                string[] separatedDateOfWeatherCondition = dateOfWeatherStateName.Split('-');

                if (weatherStateName != null &&
                    DateTime.Now.Month == Convert.ToInt32(separatedDateOfWeatherCondition[1]) &&
                    DateTime.Now.Day == Convert.ToInt32(separatedDateOfWeatherCondition[2]))
                {
                    assertFlag = true;
                    break;
                }
            }

            Assert.IsTrue(assertFlag);
        }

        public static void GetRequest(string Link)
        {
            JsonFile = null;

            WebRequest request = WebRequest.Create(Link);
            WebResponse response = request.GetResponse();

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    JsonFile = reader.ReadToEnd();
                }
            }

            response.Close();
        }
    }
}