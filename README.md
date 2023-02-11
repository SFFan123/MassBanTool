# MassBanTool
[![Build_MP](https://github.com/SFFan123/MassBanTool/actions/workflows/Build_MP.yml/badge.svg)](https://github.com/SFFan123/MassBanTool/actions/workflows/Build_MP.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/SFFan123/MassBanTool/blob/master/LICENSE)


this is a side project.

Code is probably ugly, but it's 'working' so.

## Important Notice
The .NET Framework Version is now removed.

## Safety Measures
 - Readfile has a list of allowed commands, only commands that in that list will be executed by the tool, so if someone sneaks in a /mod \<user>.
 - The Moderator rate limit of Twitch of 100 Requests in 30 secs are hard coded in the twitch client.
 

## Build it yourself

1. Get Visual Studio 2019/2022 (Community) -> https://visualstudio.microsoft.com/de/free-developer-offers/
2. Install Workflow Desktop C#
   - Additional Features SDK for .dotnet 6
3. Start Visual Studio
4. Clone the Git in Visual studio
5. Open the solution file *.sln
6. Choose your target build (Debug or Release)
   - Release is designed to run alone, outside VS.
   - Debug is build to allow diagnostics tools of VS or other debugger to hook into the app to diagnose bugs.
7. Right-click on the the MassBanTool Project and choose Build. 
   - this step might need to be done multiple times because vs is fetching the nugets in the background.

You know have an exe in the specified output dir, the console of VS tells you where it is.


<hr />

## Special thanks
Sileniful
