using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp6
{
    public partial class Form1 : Form
    {
        private string currentPrompt = "C:";
        private int lastPromptPosition = 0;

        private List<GitHubRelease> releases = new List<GitHubRelease>();
        private int currentReleaseIndex = -1;
        private bool isDownloading = false;

        private bool isCheckingForSelfUpdate = false;
        private List<GitHubRelease> selfUpdateReleases = new List<GitHubRelease>();

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        public Form1()
        {
            InitializeComponent();
            ApplyDarkTitleBar();
        }

        private void ApplyDarkTitleBar()
        {
            try
            {
                int attribute = 20;
                int useDark = 1;
                DwmSetWindowAttribute(this.Handle, attribute, ref useDark, sizeof(int));
            }
            catch
            {
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await CheckForSelfUpdatesAsync();
        }

        private async Task CheckForSelfUpdatesAsync()
        {
            txtTerminal.AppendText("Checking for updates...");

            string owner = "pi-40";
            string repo = "gio-git-github-repo-fetcher";
            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) WinFormsTerminal");
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var rawReleases = JsonSerializer.Deserialize<List<GitHubRelease>>(responseJson, options);

                        if (rawReleases != null && rawReleases.Count > 0 && rawReleases[0].assets != null && rawReleases[0].assets.Count > 0)
                        {
                            selfUpdateReleases = rawReleases;
                            isCheckingForSelfUpdate = true;

                            txtTerminal.AppendText(Environment.NewLine + $"New updates found in release: {rawReleases[0].name ?? rawReleases[0].tag_name}");
                            txtTerminal.AppendText(Environment.NewLine + "Download updates? (y/n): ");
                            txtTerminal.SelectionStart = txtTerminal.Text.Length;
                            lastPromptPosition = txtTerminal.Text.Length;
                            return;
                        }
                    }
                }
            }
            catch
            {
            }

            txtTerminal.Text = "";
            PrintPrompt();
        }

        private async Task HandleSelfUpdateSelectionAsync(string choice)
        {
            isCheckingForSelfUpdate = false;

            if (choice.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var latestRelease = selfUpdateReleases[0];
                txtTerminal.AppendText(Environment.NewLine + "Starting updates...");

                isDownloading = true;
                foreach (var asset in latestRelease.assets)
                {
                    await DownloadAssetAsync(asset);
                }
                isDownloading = false;

                txtTerminal.AppendText(Environment.NewLine + "All updates finished.");
            }

            txtTerminal.AppendText(Environment.NewLine);
            PrintPrompt();
        }

        private void PrintPrompt()
        {
            txtTerminal.AppendText(Environment.NewLine + currentPrompt);
            txtTerminal.SelectionStart = txtTerminal.Text.Length;
            lastPromptPosition = txtTerminal.Text.Length;
        }

        private async void txtTerminal_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDownloading)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Back && txtTerminal.SelectionStart <= lastPromptPosition)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if (isCheckingForSelfUpdate)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    string totalText = txtTerminal.Text;
                    if (totalText.Length < lastPromptPosition) return;
                    string choice = totalText.Substring(lastPromptPosition).Trim();

                    if (choice.Equals("y", StringComparison.OrdinalIgnoreCase) || choice.Equals("n", StringComparison.OrdinalIgnoreCase))
                    {
                        await HandleSelfUpdateSelectionAsync(choice);
                    }
                    else
                    {
                        txtTerminal.AppendText(Environment.NewLine + "Please enter 'y' or 'n': ");
                        txtTerminal.SelectionStart = txtTerminal.Text.Length;
                        lastPromptPosition = txtTerminal.Text.Length;
                    }
                }
                return;
            }

            if (e.KeyCode == Keys.Down && releases.Count > 0)
            {
                e.SuppressKeyPress = true;
                CycleReleases();
                return;
            }

            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z && releases.Count > 0 && currentReleaseIndex >= 0)
            {
                int index = e.KeyCode - Keys.A;
                var currentRelease = releases[currentReleaseIndex];

                if (index < currentRelease.assets.Count)
                {
                    e.SuppressKeyPress = true;
                    var selectedAsset = currentRelease.assets[index];

                    isDownloading = true;
                    await DownloadAssetAsync(selectedAsset);
                    isDownloading = false;

                    PrintPrompt();
                }
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string totalText = txtTerminal.Text;
                if (totalText.Length < lastPromptPosition) return;

                string commandLine = totalText.Substring(lastPromptPosition).Trim();

                if (!string.IsNullOrEmpty(commandLine))
                {
                    await ProcessCommand(commandLine);
                }
                else
                {
                    PrintPrompt();
                }
            }
        }

        private async Task ProcessCommand(string input)
        {
            txtTerminal.AppendText(Environment.NewLine);

            if (input.StartsWith("gio-git ", StringComparison.OrdinalIgnoreCase))
            {
                string url = input.Substring(8).Trim();
                await FetchGitHubReleases(url);
            }
            else
            {
                txtTerminal.AppendText($"Unknown command: {input}");
                PrintPrompt();
            }
        }

        private async Task FetchGitHubReleases(string url)
        {
            string cleanUrl = url.Replace("[", "").Replace("]", "")
                                 .Replace("https://github.com/", "")
                                 .Replace("http://github.com/", "")
                                 .Trim('/');

            string[] parts = cleanUrl.Split('/');

            if (parts.Length < 2)
            {
                txtTerminal.AppendText("Error: Invalid GitHub repository link format.");
                PrintPrompt();
                return;
            }

            string owner = parts[0].Trim();
            string repo = parts[1].Trim();
            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";

            txtTerminal.AppendText("Fetching release info from GitHub...");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) WinFormsTerminal");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        txtTerminal.AppendText(Environment.NewLine + $"Error 404: Repository '{owner}/{repo}' not found.");
                        PrintPrompt();
                        return;
                    }

                    response.EnsureSuccessStatusCode();
                    string responseJson = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var rawReleases = JsonSerializer.Deserialize<List<GitHubRelease>>(responseJson, options);

                    if (rawReleases != null && rawReleases.Count > 0)
                    {
                        releases = rawReleases;

                        foreach (var rel in releases)
                        {
                            if (rel.assets != null)
                            {
                                rel.assets = rel.assets.OrderBy(a => a.name).ToList();
                            }
                        }

                        currentReleaseIndex = -1;
                        txtTerminal.AppendText(Environment.NewLine + $"Found {releases.Count} releases. Press [Down Arrow] to view them.");
                    }
                    else
                    {
                        txtTerminal.AppendText(Environment.NewLine + "No public releases found for this repository.");
                    }
                }
            }
            catch (Exception ex)
            {
                txtTerminal.AppendText(Environment.NewLine + $"Network error: {ex.Message}");
            }

            PrintPrompt();
        }

        private void CycleReleases()
        {
            currentReleaseIndex++;
            if (currentReleaseIndex >= releases.Count)
            {
                currentReleaseIndex = 0;
            }

            var currentRelease = releases[currentReleaseIndex];

            txtTerminal.AppendText(Environment.NewLine + $"--- Release: {currentRelease.name ?? currentRelease.tag_name} ---");

            if (currentRelease.assets == null || currentRelease.assets.Count == 0)
            {
                txtTerminal.AppendText(Environment.NewLine + " [No download packages in this release]");
                return;
            }

            txtTerminal.AppendText(Environment.NewLine + "Available Packages (Press key to select):");
            for (int i = 0; i < currentRelease.assets.Count; i++)
            {
                char hotkey = (char)('A' + i);
                txtTerminal.AppendText(Environment.NewLine + $" [{hotkey}] -> {currentRelease.assets[i].name}");
            }
        }

        private async Task DownloadAssetAsync(GitHubAsset asset)
        {
            txtTerminal.AppendText(Environment.NewLine + $"Downloading: {asset.name}...");

            string destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, asset.name);

            txtTerminal.AppendText(Environment.NewLine);
            int progressLineStart = txtTerminal.Text.Length;
            txtTerminal.AppendText("[░░░░░░░░░░░░░░░░░░░░] 0%");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) WinFormsTerminal");

                    using (HttpResponseMessage response = await client.GetAsync(asset.browser_download_url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        long? totalBytes = response.Content.Headers.ContentLength;

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                      fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var buffer = new byte[8192];
                            long totalReadBytes = 0;
                            int readBytes;
                            int lastPercentage = -1;

                            while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, readBytes);
                                totalReadBytes += readBytes;

                                if (totalBytes.HasValue)
                                {
                                    int percentage = (int)((double)totalReadBytes / totalBytes.Value * 100);

                                    if (percentage != lastPercentage)
                                    {
                                        lastPercentage = percentage;
                                        UpdateConsoleProgressBar(progressLineStart, percentage);
                                    }
                                }
                            }
                        }
                    }
                }
                txtTerminal.AppendText(Environment.NewLine + $"Success! Saved directly to: {destinationPath}");
            }
            catch (Exception ex)
            {
                txtTerminal.AppendText(Environment.NewLine + $"Download Failed: {ex.Message}");
            }
        }

        private void UpdateConsoleProgressBar(int startPosition, int percentage)
        {
            int totalBlocks = 20;
            int filledBlocks = percentage / (100 / totalBlocks);
            int emptyBlocks = totalBlocks - filledBlocks;

            string filled = new string('█', filledBlocks);
            string empty = new string('░', emptyBlocks);
            string progressBarText = $"[{filled}{empty}] {percentage}%";

            txtTerminal.Select(startPosition, txtTerminal.Text.Length - startPosition);
            txtTerminal.SelectedText = progressBarText;
        }
    }

    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public List<GitHubAsset> assets { get; set; }
    }

    public class GitHubAsset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
    }
}
