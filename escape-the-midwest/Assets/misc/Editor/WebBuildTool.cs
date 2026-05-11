using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Threading;
using UnityEditor.Build.Reporting;
using System.Security.Cryptography;

public class WebBuildTool : EditorWindow
{
    [MenuItem("EECS 298/CreateAndUploadBuild")]
    public static void ShowWindow()
    {
        GetWindow<WebBuildTool>("Upload Build");
    }

    private string buildPath = "web_build/";
    private string uniqname = "";
    private string game_name = "";
    private string game_description = "";
    private string authors = "";
    private string game_url = "";

    private string game_thumbnail_path = "<game_thumbnail>";
    private Texture2D game_thumbnail_image = null;

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("WebGL Build Tool\n\n", EditorStyles.boldLabel);

        PlayerSettings.WebGL.template = "PROJECT:BetterMinimal";

        /* Uniqname */
        GUILayout.Label("Uniqname :", EditorStyles.boldLabel);
        uniqname = EditorPrefs.GetString("uniqname", "");
        uniqname = EditorGUILayout.TextField(uniqname);
        uniqname = uniqname.Replace("/", "").Replace("\\", "").Replace("\"", "").Replace("'", "").Replace("\t", " ").Replace("`", "").Replace(" ", "").Trim().ToLowerInvariant();
        EditorPrefs.SetString("uniqname", uniqname);

        /* Game Name */
        GUILayout.Label("Game Name (do not change this often) :", EditorStyles.boldLabel);
        game_name = EditorPrefs.GetString("game_name", "");
        game_name = EditorGUILayout.TextField(game_name);
        game_name = game_name.Replace("/", "").Replace("\\", "").Replace("\"", "").Replace("'", "").Replace("\t"," ").Replace("`", "").Trim();
        EditorPrefs.SetString("game_name", game_name);

        PlayerSettings.productName = game_name;

        /* Game Description */
        GUILayout.Label("Game Description :", EditorStyles.boldLabel);
        game_description = EditorPrefs.GetString("game_description", "");
        game_description = EditorGUILayout.TextField(game_description);
        game_description = game_description.Replace("/", "").Replace("\\", "").Replace("\"", "").Replace("'", "").Replace("\t", " ").Replace("`", "").Trim();
        EditorPrefs.SetString("game_description", game_description);

        /* Custom webgl template variables */
        PlayerSettings.productName = game_name;
        string s = PlayerSettings.GetTemplateCustomValue("PRODUCT_DESCRIPTION"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("PRODUCT_DESCRIPTION", game_description);
        string ss = PlayerSettings.GetTemplateCustomValue("STUDENT_FULL_NAME"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("STUDENT_FULL_NAME", authors);

        /* Authors */
        GUILayout.Label("Author of the game :", EditorStyles.boldLabel);
        authors = EditorPrefs.GetString("authors", "");
        authors = EditorGUILayout.TextField(authors);
        authors = authors.Replace("/", "").Replace("\\", "").Replace("\"", "").Replace("'", "").Replace("\t", " ").Replace("`", "").Trim();
        EditorPrefs.SetString("authors", authors);

        /* Game Thumbnail Path */
        GUILayout.Label("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n", EditorStyles.boldLabel);
        game_thumbnail_path = EditorPrefs.GetString("game_thumbnail_path", "");
        if (!string.IsNullOrEmpty(game_thumbnail_path))
            GUILayout.Label("Game Thumbnail Path : " + game_thumbnail_path, EditorStyles.boldLabel);

        if (GUILayout.Button("Choose a new game thumbnail image"))
        {
            game_thumbnail_image = null;
            game_thumbnail_path = EditorUtility.OpenFilePanel("Select an image File (jpg, png)", "", "png,jpg,jpeg");

            if (!string.IsNullOrEmpty(game_thumbnail_path))
            {
                long file_size = new System.IO.FileInfo(game_thumbnail_path).Length;
                if (file_size > 20 * 1024 * 1024) // 20MB limit
                {
                    Debug.LogError("Image file is too large. Please choose a file smaller than 20 MB.");
                    game_thumbnail_path = null;
                }
            }
        }

        if (!string.IsNullOrEmpty(game_thumbnail_path))
            EditorPrefs.SetString("game_thumbnail_path", game_thumbnail_path);

        /* Game Thumbnail Image */
        if (!string.IsNullOrEmpty(game_thumbnail_path) && game_thumbnail_image == null)
        {
            game_thumbnail_image = new Texture2D(2, 2);
            game_thumbnail_image.LoadImage(System.IO.File.ReadAllBytes(game_thumbnail_path));
            game_thumbnail_image.Apply();
        }

        if (game_thumbnail_image != null)
        {
            // Draw the thumbnail image
            float aspect_ratio = (float)game_thumbnail_image.width / (float)game_thumbnail_image.height;
            float image_height = 200.0f;
            Rect rect = new Rect(10, 240, image_height * aspect_ratio, image_height);
            GUI.DrawTexture(rect, game_thumbnail_image);
        }

        /* Prepare for thumbnail upload later */
        byte[] thumbnail_bytes = System.IO.File.ReadAllBytes(game_thumbnail_path);
        string thumbnail_hash = ComputeHash(thumbnail_bytes);
        string thumbnail_extension = System.IO.Path.GetExtension(game_thumbnail_path);
        string thumbnail_hash_w_extension = thumbnail_hash + thumbnail_extension;

        /* Custom webgl template variables */
        PlayerSettings.productName = game_name;
        PlayerSettings.companyName = uniqname;

        string sss = PlayerSettings.GetTemplateCustomValue("PRODUCT_DESCRIPTION"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("PRODUCT_DESCRIPTION", game_description);
        string sssss = PlayerSettings.GetTemplateCustomValue("STUDENT_FULL_NAME"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("STUDENT_FULL_NAME", authors);

        string thumbnail_url = @"https://eecs298-art-website-public.s3.us-east-1.amazonaws.com/student_data/images/" + thumbnail_hash_w_extension;
        string misc_meta_tags = $"<meta name=\"twitter:card\" content=\"summary_large_image\"><meta name=\"twitter:title\" content=\"{game_name}, a UMich EECS 298 Game\"><meta name=\"twitter:description\" content=\"{game_description} Created by {authors} in Austin Yarger's EECS 298 : '3D Technical Art and Animation' course (eecs298.com).\"><meta name=\"twitter:image\" content=\"{thumbnail_url}\"> <meta property=\"og:title\" content=\"{game_name}, a UMich EECS 298 Game\"><meta property=\"og:type\" content=\"website\"><meta property=\"og:url\" content=\"{game_url}\"><meta property=\"og:image\" content=\"{thumbnail_url}\"><meta property=\"og:description\" content=\"{game_description} Created by {authors} in Austin Yarger's EECS 298 : '3D Technical Art and Animation' course (eecs298.com).\">";
        string sssssss = PlayerSettings.GetTemplateCustomValue("MISC_META_TAGS");
        PlayerSettings.SetTemplateCustomValue("MISC_META_TAGS", misc_meta_tags);
        GUILayout.Label("\n\n\n\n", EditorStyles.boldLabel);

#if UNITY_EDITOR_WIN
        /* Path length warning */
        // Windows MAX_PATH is 260, but leave some margin for subfolders
        const int SAFE_LIMIT = 75;
        int current_path_length = GetProjectPathLength();
        string warning = "";
        if(current_path_length > SAFE_LIMIT)
        {
            warning = "<color=red>- WARNING : Path too long! Build might fail!</color> Read this : https://bit.ly/4r4iiB4";
        }

        GUIStyle richStyle = new GUIStyle(EditorStyles.boldLabel);
        richStyle.richText = true;

        GUILayout.Label(
            $"Project path length: {current_path_length} {warning}",
            richStyle
        );
#endif


        if (!string.IsNullOrEmpty(game_name) && !string.IsNullOrEmpty(game_description) && !string.IsNullOrEmpty(authors) && !string.IsNullOrEmpty(uniqname) && !string.IsNullOrEmpty(game_thumbnail_path) && game_thumbnail_image != null)
        {
            if (GUILayout.Button("Create and Upload New Web Game Version (may take 20+ minutes)"))
            {
                BuildWebGL();
            }
        }

        if (EditorPrefs.HasKey("game_url"))
            game_url = EditorPrefs.GetString("game_url");

        if (!string.IsNullOrEmpty(game_url))
        {
            if (GUILayout.Button("Play Web Game"))
            {
                Application.OpenURL(game_url);
                EditorGUILayout.HelpBox("URL opened in your web browser. Please check your web browser to find the game. If it doesn't appear, copy the game URL to your clipboard using the button below.", MessageType.Info);
            }
            if (GUILayout.Button("Copy game URL to clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = game_url;
                EditorGUILayout.HelpBox("URL Copied! Put this into your web browser address bar.", MessageType.Info);
            }
                
            GUILayout.Label("Web Game URL : " + game_url);
        }

        GUILayout.EndVertical();
    }

    private int GetProjectPathLength()
    {
        // Unity project root
        string project_path = Application.dataPath;

        // Normalize and get full path
        string full_path = Path.GetFullPath(project_path);

        int path_length = full_path.Length;

        return path_length;
    }

    private void BuildWebGL()
    {
        if (string.IsNullOrEmpty(uniqname) || uniqname == @"your-uniqname-here")
        {
            Debug.LogError("Uniqname cannot be empty!");
            return;
        }

        /* Prepare for thumbnail upload later */
        byte[] thumbnail_bytes = System.IO.File.ReadAllBytes(game_thumbnail_path);
        string thumbnail_hash = ComputeHash(thumbnail_bytes);
        string thumbnail_extension = System.IO.Path.GetExtension(game_thumbnail_path);
        string thumbnail_hash_w_extension = thumbnail_hash + thumbnail_extension;

        /* Custom webgl template variables */
        PlayerSettings.productName = game_name;
        PlayerSettings.companyName = uniqname;
        string s = PlayerSettings.GetTemplateCustomValue("PRODUCT_DESCRIPTION"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("PRODUCT_DESCRIPTION", game_description);
        string ss = PlayerSettings.GetTemplateCustomValue("STUDENT_FULL_NAME"); // HACK needed to get these two template variable functions to work at all.
        PlayerSettings.SetTemplateCustomValue("STUDENT_FULL_NAME", authors);

        string thumbnail_url = @"https://eecs298-art-website-public.s3.us-east-1.amazonaws.com/student_data/images/" + thumbnail_hash_w_extension;
        string misc_meta_tags = $"<meta name=\"twitter:card\" content=\"summary_large_image\"><meta name=\"twitter:title\" content=\"{game_name}, a UMich EECS 298 Game\"><meta name=\"twitter:description\" content=\"{game_description} Created by {authors} in Austin Yarger's EECS 298 : '3D Technical Art and Animation' course (eecs298.com).\"><meta name=\"twitter:image\" content=\"{thumbnail_url}\"> <meta property=\"og:title\" content=\"{game_name}, a UMich EECS 298 Game\"><meta property=\"og:type\" content=\"website\"><meta property=\"og:url\" content=\"{game_url}\"><meta property=\"og:image\" content=\"{thumbnail_url}\"><meta property=\"og:description\" content=\"{game_description} Created by {authors} in Austin Yarger's EECS 298 : '3D Technical Art and Animation' course (eecs298.com).\">";
        string sss = PlayerSettings.GetTemplateCustomValue("MISC_META_TAGS");
        PlayerSettings.SetTemplateCustomValue("MISC_META_TAGS", misc_meta_tags);

        Thread.Sleep(2 * 1000);

        // Ensure the build path exists
        System.IO.Directory.CreateDirectory(buildPath);

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm; // ensure WASM target

        // Perform the build 
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
     
        if (report.summary.totalErrors > 0)
        {
            Debug.LogError(@"Build Error Report (" + report.summary.totalErrors + ") [" + report.SummarizeErrors() + "]");
            Debug.LogError("WebGL Build Failed!!!!!!!!!!! T_T");
        }
        else
        {
            /* Upload game build */
            Debug.Log("WebGL Build completed successfully!");
            Debug.Log("Uploading to S3...");

            /* Your actions generate multiple logs you will not find, and the local police will not be pleased. Don't even attempt anything malicious. */
            const string web_build_folder = @"web_build";
            const string bucket_url = @"https://eecs298-art-website-public.s3.amazonaws.com";
            string game_bucket_key = @"student_data/" + uniqname + "/" + game_name + "/builds/" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss");
            UploadFolderToS3(web_build_folder, bucket_url, game_bucket_key);

            Debug.Log("WebGL Build upload complete!");

            /* Upload game metadata.json file */
            Debug.Log("Uploading game metadata.json file S3...");
            string metadata_bucket_key = @"student_data/" + uniqname + "/" + game_name + "/metadata.json";
            GameMetadata metadatum = new GameMetadata()
            {
                game_name = game_name,
                game_authors = authors,
                game_description = game_description,
                game_web_url = game_url,
                game_thumbnail_hash_w_extension = thumbnail_hash_w_extension
            };
            string metadata_string = JsonConvert.SerializeObject(metadatum);
            UploadStringToS3(metadata_string, bucket_url, metadata_bucket_key);

            Debug.Log("Uploading game metadata.json file complete!");

            /* Upload thumbnail file */
            Debug.Log("Uploading game thumbnail image file...");
            string thumbnail_bucket_key = @"student_data/images/" + thumbnail_hash_w_extension;
            UploadFileToS3(game_thumbnail_path, bucket_url + "/" + thumbnail_bucket_key);

            Debug.Log("Uploading game thumbnail image file complete!");

            /* Launch game for playtesting */
            Thread.Sleep(10 * 1000);
            Debug.Log("Launching game for testing...");
            game_url = @"https://eecs298.com/" + game_bucket_key + "/index.html";
            EditorPrefs.SetString("game_url", game_url);
            Application.OpenURL(game_url);
            Debug.Log("Directing developer to web game url [" + game_url + "]");
        }
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }

        return enabledScenes.ToArray();
    }

    static void UploadFolderToS3(string localFolderPath, string bucketUrl, string bucketFolderPath)
    {
        if (!Directory.Exists(localFolderPath))
        {
            Debug.LogError($"Local folder path does not exist: {localFolderPath}");
            return;
        }

        string[] files = Directory.GetFiles(localFolderPath, "*.*", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {localFolderPath}");
            return;
        }

        Debug.Log($"Uploading {files.Length} files from {localFolderPath} to {bucketFolderPath} in bucket {bucketUrl}");

        foreach (string file in files)
        {
            // Get relative path for preserving folder structure in S3
            string relativePath = Path.GetRelativePath(localFolderPath, file).Replace("\\", "/");
            string s3Path = $"{bucketFolderPath}/{relativePath}";
            string uploadUrl = $"{bucketUrl}/{s3Path}";

            UploadFileToS3(file, uploadUrl);
        }
    }

    static void UploadStringToS3(string content, string bucket, string key)
    {
        if (string.IsNullOrEmpty(content)) {
            Debug.LogError("metadata payload was empty / null!");
            return;
        }

        string s3_path = bucket + "/" + key;
        UnityWebRequest request = new UnityWebRequest(s3_path, "PUT");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
        request.SetRequestHeader("Content-Type", "application/json");

        request.SendWebRequest().completed += (operation) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Uploaded metadata file successfully.");
            }
            else
            {
                Debug.LogError($"Failed to upload metadata file.");
                Debug.LogError($"Error: {request.error}");
            }
        };
    }

    private static void UploadFileToS3(string filePath, string uploadUrl)
    {
        Debug.Log($"Uploading file: {filePath} to {uploadUrl}");

        UnityWebRequest request = new UnityWebRequest(uploadUrl, "PUT");
        byte[] fileData = File.ReadAllBytes(filePath);
        request.uploadHandler = new UploadHandlerRaw(fileData);
        request.SetRequestHeader("Content-Type", GetContentType(filePath));

        /* Get encoding type */
        if (filePath.EndsWith(".br", StringComparison.OrdinalIgnoreCase))
        {
            request.SetRequestHeader("Content-Encoding", "br");
        }

        request.SendWebRequest().completed += (operation) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Uploaded file successfully: {filePath}");
            }
            else
            {
                Debug.LogError($"Failed to upload file: {filePath}");
                Debug.LogError($"Error: {request.error}");
            }
        };
    }

    private static string GetContentType(string filePath)
    {
        if (filePath.Contains(".wasm"))
            return @"application/wasm";
        else if (filePath.Contains(".js"))
            return @"application/javascript";
        else if (filePath.Contains(".html"))
            return @"text/html";
        else if (filePath.Contains(".css"))
            return @"text/css";
        else if (filePath.Contains(".png"))
            return @"image/png";
        else if (filePath.Contains(".json"))
            return @"application/json";
        else if (filePath.Contains(".jpg") || filePath.Contains(".jpeg"))
            return @"image/jpeg";
        else if (filePath.Contains(".gif"))
            return @"image/gif";
        else if (filePath.Contains(".svg"))
            return @"image/svg+xml";
        else if (filePath.Contains(".ico"))
            return @"image/x-icon";
        else if (filePath.Contains(".txt"))
            return @"text/plain";
        else if (filePath.Contains(".xml"))
            return @"application/xml";
        else if (filePath.Contains(".mp4"))
            return @"video/mp4";
        else if (filePath.Contains(".mp3"))
            return @"audio/mpeg";
        else if (filePath.Contains(".ogg"))
            return @"audio/ogg";
        else if (filePath.Contains(".wav"))
            return @"audio/wav";
        else
            return @"application/octet-stream";
    }

    [System.Serializable]
    public class PreSignedResponse
    {
        public int statusCode { get; set; }
        public Dictionary<string, string> urls { get; set; }
        public string error { get; set; }
    }

    [System.Serializable]
    public class Payload
    {
        public string folder;
        public List<string> files;
    }

    public string ComputeHash(byte[] imageBytes)
    {
        // Create a SHA-256 hash object
        using (var sha256 = SHA256.Create())
        {
            // Compute the hash
            var hashBytes = sha256.ComputeHash(imageBytes);
            // Convert the hash to a hexadecimal string
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}

[System.Serializable]
public class GameMetadata
{
    public string game_name;
    public string game_description;
    public string game_authors;
    public string game_thumbnail_hash_w_extension;
    public string game_web_url;
}