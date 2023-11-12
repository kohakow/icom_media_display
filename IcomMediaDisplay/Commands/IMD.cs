﻿using RemoteAdmin;
using CommandSystem;
using System;
using IcomMediaDisplay.Logic;
using IcomMediaDisplay.Helpers;
using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net;
using Exiled.Permissions.Extensions;

namespace IcomMediaDisplay.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class IMD : ICommand
    {
        public string Command => "icommediadisplay";
        public string[] Aliases => new[] { "imd" };
        public string Description => "Play a Media on Intercom.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("imd.command"))
            {
                response = "You do not have permission to use this command!";
                return false;
            }

            PlaybackHandler playbackHandler = new PlaybackHandler();
            Converters converters = new Converters();
            switch (arguments.At(0))
            {
                case "play":
                    if (arguments.Count < 2)
                    {
                        response = "Not enough arguments for 'play' command.";
                        return false;
                    }
                    string id = arguments.At(1);
                    string path = IcomMediaDisplay.tempdir + "/" + id;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        response = "Invalid path for playback.";
                        return false;
                    }
                    try
                    {
                        playbackHandler.PlayFrames(path);
                        response = "Playback started (Keep an eye on Server console).";
                        return false;
                    }
                    catch (Exception ex)
                    {
                        response = "Failed to start playback. Error: " + ex.Message;
                        return false;
                    }

                case "get":
                    if (arguments.Count < 3)
                    {
                        response = "Not enough arguments for 'get' command.";
                        return false;
                    }
                    string name = arguments.At(1);
                    string url = arguments.At(2);
                    string dest = IcomMediaDisplay.GetFolder + "/" + name;
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                    {
                        response = "Invalid name or URL for file download.";
                        return false;
                    }
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(url, dest);
                            response = "File downloaded successfully.";
                            return false;
                        }
                    }
                    catch (WebException e)
                    {
                        response = "Failed to download the file. Error: " + e.Message;
                        return false;
                    }
                case "break":
                    playbackHandler.BreakFromPlayback();
                    response = "Stopped playback.";
                    return false;
                case "prep":
                    string f_id = arguments.At(1);
                    string video = arguments.At(2);
                    converters.ExportFrames(f_id, video);
                    response = "Preparing video, exporting frames. (Keep an eye on Server console)";
                    return false;
                case "help":
                    response = "--- Subcommands ---\r\nimd play <folderID> - Plays frames from an directory.\r\nimd get <URL> <Filename> - Grabs an file and downloads it to \"get\" directory.\r\nimd break - Abort Playback.\r\nimd prep <folderID> <FilenameFromGetDir> - Automatically exports frames for you, requires ffmpeg.\r\nimd help - This.";
                    return false;
                default:
                    response = "Unknown subcommand. Use <imd help> for syntax.";
                    return false;
            }

        }
    }
}
