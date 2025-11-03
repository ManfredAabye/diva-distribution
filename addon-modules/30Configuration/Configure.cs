using System;
using System.IO;
using System.Net;
using System.Text;
using OpenMetaverse;

namespace MetaverseInk.Configuration
{
    public class Configure
    {
        private static string worldName = "My World";
        private static string dbHost = "localhost";
        private static string dbSchema = "opensim";
        private static string dbUser = "opensim";
        private static string dbPasswd = "secret";
        private static string adminFirst = "Wifi";
        private static string adminLast = "Admin";
        private static string adminPasswd = "secret";
        private static string adminEmail = "admin@localhost";
        private static string ipAddress = "127.0.0.1";
#pragma warning disable CS0414 // Field assigned but never used
        private static string platform = "1"; // 1 for .NET 8+
        private static int baseLocationX = 0, baseLocationY = 0;
        private static bool confirmationRequired = false;
#pragma warning restore CS0414
        private static bool myWorldReconfig = false;
        private static string gmailAccount = string.Empty;
        private static string gmailPasswd = string.Empty;

        private enum RegionConfigStatus : uint
        {
            OK = 0,
            NeedsCreation = 1,
            NeedsEditing = 2
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Diva Distribution Configuration Tool for .NET 8");
            Console.WriteLine("Runtime: " + Environment.Version);
            GetUserInput();
            ConfigureRegions();
            ConfigureMyWorld();
            DisplayInfo();
        }

        private static void GetUserInput()
        {
            Console.Write("Name of your world [" + worldName + "]: ");
            string input = Console.ReadLine();
            if (input != string.Empty)
                worldName = input;

            Console.Write("Your external IP address or domain name [" + ipAddress + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                ipAddress = input;

            Console.Write("Database host [" + dbHost + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                dbHost = input;

            Console.Write("Database schema [" + dbSchema + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                dbSchema = input;

            Console.Write("Database user [" + dbUser + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                dbUser = input;

            Console.Write("Database password [" + dbPasswd + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                dbPasswd = input;

            Console.Write("Wifi admin first name [" + adminFirst + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                adminFirst = input;

            Console.Write("Wifi admin last name [" + adminLast + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                adminLast = input;

            Console.Write("Wifi admin password [" + adminPasswd + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                adminPasswd = input;

            Console.Write("Wifi admin email [" + adminEmail + "]: ");
            input = Console.ReadLine();
            if (input != string.Empty)
                adminEmail = input;

            Console.Write("Gmail account for notifications (optional): ");
            input = Console.ReadLine();
            if (input != string.Empty)
                gmailAccount = input;

            if (gmailAccount != string.Empty)
            {
                Console.Write("Gmail password: ");
                input = Console.ReadLine();
                if (input != string.Empty)
                    gmailPasswd = input;
            }
        }

        private static RegionConfigStatus CheckRegionConfig()
        {
            if (File.Exists("Regions/RegionConfig.ini"))
            {
                using (TextReader tr = new StreamReader("Regions/RegionConfig.ini"))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        if (line.Contains("MasterAvatar"))
                            return RegionConfigStatus.NeedsEditing;
                    }
                }
                return RegionConfigStatus.OK;
            }

            return RegionConfigStatus.NeedsCreation;
        }

        private static void ConfigureRegions()
        {
            RegionConfigStatus status = CheckRegionConfig();

            if (status == RegionConfigStatus.OK)
            {
                Console.WriteLine("Your regions have been preserved."); 
                return;
            }

            if (status == RegionConfigStatus.NeedsEditing)
            {
                Console.WriteLine("*** Warning: Master Avatar is obsolete.");
                Console.WriteLine("Please edit file Regions/RegionConfig.ini and delete all references to MasterAvatar.");
                return;
            }

            // else RegionConfigStatus.NeedsCreation
            int count = 0;
            try
            {
                using (TextReader tr = new StreamReader("Regions/RegionConfig.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("Regions/RegionConfig.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("My World"))
                                line = line.Replace("My World", worldName);
                            if (line.Contains("RegionUUID"))
                                line = line.Replace("RegionUUID", "RegionUUID = " + UUID.Random().ToString());
                            if (line.Contains("Location"))
                            {
                                count++;
                                baseLocationX = 1000 + count * 1000;
                                baseLocationY = 1000;
                                line = line.Replace("1000,1000", baseLocationX + "," + baseLocationY);
                            }
                            tw.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error configuring regions: " + e.Message);
                return;
            }
            Console.WriteLine("Your regions have been configured for first run");
        }

        private static void CheckMyWorldConfig()
        {
            if (File.Exists("config-include/MyWorld.ini"))
            {
                try
                {
                    File.Move("config-include/MyWorld.ini", "config-include/MyWorld.ini.old");
                }
                catch
                {
                    // ignore and proceed
                }

                myWorldReconfig = true;
            }
        }

        private static void ConfigureMyWorld()
        {
            CheckMyWorldConfig();

            string connString = String.Format("ConnectionString = \"Data Source={0};Database={1};User ID={2};Password={3};Old Guids=true;Allow Zero Datetime=true;\"", dbHost, dbSchema, dbUser, dbPasswd);

            try
            {
                using (TextReader tr = new StreamReader("config-include/MyWorld.ini.example"))
                {
                    using (TextWriter tw = new StreamWriter("config-include/MyWorld.ini"))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {
                            if (line.Contains("ConnectionString"))
                                line = connString;
                            if (line.Contains("127.0.0.1"))
                                line = line.Replace("127.0.0.1", ipAddress);
                            if (line.Contains("welcome_message"))
                                line = line.Replace("Your World", worldName);
                            if (line.Contains("DefaultRegion"))
                            {
                                string defRegionName = "Region_" + worldName.Replace(' ', '_') + "_1";
                                line = line.Replace("Region_My_World_1", defRegionName);
                            }
                            if (line.Contains("SmtpUsername") && gmailAccount != string.Empty)
                                line = line.Replace("your_email", gmailAccount);
                            if (line.Contains("SmtpPassword") && gmailPasswd != string.Empty)
                                line = line.Replace("secret", gmailPasswd);
                            if (line.Contains("HomeLocation"))
                                line = line.Replace("My_World", worldName + "/128/128/30");

                            tw.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error configuring MyWorld: " + e.Message);
                return;
            }
            Console.WriteLine("Your World has been successfully configured for .NET 8");
        }

        private static void DisplayInfo()
        {
            Console.WriteLine("\n***************************************************");
            Console.WriteLine("Your world is " + worldName);
            Console.WriteLine("Your loginuri is http://" + ipAddress + ":9000");
            Console.WriteLine("Your Wifi app is http://" + ipAddress + ":9000/wifi");
            Console.WriteLine("You admin account for Wifi is:");
            Console.WriteLine("  username: " + adminFirst + " " + adminLast);
            Console.WriteLine("  passwd:   " + adminPasswd +"\n");
            if (gmailAccount == string.Empty)
                Console.WriteLine("Remember to set the Smtp Account for email notifications");
            else
                Console.WriteLine("Your users get email notifications from " + gmailAccount + "@gmail.com");
            if (myWorldReconfig)
                Console.WriteLine("\nNOTE: config-include/MyWorld.ini has been reconfigured.");
            else
                Console.WriteLine("Your world's configuration is config-include/MyWorld.ini.");
            Console.WriteLine("***************************************************\n");
            Console.Write("<Press enter to exit>");
            Console.ReadLine();
        }
    }
}