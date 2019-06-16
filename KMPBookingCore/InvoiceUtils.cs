using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPBookingCore
{
    public static class InvoiceUtils
    {
        private static object _missing = System.Reflection.Missing.Value;

        //https://www.c-sharpcorner.com/UploadFile/muralidharan.d/how-to-create-word-document-using-C-Sharp/
        public static (bool, string) GenerateInvoice(string outputfilename, string invoiceTemplate,
            string clientName, string clientNo, string claimNo, string diagnosis,
            string receiptNo, DateTime date, string healthFund, string memberNo, IList<Service> services,
            decimal paymentReceived, decimal discount, decimal balance)
        {
            Application winword = null;
            bool res = false;
            string errorMsg = "";
            try
            {
                //Create an instance for word app  
                winword = new Microsoft.Office.Interop.Word.Application
                {
                    //Set animation status for word application  
                    ShowAnimation = false,

                    //Set status for word application is to be visible or not.  
                    Visible = false
                };
                object readOnly = true;
                object isVisible = false;

                //Create a missing variable for missing value  
                object infilename = invoiceTemplate;
                var document = winword.Documents.Open(infilename, _missing, readOnly,
                    _missing, _missing, _missing, _missing, _missing, _missing, _missing,
                    _missing, isVisible, _missing, _missing, _missing, _missing);
                var t = document.Tables.Cast<Table>().FirstOrDefault();
                t.Cell(2, 2).Range.Text = clientName;
                t.Cell(3, 2).Range.Text = clientNo;
                t.Cell(4, 2).Range.Text = claimNo;
                t.Cell(5, 2).Range.Text = diagnosis;
                t.Cell(2, 4).Range.Text = receiptNo;
                t.Cell(3, 4).Range.Text = date.ToShortDateString();
                t.Cell(4, 4).Range.Text = healthFund;
                t.Cell(5, 4).Range.Text = memberNo;

                var due = services.Sum(x => x.Balance);
                t.Cell(11, 3).Range.Text = due.ToDecPlaces();
                t.Cell(12, 3).Range.Text = paymentReceived.ToDecPlaces();
                t.Cell(13, 3).Range.Text = $"{discount.ToDecPlaces(0)}%";
                t.Cell(14, 3).Range.Text = balance.ToDecPlaces();

                object rref = t.Cell(9, 1);

                var rn = 8;
                foreach (var svc in services)
                {
                    if (svc.Date.HasValue)
                    {
                        // TODO culture?
                        t.Cell(rn, 1).Range.Text = svc.Date.Value.ToShortDateString();
                    }
                    t.Cell(rn, 2).Range.Text = svc.Detail;
                    t.Cell(rn, 3).Range.Text = svc.Total.ToDecPlaces();
                    t.Cell(rn, 4).Range.Text = svc.Owing.ToDecPlaces();
                    t.Cell(rn, 5).Range.Text = svc.Benefit.ToDecPlaces();
                    t.Cell(rn, 6).Range.Text = svc.Gap.ToDecPlaces();
                    t.Cell(rn, 7).Range.Text = $"{svc.Discount.ToDecPlaces(0)}%";
                    t.Cell(rn, 8).Range.Text = svc.Balance.ToDecPlaces();

                    t.Rows.Add(ref rref);
                    rn++;
                }

                // Reference: https://stackoverflow.com/questions/7097402/how-do-i-find-the-page-number-for-a-word-paragraph

                var origPagesCount = GetPagesCount(document);
                while (true)
                {
                    t.Rows.Add(ref rref);
                    var currPageCount = GetPagesCount(document);
                    if (currPageCount > origPagesCount)
                    {
                        document.Undo();
                        break;
                    }
                }

                object filename = outputfilename;
                document.SaveAs2(ref filename);
                document.Close(ref _missing, ref _missing, ref _missing);
                document = null;
                res = true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                res = false;
            }
            finally
            {
                if (winword != null)
                {
                    winword.Quit(ref _missing, ref _missing, ref _missing);
                    winword = null;
                }
            }
            return (res, errorMsg);
        }

        private static int GetPagesCount(Document document)
        {
            var PagesCountStat = WdStatistic.wdStatisticPages;
            int pagesCount = document.ComputeStatistics(PagesCountStat, ref _missing);
            return pagesCount;
        }
    }
}
