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
        private static readonly int[] identifierColIndeces = new int[] {0, 1, 2, 3};

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

                    int counter = 0;

                    Parallel.ForEach(rowList, (row) => {
                        string line = "";
                        string identifier = "";

                        for (int i = 0; i < row.Length; i++)
                        {
                            // Check if row item must be included in image identifier
                            if (Array.Exists<int>(identifierColIndeces, (item) =>
                            {
                                return i == item;
                            }))
                                identifier += row[i] + "_";

                            line += String.Format("{0}: {1}\n", headerArray[i], row[i]);
                        }

                        Console.Write(line);

                        // Generate qr code image
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(line, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20);

                        Interlocked.Increment(ref counter);

                        Directory.CreateDirectory("output");

                        // Save image to file
                        qrCodeImage.Save(String.Format("output\\{0}{1}.jpg", identifier.ToLower(), counter));
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
