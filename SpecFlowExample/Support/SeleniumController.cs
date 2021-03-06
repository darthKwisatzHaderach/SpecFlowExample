﻿using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Tracing;

namespace SpecFlowExample.Support
{
	public class SeleniumController
	{
		#region Fields

		private static SeleniumController _seleniumController;
		private readonly string _driverName;

		#endregion

		#region .ctor

		private SeleniumController(string driverName)
		{
			_driverName = driverName;
		}

		#endregion

		#region Methods

		public void Start()
		{
			Selenium = GetDriver(_driverName);

			Selenium.Manage().Timeouts().ImplicitlyWait(DefaultTimeout);
			Selenium.Manage().Window.Maximize();

			if (FeatureContext.Current != null)
				FeatureContext.Current["Browser"] = Selenium;

			Trace("Selenium started");
		}

		public void Stop()
		{
			if (Selenium == null)
				return;

			try
			{
				Selenium.Quit();
				Selenium.Dispose();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex, "Selenium stop error");
			}
			Selenium = null;
			Trace("Selenium stopped");
		}

		public void TakeScreenshot()
		{
			try
			{
				var fileNameBase = string.Format("error_{0}_{1}_{2}",
					FeatureContext.Current.FeatureInfo.Title.ToIdentifier(),
					ScenarioContext.Current.ScenarioInfo.Title.ToIdentifier(),
					DateTime.Now.ToString("yyyyMMdd_HHmmss"));

				var artifactDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testresults");
				if (!Directory.Exists(artifactDirectory))
					Directory.CreateDirectory(artifactDirectory);

				string pageSource = Selenium.PageSource;
				string sourceFilePath = Path.Combine(artifactDirectory, fileNameBase + "_source.html");
				File.WriteAllText(sourceFilePath, pageSource, Encoding.UTF8);
				Console.WriteLine("Page source: {0}", new Uri(sourceFilePath));

				ITakesScreenshot takesScreenshot = Selenium as ITakesScreenshot;

				if (takesScreenshot != null)
				{
					var screenshot = takesScreenshot.GetScreenshot();

					string screenshotFilePath = Path.Combine(artifactDirectory, fileNameBase + "_screenshot.png");

					screenshot.SaveAsFile(screenshotFilePath, ImageFormat.Png);

					Console.WriteLine("Screenshot: {0}", new Uri(screenshotFilePath));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error while taking screenshot: {0}", ex);
			}
		}

		public static SeleniumController Set(string browser)
		{
			_seleniumController = new SeleniumController(browser);

			return _seleniumController;
		}

		public static SeleniumController Instance
		{
			get
			{
				if (_seleniumController == null)
					throw new NullReferenceException("Set selenium controller, before using Selenium propery.");

				return _seleniumController;
			}
		}

		#endregion

		#region Properties

		public IWebDriver Selenium
		{
			get; set;
		}

		public static TimeSpan DefaultTimeout
		{
			get { return TimeSpan.FromSeconds(10); }
		}

		#endregion

		#region Private Methods

		private static void Trace(string message)
		{
			Console.WriteLine("-> {0}", message);
		}

		private static IWebDriver GetDriver(string browser)
		{
			switch (browser)
			{
				case "Chrome":
					return new ChromeDriver(@"Support\Drivers");
				case "Firefox":
					return new FirefoxDriver();
				case "IE":
					return new InternetExplorerDriver(@"Support\Drivers",
						new InternetExplorerOptions
						{
							IntroduceInstabilityByIgnoringProtectedModeSettings = true,
							IgnoreZoomLevel = true,
							EnableNativeEvents = false
						});
				default:
					throw new NotSupportedException(string.Format("Browser '{0}' does not exists.", browser));
			}
		}

		#endregion
	}
}