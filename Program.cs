using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace PDFMerger
{
    class Program
    {
        Display Display = new ();
        public static void Main (string [] args)
        {
            if (Capabilities.Build.IsCoreBuild)
            {
                GlobalFontSettings.FontResolver = new FailsafeFontResolver ();
            }
            StartMergeProcess ();
        }

        public static void StartMergeProcess ()
        {
            List<PdfDocument> fileList = new List<PdfDocument> ();

            Display.OutputMessage ("Accepted document extensions: .pdf .png .jpg .bmp", OutputType.Warning);

            GetDocuments (fileList);

            MergeDocuments (fileList, out PdfDocument outputDocument);

            bool isRelativeToFirstDocument = Display.InputMessage ("Save in the same path as the first document?", true);
            if (isRelativeToFirstDocument)
            {
                string path = fileList [0].IsImported ?
                    new FileInfo (fileList [0].FullPath).Directory.FullName :
                    new FileInfo (fileList [0].Info.Comment).Directory.FullName;

                Display.OutputMessage ("path: " + path, OutputType.Success);
                SaveOutput (outputDocument, true, path);
            } else
                SaveOutput (outputDocument, false);
        }
        public static void GetDocuments (List<PdfDocument> fileList)
        {
            bool inputingFiles = true;
            while (inputingFiles)
            {
                string newDocumentPath = @"" + Display.InputMessage ("Input file path: ");
                newDocumentPath = newDocumentPath.Replace ('\\', '/').Trim ('"');

                try
                {

                    if (string.IsNullOrEmpty (newDocumentPath))
                    {
                        inputingFiles = false;
                    } else if (File.Exists (newDocumentPath))
                    {
                        PdfDocument newDocument;
                        switch (Path.GetExtension (newDocumentPath))
                        {
                            case ".png" or ".bmp" or ".jpg" or ".jpeg":
                                Image pathImage = Image.FromFile (newDocumentPath);
                                XImage newImage;
                                using (MemoryStream ms = new ())
                                {
                                    pathImage.Save (ms, ImageFormat.Png);
                                    byte [] imageBytes = ms.ToArray ();
                                    newImage = XImage.FromStream (ms);
                                }
                                newDocument = new PdfDocument (new FileInfo (newDocumentPath).Directory.FullName + "/new_fileWithImage.pdf");
                                PdfPage page1 = newDocument.AddPage ();

                                page1.Width = XUnitPt.FromPoint (newImage.PointWidth);
                                page1.Height = XUnit.FromPoint (newImage.PointHeight);
                                XGraphics gfx = XGraphics.FromPdfPage (page1);
                                newDocument.Info.Comment = new FileInfo (newDocumentPath).Directory.FullName + "/new_fileWithImage.pdf";
                                gfx.DrawImage (newImage, 0, 0, newImage.PixelWidth, newImage.PixelHeight);
                                fileList.Add (newDocument);
                                newDocument.Close ();
                                Display.OutputMessage (new FileInfo (newDocumentPath).Name + " - Successfully added to merge list!", OutputType.Success);
                                break;
                            case ".pdf":
                                newDocument = PdfReader.Open (newDocumentPath, PdfDocumentOpenMode.Import);
                                fileList.Add (newDocument);
                                Display.OutputMessage (new FileInfo (newDocumentPath).Name + " - Successfully added to merge list!", OutputType.Success);
                                break;
                            default:
                                Display.OutputMessage (("Warning: File extension not supported! - " + Path.GetExtension (newDocumentPath)), OutputType.Warning);
                                break;
                        }

                    } else { throw new FileNotFoundException ($"File {newDocumentPath}, not found "); }
                } catch (Exception ex)
                {
                    Display.OutputMessage (ex.Message, OutputType.Error);
                }
            }
        }
        public static void MergeDocuments (List<PdfDocument> fileList, out PdfDocument outputDocument)
        {
            outputDocument = new PdfDocument ();
            for (int fileIdx = 0 ;fileIdx < fileList.Count ;fileIdx++)
            {
                PdfDocument specificFile = fileList [fileIdx];
                bool isImported = specificFile.IsImported;
                if (isImported)
                {
                    for (int pageIdx = 0 ;pageIdx < specificFile.PageCount ;pageIdx++)
                    {

                        outputDocument.AddPage (specificFile.Pages [pageIdx]);
                    }
                } else
                {
                    outputDocument.AddPage (PdfReader.Open (specificFile.Info.Comment, PdfDocumentOpenMode.Import).Pages [0]);
                }
            }

            Display.OutputMessage ($"Successfully merged {fileList.Count} documents!", OutputType.Success);
        }
        public static void SaveOutput (PdfDocument documentOutput, bool relativeTo, string outputFilePath = "")
        {
            string outputFileName = @"" + Display.InputMessage ("output file name: ");
            outputFileName = string.IsNullOrEmpty (outputFileName) ? "New_File" + DateTime.Now.ToString ("dd_MM_yy ss-mm-HH") : outputFileName;
            if (!relativeTo)
            {
                outputFilePath = Display.InputMessage ("output file path: ");
            }

            string savePath = outputFilePath + $"/{outputFileName}.pdf";
            Display.OutputMessage ("Success! File saved at: " + savePath, OutputType.Success);
            documentOutput.Save (savePath);
            documentOutput.Close ();
            if (Display.InputMessage ("Open the file's folder?", false))
            {
                Process.Start ("explorer.exe", outputFilePath);
            }
            if (Display.InputMessage ("Restart process? ", true))
            {
                StartProcess ();
            } else 
                Environment.Exit (0);
        }
    }
}