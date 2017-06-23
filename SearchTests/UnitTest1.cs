using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SearchTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DeserialiseFactSheet()
        {
            var testData = Properties.Resources.testJson;                           
            JObject factSheetJsonObject = JObject.Parse(testData);
            var result =  SearchExperiment.FactSheet.fromJson(factSheetJsonObject);
            Debug.WriteLine(result.FullText);
            Assert.IsNotNull(result);
        }
    }
}
                                               