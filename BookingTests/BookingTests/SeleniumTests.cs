using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace BookingTests
{
    public class UiTests
    {
        IWebDriver driver;

        [OneTimeSetUp]
        public void Setup()
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

††††††††††††driver = new ChromeDriver(path + @"\Drivers\");
            driver.Navigate().GoToUrl("https://booking.com/");
        }

        [Test]
        public void Change—urrency()
        {
            string country = "JPY";

            driver.FindElement(By.XPath("//div[@class='bui-group bui-button-group bui-group--inline bui-group--align-end bui-group--vertical-align-middle']//div[1]//button")).Click();
            Thread.Sleep(2000);
;
            driver.FindElement(By.XPath("//a[@class='bui-list-item bui-list-item--size-small ']//div[@class='bui-inline-container__main']//div[contains(string(), '"+ country + "')]")).Click();
            
            string assertCurrency = driver.FindElement(By.XPath("//div[@class='bui-group bui-button-group bui-group--inline bui-group--align-end bui-group--vertical-align-middle']//div[1]//button//span//span[1]")).Text;

            Assert.AreEqual(country, assertCurrency);
        }

        [Test]
        public void ChangeLanguage()
        {
            string country = "ja";

            driver.FindElement(By.XPath("//button[@data-modal-id='language-selection']")).Click();
            Thread.Sleep(2000);

            driver.FindElement(By.XPath("//a[@hreflang='"+ country + "']")).Click();

            string langURI = driver.FindElement(By.XPath("//header/nav[1]/div[2]/div[2]/button")).GetAttribute("baseURI");

            Regex fragmentRegex = new Regex(@"\.[a-z]{2}\.");
            Regex languageRegex = new Regex(@"[a-z]{2}");

            Match assertfragmentMatch = fragmentRegex.Match(langURI);
            Match assertLang = languageRegex.Match(assertfragmentMatch.Value);

            Assert.AreEqual(country, assertLang.Value);
        }

        [Test]
        public void GoToAirSalesPage()
        {
            driver.FindElement(By.XPath("//a[@data-decider-header='flights']")).Click();
            Thread.Sleep(2000);

            string siteURI = driver.FindElement(By.XPath("//div")).GetAttribute("baseURI");
            string pageLink = "https://www.gotogate.com";

            Regex linkRegex = new Regex(@"https://\w+\.\w+\.\w+");

            Match assertPage = linkRegex.Match(siteURI);

            Assert.AreEqual(pageLink, assertPage.Value);
        }

        [Test]
        public void Authorizing()
        {
            driver.FindElement(By.XPath("//div[@class='bui-group bui-button-group bui-group--inline bui-group--align-end bui-group--vertical-align-middle']//div[6]//a")).Click();
            Thread.Sleep(2000);

            string className = driver.FindElement(By.XPath("//div[@class='access-panel-container']")).GetAttribute("className");
            string assertText = "access-panel-container";

            driver.FindElement(By.XPath("//div[@class='guest-header']//header//nav//div//div//a")).Click();

            Assert.AreEqual(assertText, className);
        }

        [Test]
        public void FilterTest()
        {
            int adults = 2;
            int childrens = 1;
            int rooms = 1;

            DateTime today = DateTime.Today;
            today = today.AddDays(10);

            driver.FindElement(By.XPath("//input[@type='search']")).SendKeys("Kyoto");
            Thread.Sleep(1000);

            driver.FindElement(By.XPath("//ul[@role='listbox']//li[1]")).Click();
            Thread.Sleep(1000);

            driver.FindElement(By.XPath("//div[@class='bui-calendar__content']//div//table//tbody//tr//td[@data-date='" 
                + today.Year + "-" + today.ToString("MM") + "-" + today.ToString("dd") + "']")).Click();
            Thread.Sleep(1000);

            today = today.AddDays(10+2);

            driver.FindElement(By.XPath("//div[@class='bui-calendar__content']//div//table//tbody//tr//td[@data-date='"
                + today.Year + "-" + today.ToString("MM") + "-" + today.ToString("dd") + "']")).Click();
            Thread.Sleep(1000);

            driver.FindElement(By.XPath("//label[@id='xp__guests__toggle']")).Click();
            Thread.Sleep(1000);

            string xPathPeopleSamePath = "//div[@id='xp__guests__inputs-container']//div//div[@class='u-clearfix']";

            int siteAdultCount = Convert.ToInt32(driver.FindElement(By.XPath(xPathPeopleSamePath+ "//div[@class='sb-group__field sb-group__field-adults']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//span[@class='bui-stepper__display']")).Text);
            int siteChildCount = Convert.ToInt32(driver.FindElement(By.XPath(xPathPeopleSamePath+ "//div[@class='sb-group__field sb-group-children ']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//span[@class='bui-stepper__display']")).Text);
            int siteRoomCount = Convert.ToInt32(driver.FindElement(By.XPath(xPathPeopleSamePath + "//div[@class='sb-group__field sb-group__field-rooms']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//span[@class='bui-stepper__display']")).Text);

            if (siteAdultCount != adults)
                EqualPeople(adults, siteAdultCount, "adults",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group__field-adults']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-subtract-button']",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group__field-adults']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-add-button']");

            if (siteChildCount != childrens)
                EqualPeople(childrens, siteChildCount, "childrens",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group-children ']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-subtract-button']",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group-children ']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-add-button']");

            if (siteRoomCount != rooms)
                EqualPeople(rooms, siteRoomCount, "rooms",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group__field-rooms']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-subtract-button']",
                    xPathPeopleSamePath + "//div[@class='sb-group__field sb-group__field-rooms']//div//div[@class='bui-stepper__wrapper sb-group__stepper-a11y']//button[@data-bui-ref='input-stepper-add-button']");

            driver.FindElement(By.XPath("//button[@class='sb-searchbox__button ']")).Click();
            Thread.Sleep(2000);

            string pageURI = driver.FindElement(By.XPath("//div")).GetAttribute("baseURI");

            Regex linkRegex = new Regex(@"https://\w+\.\w+\.\w+/searchresults");

            bool assertPage = linkRegex.IsMatch(pageURI);

            Assert.IsTrue(assertPage);
        }

        public void EqualPeople(int NeededCount, int CurrentCount, string Type, string Minus, string Plus)
        {
            if (CurrentCount < NeededCount)
            {
                while (CurrentCount < NeededCount)
                {
                    driver.FindElement(By.XPath(Plus)).Click();

                    Thread.Sleep(1000);

                    if (Type.Equals("childrens"))
                    {
                        driver.FindElement(By.XPath("//select[@data-group-child-age='" + CurrentCount + "']")).Click();
                        Thread.Sleep(1000);

                        Random range = new Random();

                        driver.FindElement(By.XPath("//select[@data-group-child-age='" + CurrentCount + "']//option[@value='"+ range.Next(0, 18) +"']")).Click();
                        Thread.Sleep(1000);
                    }

                    CurrentCount++;
                }
            }
            else
                while (CurrentCount > NeededCount)
                {
                    driver.FindElement(By.XPath(Minus)).Click();
                    CurrentCount--;

                    Thread.Sleep(1000);
                }

        }
    }
}