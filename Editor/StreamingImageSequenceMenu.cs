using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.StreamingImageSequence;
using Debug = UnityEngine.Debug;

namespace UnityEditor.StreamingImageSequence {

    internal static class StreamingImageSequenceMenu
    {
        private const string PNG_EXTENSION = "png";
        private const string TGA_EXTENSION = "tga";

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH +  "Create Clip", false, 1)]
        private static void RegisterFilesAndCreateStreamingImageSequence()
        {
            string path = EditorUtility.OpenFilePanel("Open File", "", PNG_EXTENSION + "," + TGA_EXTENSION);
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, path, null);
        }

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH +  "TestMD5", false, 1)]
        private static void TestMD5() {
            string path = "";
            
            Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
// the code that you want to measure comes here
            // assuming you want to include nested folders
            string[] files = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

            MD5 md5 = MD5.Create();

            long totalLength = 0;
            for(int i = 0; i < files.Length; i++)
            {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes    = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                // byte[] contentBytes = File.ReadAllBytes(file);
                long length = new System.IO.FileInfo(file).Length;
                byte[] contentBytes = BitConverter.GetBytes(length);

                if (i == files.Length - 1)
                     md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                     md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                
            }

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;            
            Debug.Log("FileCount: " + files.Length 
                + " Hash: " + BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant() 
                + " Elapsed Time" + elapsedMs 
                + "TotalLength: " + totalLength );
        }
        
//----------------------------------------------------------------------------------------------------------------------

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Import AE Timeline", false, 10)]
        private static void ImportAETimeline() {
            string strPath = EditorUtility.OpenFilePanel("Open File", "", "jstimeline");
            if (strPath.Length != 0) {
                JstimelineImporter.ImportTimeline(strPath);
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Reset",false,50)]
        private static void Reset()
        {
            EditorUpdateManager.ResetImageLoading();
            PreviewTextureFactory.Reset();            
        }


//----------------------------------------------------------------------------------------------------------------------
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Loaded Images",false,52)]
        private static void ShowLoadedImages() {
            StringBuilder sb = new StringBuilder();

            for (int imageType = 0; imageType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++imageType) {
                sb.AppendLine("IMAGE_TYPE: " + imageType.ToString());

                List<string> loadedTextures = new List<string>();
                StreamingImageSequencePlugin.ListLoadedImages(imageType, (fileName) => {
                    loadedTextures.Add(fileName);
                });

                foreach (var fileName in loadedTextures) {
                    ImageLoader.GetImageDataInto(fileName,imageType, out ImageData readResult);
                    sb.Append("    ");
                    sb.Append(fileName);
                    sb.Append(". Status: " + readResult.ReadStatus);
                    sb.Append(", Size: (" + readResult.Width + ", " + readResult.Height);
                    sb.AppendLine(") ");
                }

                sb.AppendLine("----------------------------------------------------------------");
                sb.AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine("Preview Textures: ");
            IDictionary<string, PreviewTexture> previewTextures = PreviewTextureFactory.GetPreviewTextures();
            foreach (var kvp in previewTextures) {
                sb.Append("    ");
                sb.AppendLine(kvp.Key);
            }
            
            Debug.Log(sb.ToString());
        }

        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Used Image Memory",false,53)]
        private static void ShowUsedImageMemory() {
            Debug.Log($"Used memory for images: {StreamingImageSequencePlugin.GetUsedImagesMemory().ToString()} MB");
        }
        
//----------------------------------------------------------------------------------------------------------------------
        [MenuItem(StreamingImageSequenceConstants.MENU_PATH + "Debug/Show Image Load Order",false,54)]
        private static void ShowImageLoadOrder() {
            StringBuilder sb = new StringBuilder();

            for (int imageType = 0; imageType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++imageType) {
                int latestRequestFrame = StreamingImageSequencePlugin.GetImageLoadOrder(imageType);
                sb.AppendLine($"IMAGE_TYPE: {imageType.ToString()}, order: {latestRequestFrame}");
                sb.AppendLine();
            }
            sb.AppendLine("Current Frame: " + ImageLoader.GetCurrentFrame());
            Debug.Log(sb.ToString());
        }
//----------------------------------------------------------------------------------------------------------------------

    }


}