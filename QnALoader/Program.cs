using QnALoader.QnaMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

            // Call Create service
            QnaMakerKb qnaMaker = new QnaMakerKb(qnaSubscriptionKey);
            
            foreach(var factsheetId in allFactsheets)
            {
                var kbId = qnaMaker.CreateKnowledgeBase(factsheetId, $"https://dvachatbotstorage.blob.core.windows.net/factsheets/{factsheetId}.html");
                Console.WriteLine($"Successfully created KB {factsheetId} with ID {kbId}");
                Thread.Sleep(1000);    
            }

            Console.WriteLine("Completed creation of QnA knowledge bases");
        }



    }
}
