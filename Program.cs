using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ucli
{
    internal class Program
    {
        public const string Version = "0.0.1";

        public interface ICommand
        {
            void Execute(string[] args);
            string Help{ get; set; }
            string[] Aliases{get; set;}
            string Description{get; set;}
            int MinArgs{get; set;}
        }
        public class CommandGroup : ICommand
        {
            public List<ICommand> commands;
            public void Execute(string[] args)
            {
                foreach (var command in commands)
                {
                    if (command.Aliases.Contains(args[0]))
                    {
                        if(args.Length >=2 && args[1] == "help")
                        {
                            Console.WriteLine(command.Help);
                            return;
                        }
                        if (args.Length - 1 < command.MinArgs)
                        {
                            Console.WriteLine("Not enough arguments provided");
                            return;
                        }
                        command.Execute(args.Skip(1).ToArray());
                        return;
                    }
                }
            }
            public string Help
            {
                get
                {
                    string txt = "";
                    foreach (var command in commands)
                    {
                        txt += $"{command.Aliases[0]} - {command.Description}\n";
                    }

                    return txt;
                }
                set{}
            }
            public string[] Aliases { get; set; }
            public string Description { get; set; }
            public int MinArgs { get; set; } = 1;

            public CommandGroup(List<ICommand> commands, string description, params string[] aliases)
            {
                this.commands = commands;
                Aliases = aliases;
                Description = description;
            }
        }
        public class Command : ICommand
        {
            public delegate void CommandDelegate(string[] args);
            public CommandDelegate command;
            public void Execute(string[] args)
            {
                command?.Invoke(args);
            }

            public string Help { get; set; }
            public string[] Aliases { get; set; }
            public string Description { get; set; }
            public int MinArgs { get; set; }

            public Command(CommandDelegate command, string help, string description, int minArgs, params string[] aliases)
            {
                this.command = command;
                Help = help;
                Aliases = aliases;
                Description = description;
                MinArgs = minArgs;
            }
        }
        
        public static Random rnd = new Random();
        private static string ByteToString(byte[] array)
        {
            string output = "";
            foreach (var b in array)
            {
                output += b.ToString("x2");
            }
            return output;
        }
        public static List<ICommand> Commands = new List<ICommand>()
        {
            new Command((t)=>{Console.WriteLine(Version);}, "Version", "Write Version", 0, "version", "v"),
            new CommandGroup(new List<ICommand>()
            {
                new Command((args) =>
                {
                    int min = 0;
                    int max = 0;
                    if (args.Length == 1)
                    {
                        max = int.Parse(args[0]);
                    }
                    else
                    {
                        max = int.Parse(args[1]);
                        min = int.Parse(args[0]);
                    }
                    Console.WriteLine(rnd.Next(min,max));
                }, "int [min](def:0) [max]", "Generates a random integer", 1, "int", "i"),
                new Command((args) => { Console.WriteLine((float) rnd.NextDouble());}, "float", "Generates a random float", 0, "float", "f"),
                new Command((args) =>
                {
                    string charset = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
                    Console.WriteLine(charset[rnd.Next(charset.Length)]);
                }, "char", "Generates a random char", 0, "char", "c"),
                new Command((args) =>
                {
                    string charset = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
                    string output = "";
                    for (int i = 0; i < int.Parse(args[0]); i++)
                    {
                        output += charset[rnd.Next(charset.Length)];
                    }
                    Console.WriteLine(output);
                }, "string [n]", "Generates a random string", 1, "string", "s"),
                new CommandGroup(new List<ICommand>()
                {
                    new Command((args) =>
                    {
                        int red = rnd.Next(0, 255);
                        int green = rnd.Next(0, 255);
                        int blue = rnd.Next(0, 255);
                        //Console.WriteLine($"#{red.ToString("x2")}{green.ToString("x2")}{blue.ToString("x2")}");
                        Console.WriteLine($"{red},{green},{blue}");
                    }, "rgb", "Generates a random RGB color", 0, "rgb"),
                    new Command((args) =>
                    {
                        int red = rnd.Next(0, 255);
                        int green = rnd.Next(0, 255);
                        int blue = rnd.Next(0, 255);
                        Console.WriteLine($"#{red.ToString("x2")}{green.ToString("x2")}{blue.ToString("x2")}");
                        //Console.WriteLine($"{red},{green},{blue}");
                    }, "rgb", "Generates a random RGB HEX color", 0, "hex"),
                }, "Random Color", "color", "c", "col")
            }, "Random Everything", "rnd", "r", "rand", "random"),
            new CommandGroup(new List<ICommand>()
            {
                new Command((args) =>
                {
                    Console.WriteLine(ByteToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(Console.ReadLine()))));
                }, "sha256", "Hash Line By SHA-256", 0, "sha256", "256"),
                new Command((args) =>
                {
                    Console.WriteLine(ByteToString(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(Console.ReadLine()))));
                }, "sha512", "Hash Line By SHA-512", 0, "sha512", "512"),
                new Command((args) =>
                {
                    Console.WriteLine(ByteToString(new MD5Cng().ComputeHash(Encoding.UTF8.GetBytes(Console.ReadLine()))));
                }, "md5", "Hash Line By MD5", 0, "md5", "5"),
            }, "Hash Line", "hash", "h"),
            new CommandGroup(new List<ICommand>()
            {
                new Command((args) =>
                {
                    Console.WriteLine($"#{int.Parse(args[0]).ToString("x2")}{int.Parse(args[1]).ToString("x2")}{int.Parse(args[2]).ToString("x2")}"); 
                    //Console.WriteLine($"{red},{green},{blue}");
                }, "rgb2hex [red] [green] [blue]", "RGB To HEX", 3,"rgb2hex","rgbhex", "rh"),
                new Command((args) =>
                {
                    string hex = args[0];
                    if (hex.StartsWith("#"))
                    {
                        hex = hex.Substring(1);
                    }
                    Console.WriteLine($"{int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber)},{int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber)},{int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber)}");
                }, "hex2rgb [hex]", "HEX To RGB", 1,"hex2rgb","hexrgb", "hr"),
                new Command((args) =>
                {
                    int red = int.Parse(args[0]);
                    int green = int.Parse(args[1]);
                    int blue = int.Parse(args[2]);
                    //Convert rgb to cmyk 
                    float c = 1 - (red / 255f);
                    float m = 1 - (green / 255f);
                    float y = 1 - (blue / 255f);
                    float k = Math.Min(c, Math.Min(m, y));
                    c = (c - k) / (1 - k);
                    m = (m - k) / (1 - k);
                    y = (y - k) / (1 - k);
                    Console.WriteLine($"{c} {m} {y} {k}");
                }, "rgb2cmyk [red] [green] [blue]", "RGB To CMYK", 3,"rgb2cmyk","rgbcmyk", "rc"),
                new Command((args) =>
                {
                    float c = float.Parse(args[0]);
                    float m = float.Parse(args[1]);
                    float y = float.Parse(args[2]);
                    float k = float.Parse(args[3]);
                    //Convert cmyk to rgb
                    float red = (1 - c) * (1 - k);
                    float green = (1 - m) * (1 - k);
                    float blue = (1 - y) * (1 - k);
                    Console.WriteLine($"{red * 255},{green * 255},{blue * 255}");
                }, "cmyk2rgb [cyan] [magenta] [yellow] [black]", "CMYK To RGB", 4,"cmyk2rgb","cmykrgb", "cr"),
                //rbg to hsl
                new Command((args) =>
                {
                    int red = int.Parse(args[0]);
                    int green = int.Parse(args[1]);
                    int blue = int.Parse(args[2]);
                    float r = red / 255f;
                    float g = green / 255f;
                    float b = blue / 255f;
                    float max = Math.Max(r, Math.Max(g, b));
                    float min = Math.Min(r, Math.Min(g, b));
                    float h = 0;
                    float s = 0;
                    float l = (max + min) / 2;
                    if (max == min)
                    {
                        h = s = 0;
                    }
                    else
                    {
                        float d = max - min;
                        s = l > 0.5f ? d / (2 - max - min) : d / (max + min);
                        if (max == r)
                        {
                            h = (g - b) / d + (g < b ? 6 : 0);
                        }
                        else if (max == g)
                        {
                            h = (b - r) / d + 2;
                        }
                        else if (max == b)
                        {
                            h = (r - g) / d + 4;
                        }
                        h /= 6;
                    }
                    Console.WriteLine($"{h * 360},{s * 100},{l * 100}");
                }, "rgb2hsl [red] [green] [blue]", "RGB To HSL", 3,"rgb2hsl","rgbhsl", "rh"),
            }, "Convert Color", "convcol", "convertcolor", "cc", "convcolor", "convertcol"),
            new CommandGroup(new List<ICommand>()
            {
                new Command((args) =>
                {
                    FFmpeg.NET.Engine engine = new FFmpeg.NET.Engine(@"ffmpeg.exe");
                    string start = args.Length > 3 ? $"-ss {args[1]}" : "";
                    string end = args.Length > 3 ? $"-t {args[2]}" : $"-t {args[1]}";
                    engine.ExecuteAsync($"-i {args[0]} {start} {end} -async 1 {args[args.Length - 1]}").Wait();
                }, "cut [filename] ([start]) [end] [output]", "Cut a video", 3, "cut"),
                new Command((args) =>
                {
                    FFmpeg.NET.Engine engine = new FFmpeg.NET.Engine(@"ffmpeg.exe");
                    File.WriteAllText("temp.txt", $"file '{args[0]}'\nfile '{args[1]}'");
                    engine.ExecuteAsync($"-f concat -i temp.txt -c copy {args[args.Length - 1]}").Wait();
                    File.Delete("temp.txt");
                }, "merge [filename1] [filename2] [output]", "Merge a videos", 3, "merge", "m", "conc", "concatenate", "concat"),
                //convert anything to anything
                new Command((args) =>
                {
                    FFmpeg.NET.Engine engine = new FFmpeg.NET.Engine(@"ffmpeg.exe");
                    engine.ExecuteAsync($"-i {args[0]} {args[1]}").Wait();
                }, "convert [filename] [output]", "Convert a video", 2, "convert", "conv"),
            }, "Tools For Video Editing", "video", "v", "vid"),
        };
        public static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No arguments provided");
                return;
            }
            if(args[0] == "help")
            {
                foreach (var cmd in Commands)
                {
                    Console.WriteLine($"{cmd.Aliases[0]} - {cmd.Description}");
                }
                return;
            }
            foreach (var command in Commands)
            {
                if (command.Aliases.Contains(args[0]))
                {
                    if (args.Length - 1 < command.MinArgs)
                    {
                        Console.WriteLine("Not enough arguments provided");
                        return;
                    }
                    if(args.Length >= 2&& args[1] == "help")
                    {
                        Console.WriteLine(command.Help);
                        return;
                    }
                    command.Execute(args.Skip(1).ToArray());
                    return;
                }
            }
            Console.WriteLine("Unknown command");
        }
    }
}