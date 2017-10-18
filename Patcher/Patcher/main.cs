﻿using System;
using System.Windows;
using IWshRuntimeLibrary;
using System.Net;
using System.IO;
using Popup = System.Windows.MessageBox;

namespace Patcher
{
    class main
    {
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) //This function is taken straight from stackoverflow thanks to Konrad Rudolph. Rewrite
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                if (!(source.FullName == dir.FullName)) //prevent infinite copy loop
                {
                    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                }
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(System.IO.Path.Combine(target.FullName, file.Name));
        }

        public static void patch() //the main function
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "Borderlands|*.exe";
            fileDialog.Title = "Open Borderlands2.exe";
            fileDialog.InitialDirectory = @"C:\\Program Files (x86)\\Steam\\SteamApps\\common\\Borderlands 2\\Binaries\\Win32"; //I guess this isnt working
            fileDialog.RestoreDirectory = true; //this either
            var result = fileDialog.ShowDialog();
            string path = @"C:\\";
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    path = fileDialog.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }
            DirectoryInfo iBL2 = new DirectoryInfo(path); //bl2 = path to Borderlands2.exe
            DirectoryInfo inputDir = new DirectoryInfo(iBL2 + @"..\\..\\..\\..\\"); //convert to directory - IDK why I need more ..\\s then I actually should but it works so who cares
            DirectoryInfo outputDir = new DirectoryInfo(inputDir + @"\\server"); //convert to directory
            DirectoryInfo oBL2 = new DirectoryInfo(outputDir + @"\\Binaries\\Win32\\Borderlands2.exe");
            DirectoryInfo iUPK = new DirectoryInfo(inputDir + @"\\WillowGame\\CookedPCConsole\\WillowGame.upk"); // engine = path to WillowGame.upk
            DirectoryInfo oUPK = new DirectoryInfo(outputDir + @"\\WillowGame\\CookedPCConsole\\WillowGame.upk"); // engine = path to WillowGame.upk

            String decompress = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "decompress.exe");

            if (System.IO.File.Exists(iBL2.FullName)) //if borderlands2.exe exists
            {
                // -- COPY INPUT TO OUTPUT --
                try
                {
                    CopyFilesRecursively(inputDir, outputDir);

                }
                catch (IOException)
                {
                    Popup.Show("ERROR: Cannot copy Borderlands. Does it already exist?");
                }

                // -- COPY PATCHES TO BINARIES -- 
                
                using (WebClient myWebClient = new WebClient()) //download file
                {
                    try
                    {
                        myWebClient.DownloadFile("https://raw.githubusercontent.com/RobethX/BL2-MP-Mods/master/CoopPatch/cooppatch.txt", outputDir.FullName + @"\Binaries\cooppatch.txt");
                    }
                    catch (IOException)
                    {
                        //log
                    }
                }
             
                // -- RENAME UPK AND DECOMPRESSEDSIZE --
                try //incase it's already moved
                {
                    System.IO.File.Copy(oUPK.FullName, oUPK.FullName + ".bak"); //backup Engine.upk
                    System.IO.File.Move(oUPK.FullName + ".uncompressed_size", oUPK.FullName + ".uncompressed_size.bak"); //backup Engine.upk.uncompressed_size
                }
                catch (IOException)
                {
                    //log
                }

                // -- DECOMPRESS UPK --
                //var decompressing = System.Diagnostics.Process.Start(decompress, "-game=border -out=" + outputDir + @"\\WillowGame\\CookedPCConsole\\ " + iUPK.FullName); //decompress WillowGame.UPK
                var decompressing = System.Diagnostics.Process.Start(decompress, "-game=border " + iUPK.FullName); //decompress WillowGame.UPK
                decompressing.WaitForExit(); //wait for decompress.exe to finish
                FileInfo decompressedUPK = new FileInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "unpacked", iUPK.Name));
                decompressedUPK.CopyTo(oUPK.FullName, true); //move upk to cookedpcconsole


                // -- HEX EDIT UPK --
                var streamUPK = new FileStream(oUPK.FullName, FileMode.Open, FileAccess.ReadWrite);
                /*
                streamUPK.Position = 0x003F8AFD; //set position
                streamUPK.WriteByte(0x04);
                streamUPK.Position = 0x003F8AFE;
                streamUPK.WriteByte(0x3A);
                streamUPK.Position = 0x003F8AFF;
                streamUPK.WriteByte(0x53);
                streamUPK.Position = 0x003F8B00;
                streamUPK.WriteByte(0x38);
                streamUPK.Position = 0x003F8B01;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x003F8B02;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x003F8B03;
                streamUPK.WriteByte(0x04);
                streamUPK.Position = 0x003F8B04;
                streamUPK.WriteByte(0x47);
                */
                streamUPK.Position = 0x006924C7;
                streamUPK.WriteByte(0x27);

                streamUPK.Position = 0x007F9151;
                streamUPK.WriteByte(0x04);
                streamUPK.Position = 0x007F9152;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x007F9153;
                streamUPK.WriteByte(0xC6);
                streamUPK.Position = 0x007F9154;
                streamUPK.WriteByte(0x8B);
                streamUPK.Position = 0x007F9155;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x007F9156;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x007F9157;
                streamUPK.WriteByte(0x06);
                streamUPK.Position = 0x007F9158;
                streamUPK.WriteByte(0x44);
                streamUPK.Position = 0x007F9159;
                streamUPK.WriteByte(0x00);
                streamUPK.Position = 0x007F915A;
                streamUPK.WriteByte(0x04);
                streamUPK.Position = 0x007F915B;
                streamUPK.WriteByte(0x24);
                streamUPK.Position = 0x007F915C;
                streamUPK.WriteByte(0x00);
                streamUPK.Close();

                // -- HEX EDIT BORDERLANDS2.EXE --
                var streamBL2 = new FileStream(oBL2.FullName, FileMode.Open, FileAccess.ReadWrite);
                streamBL2.Position = 0x004F2590;
                streamBL2.WriteByte(0xFF);
                for (long i = 0x01B94B0C; i <= 0x01B94B10; i++)
                {
                    streamBL2.Position = i;
                    streamBL2.WriteByte(0x00);
                }
                streamBL2.Position = 0x01EF17F9; //find upk
                streamBL2.WriteByte(0x78); //willowgame.upk > xillowgame.upk
                streamBL2.Close();

                // -- CREATE SHORTCUT --
                WshShell shell = new WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(
    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\\Borderlands 2 COOP.lnk") as IWshRuntimeLibrary.IWshShortcut;
                shortcut.Arguments = "-log -debug -codermode -nosplash";
                shortcut.TargetPath = oBL2.FullName;
                shortcut.WindowStyle = 1;
                shortcut.Description = "Robeth's Borderlands 2 COOP patch";
                shortcut.WorkingDirectory = (Directory.GetParent(oBL2.FullName)).FullName;
                shortcut.IconLocation = (oBL2 + ",1");
                shortcut.Save();

                // -- ENABLE CONSOLE -- RIPPED STRAIGHT FROM BUGWORM's BORDERLANDS2PATCHER!!!!!
                try
                {
                    string tmppath = @"\\my games\\borderlands 2\\willowgame\\Config\\WillowInput.ini";
                    string iniPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + tmppath;
                    string[] temp = System.IO.File.ReadAllLines(path);
                    int i;
                    for (i = 1; i <= temp.Length; i++)
                    {
                        if (temp[i].StartsWith("ConsoleKey="))
                            break;
                    }
                    temp[i] = "ConsoleKey=~";
                    System.IO.File.WriteAllLines(path, temp);
                }
                catch
                {
                    //log
                }
                //END OF BUGWORM'S CODE

                // -- DONE --
                Popup.Show("Done! A Shortcut was placed on your desktop. Press '~' in game to open up console.");
            }
            else
            {
                Popup.Show("Borderlands2.exe not found.");
            }
        }
    }
}
