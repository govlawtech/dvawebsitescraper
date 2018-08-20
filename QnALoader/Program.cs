using QnALoader.QnaMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace QnALoader
{
    /// <summary>
    /// Loads QnAMaker with parsed DVA fact sheets.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var qnaSubscriptionKey = args[0];

            List<string> allFactsheets = Properties.Resources.factSheetCodes
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();


            var outputFile = "factSheetCodeToKBId.csv";

            // Call Create service
            QnaMakerKb qnaMaker = new QnaMakerKb(qnaSubscriptionKey);

            /**
             * QnA Maker is provided under Cognitive Services Terms. This free preview provides up to 10 transactions per minute, up to 10,000 transactions per month.
             */
            List<string> failed = new List<string>();

            using (var fs = File.Create(outputFile))
            {
                using (var os = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var factsheetId in allFactsheets)
                    {
                        try
                        {
                 
                            Thread.Sleep(11000);
                            var kbId = qnaMaker.CreateKnowledgeBase(factsheetId,
                                $"https://dvachatbotstorage.blob.core.windows.net/factsheets/{factsheetId}.html");
                            os.WriteLine($"\"{factsheetId}\",\"{kbId}\"");
                            Console.WriteLine($"Successfully created KB {factsheetId} with ID {kbId}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to create KB for {factsheetId}");
                            Console.WriteLine(e);
                            failed.Add(factsheetId);
                        }
                    }


                    Console.WriteLine("Completed creation of QnA knowledge bases");
                    
                }
            }

            File.WriteAllLines("failed.txt", failed);
        }


    }
}
