using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Reflection.Metadata;
using System.Drawing;

namespace CardExportToPDFTest
{
    public class Program
    {
        static void Main(string[] args) {
            // Boilerplate to start
            Console.WriteLine("Process Launched.");
            List<string> importList = File.ReadAllLines("ImportList.txt").ToList();
            List<string> decks = new List<string>();

            if (!Directory.Exists("Output-Decks")) {
                Console.WriteLine("No Output-Decks directory located, creating folder...");
                Directory.CreateDirectory("Output-Decks");
            }

            // Read ImportList.txt for decks to import
            foreach(string i in importList) { decks.Add(i.Split("\\").Last()); }
            Console.WriteLine($"Located {decks.Count} decks: { String.Join(", ", decks.ToArray()) }\n");

            int importCounter = 0;
            foreach (string d in decks) {
                Console.WriteLine($"Starting Export of deck {d}:");

                // Create new document
                PdfDocument document = new PdfDocument();
                document.Info.Title = $"Yugioh Deck Export - {d}";

                // Count Cards in Deck
                List<string> deckData = File.ReadAllText($"{importList[importCounter]}\\{d}.txt").Replace("[", "").Replace("]", "").Replace("\"", "").Split(",").ToList();
                deckData.RemoveAt(0);
                int count = deckData.Count();
                int totalPages = (int)Math.Ceiling((double)count / 25);
                Console.WriteLine($"- There are {count} cards in the {d}. Therefore, {totalPages} pages will be used.");

                // Configure Pages
                List<PdfPage> pages = new List<PdfPage>();
                List<XGraphics> gfxList = new List<XGraphics>();
                for (int i = 0; i < totalPages; i++) {
                    PdfPage p = document.AddPage();
                    XGraphics g = XGraphics.FromPdfPage(p);

                    p.Orientation = PageOrientation.Portrait;
                    p.Width = XUnit.FromMillimeter(295);
                    p.Height = XUnit.FromMillimeter(430);

                    pages.Add(p);
                    gfxList.Add(g);
                }

                Console.WriteLine("- Document Created. Processing cards in folder...");

                // Load Image and place on Page
                int imageXCount = 0;
                int imageYCount = 0;
                int pageListCounter = 0;
                foreach (string cardId in deckData) {
                    if (imageXCount % 5 == 0 && imageXCount > 0) {
                        if (imageYCount % 5 == 0 && imageYCount > 0) {
                            pageListCounter++;
                            imageYCount = 0;
                        } else {
                            imageYCount++;
                        }
                        imageXCount = 0;
                    }

                    // Calculate position to paste card
                    double xPos = (pages[pageListCounter].Width / 5) * imageXCount;
                    double yPos = (pages[pageListCounter].Height / 5) * imageYCount;

                    // Load Image
                    if (!File.Exists($"{importList[importCounter]}\\{cardId}.jpg")) { Console.WriteLine($"Card {cardId}.jpg non-existant, skipping..."); continue; }
                    XImage i = XImage.FromFile($"{importList[importCounter]}\\{cardId}.jpg");
                    Console.WriteLine($"  - Loaded File {cardId}.jpg");

                    // Place card on page
                    gfxList[pageListCounter].DrawImage(i, new XRect(xPos, yPos, XUnit.FromMillimeter(59), XUnit.FromMillimeter(86)));

                    imageXCount++;
                }

                // Export file
                Console.WriteLine($"- Images imported into PDF. Exporting file {d}.pdf now...");
                string fileName = $"{d}.pdf";
                document.Save($"Output-Decks\\{fileName}");

                if (File.Exists($"Output-Decks\\{fileName}")) Console.WriteLine($"- {d}.pdf succesfully exported.\n"); else { Console.WriteLine($"- {d}.pdf failed to export.\n"); }

                importCounter++;
            }

            Console.WriteLine("All decks processed, please check the Output-Decks directory. Enjoy your new cards!\nThis Console will automatically close after 10 seconds.\n\n");
            Thread.Sleep(10000);
        }
    }
}
