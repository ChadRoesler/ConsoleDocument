using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.ExtensionMethods;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Structs;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestingApp
{
    internal class Program
    {
        private static CancellationToken cancellationToken;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // optional but recommended for glyphs

            long total = 100;
            long current = 0;
            string status = "Starting...";

            var workTask = Task.Run(async () =>
            {
                for (int i = 0; i <= total; i++)
                {
                    current = i;
                    status = i == 0 ? "Starting..." : (i < 100 ? "Progressing..." : "Complete!");
                    await Task.Delay(75);
                }
            });

            ProgressState Provider() => new(current, total, status);

            // Build doc first so we can reuse its VTEnabled if you prefer
            var doc = new ConsoleDocument(100, enableVT: true)
            {
                BorderColor = PlushColor.VelvetRot,
                TextColor = PlushColor.SerenValeBlue,
                RootGlyphColor = PlushColor.ChadsCopper,
                OutlineColors = [PlushColor.DeepPink, PlushColor.DeepSkyBlue, PlushColor.GreenYellow],
                TreeNodeColor = PlushColor.PaleGreen,
                BarGraphColor = PlushColor.FireBrick,
                AlternateBarGraphColors = true,
                Blocks = []
            };

            var progressBar = new ConsoleProgressBar(
                "Sample Progress bar animated",
                vtEnabled: doc.VTEnabled, // or true
                progressProvider: Provider,
                workTask: workTask,
                description: "This is an optional multi-line description that wraps inside the box.\r\nIt stays still when the bar animates so only the top 2 rows change."
            )
            {
                BarColor = PlushColor.LimeGreen,
                EmptyColor = PlushColor.DarkGray,
                TextColor = PlushColor.DefaultForeground
            };

            // Add other blocks and progress bar
            doc.Blocks.AddRange(
            [
                new ConsoleHeader("Welcome to the Console Document System"),

                new ConsoleOutline("This is an outline block")
                {
                    ConsoleNodes = [
                        new ConsoleNode("Node 1"),
                        new ConsoleNode("Node 2")
                        {
                            ConsoleNodes = [
                                new ConsoleNode("Child Node 1"),
                                new ConsoleNode("Child Node 2")
                                {
                                    ConsoleNodes = [
                                        new ConsoleNode("Grandchild Node 1 this text is ment to be very long lorumn ipsum style, like super long i cant even remember how loing this needs to be but super long i guess?"),
                                        new ConsoleNode("Grandchild Node 2\r\nAnd returns using \\r\\n")
                                    ]
                                },
                            ]
                        },
                        new ConsoleNode("Node 3")
                        {
                            ConsoleNodes = [
                                new ConsoleNode("Child Node 3"),
                                new ConsoleNode("Child Node 4")
                                {
                                    ConsoleNodes = [
                                        new ConsoleNode("Grandchild Node 3"),
                                        new ConsoleNode("Grandchild Node 4")
                                        {
                                            ConsoleNodes = [
                                                new ConsoleNode("Great Grandchild Node 1"),
                                                new ConsoleNode("Great Grandchild Node 2")
                                            ]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                },
                new ConsoleSeperator("This is a separator with text"),

                new ConsoleTreeDiagram("Sample Tree Diagram",
                    new ConsoleNode("Root Node")
                        {
                            ConsoleNodes = [
                                new ConsoleNode("Child Node 1"),
                                new ConsoleNode("Child Node 2")
                                {
                                    ConsoleNodes = [
                                        new ConsoleNode("Grandchild Node 1"),
                                        new ConsoleNode("Grandchild Node 2")
                                    ]
                                },
                                new ConsoleNode("Child Node 3"),
                                new ConsoleNode("Child Node 4")
                                {
                                    ConsoleNodes = [
                                        new ConsoleNode("Grandchild Node 3"),
                                        new ConsoleNode("Grandchild Node 4")
                                        {
                                            ConsoleNodes = [
                                                new ConsoleNode("Great Grandchild Node 1"),
                                                new ConsoleNode("Great Grandchild Node 2")
                                            ]
                                        },
                                        new ConsoleNode("Grandchild Node 5")
                                    ]
                                },
                                new ConsoleNode("Child Node 5")
                                {
                                    ConsoleNodes = [
                                        new ConsoleNode("Grandchild Node 5"),
                                        new ConsoleNode("Grandchild Node 6")
                                    ]
                                }
                            ]
                        }),
                new ConsoleSeperator(),
                new ConsoleBarGraph("Sample Bar Graph", new List<ConsoleGraphSegment>  {
                    new ( "Category 1", 30 ),
                    new ( "Category 2", 30 ),
                    new ( "Category 3", 30 ),
                    new ( "Category 4", 500 ),
                    new ( "Category 5", 30)
                }),

                new ConsoleSeperator(),
                new ConsoleBarGraph("Sample Bar Graph2", new List<ConsoleGraphSegment> {
                    new ( "Category 1", PlushColor.AliceBlue, 3 ),
                    new ( "Category 2", PlushColor.BurlyWood,4 ),
                    new ( "Category 3", PlushColor.Chartreuse, 6 ),
                    new ( "Category 4", PlushColor.Coral, 7 ),
                    new ( "Category 5", PlushColor.LemonChiffon, 10)
                }),
                new ConsoleSeperator(),
                new ConsoleDividedBarGraph( "Sample Divided Bar Graph",
                    new List<ConsoleGraphSegment>
                    {
                        new("Category 1", PlushColor.AliceBlue, 20),
                        new("Category 2", PlushColor.BurlyWood, 20),
                        new("Category 3", PlushColor.Chartreuse, 20),
                        new("Category 4", PlushColor.Coral, 20),
                        new("Category 5", PlushColor.LemonChiffon, 15)
                    }),
                new ConsoleDividedBarGraph( "Sample Divided Bar Graph",
                    new List<ConsoleGraphSegment>
                    {
                        new("Category 1", PlushColor.AliceBlue, 50),
                        new("Category 2", PlushColor.BurlyWood, 20),
                        new("Category 3", PlushColor.Chartreuse, 100),
                        new("Category 4", PlushColor.Coral, 20),
                        new("Category 5", PlushColor.LemonChiffon, 15)
                    }),
                new ConsoleSeperator(),
                new ConsolePanel("ConsolePanelTest", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo. Proin id dolor vehicula, lobortis ante non, cursus justo. Nam non ligula nulla. Phasellus malesuada sit amet augue id ornare. In vestibulum sapien hendrerit ornare scelerisque. Sed a justo sit amet nisi vulputate ullamcorper vel vitae arcu. Vivamus suscipit et velit in faucibus. Nullam vel tristique nunc, iaculis euismod magna. Nullam ornare porttitor ipsum, eget pharetra ligula. Vivamus accumsan porttitor lacus vitae suscipit. Phasellus ut lorem id enim tempus vehicula non non risus. Vivamus non magna at sem luctus efficitur.\r\n\r\nNunc blandit elit ut est gravida, ac finibus dolor porta. Pellentesque massa nulla, vestibulum vitae consequat vitae, ultrices vel ipsum. Sed mattis feugiat nibh, a interdum nulla tristique porta. Vestibulum gravida dui sit amet nunc tempus imperdiet. Integer id enim bibendum, vehicula est quis, egestas dolor. Proin in vulputate libero. Curabitur vel mi malesuada, faucibus odio ac, porttitor ante. Mauris dolor massa, ullamcorper vitae enim vitae, tempus cursus sem. Mauris at justo ac nunc aliquam blandit. Quisque dignissim nulla sed arcu facilisis convallis. Phasellus et lectus justo. Sed feugiat nec est sit amet placerat.\r\n\r\nClass aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vestibulum volutpat pellentesque massa ut hendrerit. Suspendisse vel ipsum ut sapien viverra varius vitae id leo. Morbi tincidunt vel mauris sit amet scelerisque. Aliquam ultrices, erat non condimentum vestibulum, ex arcu gravida orci, in porttitor tellus augue vitae ante. Vestibulum lacinia accumsan ligula, nec mattis lorem aliquet quis. Phasellus vel mollis dolor. Sed porttitor purus vitae turpis pellentesque malesuada. Cras enim ipsum, porta ut pulvinar a, faucibus vitae odio. Fusce vel gravida leo. Vivamus pulvinar lacus nulla, sollicitudin tincidunt dui tincidunt ac. Pellentesque sodales semper mollis. Sed accumsan mauris sed est vulputate, ut elementum nisi pellentesque.\r\n\r\nAliquam fermentum magna eu nisi hendrerit, vulputate posuere metus bibendum. Maecenas vestibulum dignissim ante, condimentum dapibus massa porttitor a. Praesent quam est, finibus quis felis sed, gravida molestie nisi. Proin sagittis lobortis eros. Proin efficitur, quam vitae efficitur faucibus, ante enim malesuada est, a sollicitudin orci nulla non lorem. Donec vitae massa accumsan, maximus lorem at, venenatis augue. Nullam consequat eros ac sagittis hendrerit.\r\n\r\nSed eu sapien sed lacus ornare mollis. Phasellus vel arcu quis risus pretium tempor. Proin pretium vulputate dui sed ullamcorper. Aenean ut molestie neque. Quisque vulputate dolor at consectetur accumsan. Duis non diam erat. Nunc id rhoncus justo, quis eleifend enim. Praesent eget scelerisque sem. Nulla facilisis diam quis magna molestie commodo. Cras vestibulum mauris ac urna egestas, a suscipit risus fermentum. Donec nec metus nunc. Ut blandit nunc ut eros cursus, sit amet porttitor turpis pretium. Cras non diam at dui aliquam gravida. Donec eu enim pulvinar, interdum ex vitae, aliquet augue."),
                new ConsoleSeperator(),

                new ConsoleSeperator(),
                new ConsoleTable("Sample Data Table", BuildGroupingTestTable().ToConsoleTableHierarchy()),

                // Live progress bar demo (animates while the task runs)
                progressBar,

                new ConsoleFooter("")
            ]);


            await doc.RenderFullScreenLiveAsync(refreshMs: 100, cancellationToken);
            //doc.Render();

            await workTask;
            await LiveRegionRenderer.StopAsync();
        }

        public static DataTable BuildGroupingTestTable()
        {
            var table = new DataTable();
            table.Columns.Add("Column0");
            table.Columns.Add("Column1");
            table.Columns.Add("Column2");
            table.Columns.Add("Column3");

            // A group
            table.Rows.Add("A", "X", "M", "1");
            table.Rows.Add("A", "X", "M", "2");
            table.Rows.Add("A", "X", "M", "2");
            table.Rows.Add("A", "X", "N", "3");
            table.Rows.Add("A", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.", "O", "4");

            // B group
            table.Rows.Add("B", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.", "M", "5");
            table.Rows.Add("B", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.", "M", "6");
            table.Rows.Add("B", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.", "N", "7");
            table.Rows.Add("B", "Y", "O", "8");

            // C group
            table.Rows.Add("C", "Z", "P", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.");
            table.Rows.Add("C", "Z", "P", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ultrices velit a est sagittis, nec maximus nisl commodo.");

            // D group
            table.Rows.Add("D", "W", "P", "DINGLE DANGLE HIUNGLD.");
            table.Rows.Add("D", "X", "N", "3");

            return table;
        }
    }
}