using QnALoader.QnaMaker;
using System;
using System.Collections.Generic;

namespace QnALoader
{
    /// <summary>
    /// Loads QnAMaker with parsed DVA fact sheets.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Get all MRCA factsheet details
            List<string> mrcaFactsheets = GetMRCAFactsheetIDs();

            // Call Create service
            QnaMakerKb qnaMaker = new QnaMakerKb();
            foreach(var factsheetId in mrcaFactsheets)
            {
                var kbId = qnaMaker.CreateKnowledgeBase(factsheetId, $"https://dvachatbotstorage.blob.core.windows.net/factsheets/{factsheetId}.html");
                
                Console.WriteLine($"Successfully created KB {factsheetId} with ID {kbId}");
            }

            Console.WriteLine("Completed creation of QnA knowledge bases");
        }

        private static List<string> GetMRCAFactsheetIDs()
        {
            List<string> result = new List<string>();
            result.Add("MRC01");
            result.Add("MRC04");
            result.Add("MRC05");
            result.Add("MRC07");
            result.Add("MRC08");
            result.Add("MRC09");
            result.Add("MRC10");
            result.Add("MRC17");
            result.Add("MRC18");
            result.Add("MRC25");
            result.Add("MRC27");
            result.Add("MRC30");
            result.Add("MRC31");
            result.Add("MRC33");
            result.Add("MRC34");
            result.Add("MRC35");
            result.Add("MRC36");
            result.Add("MRC39");
            result.Add("MRC40");
            result.Add("MRC41");
            result.Add("MRC42");
            result.Add("MRC43");
            result.Add("MRC45");
            result.Add("MRC47");
            result.Add("MRC49");
            result.Add("MRC50");

            return result;
        }
    }
}
