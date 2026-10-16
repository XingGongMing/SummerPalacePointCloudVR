#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils
{
    public class PanoramaUtils : MonoBehaviour
    {
        public int interceptionResolution = 4096;

        private const string TexturesPath = "Assets/Textures/";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Export360Panorama();
            }
        }

        public void Export360Panorama()
        {
            var assetPath = Export360Panorama(interceptionResolution);

            AssetDatabase.Refresh();

            var panoramaTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Debug.Log(assetPath);
        }

        /// <summary>
        /// 导出全景图
        /// camera:用来拍场景里内容的相机
        /// width:全景图的宽度，高度是宽度的1/2
        /// </summary>
        private string Export360Panorama(int width)
        {
            var cubemap = new RenderTexture(width, width, 0)
            {
                dimension = TextureDimension.Cube
            };

            if (Camera.main != null) Camera.main.RenderToCubemap(cubemap, 63, Camera.MonoOrStereoscopicEye.Mono);

            var equirect = new RenderTexture(width, width / 2, 0);

            cubemap.ConvertToEquirect(equirect);

            var tex = new Texture2D(equirect.width, equirect.height, TextureFormat.ARGB32, false, true);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            var bytes = tex.EncodeToPNG();
            // var fileName = $"{System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.CurrentCulture)}.png";
            var fileName = $"{Camera.main.name}.png";
            var editorPath = Path.Combine(Application.dataPath, "Textures", fileName);
            File.WriteAllBytes(editorPath, bytes);

            // 新增：将新生成的纹理添加到Unity的资源数据库中
            AssetDatabase.ImportAsset(TexturesPath + fileName);

            Debug.Log($"全景图已导出至: {editorPath}");

            return TexturesPath + fileName;
        }
    }
}
#endif