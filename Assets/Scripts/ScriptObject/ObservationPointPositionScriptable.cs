using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptObject
{
    [CreateAssetMenu(fileName = "观测位置配置文件", menuName = "位置文件选项/观测位置配置文件", order = 1)]
    public class ObservationPointPositionScriptable : ScriptableObject
    {
        [LabelText("观测点位置信息")] public List<ObservationPointPositionInfo> infos;
    }

    [System.Serializable]
    public class ObservationPointPositionInfo
    {
        /// <summary>
        /// 观测点
        /// </summary>
        public Transform observationPoint;

        /// <summary>
        /// 播放内容
        /// </summary>
        public List<IntroductionMedia> introductionChart;

        public string editorLine = " ---------------------------------------- ";
    }

    [System.Serializable]
    public class IntroductionMedia
    {
        public enum MediaType
        {
            Image,
            Video
        }

        public MediaType mediaType;

        /// <summary>
        /// 只用一个字段即可，根据类型判断是图像还是视频
        /// 可以是 Texture 或 VideoClip（或其他）
        /// </summary>
        public Object mediaContent;

        /// <summary>
        /// 音频
        /// </summary>
        public AudioClip audioClip;
    }
}