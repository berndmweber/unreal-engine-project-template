/* Copyright (c) 2020, nVisionary, Inc. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of this project nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without
 *   specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using UnrealBuildTool;
using System.Collections.Generic;
using System.IO;
using System;
using System.ComponentModel;
using System.Diagnostics;

public class your_project_name_here : ModuleRules
{
	public your_project_name_here(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "HeadMountedDisplay" });

        VersionBuild(GetGitRepo());
    }

    private GitRepoData GetGitRepo()
    {
        string RepoPath = @"C:\Unreal Projects\your_project_name_here\";

        return GitRepoData.GetRepoHandle(RepoPath);
    }

    private void VersionBuild(GitRepoData gitRepo)
    {
        string VERSION_HEADER_FILE = @"C:\Unreal Projects\your_project_name_here\Source\your_project_name_here\GameVersion.h";
        string UNREAL_DEFAULT_GAME_FILE = @"C:\Unreal Projects\your_project_name_here\Config\DefaultGame.ini";
        string UNREAL_DEFAULT_ENGINE_FILE = @"C:\Unreal Projects\your_project_name_here\Config\DefaultEngine.ini";

        const string GAME_VERSION_MAJOR_TEXT = @"#define GAME_VERSION_MAJOR";
        const string GAME_VERSION_MINOR_TEXT = @"#define GAME_VERSION_MINOR";
        const string GAME_VERSION_PATCH_TEXT = @"#define GAME_VERSION_PATCH";
        const string GAME_VERSION_BUILD_TEXT = @"#define GAME_VERSION_BUILD";
        const string GAME_VERSION_USES_GIT_TEXT = @"#define GAME_VERSION_USES_GIT";
        const string GAME_VERSION_GIT_SHA_TEXT = @"#define GAME_VERSION_GIT_SHA";
        const string GAME_VERSION_GIT_BRANCH_TEXT = @"#define GAME_VERSION_GIT_BRANCH";
        const string UNREAL_PROJECT_VERSION_TEXT = @"ProjectVersion=";
        const string UNREAL_VERSION_DISPLAY_NAME_TEXT = @"VersionDisplayName=";

        int GVBTPos = GAME_VERSION_BUILD_TEXT.Length + 1;
        int GVGSTPos = GAME_VERSION_GIT_SHA_TEXT.Length + 1;
        int GVGBTPos = GAME_VERSION_GIT_BRANCH_TEXT.Length + 1;
        int GVUGTPos = GAME_VERSION_USES_GIT_TEXT.Length + 1;
        int UPVTPos = UNREAL_PROJECT_VERSION_TEXT.Length;

        System.Console.WriteLine("Modifying: " + VERSION_HEADER_FILE + ", " + UNREAL_DEFAULT_ENGINE_FILE + " and " + UNREAL_DEFAULT_GAME_FILE + ".");

        if (File.Exists(VERSION_HEADER_FILE))
        {
            FileInfo VHFInfo = new FileInfo(VERSION_HEADER_FILE);
            FileInfo UDGFInfo = new FileInfo(UNREAL_DEFAULT_GAME_FILE);
            FileInfo UDEFInfo = new FileInfo(UNREAL_DEFAULT_ENGINE_FILE);
            VHFInfo.IsReadOnly = false;
            UDGFInfo.IsReadOnly = false;
            UDEFInfo.IsReadOnly = false;

            var VHFLines = new List<string>(File.ReadAllLines(VERSION_HEADER_FILE));
            int totalLines = VHFLines.Count;
            int majorNumber = 0;
            int minorNumber = 0;
            int patchNumber = 0;
            int buildNumber = 0;
            int usesGit = 0;

            using (var sr = new StreamReader(VERSION_HEADER_FILE))
            {
                for (int curLine = 0; curLine < totalLines; curLine++)
                {
                    string curLineStr = sr.ReadLine();
                    if (curLineStr.Contains(GAME_VERSION_MAJOR_TEXT))
                    {
                        majorNumber = int.Parse(curLineStr.Substring(GVBTPos));
                        continue;
                    }
                    if (curLineStr.Contains(GAME_VERSION_MINOR_TEXT))
                    {
                        minorNumber = int.Parse(curLineStr.Substring(GVBTPos));
                        continue;
                    }
                    if (curLineStr.Contains(GAME_VERSION_PATCH_TEXT))
                    {
                        patchNumber = int.Parse(curLineStr.Substring(GVBTPos));
                        continue;
                    }
                    if (curLineStr.Contains(GAME_VERSION_BUILD_TEXT))
                    {
                        buildNumber = int.Parse(curLineStr.Substring(GVBTPos)) + 1;
                        System.Console.WriteLine("New Build number generated: " + buildNumber.ToString());
                        VHFLines.RemoveAt(curLine);
                        VHFLines.Insert(curLine, (GAME_VERSION_BUILD_TEXT + " " + buildNumber));

                        var USFLines = new List<string>(File.ReadAllLines(UNREAL_DEFAULT_GAME_FILE));
                        int totalLinesu = USFLines.Count;
                        bool foundVersion = false;
                        using (var sru = new StreamReader(UNREAL_DEFAULT_GAME_FILE))
                        {
                            for (int curLineu = 0; curLineu < totalLinesu; curLineu++)
                            {
                                string curLineStru = sru.ReadLine();
                                if (curLineStru.Contains(UNREAL_PROJECT_VERSION_TEXT))
                                {
                                    System.Console.WriteLine("Modifying " + UNREAL_DEFAULT_GAME_FILE);
                                    foundVersion = true;
                                    sru.Close();
                                    USFLines.RemoveAt(curLineu);
                                    USFLines.Insert(curLineu, (UNREAL_PROJECT_VERSION_TEXT + majorNumber + "." + minorNumber + "." + patchNumber + "." + buildNumber));
                                    File.WriteAllLines(UNREAL_DEFAULT_GAME_FILE, USFLines);
                                    break;
                                }
                            }
                            if (!foundVersion)
                            {
                                sru.Close();
                                USFLines.Add((UNREAL_PROJECT_VERSION_TEXT + majorNumber + "." + minorNumber + "." + patchNumber + "." + buildNumber));
                                File.WriteAllLines(UNREAL_DEFAULT_GAME_FILE, USFLines);
                            }
                        }
                        var UEFLines = new List<string>(File.ReadAllLines(UNREAL_DEFAULT_ENGINE_FILE));
                        totalLinesu = UEFLines.Count;
                        foundVersion = false;
                        using (var sru = new StreamReader(UNREAL_DEFAULT_ENGINE_FILE))
                        {
                            for (int curLineu = 0; curLineu < totalLinesu; curLineu++)
                            {
                                string curLineStru = sru.ReadLine();
                                if (curLineStru.Contains(UNREAL_VERSION_DISPLAY_NAME_TEXT))
                                {
                                    System.Console.WriteLine("Modifying " + UNREAL_DEFAULT_ENGINE_FILE);
                                    foundVersion = true;
                                    sru.Close();
                                    UEFLines.RemoveAt(curLineu);
                                    UEFLines.Insert(curLineu, (UNREAL_VERSION_DISPLAY_NAME_TEXT + majorNumber + "." + minorNumber + "." + patchNumber + "." + buildNumber));
                                    File.WriteAllLines(UNREAL_DEFAULT_ENGINE_FILE, UEFLines);
                                    break;
                                }
                            }
                            if (!foundVersion)
                            {
                                sru.Close();
                                // Let's not do this since it needs to be in the right category
                                //UEFLines.Add((UNREAL_VERSION_DISPLAY_NAME_TEXT + majorNumber + "." + minorNumber + "." + patchNumber + "." + buildNumber));
                                //File.WriteAllLines(UNREAL_DEFAULT_ENGINE_FILE, UEFLines);
                            }
                        }
                    }
                    if (curLineStr.Contains(GAME_VERSION_USES_GIT_TEXT))
                    {
                        usesGit = int.Parse((curLineStr.Substring(GVUGTPos)));
                        if (usesGit == 0)
                        {
                            if (gitRepo != null)
                            {
                                usesGit = 1;
                                System.Console.WriteLine("System uses Git now.");
                                VHFLines.RemoveAt(curLine);
                                VHFLines.Insert(curLine, (GAME_VERSION_USES_GIT_TEXT + " 1"));
                            }
                            else
                            {
                                System.Console.WriteLine("No Git used.");
                                sr.Close();
                                break;
                            }
                        }
                    }
                    if (curLineStr.Contains(GAME_VERSION_GIT_SHA_TEXT))
                    {
                        string sha = curLineStr.Substring(GVGSTPos);
                        /* Need to cut off the " */
                        sha = sha.Substring(1, sha.Length - 2);
                        if (usesGit == 1)
                        {
                            string gitSHA = gitRepo.CurrentSha;
                            if (sha != gitSHA)
                            {
                                System.Console.WriteLine("New Git Sha found : " + gitSHA);
                                VHFLines.RemoveAt(curLine);
                                VHFLines.Insert(curLine, (GAME_VERSION_GIT_SHA_TEXT + " \"" + gitSHA + "\""));
                            }
                        }
                    }
                    if (curLineStr.Contains(GAME_VERSION_GIT_BRANCH_TEXT))
                    {
                        string branch = curLineStr.Substring(GVGBTPos);
                        /* Need to cut off the " */
                        branch = branch.Substring(1, branch.Length - 2);
                        if (usesGit == 1)
                        {
                            string gitBranch = gitRepo.BranchName;
                            if (branch != gitBranch)
                            {
                                System.Console.WriteLine("New Git branch found : " + gitBranch);
                                VHFLines.RemoveAt(curLine);
                                VHFLines.Insert(curLine, (GAME_VERSION_GIT_BRANCH_TEXT + " \"" + gitBranch + "\""));
                            }
                        }

                        /* This is the last entry we're looking for */
                        sr.Close();
                        break;
                    }
                }
            }
            File.WriteAllLines(VERSION_HEADER_FILE, VHFLines);
        }
        else
        {
            throw new BuildException("Failed to get " + VERSION_HEADER_FILE + ", " + UNREAL_DEFAULT_ENGINE_FILE + " or " + UNREAL_PROJECT_VERSION_TEXT + ". Make sure they exist!");
        }
        return;
    }
}

class GitRepoData : IDisposable
{
    public static GitRepoData GetRepoHandle(string path)
    {
        if ((path != null) && Directory.Exists(path))
        {
            var gitRepoData = new GitRepoData(path);
            if (gitRepoData.IsGitRepository)
            {
                return gitRepoData;
            }
        }
        return null;
    }

    public string CurrentSha
    {
        get
        {
            return RunCommand("rev-parse --short=7 HEAD");
        }
    }

    public string BranchName
    {
        get
        {
            return RunCommand("rev-parse --abbrev-ref HEAD");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _gitProcess.Dispose();
        }
    }

    private bool _disposed;
    private readonly Process _gitProcess;

    private GitRepoData(string path)
    {
        var processInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            FileName = "git.exe",
            CreateNoWindow = true,
            WorkingDirectory = path
        };

        _gitProcess = new Process();
        _gitProcess.StartInfo = processInfo;
    }

    private bool IsGitRepository
    {
        get
        {
            return !String.IsNullOrWhiteSpace(RunCommand("log -1"));
        }
    }

    private string RunCommand(string args)
    {
        _gitProcess.StartInfo.Arguments = args;
        _gitProcess.Start();
        string output = _gitProcess.StandardOutput.ReadToEnd().Trim();
        _gitProcess.WaitForExit();
        return output;
    }
}
