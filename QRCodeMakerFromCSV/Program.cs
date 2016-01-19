using Microsoft.VisualBasic.FileIO;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QRCodeMakerFromCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No source file location parameter provided.");
            }
            else
            {
                try
                {
                    string sourceFile = args[0];
                    Console.WriteLine("Source file: " + args[0]);

                    TextFieldParser parser = new TextFieldParser(sourceFile);
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    // Get headers
                    string[] headerArray = parser.ReadFields();

                    // Get contents
                    List<string[]> rowList = new List<string[]>();
                    while (!parser.EndOfData)
                    {
                        rowList.Add(parser.ReadFields());
                    }

                    Parallel.ForEach(rowList, (row) =>
                    {
                        try
                        {
                            string line = "";

                            for (int i = 0; i < row.Length; i++)
                            {
                                line += String.Format("{0}: {1}\n", headerArray[i], row[i]).ToUpper();
                            }

                            Console.Write(line);

                            // Create identifier
                            string identifier = String.Format("{0}, {1} {2}", row[2], row[0], row[1]).ToUpper().Replace("N/A", String.Empty);

                            // Generate qr code image
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(line, QRCodeGenerator.ECCLevel.Q);
                            QRCode qrCode = new QRCode(qrCodeData);
                            Bitmap qrCodeImage = qrCode.GetGraphic(20);

                            Directory.CreateDirectory("output");

                            // Save image to file
                            qrCodeImage.Save(String.Format("output\\{0}.jpg", identifier));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }      
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                           
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
