// Copyright (c) 2025 Cocolinux0101, adapted and revised from gsplat-unity

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Gsplat.Editor
{
    [ScriptedImporter(1, "ply")]
    public class GsplatImporter : ScriptedImporter
    {
        static string ReadLine(FileStream fs)
        {
            List<byte> byteBuffer = new List<byte>();
            while (true)
            {
                int b = fs.ReadByte();
                if (b == -1 || b == '\n') break;
                byteBuffer.Add((byte)b);
            }
            if (byteBuffer.Count > 0 && byteBuffer.Last() == '\r')
                byteBuffer.RemoveAt(byteBuffer.Count - 1);
            return Encoding.UTF8.GetString(byteBuffer.ToArray());
        }

        public static void ReadPlyHeader(FileStream fs, out uint vertexCount, out List<string> propertyNames)
        {
            vertexCount = 0;
            propertyNames = new List<string>();
            string line;
            while ((line = ReadLine(fs)) != null && line != "end_header")
            {
                string[] tokens = line.Split(' ');
                if (tokens.Length == 3 && tokens[0] == "element" && tokens[1] == "vertex")
                    vertexCount = uint.Parse(tokens[2]);
                else if (tokens.Length == 3 && tokens[0] == "property")
                    propertyNames.Add(tokens[2]);
            }
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var gsplatAsset = ScriptableObject.CreateInstance<GsplatAsset>();
            var bounds = new Bounds();

            using (var fs = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= 2L * 1024 * 1024 * 1024)
                {
                    if (GsplatSettings.Instance.ShowImportErrors)
                        Debug.LogError($"{ctx.assetPath} import error: files >2GB not supported");
                    return;
                }

                ReadPlyHeader(fs, out var vertexCount, out var propertyNames);

                // Map core properties dynamically
                int idxX = propertyNames.IndexOf("x");
                int idxY = propertyNames.IndexOf("y");
                int idxZ = propertyNames.IndexOf("z");

                int idxFdc0 = propertyNames.IndexOf("f_dc_0");
                int idxFdc1 = propertyNames.IndexOf("f_dc_1");
                int idxFdc2 = propertyNames.IndexOf("f_dc_2");

                int idxOpacity = propertyNames.IndexOf("opacity");

                int idxScale0 = propertyNames.IndexOf("scale_0");
                int idxScale1 = propertyNames.IndexOf("scale_1");
                int idxScale2 = propertyNames.IndexOf("scale_2");

                int idxRot0 = propertyNames.IndexOf("rot_0");
                int idxRot1 = propertyNames.IndexOf("rot_1");
                int idxRot2 = propertyNames.IndexOf("rot_2");
                int idxRot3 = propertyNames.IndexOf("rot_3");

                // Detect SH/f_rest properties
                var shIndices = propertyNames
                    .Select((name, idx) => new { name, idx })
                    .Where(p => p.name.StartsWith("f_rest_") || p.name.StartsWith("sh_"))
                    .Select(p => p.idx)
                    .ToArray();

                // Allocate arrays
                gsplatAsset.Positions = new Vector3[vertexCount];
                gsplatAsset.Colors = new Vector4[vertexCount];
                gsplatAsset.Scales = new Vector3[vertexCount];
                gsplatAsset.Rotations = new Vector4[vertexCount];

                bool shDivisibleBy3 = shIndices.Length % 3 == 0;
                int shVectorCount = shDivisibleBy3 ? shIndices.Length / 3 : shIndices.Length;
                if (shIndices.Length > 0)
                    gsplatAsset.SHs = new Vector3[vertexCount * (shDivisibleBy3 ? shVectorCount : 1)];

                gsplatAsset.SplatCount = vertexCount;
                gsplatAsset.SHBands = GsplatUtils.CalcSHBandsFromCoefficientCount(shIndices.Length);

                var buffer = new byte[propertyNames.Count * sizeof(float)];

                for (uint i = 0; i < vertexCount; i++)
                {
                    int readBytes = fs.Read(buffer, 0, buffer.Length);
                    if (readBytes != buffer.Length)
                    {
                        if (GsplatSettings.Instance.ShowImportErrors)
                            Debug.LogError($"{ctx.assetPath} import error: unexpected EOF at vertex {i}");
                        return;
                    }

                    var properties = MemoryMarshal.Cast<byte, float>(buffer);

                    // Position
                    gsplatAsset.Positions[i] = new Vector3(
                        properties[idxX],
                        properties[idxY],
                        properties[idxZ]
                    );

                    // Color
                    gsplatAsset.Colors[i] = new Vector4(
                        properties[idxFdc0],
                        properties[idxFdc1],
                        properties[idxFdc2],
                        GsplatUtils.Sigmoid(properties[idxOpacity])
                    );

                    // Scale
                    gsplatAsset.Scales[i] = new Vector3(
                        Mathf.Exp(properties[idxScale0]),
                        Mathf.Exp(properties[idxScale1]),
                        Mathf.Exp(properties[idxScale2])
                    );

                    // Rotation
                    gsplatAsset.Rotations[i] = new Vector4(
                        properties[idxRot0],
                        properties[idxRot1],
                        properties[idxRot2],
                        properties[idxRot3]
                    ).normalized;

                    // SH/f_rest
                    if (shIndices.Length > 0)
                    {
                        if (shDivisibleBy3)
                        {
                            for (int j = 0; j < shVectorCount; j++)
                            {
                                gsplatAsset.SHs[i * shVectorCount + j] = new Vector3(
                                    properties[shIndices[j * 3]],
                                    properties[shIndices[j * 3 + 1]],
                                    properties[shIndices[j * 3 + 2]]
                                );
                            }
                        }
                        else
                        {
                            for (int j = 0; j < shIndices.Length; j++)
                            {
                                gsplatAsset.SHs[i] = new Vector3(properties[shIndices[j]], 0, 0);
                            }
                        }
                    }

                    // Bounds
                    if (i == 0) bounds = new Bounds(gsplatAsset.Positions[i], Vector3.zero);
                    else bounds.Encapsulate(gsplatAsset.Positions[i]);

                    EditorUtility.DisplayProgressBar(
                        "Importing Gsplat Asset",
                        "Reading vertices",
                        i / (float)vertexCount
                    );
                }
            }

            gsplatAsset.Bounds = bounds;
            ctx.AddObjectToAsset("gsplatAsset", gsplatAsset);
            EditorUtility.ClearProgressBar();
        }
    }
}
