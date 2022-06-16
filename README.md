# MassBanTool
[![Build_MP](https://github.com/SFFan123/MassBanTool/actions/workflows/Build_MP.yml/badge.svg)](https://github.com/SFFan123/MassBanTool/actions/workflows/Build_MP.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/SFFan123/MassBanTool/blob/master/LICENSE)


this is a side project.

Code is probably ugly, but it's 'working' so.

## Important Notice
Currently rewriting the tool on .net (core) 6 so it can run on Windows and Linux. This is still in Progress and the App on .net 6 is not working yet.
The old version ist still in this Repo (/MassBanTool_NET)

## Info
Using a delay of under 1500 ms between messages will negatively impact your ability to write in other channels which you do not have moderation privileges.
(over other clients like webbrowser, Chatterino 2, ...).


## Safety Measures
 - When joining your channel the tool will fetch the modlist and checks that the mods can't be target of a command.
 - Readfile has a list of allowed commands, only commands that in that list will be executed by the tool, so if someone sneaks in a /mod \<user>.
 - The Moderator rate limit of Twitch of 100 Messages in 30 secs are hard coded in the twitch client. - The tool alone can not get you a global Timeout.
 - The tool does not allow execution in a channel for which you do not have moderation privileges.
 

## Build it yourself

1. Get Visual Studio 2019/2022 (Community) -> https://visualstudio.microsoft.com/de/free-developer-offers/
2. Install Workflow Desktop C#
   - Additional Features SDK for .Net Framework 4.8
3. Start Visual Studio
4. Clone the Git in Visual studio
5. Open the solution file *.sln
6. Choose your target build (Debug or Release)
   - Release is designed to run alone, outside VS.
   - Debug is build to allow diagnostics tools of VS or other debugger to hook into the app to diagnose bugs.
7. Right-click on the the MassBanTool_NET Project and choose Build. 
   - this step might need to be done multiple times because vs is fetching the nugets in the background.

You know have an exe in the specified output dir, the console of VS tells you where it is.


<hr />

## Special thanks
Sileniful
